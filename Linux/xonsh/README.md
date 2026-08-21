# C# productivity suite

87 C# programs, ported from the xonsh/Python originals in [../Xonsh](../Xonsh).

Each one is a **C# file-based app**: a single executable file that declares its own
NuGet packages inline. There is no `.csproj` and no build step to remember — the
first run compiles, later runs use the cache.

## Running one

```sh
./media-length                     # in a folder of media
./audio-speed-change -s 1.5 *.opus
./file-tag physics lecture-*.mkv
./<anything> --help
```

Nothing is installed anywhere. These run in place, and find each other in place.

## The header every program starts with

```sh
#!/bin/sh
//bin/true; /opt/anaconda3/envs/dotnet/lib/dotnet/dotnet "$0" -- "$@"; __rc=$?; [ -t 1 ] && printf '\033[?1l\033>'; exit $__rc
#:package Spectre.Console@0.57.2
#:property Configuration=Release
#:include utilities.cs
```

Line 2 is doing two jobs at once. To `sh` it runs `/bin/true`, runs the file under
`dotnet`, then restores the terminal and passes the exit code on. To the C#
compiler it is an ordinary `//` comment, so the `#:` directives below it still parse.

Three reasons it is shaped that way rather than a plain `#!/…/dotnet` shebang:

- **`--` stops `dotnet run` from eating the program's own flags.** Without it,
  `dotnet run` claims `--help`, `-c`, `-f`, `-e`, `--file` and others before the
  program ever sees them. This suite uses `-f` (`epub-convert-multiple` font size),
  `-e` (`media-split-equal` equal parts) and `--help` everywhere, so they would
  silently go missing.
- **It is about twice as fast.** Measured on this machine: 0.36 s per run with the
  `--`, 0.68 s without.
- **It can restore the terminal after `dotnet` has exited** — see below.

The absolute path to `dotnet` is deliberate — ranger, `.desktop` actions and cron
do not have the conda environment activated.

## Shared code

| File | Contents | Included by |
| --- | --- | --- |
| `utilities.cs` | `Ui`, `Proc`, `Fs`, `Media`, `Crypto`, `Snd`, `Clip`, `Num`, `Sys`, `PassStore`, `Paths`, `Args` | all 87 |
| `tui.cs` | `Dlg` — modal dialogs | the 5 that need a dialog box |

`#:include` compiles the file into the including program, so **every type
`utilities.cs` mentions must resolve in every program that includes it**. That is
why `utilities.cs` references only the BCL and Spectre.Console, and why the
Terminal.Gui helpers live in a separate `tui.cs` — otherwise all 87 programs would
have to carry Terminal.Gui.

Notable pieces:

- **`Args`** — an argparse-compatible parser. Same flag syntax, same `--help`
  layout, same exit codes as the Python originals.
- **`Proc`** — `Run` / `Ok` / `Capture` / `Spawn` / `Call`, mapping onto xonsh's
  bare commands, `!( )`, `$( )` and trailing `&`. Everything goes through
  `ArgumentList`, never a concatenated string, so filenames with spaces or quotes
  are safe.
- **`Sys`** — every destructive operation, behind one `DryRun` switch.
- **`Ui.TrackParallel`** — progress bar over work that runs concurrently, used
  where the loop is a subprocess per file (ffprobe, cwebp, pdftotext).

## Composition

Programs call each other by name, resolved via `Proc.Call`, which prefers the copy
sitting next to the running script and falls back to `PATH`. So composition works
straight out of this folder, installed or not.

```
audio-process ──────────────► audio-speed-change / audio-volume-change
media-length-move-tui ──────► media-length-move
epub-convert-multiple-tui ──► epub-convert-multiple / epub-split
image-watermark-tui ────────► image-watermark
directory-file-permission-set-tui ─► directory-file-permission-set
media-split-equal ──────────► file-tag-percentage
pdf-convert-audio ──────────► pdf-convert-text ──► text-to-speech
password-generate ──────────► get-passphrase-strength
shortcut-update ────────────► snapper-rollback-snapshot-delete, hdd-size
update-shutdown ────────────► shortcut-update
```

