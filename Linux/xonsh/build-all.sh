#!/usr/bin/env bash
# =============================================================================
#  build-all.sh — compile every program in this folder and report what fails.
#
#  This ONLY builds, in place. It installs nothing, copies nothing, and creates
#  no symlinks. Installing is a separate, manual decision — see README.md.
#
#  Why bother: a file-based app compiles on first run, so without this the first
#  real invocation of each program pays a several-second build. Running this once
#  warms every build cache and, more importantly, tells you immediately if
#  anything no longer compiles -- or compiles with warnings.
#
#  Builds pass --no-incremental deliberately. MSBuild only prints warnings from
#  the compilations it actually runs, so a cached build reports nothing and a
#  warning can sit in the tree unnoticed indefinitely. That is not hypothetical:
#  ocr-genai carried IL2026/IL3050 for exactly that reason until it failed at
#  runtime. The cost is roughly half a second per program.
#
#      ./build-all.sh            build everything
#      ./build-all.sh audio      build only programs matching "audio"
# =============================================================================

set -uo pipefail

REPO="$(cd "$(dirname "$0")" && pwd)"
DOTNET=/opt/anaconda3/envs/dotnet/lib/dotnet/dotnet
FILTER="${1:-}"

# --- Preconditions -----------------------------------------------------------

if [ ! -x "$DOTNET" ]; then
    echo "error: dotnet not found at $DOTNET" >&2
    exit 1
fi

if ! "$DOTNET" --version | grep -q '^10\.'; then
    echo "error: need the .NET 10 SDK; found $("$DOTNET" --version)" >&2
    exit 1
fi

echo "SDK $("$DOTNET" --version)"
echo

# --- External tools ----------------------------------------------------------
# These are warnings, not errors: most programs need only a few of them, and a
# missing tool matters only when you run the program that uses it.

missing=()
for tool in ffmpeg ffprobe ffplay mkvmerge mkvextract mkvinfo cwebp gif2webp \
            tesseract pandoc ebook-convert gs pdftotext magick pass pwgen \
            figlet fzf nms fabric trash-put notify-send rsync mpv; do
    command -v "$tool" >/dev/null 2>&1 || missing+=("$tool")
done

command -v wl-copy >/dev/null 2>&1 || command -v xclip >/dev/null 2>&1 \
    || missing+=("wl-copy/xclip (clipboard)")

if [ ${#missing[@]} -gt 0 ]; then
    echo "Missing external tools (only matters for the programs that use them):"
    printf '  %s\n' "${missing[@]}"
    echo
fi

# --- Build -------------------------------------------------------------------

failed=()
warned=()
built=0
warnings=0

for src in "$REPO"/*; do
    name="$(basename "$src")"

    # Skip the shared includes, config, docs, assets, and this script itself.
    case "$name" in
        *.cs|*.json|*.md|*.sh|*.xsh|oxygen-sound-theme) continue ;;
    esac

    [ -f "$src" ] || continue

    # Skip anything that is not a C# file-based app. The suite keeps a couple of
    # scripts in their original language — fontpreview-ueberzug is POSIX sh, and
    # epub-split is xonsh — and neither is ours to compile.
    read -r shebang < "$src" || continue
    case "$shebang" in
        *xonsh*|*python*) continue ;;
    esac
    grep -q '^#:' "$src" || continue
    [ -n "$FILTER" ] && [[ "$name" != *"$FILTER"* ]] && continue

    printf '  %-36s ' "$name"

    if output=$("$DOTNET" build "$src" --no-incremental 2>&1); then
        warns=$(printf '%s' "$output" | grep -cE 'warning [A-Z]+[0-9]+')
        if [ "$warns" -gt 0 ]; then
            echo "ok ($warns warning(s))"
            printf '%s\n' "$output" \
                | grep -oE 'warning [A-Z]+[0-9]+: .*' \
                | sort -u | head -5 | sed 's/^/      /'
            warned+=("$name")
            warnings=$((warnings + warns))
        else
            echo "ok"
        fi
        built=$((built + 1))
    else
        echo "FAILED"
        echo "$output" | grep -E 'error [A-Z]+[0-9]+' | head -5 | sed 's/^/      /'
        failed+=("$name")
    fi
done

# --- Report ------------------------------------------------------------------

echo
echo "built $built, failed ${#failed[@]}, warnings $warnings"

if [ ${#warned[@]} -gt 0 ]; then
    echo "  warnings in:"
    printf '    %s\n' "${warned[@]}"
fi

if [ ${#failed[@]} -gt 0 ]; then
    printf '  %s\n' "${failed[@]}"
    exit 1
fi
