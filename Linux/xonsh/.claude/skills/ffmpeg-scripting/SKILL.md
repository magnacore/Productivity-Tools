---
name: ffmpeg-scripting
description: Traps when writing or debugging a program that drives ffmpeg or ffprobe - conversion, probing, splitting, deinterlacing, or measuring quality. Use when adding a conversion mode, changing encoder arguments, parsing ffprobe output, choosing a CRF or bitrate, or investigating a media conversion that "succeeded" but produced nothing, produced the wrong thing, or silently skipped files. Every trap here produced a plausible wrong answer rather than an error.
---

# Driving ffmpeg and ffprobe from a program

Everything below was found by a real failure in this repo, most of it in
`CS/video-convert`. The common thread is worth stating before the details:

**None of these produce an error. They produce a plausible wrong answer.** A
conversion that reports success and writes nothing. A folder of videos that gets
copied instead of converted. A quality measurement that ranks CRF 24 and CRF 37 as
identical. The exit code is clean every time.

So the discipline that actually catches them is: **check the artifact, not the exit
code.** Probe the output file, `idet` it, `md5` its stream, count its frames. If you
are about to report a number to someone, first run the measurement against a case
whose answer you already know.

---

## 1. ffmpeg exits 0 having written nothing

Seen at least three ways:

- `-n` with an existing output: prints `File 'x' already exists. Exiting.`, exits 0.
- An encoder that fails to open (see libopus surround below): prints
  `Nothing was written into output file`, exits 0.
- An unknown muxer (see the extension trap below).

**Never trust the exit code alone.** `Media.ConvertWithProgress` in `utilities.cs`
exists for this: it records the destination's existence, size and mtime beforehand and
requires them to have changed. That check is what turned the 5.1 failure into a
reported error instead of a silent zero-byte file.

A first pass that writes to `/dev/null` has no output to check, which is why
`Media.RunWithProgress` is split out — use it for pass 1, `ConvertWithProgress` for
pass 2.

## 2. ffmpeg picks the muxer from the final extension

Writing to a temporary name and moving it into place is the right pattern for
resumable work, but the temp name must keep the real extension:

```
clip #fast.mkv.partial     ->  "Requested output format 'partial' is not known"
clip #fast.partial.mkv     ->  works
```

Put the marker **before** the extension.

## 3. ffprobe's CSV is not one clean value per line

This cost two separate fixes to the same four-line function.

```
$ ffprobe -v error -select_streams v:0 -show_entries stream=field_order -of csv=p=0 camera.MTS
tt
                      <- AVCHD carries the same programme twice
tt

$ ffprobe ... VTS_01.VOB
tt,                   <- trailing comma
```

Comparing the whole reply against `"tt"` called both files progressive, so neither was
deinterlaced — and the conversion succeeded, so nothing announced it. Parse defensively:

```csharp
string value = output
    .Split(['\n', '\r', ','], StringSplitOptions.RemoveEmptyEntries
                            | StringSplitOptions.TrimEntries)
    .FirstOrDefault() ?? string.Empty;
```

For a per-stream value where ffmpeg's own default selection matters (audio channels),
take the **maximum across streams**, not the first: ffmpeg picks the audio track with
the most channels, so your probe must agree with what it will choose.

## 4. `-v error` suppresses the summary line you are trying to read

`idet`, `ssim` and `psnr` print their summaries at INFO level. With `-v error` you get
an empty result and may conclude the filter did nothing. Drop the flag, or use
`-v info`, when the filter's own output is the point.

## 5. ssim and psnr pair frames by timestamp, not by index

Comparing an x264-in-MKV reference against a VP9-in-MKV encode gave **SSIM 0.794 for
both CRF 24 and CRF 37** — a four-fold bitrate difference scoring identically. The
frame counts matched; the timebases did not, so framesync paired the wrong frames.

Force both onto a frame index:

```
-lavfi "[0:v]settb=1/50,setpts=N[a];[1:v]settb=1/50,setpts=N[b];[a][b]ssim"
```

**Sanity-check the harness before trusting any number**: compare the reference against
itself. If it is not `All:1.000000`, the measurement is broken, not the codec.

Also: encode every variant from one lossless intermediate rather than re-decoding the
source each time. A damaged GOP (`reference picture missing during reorder`) can yield
a different number of leading frames per decode, which misaligns everything.

## 6. Two-pass settings must be identical across both passes

libx264 records the timebase in its stats file and refuses a second pass that
disagrees:

```
timebase mismatch with 1st pass (1/30 vs 8192/245515)
```

This came from `-r 30 -fps_mode cfr` being applied to pass 2 only. A 30 fps source
hides it — the timebase matches either way — and anything at 29.97 fails outright.
Put rate and filter settings in the shared argument list, not in one pass.