The notification sound is the one exception: `Snd.Success()` calls `ffplay`
directly rather than going through `audio-play`, because a nested .NET process
would add roughly a second to every program in the suite. `audio-play` still
exists for ranger bindings and shell aliases.

## CLI first, dialogs in wrappers

Every program is a first-class CLI tool: all of its input arrives as arguments, and it
never prompts. Anything that needs to ask the user is a separate `*-tui` wrapper that
collects the answers and passes them to the base program.

This is not just tidiness. A program that prompts cannot be driven from cron, a pipe,
or another program — `media-split-equal` used to die outright with *"Cannot show
selection prompt since the current terminal isn't interactive"* the moment stdin was
not a terminal. Every program listed under **Parallel conversion** is now runnable
with stdin, stdout and stderr all redirected.

It also removes a real bug. `pdf-split` and `text-split` each took a size as an
argument *and* then showed a menu that overwrote it, so the value configured in the
caller was silently discarded unless you happened to pick "Custom". The argument is
now authoritative and the menus are gone.

Where a prompt asked whether to keep the originals, that is now a `-d/--delete` flag,
defaulting to off — the same shape `audio-speed-change` and `audio-volume-change`
always had. Without it the originals are moved into `Original_<kind>/`; with it they
go to the trash.

**No CLI program prompts.** Every one of them runs with stdin, stdout and stderr
redirected. The wrappers are:

| Wrapper | Collects | Base program |
| --- | --- | --- |
| `audio-add-music-tui` | which file is the music, its level, playback speed | `audio-add-music` |
| `audio-convert-foss-tui` | a use case → bitrate + codec | `audio-convert-foss` |
| `audio-process-tui` | speed or volume, and the value | `audio-speed-change` / `audio-volume-change` |
| `directory-file-permission-set-tui` | a permission preset | `directory-file-permission-set` |
| `epub-convert-multiple-tui` | output format and page size | `epub-convert-multiple` / `epub-split` |
| `file-rename-extension-tui` | the new extension | `file-rename-extension` |
| `image-watermark-tui` | the watermark text | `image-watermark` |
| `media-length-move-tui` | listening hours, sort order | `media-length-move` |
| `mkv-extract-track-tui` | a track number **per file**, after showing `mkvinfo` | `mkv-extract-track` |
| `otp-copy-tui` · `password-copy-tui` · `password-show-tui` | which store entry | the matching CLI |
| `password-generate-tui` | password or seed phrase, and its length | `password-generate` |
| `video-download-tui` | the download profile | `video-download` |

`audio-process-tui` was `audio-process`: it already held no logic of its own, only a
menu in front of two other programs, so it was renamed rather than split.

The three store pickers keep `Ui.Select` rather than a Terminal.Gui dialog, on
purpose: a password store has too many entries to arrow through and `Dlg.Choose` has
no search. The separation that matters is CLI versus wrapper, not which widget the
wrapper uses.

`mkv-extract-track` is the one place the split costs something. It used to print
`mkvinfo` and ask for a track per file, inside its loop. The CLI now takes one
`--track` for the whole batch; the wrapper preserves the per-file choice by running
`mkvinfo` itself and calling the CLI once per file.

## Dialog wrappers

The `*-tui` programs put up a dialog and then hand the answers to their base program.
They hold no logic of their own.

Choices render as a **vertical list**, navigated with the up and down arrows and
confirmed with Enter — the same shape as the `prompt_toolkit` `radiolist_dialog`
they replace. (Not a `MessageBox`: that lays its choices out as a row of buttons,
which navigate with Tab and Left/Right, so the arrow keys do nothing.)

Built on the scoped `Application.Create()` API; the static `Application.Init/Run/
Shutdown/Instance` members are obsolete in Terminal.Gui 2.4 and warn on every build.

Terminal.Gui needs a terminal it can measure, and draws nothing rather than
complaining when it cannot get one. Two things make that safe here:

