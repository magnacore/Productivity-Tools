- .NET is available in /opt/anaconda3/envs/dotnet/lib/dotnet/dotnet
- Write well documented code
- Write secure code
- Write maintainable code
- Write extendable code
- Write modular code
- Do not use var - use explicit types
- Do not modify anything outside the repo directory without permission
- Use gitflow methodology for branches

## Working agreements

- Nothing is pushed. `main` and `develop` are local; the user will create the remote later.
- Only `CS/` is a git repository.
- `~/.local/bin` is a symlink to `CS/`. There is no install step: the working tree **is**
  the live toolset, so an edit is in effect the moment it is written. Three things follow.
  A half-finished or broken file breaks the running system immediately -- stripping the
  `#!/bin/sh` line from five programs once left them unrunnable until it was noticed.
  Switching branches switches the user's tools underneath them, so a program can quietly
  behave differently mid-session. And an abandoned experiment needs no reinstall to undo,
  because nothing was ever copied anywhere.
- Do risky or bulk work in a `git worktree`, not by checking out in place:
  `git worktree add ../CS-work feature/x` keeps `CS/` on `develop` while the work happens
  elsewhere, so the live tools never see it.
- Never risk data. No forced or lazy unmounts, no overwrite without a check, and read a file before deleting or replacing it.
- If you come across an important piece of learning that will be beneficial in the future, turn it into a skill.
