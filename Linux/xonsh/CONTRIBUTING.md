# Working on this repository

Branching follows **gitflow**. `git-flow` (the AVH helper) is not installed here, so
the plain-git equivalents are given below — they are what the helper runs anyway.

## The two permanent branches

| Branch | Holds |
| --- | --- |
| `main` | Only what is installed and known to work. Every commit is a release and carries a tag. Never commit here directly. |
| `develop` | Integration branch. Work lands here first and sits until it is released. |

`main` is the revert point: `git switch main` gets you a state that ran.

## Feature branches

For anything new or changed — a program, a fix, a rework.

```sh
git switch develop
git switch -c feature/rework-mkv-track-selection

# ... work, committing as you go ...
./build-all.sh                     # must report 0 failed, 0 warnings

git switch develop
git merge --no-ff feature/rework-mkv-track-selection
git branch -d feature/rework-mkv-track-selection
```

`--no-ff` is deliberate: it keeps the feature's commits grouped under one merge
commit, so a whole change can be reverted as a unit.

## Release branches

When `develop` is ready to become what you actually run.

```sh
git switch develop
git switch -c release/1.1.0

# ... only fixes here, no new work ...

git switch main
git merge --no-ff release/1.1.0
git tag -a v1.1.0 -m "What changed in this release"

git switch develop                 # so the fixes are not lost
git merge --no-ff release/1.1.0
git branch -d release/1.1.0
```

Then install from `main`.

## Hotfix branches

For something broken in what you are currently running, when `develop` has unfinished
work you do not want to ship.

```sh
git switch main
git switch -c hotfix/1.1.1

# ... fix it ...

git switch main
git merge --no-ff hotfix/1.1.1
git tag -a v1.1.1 -m "What this fixes"

git switch develop
git merge --no-ff hotfix/1.1.1
git branch -d hotfix/1.1.1
```

## Before merging anything into develop

```sh
./check.sh          # unit tests + invariants
./build-all.sh      # must end: built N, failed 0, warnings 0
```

`build-all.sh` passes `--no-incremental`, so warnings are surfaced rather than hidden
behind a cached build — a cached build prints nothing, which is how an IL2026 sat
unnoticed in `ocr-genai` until it failed at runtime.

`./check.sh --fast` skips the `--help` sweep, which is the slow part.

### What check.sh covers

**Unit tests** (`tests/`) for `utilities.cs`. That file is `#:include`d into all 104
programs, so a bug in it reaches every one of them — and it holds nothing but type
declarations, which is what lets an ordinary project compile it as a source file. The
programs themselves are top-level statements in file-based apps and cannot be
referenced at all, so they are covered by the invariants below and by running them.

Only pure logic is unit-tested: `Args`, `Fs.SplitExt`, `Fs.Slug`, `Media.Hms`,
`Num.ToWords`, `Ui.FitDescription`, `Fs.FindUrls`, `Fs.TitleCase`. Anything that shells
out or draws to the terminal is not.

**Invariants** that no compiler can check, each of which has been broken at least once:

| Check | Why |
| --- | --- |
| no base program prompts | a prompting CLI cannot be driven from cron, a pipe or another program, and dies outright with no terminal |
| wrappers use dialogs, not dropdowns | except the three password/OTP pickers, where a searchable list beats a Terminal.Gui one |
| every `-tui` has `--dry-run` | the only way to see what a wrapper would run |
| no package version drift | two versions means two copies of a dependency and two build caches |
| all programs executable | a clone without the exec bit is inert |
| no `pymv`/`pycp` dependency | removed for being ~700x slower than a rename, and for losing timestamps |
| every wrapper's base program exists | a wrapper pointing at a renamed program fails only when you press the key |
| `--help` works everywhere | proves each program compiles and parses its own arguments |

Adding a check is usually cheaper than the bug it prevents. The `--dry-run` one found a
real gap the first time it ran.

## What this repository does not cover

Installation. These programs run from `~/.local/bin`, and copying them there is a
separate, manual step. A file edited here has no effect until it is installed — and
`utilities.cs` and `tui.cs` must be installed alongside, since every program is
compiled with them.

The xonsh originals in `../Xonsh` are not tracked here. They are the reference for
what the ported behaviour is supposed to be, and worth keeping for that reason.