- **The size is measured before Terminal.Gui starts**, via `Console.WindowWidth` —
  an ioctl that writes nothing and reads nothing. Terminal.Gui's own method is to
  write escape queries (`CSI 18 t`, `CSI 6 n`) and read the replies off stdin;
  under some launchers those replies never arrive, and it is left believing the
  screen is 0x0. Worse, if we then fell back, those unread replies sat in the input
  buffer and the Spectre prompt read them as keystrokes — desynchronising its key
  parser so arrow keys arrived as literal `^[OB` text and the list would not move.
  Measuring first avoids both, and the real size is handed to `app.Screen` when
  Terminal.Gui's own answer is unusable.
- **Any remaining failure is reported, not swallowed.** A silent fallback once made
  a real bug look like a design choice. `TUI_QUIET=1` suppresses the notice,
  `TUI_TRACE=1` adds a stack trace.

With no terminal at all it says so and exits cleanly, so these still behave
sensibly from a pipe or a cron job.

## Building

```sh
./build-all.sh          # compile everything, report failures
./build-all.sh audio    # only programs matching "audio"
```

This builds in place. It installs nothing. Run it once after cloning so the first
real use of each program does not pay the compile, and again after editing
`utilities.cs` — a change there invalidates all 87 caches.

Build output goes to `~/.local/share/dotnet/runfile` (~2 MB per program).

## Installing

Not done for you, by design. When you want it:

```sh
for f in <program-names>; do ln -sfn "$PWD/$f" ~/.local/bin/"$f"; done
ln -sfn "$PWD/utilities.cs" ~/.local/bin/utilities.cs
ln -sfn "$PWD/tui.cs"       ~/.local/bin/tui.cs
ln -sfn "$PWD/global.json"  ~/.local/bin/global.json
```

`utilities.cs`, `tui.cs` and `global.json` must be alongside the symlinks:
`#:include` resolves relative to the **entry file's own directory**, which for a
symlinked program is `~/.local/bin`, not this folder.

Note that these share names with the xonsh originals. Whichever comes first on
`PATH` wins, so install deliberately.

## Packages

Six, total. Only Spectre.Console is on more than five programs.

| Package | Used by |
| --- | --- |
| `Spectre.Console` 0.57.2 | all 87 (replaces `rich` and `simple_term_menu`) |
| `Terminal.Gui` 2.4.17 | the 5 dialog programs (replaces `prompt_toolkit`) |
| `PdfPig` 0.1.15 | `pdf-combine`, `pdf-split` |
| `Google.Cloud.TextToSpeech.V1` | `text-to-speech` |
| `Google.Apis.Auth` | `ocr-genai` |
| `ScottPlot` 5.x | `audiobook-distribution` |

Versions are pinned rather than `@*` so a future release cannot silently change 87
programs at once.

Everything else shells out to the tools the originals used — `ffmpeg`, `mkvmerge`,
`cwebp`, `tesseract`, `pandoc`, `ebook-convert`, `gs`, `pdftotext`, `magick`,
`pass`, `pwgen`, `rsync`, `btrfs`. Replacing those with libraries would change
behaviour for no gain.

Deliberately **not** used, and why:

- **TagLibSharp** — `utilities.cs` cannot reference extra packages, so durations
  must come from `ffprobe` anyway. Two engines would make `media-length` and
  `media-length-move` disagree about the same folder. Parallel `ffprobe` removes
  the speed argument.
- **ImageSharp** — v3+ carries the Six Labors Split Licence, which is not free for
  every organisation, and v2.x is end-of-life. `magick` is installed and does the
  job.
- **TextCopy** — shells out to `xclip`/`wl-copy` itself, and clipboard access has
  to live in `utilities.cs`, where no package is allowed.
- **Humanizer** — its `ToWords()` is `long`-only, and `password-generate` reports a
  40-digit number.
- **Spectre.Console.Cli** — separate package, stable line lags Spectre.Console, and
  its `CommandApp` model fights top-level statements.

## The terminal is handed back the way it was found

The .NET host applies terminfo's `smkx` on startup — `ESC[?1h ESC=`, application
cursor-key mode — and never emits the matching `rmkx`. Verified: `dotnet --version`
alone leaves it set, while `/bin/echo` does not. Spectre and Terminal.Gui do the
same when they take the keyboard.

