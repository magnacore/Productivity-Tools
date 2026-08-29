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
./build-all.sh
```

It must end with `built N, failed 0, warnings 0`. It passes `--no-incremental`, so
warnings are surfaced rather than hidden behind a cached build — a cached build prints
nothing, which is how an IL2026 sat unnoticed in `ocr-genai` until it failed at
runtime.

## What this repository does not cover

Installation. These programs run from `~/.local/bin`, and copying them there is a
separate, manual step. A file edited here has no effect until it is installed — and
`utilities.cs` and `tui.cs` must be installed alongside, since every program is
compiled with them.

The xonsh originals in `../Xonsh` are not tracked here. They are the reference for
what the ported behaviour is supposed to be, and worth keeping for that reason.
