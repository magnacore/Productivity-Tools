#!/usr/bin/env bash
# =============================================================================
#  check.sh — the suite's own rules, enforced.
#
#  Two parts:
#    1. unit tests for utilities.cs (tests/), which is compiled into all 104
#       programs, so a bug in it reaches every one of them
#    2. invariants that no compiler can check, each of which has been broken at
#       least once and cost real debugging
#
#      ./check.sh          run everything
#      ./check.sh --fast   skip the --help sweep, which is the slow part
# =============================================================================

set -uo pipefail

REPO="$(cd "$(dirname "$0")" && pwd)"
DOTNET=/opt/anaconda3/envs/dotnet/lib/dotnet/dotnet
FAST=0
[ "${1:-}" = "--fast" ] && FAST=1

failures=0
pass() { printf '  \033[32mok\033[0m    %s\n' "$1"; }
fail() { printf '  \033[31mFAIL\033[0m  %s\n' "$1"; failures=$((failures + 1)); }

# Every C# program in the folder: extensionless, has a #: directive, not xonsh/sh.
programs() {
    for src in "$REPO"/*; do
        local name; name="$(basename "$src")"
        case "$name" in *.cs|*.json|*.md|*.sh|*.xsh|oxygen-sound-theme) continue ;; esac
        [ -f "$src" ] || continue
        read -r shebang < "$src" || continue
        case "$shebang" in *xonsh*|*python*) continue ;; esac
        grep -q '^#:' "$src" || continue
        echo "$name"
    done
}

echo "── unit tests ──────────────────────────────────────────────"
if out=$("$DOTNET" run --project "$REPO/tests/Utilities.Tests.csproj" 2>&1); then
    pass "$(printf '%s' "$out" | grep -oE 'Total: [0-9]+, Errors: [0-9]+, Failed: [0-9]+' | tail -1)"
else
    fail "utilities.cs unit tests"
    printf '%s\n' "$out" | grep -E '\[FAIL\]|Assert\.' | head -12 | sed 's/^/        /'
fi

echo
echo "── invariants ──────────────────────────────────────────────"

# 1. No CLI program prompts. Broken twice; a prompting CLI cannot be driven from
#    cron, a pipe, or another program, and dies outright without a terminal.
offenders=""
for p in $(programs); do
    case "$p" in *-tui) continue ;; esac
    if grep -qE 'Ui\.(Select|SelectMany|Confirm|Ask|AskInt|AskDouble)\(|Dlg\.' "$REPO/$p"; then
        offenders="$offenders $p"
    fi
done
[ -z "$offenders" ] && pass "no base program prompts" \
                    || fail "these prompt but are not -tui wrappers:$offenders"

# 2. No wrapper uses a Spectre dropdown. The password/OTP pickers used to be
#    exempt; they now go through Ui.Pick (fzf), so the rule is absolute.
offenders=""
for p in "$REPO"/*-tui; do
    grep -qE 'Ui\.(Select|Confirm|Ask|AskInt|AskDouble)\(' "$p" \
        && offenders="$offenders $(basename "$p")"
done
[ -z "$offenders" ] && pass "wrappers use dialogs, not dropdowns" \
                    || fail "wrappers still using a dropdown:$offenders"

# 3. Every wrapper can show what it would run without running it.
offenders=""
for p in "$REPO"/*-tui; do
    grep -q '"--dry-run"' "$p" || offenders="$offenders $(basename "$p")"
done
[ -z "$offenders" ] && pass "every -tui has --dry-run" \
                    || fail "wrappers without --dry-run:$offenders"

# 4. One package version across the suite. A split version means two copies of a
#    dependency and two build caches for no reason.
drift=$(grep -h '^#:package' "$REPO"/* 2>/dev/null | sort -u \
        | sed -E 's/^#:package ([^@]+)@.*/\1/' | sort | uniq -d)
[ -z "$drift" ] && pass "no package version drift" \
                || fail "packages pinned at more than one version: $(echo "$drift" | tr '\n' ' ')"

# 5. The programs must stay executable, or a fresh clone is inert.
offenders=""
for p in $(programs); do [ -x "$REPO/$p" ] || offenders="$offenders $p"; done
[ -z "$offenders" ] && pass "all programs executable" \
                    || fail "not executable:$offenders"

# 6. pymv/pycp were removed for being ~700x slower than a rename and for losing
#    timestamps. Only comments should mention them now.
if grep -l 'pymv\|pycp' "$REPO"/* 2>/dev/null | grep -qv 'README\|utilities.cs\|check.sh'; then
    fail "pymv/pycp is called again somewhere"
else
    pass "no pymv/pycp dependency"
fi

# 7. Everything a wrapper hands off to must exist.
offenders=""
for p in "$REPO"/*-tui; do
    for target in $(grep -oE 'Proc\.Call\("[a-z0-9-]+"' "$p" | sed 's/.*"\(.*\)"/\1/' | sort -u); do
        [ -f "$REPO/$target" ] || offenders="$offenders $(basename "$p")->$target"
    done
done
[ -z "$offenders" ] && pass "every wrapper's base program exists" \
                    || fail "wrappers pointing at a missing program:$offenders"

# 8. --help must work everywhere: it is the only contract a caller can rely on,
#    and it proves the program compiles and parses its own arguments.
if [ "$FAST" = "1" ]; then
    printf '  \033[33mskip\033[0m  --help sweep (--fast)\n'
else
    offenders=""
    for p in $(programs); do
        "$REPO/$p" --help >/dev/null 2>&1 || offenders="$offenders $p"
    done
    [ -z "$offenders" ] && pass "--help works for every program" \
                        || fail "--help failed for:$offenders"
fi

# 9. Explicit types, never `var`. AGENT.md has always required this, but nothing
#    enforced it and the suite quietly drifted to 798 uses. Beyond house style, the
#    concrete cost is hidden nullability: `var parent = Path.GetDirectoryName(p)` is a
#    string? and nothing on the line says so, which makes the null check below it look
#    like defensive habit rather than a requirement.
#
#    Comment lines are skipped so this very rule can be written about in prose. The
#    pattern wants a declaration -- `var` followed by a name or a deconstruction --
#    rather than the bare word, so "var" inside a string or an identifier is ignored.
offenders=""
for src in $(programs) utilities.cs tui.cs $(cd "$REPO" && echo tests/*.cs); do
    [ -f "$REPO/$src" ] || continue
    n=$(grep -nE '(^|[[:space:];({])var[[:space:]]+[A-Za-z_(]' "$REPO/$src" 2>/dev/null \
        | grep -cvE '^[0-9]+:[[:space:]]*(//|\*|/\*)') || true
    [ "${n:-0}" -gt 0 ] && offenders="$offenders $src($n)"
done
[ -z "$offenders" ] && pass "explicit types, no var" \
                    || fail "var used in:$offenders"

echo
if [ "$failures" -eq 0 ]; then
    echo "all checks passed"
else
    echo "$failures check(s) failed"
    exit 1
fi