Left set, arrow keys keep arriving as SS3 (`ESC O B` rather than `ESC [ B`) after
the program has exited, so whatever runs next sees them as stray text — a file
manager stops responding to the arrows and fills with `^[OB`.

So the shell trampoline restores it, after `dotnet` has fully exited:

```sh
//bin/true; …/dotnet "$0" -- "$@"; __rc=$?; [ -t 1 ] && printf '\033[?1l\033>'; exit $__rc
```

The reset has to come from the shell rather than from C#: a `ProcessExit` handler
runs *before* the host's own console teardown, which re-applies `smkx` afterwards.
`[ -t 1 ]` keeps the escape out of redirected output — several of these programs
exist to be read by other programs. `Term` in `utilities.cs` additionally drains
buffered input after a prompt.


## Notification sounds and backgrounded processes

`audio-play`'s header is the one exception in the suite — stdout goes to
`/dev/null` and there is no terminal reset:

```sh
//bin/true; exec …/dotnet "$0" -- "$@" >/dev/null
```

Callers background it (`audio-play … &`). A backgrounded .NET process takes a
moment to start, so by the time its runtime emits `smkx` the foreground command
has finished and a file manager has already redrawn the screen — those escapes
land in the middle of its UI, and the trailing reset un-sets the keypad mode the
file manager just set for itself. Measured: 21 bytes of escape sequences arriving
after the foreground command exited; with stdout on `/dev/null`, zero.

That is enough when the caller is `/bin/sh`. It is **not** enough under xonsh,
whose job control still lets the escapes through — measured at 14 bytes even with
the redirect in place. So the xonsh `epub-split` plays no sound at all;
`epub-convert-multiple-tui` plays it afterwards, from the foreground, where `Snd`
reaches `ffplay` directly and writes nothing.


## Rules after external output

A rule drawn straight after a child process can come out as a short broken stub.
`nms`, which animates a password into view for `password-show`, finishes without a
trailing newline, so the rule started at column 8 and wrapped.

`Console.CursorLeft` cannot help — it reports 0 regardless of what a child wrote,
and even after our own writes. So `Proc` notes whenever a child inherits the
terminal, and `Ui.Begin`/`End`/`Show` start on a fresh line when one has. The cost
is at most one blank line where the child already ended cleanly, which reads as
deliberate spacing.

## Menu navigation

Menus wrap: up from the first entry lands on the last, down from the last on the
first. With a five-item list the wanted entry is often the bottom one, and a
single press up beats scrolling past everything.

Menus longer than seven entries also accept **type-to-search**, which is what
`simple_term_menu`'s search gave the originals. That covers the password store
(45 entries) and the OTP store (26); shorter menus stay plain so a two-line
confirmation does not carry a search hint.

## Backing out of a menu

Every menu takes **Escape** to cancel. Because these menus gate the work that
follows, cancelling one cancels the command: it prints `Cancelled.`, exits 0, and
touches nothing. 17 programs have menus and all of them behave this way.

Note that a lone Escape can take a moment to register — the terminal has to rule
out an escape *sequence* (an arrow key sends `ESC [ A`) before it can treat the
byte as a bare Escape. Pressing it twice is instant.

`q` is not a cancel key. Spectre's prompt reads its own keys and exposes no hook
for arbitrary ones, so `q` would need a hand-rolled prompt to replace it.

## Parallel conversion

Eleven programs process their inputs concurrently and take `-j/--jobs`:

`audio-speed-change` · `audio-volume-change` · `audio-convert-foss` ·
`video-convert-audio` · `image-convert` · `image-convert-text` · `pdf-convert-text` ·
`document-convert` · `epub-convert-multiple` · `image-watermark-tui` · `pdf-split`

Measured on this machine:

