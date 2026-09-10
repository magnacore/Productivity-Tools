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
  the live toolset, so an edit is in effect the moment it is written, and an abandoned
  experiment needs no reinstall to undo.
- Checking a feature branch out here is how the user tests it -- that is the point of the
  symlink, not a hazard to design around. What matters is never *leaving* the tree broken:
  after any bulk or scripted edit, verify the programs still run before moving on. A script
  that stripped the `#!/bin/sh` line off five programs once left them unrunnable, and
  because the tree is live they were unrunnable *for the user*, not just in a sandbox.
  `./check.sh` catches exactly this -- its `--help works for every program` invariant.
- Never risk data. No forced or lazy unmounts, no overwrite without a check, and read a file before deleting or replacing it.
- If you come across an important piece of learning that will be beneficial in the future, turn it into a skill.