Give every file its **own** `-passlogfile`, in a temp directory. A single shared log is
the only reason a converter has to run serially, and it silently sizes one video
against another's statistics.

Those logs are not small: x264's `mbtree` file was **38 MB for a six-minute video**,
and `$TMPDIR` is often tmpfs, i.e. RAM. Delete them in a `finally` **and** on
`PosixSignalRegistration` for SIGINT/SIGTERM/SIGHUP/SIGQUIT — .NET does not raise
`ProcessExit` when a signal terminates the process, so Ctrl-C leaks them otherwise.
See `Fs.TempDirectory`.

## 7. libopus refuses surround twice over

```
[libopus] Invalid channel layout 5.1(side) for specified mapping family -1.
```

Two distinct problems in one message:

- mapping family 0 (the default) handles only mono and stereo;
- ffmpeg labels most broadcast surround `5.1(side)`, which Opus does not recognise
  **even under family 1** — it wants the back-channel spelling.

Setting the family alone still fails. Both are fixed by listing the layouts and letting
ffmpeg convert to the nearest:

```
-af aformat=channel_layouts=mono|stereo|3.0|quad|5.0|5.1|6.1|7.1 -mapping_family 1
```

Nothing is downmixed — 5.1 stays 5.1, 7.1 stays 7.1. Apply it only above two channels
so mono and stereo output stays byte-identical.

## 8. Deinterlacing: `deint=interlaced` does not do what the name suggests

It controls whether a frame is **filtered**, not whether one is **emitted per field**.
With `mode=send_field` a progressive 30 fps source still comes out at 60 fps — twice
the frames, twice the encode, nothing gained.

Decide per file from the container flag (`field_order` in `tt`/`bb`/`tb`/`bt`) and omit
the filter entirely otherwise. Then pick the mode by intent:

- `send_field` — a frame per field, 1080i50 becomes 1080p50. Keeps the motion. Right
  when fidelity is the goal.
- `send_frame` — one frame per frame. Combing gone, frame count untouched. Right when
  size or speed is the goal.

Deinterlacing is **not** the expensive choice: an interlaced 576-line source came to
500 KB with the combing encoded into it and 416 KB once deinterlaced. Combing is
high-frequency detail and the encoder pays for it.

Inverse telecine is a different operation (`fieldmatch` + `decimate`) for NTSC film
carried as 29.97i. Do not guess at it: applying it to genuinely interlaced video
discards one frame in five permanently, whereas skipping it only costs softness. The
signature is a large repeated-field count from `idet`.

## 9. "Is this a video?" is not a question the system can be trusted with

Twice a whole corpus was copied instead of converted because an extension was not
recognised:

- `.rmvb` — the system database simply did not know it.
- `.mts` — the system database **did** know it, and was wrong: Debian maps it to
  `model/vnd.mts`, a CAD format.

The second is the nastier one, because declaring `.mts` in your own table was not
enough — `/etc/mime.types` was applied on top and overwrote it. Python's
`mimetypes.init()` works that way and the original scripts copied the behaviour.

**Your own table must win; the system database fills in the rest.** And put a test on
it: `CS/tests/MimeTests.cs`. Camcorder and DVD extensions worth carrying: `.mts`,
`.m2ts`, `.vob`, `.m2v`, `.tod`. Leave `.mod` out — it is also the Amiga tracker
format.

## 10. Scaling and aspect

- `scale=-2:'min(H,ih)'` caps height without upscaling; `-2` keeps the width even,
  which chroma subsampling requires. The quotes protect the comma inside `min()` when
  the filter sits in a chain.
- It keys on **height**, so a 2560x720 ultrawide passes through untouched, and a
  portrait 1080x1920 is reduced hard — its short side goes from 1080 to 405.
- Anamorphic sources keep their pixel aspect: 1440x1080 AVCHD stays 1440x1080 with 16:9
  recorded in the container. Stretching to square pixels adds a third more pixels and
  no detail.

## 11. Checking that a change is a no-op

When a change is supposed to leave some class of input alone, prove it rather than
reasoning about it. Encode the same file with the old arguments by hand and compare:

```bash
ffmpeg -v error -i out_new.mkv -map 0:v -f md5 -
ffmpeg -v error -i out_old.mkv -map 0:v -f md5 -
```

Identical MD5s are the only convincing evidence that an existing corpus still converts
to what it converted to before.

## Shell traps while testing this stuff

- `pkill -f ffmpeg` matches your own shell and kills it (exit 144). Kill by recorded
  pid or process group instead.
- `setsid cmd &` forks, so `$!` is not the process you want to signal.
- `$?` after a pipeline is the **last** command's status, not ffmpeg's.
- Filenames here contain spaces and `#`. Quote everything; never word-split `find`.