| Program | Serial | Parallel | |
| --- | --- | --- | --- |
| `audio-speed-change`, eight 3-minute Opus files | 5.6 s | 1.6 s at `-j 8` | 3.5× |
| `epub-convert-multiple`, four epubs to PDF | 7.6 s | 2.5 s at `-j 4` | 3.1× |
| `image-watermark-tui`, eight 1200×900 images | 7.6 s | 1.7 s at `-j 8` | 4.4× |
| `pdf-split`, seven 60-page PDFs | 1.9 s | 0.9 s at `-j 8` | 2.2× |
| `epub-split` (xonsh), six epubs → 162 parts | 3.4 s | 1.0 s at `-j 6` | 3.5× |
| `epub-convert-multiple`, six epubs → PDF | 17.1 s | 4.5 s at `-j 6` | 3.8× |

The binaries these drive (Opus, cwebp, tesseract, pdftotext, Calibre, magick) each
run one job on one core, so the win comes from running several at once rather than
from threading any one of them.

`pdf-split` is the exception to the rule below: its work is in-process (PdfPig page
copies and one file write per part) rather than a child process, but a long book is
enough of both to be worth overlapping.

### One display for every parallel program

Anything with `-j` uses the same **nested** display, via `Ui.Track2`: a bar per item
on top, and the overall bar pinned underneath. (The xonsh `epub-split` matches it
with rich's equivalent.) A single shared bar tells you how many items are done but
not which are running, which reads as though nothing is happening in parallel at all
— so every one of these programs shows what is in flight.

The per-item bars are inserted *before* the overall bar rather than after it
(`ProgressContext.AddTaskBefore`), so the overall bar is always the bottom-most row.
On a run with more items than the terminal has rows, the finished bars scroll up out
of view and the overall bar stays on screen where it can still be read. Every
description goes through `Ui.FitDescription`, so all the bars start in the same
column whatever the file is called.

What fills a per-item bar depends on what the tool will tell us:

| Programs | Unit | Source |
| --- | --- | --- |
| the four ffmpeg converters | % of the source's duration | `-progress pipe:1` → `out_time_us=`, against `Media.Duration`, via `Media.ConvertWithProgress` |
| `epub-convert-multiple` | % reported by Calibre | `ebook-convert` prints `34% Running transforms on e-book...` on stdout |
| `pdf-split` | pages | counted directly |
| the rest | one step | cwebp, tesseract, pdftotext, pandoc and magick report nothing, so the bar is a single step and the spinner shows the file is in flight |

Both progress-parsing paths go through `Proc.OkStreaming`, which hands each stdout
line to a callback as it arrives while draining stderr on a separate task.

`Media.ConvertWithProgress` does not take ffmpeg's exit code as the last word on
whether a conversion happened. Two ways it lies:

- **It opens the output before it discovers it cannot encode**, leaving a nought-byte
  file where a converted one should be — indistinguishable from a success at a glance.
  Ask `audio-convert-foss` for 450 kbps on mono audio and libopus refuses (it caps at
  256 kbps *per channel*), and that is what you used to be left with.
- **`-n` refusals exit 0.** With `-n` (which `video-convert-audio` passes) and an
  output that already exists, this ffmpeg build prints *"File 'x' already exists.
  Exiting."* to stderr and then exits **zero**. Taken at face value that counted as a
  conversion: the untouched file got stamped with the source's date and the source was
  filed away into `Original_<kind>/` as though it were done.

So success means *the output was actually written* — the destination's existence, size
and mtime are compared across the run — and a failed run removes an output it created
itself. It never removes one that was already there, which is exactly the file the
`-n` refusal is protecting.

A single-step bar carries a small bonus: it is only advanced on success, so a file
that failed keeps a visibly empty bar rather than a full one. ffmpeg's last report
usually stops just short of the end, and a `txt` conversion reports far fewer stages
than a `pdf` one, so those bars are topped up to 100% when the tool exits cleanly.

For everything else the selection criterion is a **per-file child process**. Programs
that only rename files — `file-tag`, `file-number-remove`, `file-rename-extension` and
the rest — get
no `-j`, and should not: `rename(2)` is tens of microseconds, so the program's own
~0.36 s launch dwarfs the work, and there is no child process to overlap.

Several serial loops are serial because parallelism would be *wrong*, not slow.
`file-number`, `file-number-deep`, `file-tag-percentage`, `file-timestamp-sync` and
`directory-number` each mutate a counter that determines the output filename.
`video-convert-whatsapp` shares one `-passlogfile` across iterations,
`media-split-equal` finds its output by globbing the current directory, and
`mkv-extract-track` prompts inside its loop.

`-j` has no hardcoded default. It is computed at startup from the machine:

```
min( usable cores / 2,  MemAvailable / 512 MB )
```

`Environment.ProcessorCount` already follows CPU affinity and cgroup quotas, so
"usable cores" is what this process may actually run on, not what is installed —
verified: `taskset -c 0-3` yields 2, `taskset -c 0` yields 1. Halved because the
tools being driven thread internally, and one process per core oversubscribes for
very little extra throughput: the same run costs 9.3 s of CPU serially and 14.1 s
in parallel. The memory term stops a 64-core box with 2 GB free from starting 32
copies of xelatex.

`--help` shows the working:

```
-j JOBS  Files to process at once, 1 for one at a time
         [16 cores/2 = 8, 29128 MB free/512 MB = 56 -> 8; set JOBS to override]
```

Set `JOBS` in the environment to override it everywhere at once.

Use **`-j 1`** on a spinning disk, where several concurrent read/write streams
lose more to seeking than they gain, or when you want the output in a
deterministic order.

Two consequences worth knowing:

- **Message order is arbitrary.** Spectre serialises the writes so lines arrive
  intact, but which file reports first is down to timing. Sorting is unaffected —
  see the section below.
- **One error sound, at the end.** A per-failure beep would become a pile of
  overlapping `ffplay` processes; instead the run reports `N file(s) failed` and
  exits non-zero.

## Derived files keep their source's date

Every program that produces a file from another file copies the source's
modification time onto the output, via `Fs.CopyTimestamp`.

This matters because these folders get sorted by modified time. Without it, the
output order is whatever order the files happened to finish in — and for the
programs that convert concurrently (`pdf-convert-text`, `image-convert`,
`image-convert-text`, `epub-convert-multiple`) that order is arbitrary, so a folder
of extracted text comes out jumbled relative to the PDFs it came from. Carrying the
date across means a modified-time sort of the output reads in exactly the same order
as the input.

Splitters (`pdf-split`, `text-split`, `media-split-equal`,
`video-convert-whatsapp`) give every part the source's date, so the group as a whole
stays where the original was — and each part is offset **one millisecond** further
than the last, via `CopyTimestamp`'s third argument.

That offset is not cosmetic. Parts written inside the same millisecond end up with
byte-identical timestamps, which leaves a modified-time sort nothing to order them
by, and a file manager's tie-break can then show part 003 above part 001.

Where two files are merged into one, the output is named after whichever input it is
really a version of, and inherits *that* file's date: `embed-subtitle` takes the
video's (not the subtitle's), `audio-add-music` takes the speech track's (not the
music's), and `mkv-extract-track` takes the container's.

True combiners (`pdf-combine`, `media-combine`, `image-combine-pdf`) are left alone
— there is no single source date to inherit, so the output is genuinely new and gets
the current time.

**Renaming does not disturb any of this.** `rename(2)` updates a file's ctime, never
its mtime, so the tagging and numbering programs can reorder names freely without
touching a modified-time sort. This is why `media-split-equal` can stamp its parts
and *then* hand them to `file-tag-percentage` — the stamps survive the rename.

### The one way to lose this: copying without `--preserve`

All of the above only works if the *sources* carry meaningful dates. `cp` does not
preserve them unless you ask:

```sh
cp -rv --reflink=auto            src dst   # every copy gets the CURRENT time
cp -rv --reflink=auto --preserve=timestamps src dst   # dates carried across
```

`--reflink` and `--preserve` are orthogonal — the first controls data blocks (CoW
sharing), the second controls metadata. ranger's own `yy pp` is safe: it goes through
`copystat` → `os.utime(dst, ns=(...))` and preserves mtime to the nanosecond. A
`shell cp` binding does not, unless the flag is there.

On btrfs this is much worse than it looks, because a reflink copy is metadata-only and
so takes the same ~2 ms whether the files are 2 MB or 2 GB. Measured on three 200 MB
files:

| copy | mtime spread across the three files |
| --- | --- |
| `--reflink=always`, no preserve | **0.000000 s — byte-identical** |
| `--reflink=never`, no preserve | 0.188 s |
| `--reflink=always --preserve=timestamps` | the real gap, untouched |

Identical source dates mean the parts collide too — every group's part 1 lands on the
same stamp, every group's part 2 on the same stamp — and a modified-time listing
degenerates into whatever the file manager's tie-break decides:

```
1787111491.995283  vid-01-part-001-002r-050p.mkv
1787111491.995283  vid-02-part-001-002r-050p.mkv     <- tie
1787111491.996283  vid-01-part-002-001r-100p.mkv
1787111491.996283  vid-02-part-002-001r-100p.mkv     <- tie
```

A plain non-reflink `cp` gets away with it by accident: it is slow enough (~96 ms per
file above) that the copies land far enough apart not to collide, so the groups stay
separate even though the original dates are gone.

The fix belongs on the copy side, not here — no amount of offsetting can recover an
ordering that was destroyed before these programs ran.

## Destructive programs

Everything that deletes, overwrites or powers something off takes `--dry-run`,
which prints the exact commands instead of running them:

```sh
./laptop-backup-c --dry-run
./shortcut-update --dry-run --clean
./queue-move --dry-run
```

These were verified against the xonsh originals by comparing dry-run output
line-by-line, and were never executed during the port.

## Differences from the originals

Deliberate, all of them:

- **Bugs fixed.** `queue-move` hardcoded `/media/manuj/` on one line where every
  other used `$USER`. `snapper-rollback-snapshot-delete` printed "Would delete…"
  but deleted regardless. `directory-file-permission-set` prefixed paths with `./`,
  so it only ever worked on folders directly below the working directory.
  `file-number-deep` called `listdir()` inside `os.walk()`, visiting files once per
  directory level. Several `except argparse.ArgumentError` handlers referenced an
  out-of-scope `parser`.
- **`file-timestamp-sync` no longer sleeps.** It ran `touch` then `sleep` per file,
  so 100 files at a 60-second gap took 100 minutes of wall clock. It now sets the
  timestamps directly, with identical spacing, instantly.
- **`shortcut-update` no longer wipes `~/.local/share/dotnet/runfile`.** That is
  where this suite's compiled output lives; the original `rm -rf` would have forced
  all 88 programs to rebuild every other Saturday. It now prunes artifacts older
  than 30 days.
- **`get-passphrase-strength` is exact.** The original divided a 47-digit keyspace
  through a double, losing everything past the sixteenth digit. It now uses exact
  integer arithmetic.
- **Parallelism** where the work is independent: `media-length`, `media-length-tag`,
  `image-convert`, `image-convert-text`, `pdf-convert-text`.
- **`--help` and `--dry-run`** on programs that previously had neither.
- **`epub-split` is left in xonsh**, sitting in this folder alongside the C#
  programs (with `utilities.xsh` beside it). Its EPUB chunking — cutting XHTML on
  tag boundaries, reopening ancestor elements across the cut, re-injecting
  `<head>` metadata, copying only referenced media — works, and reimplementing it
  would risk producing subtly broken books for no gain.
  `epub-convert-multiple-tui` invokes it through the xonsh interpreter explicitly,
  checking the shebang first so a same-named C# file can never be handed to xonsh.

  It does, however, match the C# programs' behaviour: it takes `-j/--jobs` with the
  same machine-derived default, splits books concurrently on a thread pool (zlib and
  lxml release the GIL for the heavy work, so this is a real 3.5× on six books), and
  draws the same nested display — a bar per book with an overall bar underneath. Two
  things used to corrupt that display and are now handled: ebooklib triggers an lxml
  `FutureWarning` on every `read_epub`, which is silenced before ebooklib is
  imported; and `file-tag-percentage` draws its own progress bar, so it is run as a
  plain subprocess with its output discarded rather than through xonsh's
  `@()` form — which also makes it safe to call from a worker thread.
- **`fontpreview-ueberzug`** is copied unchanged. It was already a POSIX shell
  script driving fzf and ImageMagick, not Python.
