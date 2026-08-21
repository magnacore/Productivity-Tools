// =============================================================================
//  utilities.cs — shared library for the C# productivity suite.
//
//  Every script pulls this in with:
//      #:include utilities.cs
//
//  `#:include` adds this file as a Compile item in the SAME compilation as the
//  including script. That means every type mentioned anywhere in here must
//  resolve in every script that includes it. So this file may reference ONLY
//  the BCL and Spectre.Console — nothing else. Terminal.Gui helpers live in the
//  separate `tui.cs`, which only the dialog wrappers include.
//
//  This file declares types only (no top-level statements), so that including
//  scripts can keep using top-level statements themselves.
//
//  Ported from utilities.xsh and py_utilities.py.
//
//  Analyzer posture: this file is clean under `AnalysisMode=All` apart from two
//  rules it deliberately violates.
//
//    CA1308 (prefer ToUpperInvariant) — the rule guards against lossy casing in
//      security comparisons. Here lowercasing IS the requirement: Slug() produces
//      filenames, and a case-insensitive filesystem would let "Report.pdf" and
//      "report.pdf" overwrite each other.
//
//    CA1050 (declare types in namespaces) — these types are #include'd into 88
//      programs that use them from top-level statements. A namespace would mean a
//      `using` line in every one of them, for no benefit inside a single-file app.
// =============================================================================

global using Spectre.Console;
global using System.Diagnostics;
global using System.Globalization;
global using System.Text;

using System.Numerics;
using System.Text.RegularExpressions;

// =============================================================================
//  Paths — every hardcoded location in the suite, in one place.
// =============================================================================
internal static class Paths
{
    public static string Home { get; } =
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

    public static string LocalBin => Path.Combine(Home, ".local", "bin");

    /// Where the Oxygen notification sounds live. Read by Snd, not by the programs.
    internal static string SoundTheme => Path.Combine(LocalBin, "oxygen-sound-theme");

    public static string ProductivitySystem => Path.Combine(Home, "Productivity_System");

    /// The inbox that clipboard-*, video-process and friends drop files into.
    public static string TaskCaptureBin =>
        Path.Combine(ProductivitySystem, "01 TASK CAPTURE BIN", "00 COPY TO HDD");

    public static string SensitiveData => Path.Combine(Home, "Documents", "SENSITIVE DATA");
    public static string Backups       => Path.Combine(Home, "Backups");
    public static string Downloads     => Path.Combine(Home, "Downloads");

    /// State stamps written by the backup scripts (`date > ~/.local/state/shortcut/x.txt`).
    public static string ShortcutState => Path.Combine(Home, ".local", "state", "shortcut");

    /// The current user name, for the `/media/$USER/...` paths the backup scripts use.
    public static string User { get; } =
        Environment.GetEnvironmentVariable("USER")
        ?? Environment.GetEnvironmentVariable("LOGNAME")
        ?? Environment.UserName;

    public static string Media => Path.Combine("/media", User);

    // --- Identity of the running script (works under the shebang launcher) ---

    /// Only ScriptDir is derived from this; nothing outside this class needs it.
    private static string ScriptPath { get; } =
        AppContext.GetData("EntryPointFilePath") as string ?? Environment.ProcessPath ?? "script";

    public static string ScriptDir { get; } =
        AppContext.GetData("EntryPointFileDirectoryPath") as string
        ?? Path.GetDirectoryName(ScriptPath) ?? ".";
}

// =============================================================================
//  Ui — replaces Python `rich` and `simple_term_menu`.
// =============================================================================
internal static class Ui
{
    /// console.rule(f"[bold cyan]{title}", style="cyan")
    ///
    /// The title is escaped, because titles are built from file names and tags
    /// and a stray '[' would otherwise be read as markup and throw.
    public static void Begin(string title, string style = "cyan") =>
        AnsiConsole.Write(new Rule($"[bold {style}]{Esc(title)}[/]").RuleStyle(style));

    /// console.rule(style="cyan") — the closing bookend every script prints.
    public static void End(string style = "cyan") =>
        AnsiConsole.Write(new Rule().RuleStyle(style));

    // --- Printing ------------------------------------------------------------

    /// Write a line of Spectre markup. Safe to call from inside a Track: Spectre
    /// scrolls it above the live progress bar, the way rich's print() does.
    ///
    /// Takes markup, so any interpolated file name needs Esc(). The colour
    /// helpers below escape for you and are the better choice for plain text.
    public static void Line(string markup) => AnsiConsole.MarkupLine(markup);

    public static void Ok(string text)   => AnsiConsole.MarkupLine($"[green]{Esc(text)}[/]");
    public static void Good(string text) => AnsiConsole.MarkupLine($"[bold green]{Esc(text)}[/]");
    public static void Warn(string text) => AnsiConsole.MarkupLine($"[yellow]{Esc(text)}[/]");
    public static void Err(string text)  => AnsiConsole.MarkupLine($"[bold red]{Esc(text)}[/]");
    public static void Info(string text) => AnsiConsole.MarkupLine($"[bold cyan]{Esc(text)}[/]");

    /// Escape text that must not be read as Spectre markup (filenames with [ ]).
    public static string Esc(string text) => Markup.Escape(text ?? string.Empty);

    /// Above this many options a menu offers type-to-search.
    private const int SearchThreshold = 7;

    /// True when we own a real terminal. Prompts are only usable when this holds.
    public static bool Interactive => AnsiConsole.Profile.Capabilities.Interactive;

    // --- Progress ------------------------------------------------------------

    /// rich.progress.track(items, description=...)
    public static void Track<T>(IEnumerable<T> items, string description, Action<T> body)
        => Track(items, description, (item, _) => body(item));

    /// Same, with the zero-based index of the item.
    public static void Track<T>(IEnumerable<T> items, string description, Action<T, int> body)
    {
        var list = items as IList<T> ?? items.ToList();
        if (list.Count == 0) return;

        AnsiConsole.Progress()
            .AutoClear(false)
            .Columns(ProgressColumns())
            .Start(ctx =>
            {
                var task = ctx.AddTask(description, maxValue: list.Count);
                for (int i = 0; i < list.Count; i++)
                {
                    body(list[i], i);
                    task.Increment(1);
                }
            });
    }

    /// Like Track, but runs the items concurrently. For per-item work dominated by
    /// a subprocess — ffmpeg, cwebp, pdftotext — this turns a serial wait into a
    /// parallel one.
    ///
    /// Writing to the console from the body is safe: Spectre serialises its own
    /// output, so messages still arrive intact above the bar. Their *order*
    /// becomes arbitrary, which is the one visible difference from Track.
    public static void TrackParallel<T>(IReadOnlyList<T> items, string description,
                                        Action<T> body, int? maxParallel = null)
        => TrackParallel(items, description, item => { body(item); return 0; }, maxParallel);

    /// The same, collecting a result per item in the original order.
    public static IReadOnlyList<TResult> TrackParallel<T, TResult>(
        IReadOnlyList<T> items, string description, Func<T, TResult> body,
        int? maxParallel = null)
    {
        var results = new TResult[items.Count];
        if (items.Count == 0) return results;

        AnsiConsole.Progress()
            .AutoClear(false)
            .Columns(ProgressColumns())
            .Start(ctx =>
            {
                var task = ctx.AddTask(description, maxValue: items.Count);
                var gate = new Lock();

                Parallel.For(0, items.Count,
                    new ParallelOptions
                    {
                        MaxDegreeOfParallelism = maxParallel ?? Environment.ProcessorCount
                    },
                    i =>
                    {
                        results[i] = body(items[i]);
                        // ProgressTask is not thread safe.
                        lock (gate) task.Increment(1);
                    });
            });

        return results;
    }

    /// An overall bar plus a bar per item, with the items processed concurrently.
    ///
    /// The overall bar is created first but every per-item bar is inserted *before*
    /// it, so it stays the bottom-most row however many items are in flight. That is
    /// what makes a long run readable: once there are more items than the terminal
    /// has rows the finished bars scroll up out of view, and the overall bar stays
    /// at the bottom where it can still be seen.
    ///
    /// `body` is handed a factory. Call it with a description and a maximum to get
    /// back an `advance` callback for that item's own bar, then call that with how
    /// much to move it on by — 1 per unit of work when the item counts its own
    /// steps, or the difference since last time when the item reports a percentage.
    /// It is safe to call from the worker thread, because the locking is in here
    /// rather than in the caller.
    ///
    /// Finished bars are stopped, not removed, so they stay on screen as a record of
    /// what was done. Accumulating them only stays tidy because every description is
    /// padded to one width by FitDescription — Spectre sizes the description column
    /// to the widest one currently shown, so ragged descriptions would drag every bar
    /// rightwards to meet the longest name in the run.
    public static void Track2<T>(IReadOnlyList<T> items, string overallDescription,
                                 Action<T, Func<string, int, Action<double>>> body,
                                 int? maxParallel = null)
    {
        if (items.Count == 0) return;

        AnsiConsole.Progress()
            .AutoClear(false)
            .Columns(ProgressColumns())
            .Start(ctx =>
            {
                var overall = ctx.AddTask(overallDescription, maxValue: items.Count);

                // One lock over every mutation of the progress state. AddTaskBefore
                // rewrites the shared task list while the render thread is reading
                // it, and ProgressTask is not thread safe. It is held for a few
                // microseconds at a time, so the contention is not worth avoiding.
                var gate = new Lock();

                Parallel.For(0, items.Count,
                    new ParallelOptions
                    {
                        MaxDegreeOfParallelism = maxParallel ?? Environment.ProcessorCount
                    },
                    i =>
                    {
                        var mine = new List<ProgressTask>();

                        body(items[i], (description, max) =>
                        {
                            ProgressTask task;
                            lock (gate)
                            {
                                task = ctx.AddTaskBefore(description, overall,
                                                         autoStart: true, maxValue: max);
                                mine.Add(task);
                            }
                            return amount => { lock (gate) task.Increment(amount); };
                        });

                        lock (gate)
                        {
                            foreach (var task in mine) task.StopTask();
                            overall.Increment(1);
                        }
                    });
            });
    }

    /// A bar driven by an accumulating quantity rather than an item count —
    /// media-length-move fills it with seconds of media until the target is hit.
    /// The body receives an `advance` callback and returns when it is done.
    public static void Meter(string description, double total, Action<Action<double>> body)
    {
        AnsiConsole.Progress()
            .AutoClear(false)
            .Columns(ProgressColumns())
            .Start(ctx =>
            {
                var task = ctx.AddTask(description, maxValue: total <= 0 ? 1 : total);
                body(amount => task.Increment(amount));
                task.Value = task.MaxValue;
            });
    }

    /// The column set every bar in the suite uses. Matches what rich's `track` showed
    /// in the xonsh originals: description, bar, percentage, estimated time remaining.
    ///
    /// The time reads `--:--:--` until there are enough samples to estimate from, and
    /// `00:00:00` once the task is finished — the bars are kept on screen
    /// (AutoClear(false)), so a completed one holds that zero rather than the total.
    private static ProgressColumn[] ProgressColumns() =>
    [
        // Left-aligned, as rich's TextColumn is. Spectre right-aligns by default,
        // which only shows up when a Progress holds bars whose descriptions differ
        // in length — the shorter ones get shoved right to meet the longest.
        new TaskDescriptionColumn { Alignment = Justify.Left },
        new ProgressBarColumn(),
        new PercentageColumn(),
        new RemainingTimeColumn(),
        new SpinnerColumn(),
    ];

    /// Trim or pad a progress description to a fixed width, ellipsising the overflow.
    ///
    /// For bars whose description changes from item to item. Spectre sizes the
    /// description column to the widest one *currently shown*, so a varying name
    /// slides the bar left and right as the run proceeds. Passing every description
    /// through this — including the ones that never change, so the column keeps its
    /// width as bars come and go — holds the bar still for the whole run.
    ///
    /// Give it the plain text only, never markup: it counts and pads characters, so
    /// it would happily cut a tag in half. Wrap the result in markup afterwards.
    /// The budget is two fifths of the terminal, bounded so the text is not reduced
    /// to nothing on a narrow terminal nor allowed to crowd out the bar on a wide one.
    public static string FitDescription(string text)
    {
        int width = Math.Clamp(TerminalWidth * 2 / 5, 20, 56);
        text ??= string.Empty;

        // Length, not display width: a name of CJK or emoji characters measures wider
        // than this on screen. Book and media file names in this collection are not,
        // and getting it exactly right needs Spectre's own cell-width measurement.
        if (text.Length <= width) return text.PadRight(width);

        // Cut from the middle, not the end. What distinguishes two long names is as
        // often at the end as the start — "…Animal Rights.pdf" and "…Animal Welfare.pdf"
        // are told apart by their tails, and cutting there would render both the same.
        // Keeping both ends also preserves the extension and the trailing colon.
        int keep = width - 1;               // one character goes to the ellipsis
        int head = (keep + 1) / 2;          // odd character to the head
        int tail = keep - head;

        return string.Concat(text.AsSpan(0, head), "…", text.AsSpan(text.Length - tail));
    }

    /// The terminal's width, or a reasonable assumption when there is no terminal.
    private static int TerminalWidth
    {
        get
        {
            try
            {
                return Console.IsOutputRedirected ? 100 : Console.WindowWidth;
            }
            catch (Exception ex) when (ex is IOException or ArgumentOutOfRangeException)
            {
                return 100;
            }
        }
    }

    // --- Prompts (replace simple_term_menu.TerminalMenu) ---------------------

    /// Present a menu and return the chosen option.
    ///
    /// Escape backs out: these menus gate the work that follows, so cancelling one
    /// means cancelling the command. Abort() ends the program quietly rather than
    /// returning a value the caller would have to check at 20-odd call sites.
    public static string Select(IReadOnlyList<string> options, string title = "Make a choice: ")
        => Select(options, s => s, title);

    public static T Select<T>(IReadOnlyList<T> options, Func<T, string> label,
                              string title = "Make a choice: ") where T : notnull
    {
        if (options.Count == 0)
            throw new InvalidOperationException("Select called with no options.");
        if (options.Count == 1) return options[0];

        Term.Drain();
        Term.KeyboardTaken();

        // CancelResult has to hand back a T, so the flag records that Escape was
        // pressed and the value it returns is never used.
        bool cancelled = false;

        var prompt = new SelectionPrompt<T>()
            .Title($"[bold cyan]{Esc(title)}[/] [grey](Esc to cancel)[/]")
            .PageSize(Math.Min(Math.Max(options.Count + 2, 4), 20))
            .MoreChoicesText("[grey](move up and down to see more)[/]")
            .UseConverter(o => Esc(label(o)))
            .AddChoices(options);

        // Up from the first entry lands on the last, and down from the last on the
        // first. With short menus the wanted item is often the bottom one, and one
        // press up beats scrolling the whole list.
        prompt.WrapAround = true;

        // Type to filter, once a list is long enough that arrowing through it is a
        // chore. The password store runs to dozens of entries, which is what
        // simple_term_menu's search was for in the originals. Short menus are left
        // plain so the search hint does not clutter a two-line confirmation.
        if (options.Count > SearchThreshold)
        {
            prompt.SearchEnabled = true;
            prompt.SearchPlaceholderText = "[grey](type to search)[/]";
        }

        prompt.CancelResult = () => { cancelled = true; return options[0]; };

        var result = AnsiConsole.Prompt(prompt);

        if (cancelled) Abort();
        return result;
    }

    /// The user backed out. Say so and stop, without the error sound — cancelling
    /// deliberately is not a failure.
    [System.Diagnostics.CodeAnalysis.DoesNotReturn]
    public static void Abort(string message = "Cancelled.")
    {
        Warn(message);
        End();
        Environment.Exit(0);
    }

    /// The suite's ubiquitous ["No", "Yes"] menu, with "No" listed first so it is
    /// the resting selection — same as every simple_term_menu call being replaced.
    public static bool Confirm(string question)
        => Select(["No", "Yes"], question) == "Yes";

    public static string Ask(string question, string? defaultValue = null)
    {
        Term.KeyboardTaken();
        var prompt = new TextPrompt<string>($"[bold cyan]{Esc(question)}[/]").AllowEmpty();
        if (defaultValue is not null) prompt.DefaultValue(defaultValue);
        return AnsiConsole.Prompt(prompt);
    }

    public static int AskInt(string question, int defaultValue)
    {
        Term.KeyboardTaken();
        return AnsiConsole.Prompt(new TextPrompt<int>($"[bold cyan]{Esc(question)}[/]")
            .DefaultValue(defaultValue));
    }

    public static double AskDouble(string question, double defaultValue)
    {
        Term.KeyboardTaken();
        return AnsiConsole.Prompt(new TextPrompt<double>($"[bold cyan]{Esc(question)}[/]")
            .DefaultValue(defaultValue));
    }

    // --- Tables --------------------------------------------------------------

    /// The recurring rich table: box.DOUBLE, a title, and named columns.
    /// rich's row_styles=["dim", ""] has no Spectre equivalent, so callers
    /// alternate row styling themselves via `Dim(index, text)`.
    public static Table NewTable(string title, params string[] columns)
    {
        var table = new Table().Border(TableBorder.Double);
        if (!string.IsNullOrEmpty(title)) table.Title(title);
        foreach (var c in columns) table.AddColumn(c);
        return table;
    }

    /// Alternating row style, reproducing rich's row_styles=["dim", ""].
    public static string Dim(int rowIndex, string markup)
        => rowIndex % 2 == 0 ? $"[dim]{markup}[/]" : markup;

    /// The "────────  ───" separator row the media tables put before their totals.
    public static void AddSeparator(Table table)
        => table.AddRow(new Text(new string('─', 8)), new Rule().RuleStyle("white"));

    public static void Show(Table table) => AnsiConsole.Write(table);

    // --- Standard exits ------------------------------------------------------

    /// "No files selected!" + error sound + closing rule. Returns 1.
    public static int NoFiles(string message = "No files selected!")
    {
        Err(message);
        Snd.Error();
        End();
        return 1;
    }

    /// Success sound + closing rule. Returns 0.
    public static int Done()
    {
        Snd.Success();
        End();
        return 0;
    }

    /// Close out a run that counted its failures.
    ///
    /// One sound at the end, not one per failure: when files are processed
    /// concurrently a per-failure beep becomes a pile of overlapping ffplay
    /// processes, which tells you less than a single error tone and a count.
    public static int Done(int failures)
    {
        if (failures == 0) return Done();

        Err($"{failures} file(s) failed.");
        Snd.Error();
        End();
        return 1;
    }
}

// =============================================================================
//  Term — hand the terminal back the way we found it.
//
//  Spectre.Console and Terminal.Gui both switch the terminal into application
//  cursor-key mode when they take over the keyboard — terminfo's `smkx`, which is
//  ESC[?1h followed by ESC=. Neither emits the matching `rmkx` on the way out.
//
//  Left set, the arrow keys keep arriving as SS3 sequences (ESC O B rather than
//  ESC [ B) after the program has exited, so whatever runs next sees them as
//  stray text: a file manager's UI stops responding to the arrows and fills with
//  "^[OB", a shell prompt does the same.
//
//  So: note when something has taken the keyboard, and put it back at exit.
// =============================================================================
internal static class Term
{
    private static bool _needsRestore;

    /// Call before handing the keyboard to a prompt or a dialog.
    internal static void KeyboardTaken()
    {
        if (_needsRestore) return;
        _needsRestore = true;

        // At exit rather than after each prompt: a script may show several in a
        // row, and restoring between them would fight whatever shows the next one.
        AppDomain.CurrentDomain.ProcessExit += (_, _) => Restore();
    }

    /// Return the cursor keys and keypad to normal mode, and discard anything the
    /// terminal queued while the prompt owned it.
    /// Throw away anything already sitting in the input buffer.
    ///
    /// Called before a prompt opens, not just after. A launcher that hands over the
    /// terminal can leave bytes behind — its own keystroke handling, a curses
    /// suspend, a terminal query reply — and a prompt that starts reading without
    /// clearing them treats them as answers. Three dialogs in a row (as
    /// audio-add-music-tui has) can be answered entirely by that debris, so the
    /// program looks as though it skipped straight past them.
    internal static void Drain()
    {
        try
        {
            while (Console.KeyAvailable) Console.ReadKey(intercept: true);
        }
        catch (Exception ex) when (ex is InvalidOperationException or IOException)
        {
            // KeyAvailable throws when stdin is redirected; nothing to drain.
        }
    }

    internal static void Restore()
    {
        if (!_needsRestore) return;
        _needsRestore = false;

        // Never write escape codes into a redirected stdout: several of these
        // programs exist to be read by other programs.
        if (Console.IsOutputRedirected) return;

        try
        {
            Console.Out.Write("\u001b[?1l\u001b>");
            Console.Out.Flush();
        }
        catch (IOException)
        {
            // The terminal has gone; nothing to hand back.
        }

        try
        {
            while (Console.KeyAvailable) Console.ReadKey(intercept: true);
        }
        catch (Exception ex) when (ex is InvalidOperationException or IOException)
        {
            // No console input to drain.
        }
    }
}

// =============================================================================
//  Proc — replaces xonsh's !(), $(), and trailing '&'.
//
//  Everything goes through ProcessStartInfo.ArgumentList, never a concatenated
//  command string, so filenames containing spaces, quotes or $ are always safe.
// =============================================================================
internal static class Proc
{
    /// A bare `cmd ...` line in xonsh — runs to completion with the child's
    /// output going straight to the terminal, and returns the exit code.
    public static int Run(string exe, params string[] args)
        => Run(exe, (IEnumerable<string>)args);

    public static int Run(string exe, IEnumerable<string> args, bool quiet = false,
                          string? cwd = null, IDictionary<string, string>? env = null)
    {
        var psi = NewPsi(exe, args, cwd, env);
        if (quiet)
        {
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
        }

        using var p = Process.Start(psi);
        if (p is null) return 127;

        if (quiet)
        {
            // Drain both pipes concurrently, otherwise a chatty child fills one
            // buffer and blocks forever while we wait on the other.
            var outTask = p.StandardOutput.ReadToEndAsync();
            var errTask = p.StandardError.ReadToEndAsync();
            p.WaitForExit();
            Task.WaitAll(outTask, errTask);
        }
        else
        {
            p.WaitForExit();
        }

        return p.ExitCode;
    }

    /// Like Ok, but hands each line the child writes to stdout to `onLine` as it
    /// arrives instead of discarding it.
    ///
    /// For children that report their own progress — Calibre's ebook-convert prints
    /// "34% Running transforms on e-book..." and friends — so a caller can drive a
    /// real progress bar off it rather than guessing.
    public static bool OkStreaming(string exe, IEnumerable<string> args,
                                   Action<string> onLine)
    {
        var psi = NewPsi(exe, args, cwd: null, env: null);
        psi.RedirectStandardOutput = true;
        psi.RedirectStandardError = true;

        using var p = Process.Start(psi);
        if (p is null) return false;

        // stderr is drained on its own task for the usual reason: left unread, a
        // chatty child fills that pipe's buffer and blocks forever while we are
        // still reading stdout.
        var drainErr = p.StandardError.ReadToEndAsync();

        while (p.StandardOutput.ReadLine() is { } line) onLine(line);

        p.WaitForExit();
        drainErr.Wait();

        return p.ExitCode == 0;
    }

    /// xonsh  if !(cmd ...)  — true on success.
    ///
    /// Like xonsh's !(), the child's output is captured rather than printed. That
    /// matters here: these calls sit inside conversion loops driven by a live
    /// progress bar, and a chatty ffmpeg would shred the display.
    public static bool Ok(string exe, params string[] args)
        => Run(exe, args, quiet: true) == 0;

    public static bool Ok(string exe, IEnumerable<string> args)
        => Run(exe, args, quiet: true) == 0;

    /// Run a command and return its trimmed output, or "" if it failed.
    ///
    /// Trimming is right for the usual case — reading a value a tool printed,
    /// where the trailing newline is noise.
    public static string Capture(string exe, params string[] args)
        => CaptureAll(exe, args).Out.Trim();

    /// Run a command and report whether it worked, along with whatever it said.
    ///
    /// For failures that need explaining rather than merely detecting: Ok() tells
    /// you something went wrong, this tells you what.
    public static (bool Ok, string Error) TryRun(string exe, IEnumerable<string> args)
    {
        var (code, stdout, stderr) = CaptureAll(exe, args);
        var text = string.IsNullOrWhiteSpace(stderr) ? stdout : stderr;
        return (code == 0, text.Trim());
    }

    /// The same, byte for byte, with nothing trimmed.
    ///
    /// For content rather than values: clipboard text is the case here, where a
    /// leading or trailing space is part of what the user copied and silently
    /// eating it would break the round trip through the cipher scripts.
    public static string CaptureRaw(string exe, params string[] args)
        => CaptureAll(exe, args).Out;

    private static (int Code, string Out, string Err) CaptureAll(
        string exe, IEnumerable<string> args, string? stdin = null, string? cwd = null)
    {
        var psi = NewPsi(exe, args, cwd, null);
        psi.RedirectStandardOutput = true;
        psi.RedirectStandardError = true;
        psi.RedirectStandardInput = stdin is not null;

        try
        {
            using var p = Process.Start(psi);
            if (p is null) return (127, string.Empty, $"could not start {exe}");

            if (stdin is not null)
            {
                p.StandardInput.Write(stdin);
                p.StandardInput.Close();
            }

            // Read both streams concurrently — reading one to the end first can
            // deadlock if the child fills the other pipe. Waiting on the reads
            // rather than on the process also guarantees both pipes are drained
            // before the exit code is taken.
            var outTask = p.StandardOutput.ReadToEndAsync();
            var errTask = p.StandardError.ReadToEndAsync();
            Task.WaitAll(outTask, errTask);
            p.WaitForExit();

            return (p.ExitCode, outTask.GetAwaiter().GetResult(), errTask.GetAwaiter().GetResult());
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception
                                       or IOException
                                       or InvalidOperationException
                                       or ObjectDisposedException)
        {
            // The command could not be started at all — no such binary, not
            // executable. 127 is the shell's own code for that.
            return (127, string.Empty, ex.Message);
        }
    }

    /// Feed text into a command's stdin:  echo @(text) | fabric -sp @(pattern)
    public static int Pipe(string stdinText, string exe, params string[] args)
    {
        var psi = NewPsi(exe, args, null, null);
        psi.RedirectStandardInput = true;

        using var p = Process.Start(psi);
        if (p is null) return 127;
        p.StandardInput.Write(stdinText);
        p.StandardInput.Close();
        p.WaitForExit();
        return p.ExitCode;
    }

    /// xonsh  cmd ... &  — fire and forget, output discarded, parent never waits.
    ///
    /// Routed through `sh -c 'exec "$0" "$@" >/dev/null 2>&1'` so the redirection
    /// is real (the child keeps writing happily after we exit) while the arguments
    /// still travel as argv, so no shell quoting is involved.
    public static void Spawn(string exe, params string[] args)
    {
        try
        {
            var psi = new ProcessStartInfo("/bin/sh") { UseShellExecute = false };
            psi.ArgumentList.Add("-c");
            psi.ArgumentList.Add("exec \"$0\" \"$@\" >/dev/null 2>&1");
            psi.ArgumentList.Add(exe);
            foreach (var a in args) psi.ArgumentList.Add(a);
            Process.Start(psi);
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or IOException)
        {
            // A missing helper — no ffplay, no notify-send — must never break the
            // real work. Anything other than "could not start it" still surfaces.
        }
    }

    /// Run a real shell command line. Reserved for the few places where globbing
    /// or redirection is the point (e.g. `gs ... even/*.pdf`, `ps aux | fzf`).
    public static int Shell(string commandLine)
    {
        var psi = new ProcessStartInfo("/bin/sh") { UseShellExecute = false };
        psi.ArgumentList.Add("-c");
        psi.ArgumentList.Add(commandLine);
        using var p = Process.Start(psi);
        if (p is null) return 127;
        p.WaitForExit();
        return p.ExitCode;
    }

    public static string ShellCapture(string commandLine)
    {
        var psi = new ProcessStartInfo("/bin/sh") { UseShellExecute = false, RedirectStandardOutput = true };
        psi.ArgumentList.Add("-c");
        psi.ArgumentList.Add(commandLine);
        using var p = Process.Start(psi);
        if (p is null) return string.Empty;
        var text = p.StandardOutput.ReadToEnd();
        p.WaitForExit();
        return text;
    }

    /// Resolve one of this suite's own programs.
    ///
    /// Prefers the copy sitting next to the running script, so the collection
    /// composes correctly straight out of the source directory, whether or not it
    /// has been installed onto PATH. Falls back to the bare name so an installed
    /// copy elsewhere on PATH still wins if there is no sibling.
    private static string Sibling(string program)
    {
        var candidate = Path.Combine(Paths.ScriptDir, program);
        return File.Exists(candidate) ? candidate : program;
    }

    /// Run a sibling program in this suite. The composability primitive.
    public static int Call(string program, params string[] args)
        => Run(Sibling(program), args);

    public static int Call(string program, IEnumerable<string> args, bool quiet = false)
        => Run(Sibling(program), args, quiet: quiet);

    /// Is this executable on PATH? Lets scripts degrade gracefully.
    public static bool Exists(string exe)
    {
        if (exe.Contains('/', StringComparison.Ordinal)) return File.Exists(exe);
        var path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        foreach (var dir in path.Split(':', StringSplitOptions.RemoveEmptyEntries))
        {
            var candidate = Path.Combine(dir, exe);
            if (File.Exists(candidate)) return true;
        }
        return false;
    }

    private static ProcessStartInfo NewPsi(string exe, IEnumerable<string> args,
                                           string? cwd, IDictionary<string, string>? env)
    {
        var psi = new ProcessStartInfo(exe) { UseShellExecute = false };
        foreach (var a in args) psi.ArgumentList.Add(a);
        if (cwd is not null) psi.WorkingDirectory = cwd;
        if (env is not null)
            foreach (var kv in env) psi.Environment[kv.Key] = kv.Value;
        return psi;
    }
}

// =============================================================================
//  Fs — filenames, mime types, timestamps, and the "what to do with the
//  original file" convention shared by every converter in the suite.
// =============================================================================
internal static partial class Fs
{
    /// The ways a filesystem operation can legitimately fail: the file moved, the
    /// disk is full, the path is not ours to touch, the name is not one this
    /// filesystem accepts.
    ///
    /// Used as an exception filter so those are reported and stepped over, while
    /// anything else — a null reference, an overflow, a bug — still propagates
    /// instead of being mislabelled "could not rename".
    internal static bool IsExpectedIoFailure(Exception ex)
        => ex is IOException
            or UnauthorizedAccessException
            or NotSupportedException
            or ArgumentException;

    // Regexes are source-generated rather than built at runtime. Besides being
    // the current guidance, it matters here: RegexOptions.Compiled pays a JIT
    // cost on first use to buy faster matching later, which is the wrong trade
    // for a process that exits in under a second. A generated regex has neither
    // cost — the matcher is emitted at build time.

    /// Everything Django's slugify strips, once the text is ASCII.
    [GeneratedRegex(@"[^a-zA-Z0-9_\s-]")]
    private static partial Regex NonWordAscii();

    /// The same, keeping Unicode word characters.
    [GeneratedRegex(@"[^\w\s-]")]
    private static partial Regex NonWordUnicode();

    /// A run of dashes or whitespace, collapsed to a single dash.
    [GeneratedRegex(@"[-\s]+")]
    private static partial Regex DashOrSpaceRun();

    /// A URL inside arbitrary text. Ends on whitespace or any of the bracketing
    /// and quoting characters, including the four curly quotes.
    [GeneratedRegex(
        @"\b((?:https?://|www\d{0,3}[.]|[a-z0-9.\-]+[.][a-z]{2,4}/)" +
        @"(?:[^\s()<>]+|\(([^\s()<>]+|(\([^\s()<>]+\)))*\))+" +
        @"(?:\(([^\s()<>]+|(\([^\s()<>]+\)))*\)" +
        "|[^\\s`!()\\[\\]{};:'\".,<>?«»“”‘’]))",
        RegexOptions.IgnoreCase)]
    private static partial Regex UrlPattern();

    // --- Mime ----------------------------------------------------------------

    private static readonly Lazy<Dictionary<string, string>> MimeMap = new(LoadMimeMap);

    /// Python's mimetypes.guess_type(). part=0 -> "audio", part=1 -> "matroska".
    /// Returns null when the extension is unknown, exactly as guess_type does.
    public static string? Mime(string file, int part = 0)
    {
        var ext = SplitExt(file).Ext;
        if (ext.Length == 0) return null;

        if (!MimeMap.Value.TryGetValue(ext.ToLowerInvariant(), out var mime)) return null;

        var pieces = mime.Split('/');
        if (part >= pieces.Length) return null;
        return pieces[part].Trim().ToLowerInvariant();
    }

    /// utilities.xsh notes get_mime failing on .mka. /etc/mime.types actually maps
    /// it correctly now, but the explicit check is kept so the suite behaves the
    /// same on a machine with a thinner mime database.
    public static bool IsAudio(string f) => Mime(f) == "audio" || Ext(f) == ".mka";
    public static bool IsVideo(string f) => Mime(f) == "video";
    public static bool IsImage(string f) => Mime(f) == "image";
    public static bool IsText(string f)  => Mime(f) == "text";
    public static bool IsPdf(string f)   => Mime(f, 1) == "pdf";
    public static bool IsEpub(string f)  => Mime(f, 1) is "epub" or "epub+zip";
    public static bool IsMedia(string f) => IsAudio(f) || IsVideo(f);

    private static Dictionary<string, string> LoadMimeMap()
    {
        // Start from the extensions the suite actually cares about, so it keeps
        // working even where /etc/mime.types is absent or minimal.
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [".mka"] = "audio/matroska",  [".mp3"]  = "audio/mpeg",   [".opus"] = "audio/ogg",
            [".ogg"] = "audio/ogg",       [".oga"]  = "audio/ogg",    [".flac"] = "audio/flac",
            [".m4a"] = "audio/mp4",       [".aac"]  = "audio/aac",    [".wav"]  = "audio/x-wav",
            [".wma"] = "audio/x-ms-wma",
            [".mkv"] = "video/matroska",  [".mp4"]  = "video/mp4",    [".m4v"]  = "video/mp4",
            [".webm"] = "video/webm",     [".avi"]  = "video/x-msvideo",
            [".mov"] = "video/quicktime", [".wmv"]  = "video/x-ms-wmv",
            [".mpg"] = "video/mpeg",      [".mpeg"] = "video/mpeg",   [".flv"]  = "video/x-flv",
            [".jpg"] = "image/jpeg",      [".jpeg"] = "image/jpeg",   [".jpe"]  = "image/jpeg",
            [".png"] = "image/png",       [".gif"]  = "image/gif",    [".webp"] = "image/webp",
            [".bmp"] = "image/bmp",       [".tif"]  = "image/tiff",   [".tiff"] = "image/tiff",
            [".svg"] = "image/svg+xml",   [".avif"] = "image/avif",   [".heic"] = "image/heic",
            [".txt"] = "text/plain",      [".text"] = "text/plain",   [".srt"]  = "text/plain",
            [".md"]  = "text/markdown",   [".markdown"] = "text/markdown",
            [".html"] = "text/html",      [".htm"]  = "text/html",    [".csv"]  = "text/csv",
            [".pdf"] = "application/pdf", [".epub"] = "application/epub+zip",
            [".tar"] = "application/x-tar", [".zip"] = "application/zip",
        };

        // /etc/mime.types wins where it has an opinion, matching Python's init(),
        // which lets the system database override the built-in table.
        foreach (var file in new[] { "/etc/mime.types", Path.Combine(Paths.Home, ".mime.types") })
        {
            if (!File.Exists(file)) continue;
            try
            {
                foreach (var raw in File.ReadLines(file))
                {
                    var line = raw.Trim();
                    if (line.Length == 0 || line[0] == '#') continue;

                    var parts = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length < 2) continue;

                    foreach (var extension in parts.Skip(1))
                        map["." + extension] = parts[0];
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // An unreadable mime database just leaves the built-in table in
                // place; every extension the suite cares about is already there.
            }
        }

        return map;
    }

    // --- Names ---------------------------------------------------------------

    /// Split a path into everything-but-the-extension and the extension, keeping
    /// any directory part on the name: "docs/notes.tar.gz" -> ("docs/notes.tar", ".gz").
    ///
    /// Not Path.GetExtension: that treats a dotfile as all extension, so
    /// ".bashrc" would come back as ("", ".bashrc") and a rename would destroy the
    /// name. Here a leading dot run is part of the name, so ".bashrc" splits to
    /// (".bashrc", "") — which is what every caller needs when building an output
    /// name from an input one.
    public static (string Name, string Ext) SplitExt(string file)
    {
        var directory = Path.GetDirectoryName(file);
        var baseName = Path.GetFileName(file);

        int leadingDots = 0;
        while (leadingDots < baseName.Length && baseName[leadingDots] == '.') leadingDots++;

        // A dot within the leading run separates nothing; so does no dot at all,
        // since LastIndexOf then returns -1.
        int dot = baseName.LastIndexOf('.');
        var (name, ext) = dot < leadingDots
            ? (baseName, string.Empty)
            : (baseName[..dot], baseName[dot..]);

        return (string.IsNullOrEmpty(directory) ? name : Path.Join(directory, name), ext);
    }

    public static string Ext(string file) => SplitExt(file).Ext.ToLowerInvariant();

    /// datetime.now().strftime('%Y-%m-%d_%H-%M-%S')
    public static string Timestamp() =>
        DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture);

    /// str(uuid.uuid4())
    public static string Uuid() => Guid.NewGuid().ToString();

    /// Django's slugify: decompose, drop anything non-ASCII, lowercase, strip
    /// punctuation, collapse runs of dashes and whitespace to one dash, then trim.
    ///
    /// Lowercasing is deliberate: it stops case-insensitive filesystems from
    /// silently overwriting one file with another.
    public static string Slug(string value, bool allowUnicode = false)
    {
        value ??= string.Empty;

        if (allowUnicode)
        {
            value = NonWordUnicode().Replace(
                value.Normalize(NormalizationForm.FormKC).ToLowerInvariant(), string.Empty);
        }
        else
        {
            // Decomposing first turns "é" into "e" plus a combining accent, so
            // dropping the non-ASCII characters keeps the base letter rather than
            // losing the whole thing.
            var ascii = string.Concat(value.Normalize(NormalizationForm.FormD).Where(char.IsAscii));
            value = NonWordAscii().Replace(ascii.ToLowerInvariant(), string.Empty);
        }

        return DashOrSpaceRun().Replace(value, "-").Trim('-', '_');
    }

    /// title_case(): "some-file_name" -> "Some File Name"
    public static string TitleCase(string input)
    {
        input = (input ?? string.Empty).Replace('-', ' ').Replace('_', ' ');
        // Python's str.title() uppercases after every non-alphabetic character.
        var sb = new StringBuilder(input.Length);
        bool startOfWord = true;
        foreach (var ch in input)
        {
            if (char.IsLetter(ch))
            {
                sb.Append(startOfWord ? char.ToUpperInvariant(ch) : char.ToLowerInvariant(ch));
                startOfWord = false;
            }
            else
            {
                sb.Append(ch);
                startOfWord = true;
            }
        }
        return sb.ToString();
    }

    /// The body of the `file-rename-valid` script: slugify each name, renaming on
    /// disk, and return the new names in order.
    ///
    /// This replaces utilities.xsh's set_valid_file_names(), which shelled out to
    /// `file-rename-valid` and then ast.literal_eval'd a Python list off stdout.
    /// Done in-process there is no subprocess and no parsing.
    public static IReadOnlyList<string> RenameToValid(IEnumerable<string> files)
    {
        var result = new List<string>();

        foreach (var file in files)
        {
            // Slugify the file's own name only. Slug() strips '/' along with every
            // other non-word character, so handing it a path would flatten
            // "sub dir/Track 01.mp3" into "sub-dirtrack-01.mp3" and quietly move
            // the file into the working directory. Keep the directory aside.
            var directory = Path.GetDirectoryName(file) ?? string.Empty;
            var (stem, ext) = SplitExt(Path.GetFileName(file));

            var validName = Slug(stem);
            var validExt = Slug(ext);

            string clean;
            if (ext.Length == 0)
            {
                // No extension means we are dealing with a directory.
                clean = validName.Length == 0 ? Uuid() : validName;
            }
            else
            {
                if (validName.Length == 0)      clean = $"{Uuid()}.{validExt}";
                else if (validName == "tar")    clean = $"{Uuid()}tar.{validExt}";
                else                            clean = $"{validName}.{validExt}";

                // Restore the ".tar" in names like "archive.tar.gz", which the
                // slug pass flattened to "archivetar.gz".
                clean = clean.Replace("tar.", ".tar.", StringComparison.Ordinal);
            }

            // Put the directory back, so the file is renamed where it lives and
            // callers get a path they can still open.
            var target = directory.Length == 0 ? clean : Path.Join(directory, clean);

            result.Add(target);
            try
            {
                if (file != target) Move(file, target);
            }
            catch (Exception ex) when (IsExpectedIoFailure(ex))
            {
                Ui.Err($"Could not rename {Ui.Esc(file)}: {ex.Message}");
            }
        }

        return result;
    }

    // --- Moving / deleting ---------------------------------------------------

    /// Move a file or directory, creating the destination's parent if needed.
    public static void Move(string source, string destination)
    {
        var parent = Path.GetDirectoryName(Path.GetFullPath(destination));
        if (!string.IsNullOrEmpty(parent)) Directory.CreateDirectory(parent);

        if (Directory.Exists(source)) Directory.Move(source, destination);
        else File.Move(source, destination, overwrite: false);
    }

    /// handle_original_file(): either trash the source, or tuck it away under
    /// ./Original_<folder>/ so the conversion is reversible.
    public static void HandleOriginal(string file, bool delete, string folder = "Files")
    {
        if (delete)
        {
            Trash(file);
            return;
        }

        var dir = $"./Original_{folder}";
        Directory.CreateDirectory(dir);
        try
        {
            Move(file, Path.Combine(dir, Path.GetFileName(file)));
        }
        catch (Exception ex) when (IsExpectedIoFailure(ex))
        {
            Ui.Err($"Could not move {Ui.Esc(file)} to {Ui.Esc(dir)}: {ex.Message}");
        }
    }


    /// trash-put — a recoverable delete, never an unlink.
    public static void Trash(string path) => Proc.Run("trash-put", path);

    // --- Timestamps ----------------------------------------------------------

    /// The `touch -d "$(stat -c %y src)" dst` idiom: give the output the same
    /// modification time as its source, so date-sorted listings stay meaningful.
    ///
    /// `sequenceIndex` matters when one source yields several parts. Giving them
    /// all the identical stamp leaves nothing to order them by, and a file manager
    /// then falls back to its own tie-break — which, sorted newest-first, lists
    /// part 3 before part 1. Offsetting each part by a millisecond keeps the whole
    /// group sitting where the source did while preserving the order within it.
    public static void CopyTimestamp(string source, string destination, int sequenceIndex = 0)
    {
        try
        {
            File.SetLastWriteTime(destination,
                                  File.GetLastWriteTime(source).AddMilliseconds(sequenceIndex));
        }
        catch (Exception ex) when (IsExpectedIoFailure(ex))
        {
            Ui.Warn($"Could not copy timestamp to {destination}: {ex.Message}");
        }
    }

    // --- Argv helpers --------------------------------------------------------

    /// The near-universal preamble: take the file arguments and sort them.
    public static IReadOnlyList<string> SortedFiles(IEnumerable<string> argv)
        => [.. argv.Order(StringComparer.Ordinal)];

    /// Every file in the current directory, by name.
    public static IReadOnlyList<string> CurrentDirFiles()
        => [.. Directory.EnumerateFiles(Directory.GetCurrentDirectory())
                        .Select(Path.GetFileName)
                        .OfType<string>()
                        .Order(StringComparer.Ordinal)];

    // --- URLs ----------------------------------------------------------------

    /// Every URL in a block of text.
    public static IReadOnlyList<string> FindUrls(string text)
        => [.. UrlPattern().Matches(text ?? string.Empty).Select(m => m.Groups[1].Value)];
}

// =============================================================================
//  Media — durations, via ffprobe.
//
//  Deliberately not TagLibSharp: utilities.cs may not reference extra packages,
//  and having two duration engines would make media-length and media-length-move
//  disagree on the same folder. ffprobe also reads the real stream, where tag
//  metadata lies on VBR and truncated files.
// =============================================================================
internal static class Media
{
    /// get_duration() — seconds, or 0 with a warning when ffprobe has no answer.
    /// Run an ffmpeg conversion, reporting progress against the source's duration.
    ///
    /// Asked for "-progress pipe:1", ffmpeg writes "out_time_us=" lines to stdout
    /// roughly twice a second. Measured against the source's own length that is a
    /// real percentage, which is what the per-file bars in the parallel converters
    /// show. A conversion that finishes inside one reporting interval simply gets a
    /// single report and the bar completes — measured on this machine, 30 minutes of
    /// flac to opus produced 15 reports, and 40 seconds of opus produced one.
    ///
    /// `advance` is called with how far to move the bar on, never with an absolute
    /// value, which is what Ui.Track2's callback expects.
    ///
    /// `destination` is named separately from the args it also appears in, so that a
    /// failed run can clean up after itself. ffmpeg opens the output before it
    /// discovers it cannot encode, which otherwise leaves a nought-byte file sitting
    /// where a converted one should be — indistinguishable from a success at a
    /// glance, and with no inherited timestamp to give it away.
    public static bool ConvertWithProgress(string source, string destination,
                                           IReadOnlyList<string> args,
                                           Action<double> advance)
    {
        const string Marker = "out_time_us=";

        double duration = Duration(source);
        double reported = 0;

        // Only ever remove a file this call created. ffmpeg is asked to refuse an
        // existing output in places (video-convert-audio passes -n), and that
        // refusal is a failure in which the existing file is untouched and must
        // stay that way.
        //
        // The before-state is also how the refusal is *detected*: this ffmpeg build
        // prints "File 'x' already exists. Exiting." and then exits 0, so the exit
        // code alone would call it a success — and the caller would go on to stamp
        // the untouched file and file the source away as converted.
        var existing = new FileInfo(destination);
        bool destinationExisted = existing.Exists;
        long existingLength = destinationExisted ? existing.Length : 0;
        DateTime existingWritten = destinationExisted ? existing.LastWriteTimeUtc : default;

        // A global option, so it is safe at the front, before the caller's -i.
        var full = new List<string> { "-progress", "pipe:1" };
        full.AddRange(args);

        bool ok = Proc.OkStreaming("ffmpeg", full, line =>
        {
            // Early lines carry "N/A" rather than a number; TryParse rejects those.
            if (duration <= 0 || !line.StartsWith(Marker, StringComparison.Ordinal)) return;
            if (!long.TryParse(line.AsSpan(Marker.Length), out var micros)) return;

            double percent = Math.Clamp(micros / 1_000_000.0 / duration * 100.0, 0, 100);
            if (percent > reported)
            {
                advance(percent - reported);
                reported = percent;
            }
        });

        // A zero exit is necessary but not sufficient: require that the output was
        // actually written. Otherwise a refusal or a silent failure passes for a
        // conversion and the source gets treated as done.
        if (ok)
        {
            var written = new FileInfo(destination);

            ok = destinationExisted
                ? written.Exists && (written.Length != existingLength ||
                                     written.LastWriteTimeUtc != existingWritten)
                : written.Exists && written.Length > 0;
        }

        // ffmpeg's last report usually stops a little short of the true end.
        if (ok && reported < 100) advance(100 - reported);

        if (!ok && !destinationExisted)
        {
            try
            {
                if (File.Exists(destination)) File.Delete(destination);
            }
            catch (Exception ex) when (Fs.IsExpectedIoFailure(ex))
            {
                Ui.Warn($"Could not remove the failed output {destination}: {ex.Message}");
            }
        }

        return ok;
    }

    public static double Duration(string file)
    {
        var text = Proc.Capture("ffprobe", "-v", "quiet", "-of", "csv=p=0",
                                "-show_entries", "format=duration", file);

        if (string.IsNullOrWhiteSpace(text) ||
            !double.TryParse(text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var seconds))
        {
            Ui.Err($"{file} is not returning a valid duration.");
            return 0.0;
        }

        return seconds;
    }

    /// Durations for many files at once, order preserved. One ffprobe fork costs
    /// ~15 ms, so a few hundred files serially is several seconds of dead time.
    public static IReadOnlyList<double> Durations(IReadOnlyList<string> files)
    {
        var results = new double[files.Count];
        Parallel.For(0, files.Count,
            new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
            i => results[i] = Duration(files[i]));
        return results;
    }

    /// str(datetime.timedelta(seconds=round(x))) — "H:MM:SS", hours not zero-padded
    /// and allowed to exceed 24.
    public static string Hms(double seconds)
    {
        var total = (long)Math.Round(seconds, MidpointRounding.ToEven);
        bool negative = total < 0;
        if (negative) total = -total;

        long h = total / 3600, m = total % 3600 / 60, s = total % 60;
        return $"{(negative ? "-" : string.Empty)}{h}:{m:D2}:{s:D2}";
    }
}

// =============================================================================
//  Crypto — the Vigenère and ROT helpers behind the clipboard scripts.
//  These are toys for casual obfuscation, not security primitives.
// =============================================================================
internal static class Crypto
{
    /// generateKey(): repeat the keyword until it is as long as the text.
    public static string VigenereKey(string text, string keyword)
    {
        if (string.IsNullOrEmpty(keyword)) return string.Empty;
        if (text.Length == keyword.Length) return keyword;

        var sb = new StringBuilder(keyword);
        for (int i = 0; i < text.Length - keyword.Length; i++)
            sb.Append(keyword[i % keyword.Length]);
        return sb.ToString();
    }

    /// cipherText(): shift each character by the key, wrapping within printable ASCII.
    public static string Encrypt(string plain, string key, int asciiMin = 32, int asciiMax = 126)
    {
        int span = asciiMax + 1 - asciiMin;
        var sb = new StringBuilder(plain.Length);
        for (int i = 0; i < plain.Length; i++)
        {
            int x = ((plain[i] - asciiMin) + (key[i] - asciiMin)) % span;
            sb.Append((char)(x + asciiMin));
        }
        return sb.ToString();
    }

    /// originalText(): the inverse of Encrypt.
    public static string Decrypt(string cipher, string key, int asciiMin = 32, int asciiMax = 126)
    {
        int span = asciiMax + 1 - asciiMin;
        var sb = new StringBuilder(cipher.Length);
        for (int i = 0; i < cipher.Length; i++)
        {
            int x = ((cipher[i] - asciiMin) - (key[i] - asciiMin) + span) % span;
            sb.Append((char)(x + asciiMin));
        }
        return sb.ToString();
    }

    /// Letters only.
    public static string Rot13(string input)
    {
        var sb = new StringBuilder(input.Length);
        foreach (var c in input)
        {
            if (c is >= 'a' and <= 'z')      sb.Append((char)((c - 'a' + 13) % 26 + 'a'));
            else if (c is >= 'A' and <= 'Z') sb.Append((char)((c - 'A' + 13) % 26 + 'A'));
            else                             sb.Append(c);
        }
        return sb.ToString();
    }

    /// Letters by 13 and digits by 5, so the transform is its own inverse.
    public static string Rot13And5(string input)
    {
        var sb = new StringBuilder(input.Length);
        foreach (var c in input)
        {
            if (c is >= 'a' and <= 'z')      sb.Append((char)((c - 'a' + 13) % 26 + 'a'));
            else if (c is >= 'A' and <= 'Z') sb.Append((char)((c - 'A' + 13) % 26 + 'A'));
            else if (c is >= '0' and <= '9') sb.Append((char)((c - '0' + 5) % 10 + '0'));
            else                             sb.Append(c);
        }
        return sb.ToString();
    }

    /// get_password_entropy(): bits of entropy, and the average number of guesses
    /// to break it. Computed as length * log2(pool) rather than log2(pool**length)
    /// so nothing overflows; the attempt count needs BigInteger regardless.
    public static (double Entropy, BigInteger Attempts) PasswordEntropy(
        int passwordLength, int poolSize = 92)
    {
        double entropy = passwordLength * Math.Log2(poolSize);
        var attempts = Pow2(entropy - 1);
        return (entropy, attempts);
    }

    /// 2**x, for an x far too large for double to represent the result.
    ///
    /// The integer part becomes an exact BigInteger.Pow; the fractional part is a
    /// factor in [1, 2) applied afterwards. Scaling that factor by a power of two
    /// keeps the correction exact in binary — no decimal rounding sneaks in.
    public static BigInteger Pow2(double exponent)
    {
        if (exponent <= 0) return BigInteger.Zero;

        int whole = (int)Math.Floor(exponent);
        double fraction = exponent - whole;

        var value = BigInteger.Pow(2, whole);
        if (fraction == 0) return value;

        const int Precision = 20;   // bits kept from the fractional factor
        var scaled = new BigInteger(Math.ScaleB(Math.Pow(2, fraction), Precision));
        return value * scaled >> Precision;
    }
}

// =============================================================================
//  Snd — the Oxygen notification sounds every script signs off with.
//
//  Calls ffplay directly rather than going through the `audio-play` script:
//  a nested file-based app costs ~1 s of startup, which would be paid by every
//  script in the suite and repeatedly inside error loops. `audio-play` still
//  ships as a command for ranger and other external callers.
// =============================================================================
internal static class Snd
{
    public static void Play(string oggPath)
        => Proc.Spawn("ffplay", "-loglevel", "quiet", "-autoexit", "-nodisp", oggPath);

    public static void Success() => Play(Path.Combine(Paths.SoundTheme, "Oxygen-K3B-Finish-Success.ogg"));
    public static void Error()   => Play(Path.Combine(Paths.SoundTheme, "Oxygen-K3B-Finish-Error.ogg"));

    /// A desktop toast, for the clipboard scripts that report in the background.
    public static void Notify(string summary, string body = "", int timeoutMs = 10000)
        => Proc.Spawn("notify-send", "-t", timeoutMs.ToString(CultureInfo.InvariantCulture), summary, body);
}

// =============================================================================
//  Clip — replaces pyperclip. Wayland and X11, no package required.
// =============================================================================
internal static class Clip
{
    private static bool Wayland =>
        !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WAYLAND_DISPLAY"));

    /// Raw, not trimmed: the cipher scripts warn about leading and trailing
    /// spaces precisely because they are part of the payload.
    public static string Paste()
        => Wayland && Proc.Exists("wl-paste")
            ? Proc.CaptureRaw("wl-paste", "-n")
            : Proc.CaptureRaw("xclip", "-selection", "clipboard", "-o");

    public static void Copy(string text)
    {
        if (Wayland && Proc.Exists("wl-copy")) Proc.Pipe(text, "wl-copy");
        else Proc.Pipe(text, "xclip", "-selection", "clipboard");
    }

    public static void Clear() => Copy(string.Empty);
}

// =============================================================================
//  Jobs — how many files to work on at once.
//
//  The programs that convert media are bound by an encoder that mostly runs on
//  one core, so processing several files at once is close to a linear win. But
//  they are not free-running: each one also spawns threads of its own, and on a
//  spinning disk several concurrent read/write streams can be slower than one.
//  Hence a dial rather than a hardcoded degree of parallelism.
// =============================================================================
internal static class Jobs
{
    /// Roughly what one concurrent encoder or typesetter wants. xelatex is the
    /// hungriest of them; ffmpeg on audio and cwebp want far less.
    private const int MemoryPerJobMb = 512;

    /// Where the number below came from, for --help and for diagnosing a surprise.
    ///
    /// Declared BEFORE Default: field initialisers run in declaration order, so
    /// putting this after would blank whatever Compute() had just written into it.
    internal static string Derivation { get; private set; } = string.Empty;

    /// How many files to work on at once on THIS machine.
    ///
    /// Nothing here is a fixed number: it is derived at startup from the cores and
    /// memory actually available to this process, so the same script does the
    /// right thing on a laptop, a 64-core workstation, or inside a container with
    /// a CPU quota.
    internal static int Default { get; } = Compute();

    private static int Compute()
    {
        // An explicit policy wins over anything inferred.
        var configured = Environment.GetEnvironmentVariable("JOBS");
        if (int.TryParse(configured, NumberStyles.Integer, CultureInfo.InvariantCulture,
                         out var requested) && requested >= 1)
        {
            Derivation = $"JOBS={requested}";
            return requested;
        }

        // ProcessorCount already follows CPU affinity and cgroup quotas, so this
        // is how many cores this process may actually use, not how many are
        // installed. Halved because the tools being driven thread internally —
        // one process per core oversubscribes for very little extra throughput.
        int cores = Environment.ProcessorCount;
        int byCpu = Math.Max(1, cores / 2);

        // Each job is a whole separate process, so memory is the other ceiling.
        int? availableMb = AvailableMemoryMb();
        int byMemory = availableMb is int mb && mb > 0
            ? Math.Max(1, mb / MemoryPerJobMb)
            : int.MaxValue;

        int chosen = Math.Min(byCpu, byMemory);

        Derivation = availableMb is int m
            ? $"{cores} cores/2 = {byCpu}, {m} MB free/{MemoryPerJobMb} MB = {byMemory} -> {chosen}"
            : $"{cores} cores/2 = {byCpu} (memory unknown)";

        return chosen;
    }

    /// Memory the kernel says is actually available right now, or null if it will
    /// not say — which is the case anywhere without a Linux-style /proc.
    private static int? AvailableMemoryMb()
    {
        const string MemInfo = "/proc/meminfo";

        try
        {
            foreach (var line in File.ReadLines(MemInfo))
            {
                if (!line.StartsWith("MemAvailable:", StringComparison.Ordinal)) continue;

                var fields = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
                if (fields.Length >= 2 &&
                    long.TryParse(fields[1], NumberStyles.Integer, CultureInfo.InvariantCulture,
                                  out var kilobytes))
                    return (int)(kilobytes / 1024);
            }
        }
        catch (Exception ex) when (Fs.IsExpectedIoFailure(ex))
        {
            // No /proc, or it is not readable. The CPU count alone will do.
        }

        return null;
    }

    /// Add -j/--jobs to a program whose inputs can be handled independently.
    /// Args.Usage appends the computed default itself, so the text does not repeat it.
    internal static Args Add(Args args)
        => args.Opt("-j", "--jobs",
                    @default: Default.ToString(CultureInfo.InvariantCulture),
                    help: $"Files to process at once, 1 for one at a time " +
                          $"[{Derivation}; set JOBS to override]");

    /// Read the flag back, never below one and never wildly above what the machine
    /// can run.
    internal static int From(ArgVals values)
        => Math.Clamp(values.Int("jobs", Default), 1, Environment.ProcessorCount * 2);
}

// =============================================================================
//  Sys — the destructive operations behind the backup and maintenance scripts.
//
//  Everything here can delete or overwrite data, so it all routes through one
//  place that honours DryRun. Set Sys.DryRun and a script prints exactly what it
//  would run, in a form you can read back against the original, without touching
//  the disk.
// =============================================================================
internal static class Sys
{
    /// When true, commands are printed rather than executed.
    public static bool DryRun { get; set; }

    /// Run a command, or show it under DryRun. Returns 0 when skipped, so a
    /// dry run walks the whole script rather than stopping at the first step.
    public static int Run(string exe, params string[] args)
    {
        if (DryRun)
        {
            Ui.Line($"[dim]{Ui.Esc(Format(exe, args))}[/]");
            return 0;
        }
        return Proc.Run(exe, args);
    }

    /// Run a command as root. Called out separately because these are the ones
    /// worth reading twice.
    public static int Sudo(string exe, params string[] args)
        => Run("sudo", [exe, .. args]);

    /// rsync, with the destructive flags supplied by the caller.
    public static int Rsync(params string[] args) => Run("rsync", args);

    public static int SudoRsync(params string[] args) => Sudo("rsync", args);

    /// Record when a backup last succeeded — the equivalent of `date > file`.
    /// These stamps are what tell you a drive is stale.
    public static void Stamp(string stateName)
    {
        var path = Path.Combine(Paths.ShortcutState, $"{stateName}.txt");
        StampFile(path);
    }

    public static void StampFile(string path)
    {
        if (DryRun)
        {
            Ui.Line($"[dim]date > {Ui.Esc(path)}[/]");
            return;
        }

        try
        {
            var parent = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(parent)) Directory.CreateDirectory(parent);

            // `date` with no format, matching what the shell scripts wrote.
            File.WriteAllText(path, Proc.Capture("date") + Environment.NewLine);
        }
        catch (Exception ex) when (Fs.IsExpectedIoFailure(ex))
        {
            Ui.Warn($"Could not write the date stamp {path}: {ex.Message}");
        }
    }

    // --- btrfs ---------------------------------------------------------------

    /// Delete a subvolume. Missing subvolumes are not an error: on a first run
    /// there is nothing to rotate out yet.
    public static void DeleteSubvolume(string path) => Sudo("btrfs", "subvolume", "delete", path);

    /// Take a read-only snapshot. Read-only because a backup that can be edited
    /// in place is not a backup.
    public static void Snapshot(string source, string destination)
        => Sudo("btrfs", "subvolume", "snapshot", "-r", source, destination);

    /// The common one-deep rotation: throw away @previous, make the current state
    /// the new @previous, ready for a fresh sync into @current.
    public static void RotatePreviousSnapshot(string current, string previous)
    {
        Ui.Info("Deleting previous snapshot...");
        DeleteSubvolume(previous);

        Ui.Info("Creating a new read only snapshot...");
        Snapshot(current, previous);
    }

    /// Remove a file if it is there, quietly.
    public static void Remove(string path)
    {
        if (DryRun) { Ui.Line($"[dim]rm -f {Ui.Esc(path)}[/]"); return; }

        try
        {
            File.Delete(path);   // already a no-op when the file is absent
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Ui.Warn($"Could not remove {path}: {ex.Message}");
        }
    }

    /// Move everything inside a directory (not the directory itself) elsewhere,
    /// via pymv, which shows a progress bar and prompts before overwriting.
    ///
    /// The contents are listed explicitly rather than passing a glob, so a source
    /// directory that is empty is a no-op instead of an error about "dir/*".
    public static void MoveContents(string sourceDir, string destinationDir)
    {
        if (!Directory.Exists(sourceDir))
        {
            Ui.Warn($"Skipping {sourceDir} (not present)");
            return;
        }

        var entries = Directory.GetFileSystemEntries(sourceDir)
            .OrderBy(e => e, StringComparer.Ordinal)
            .ToList();

        if (entries.Count == 0)
        {
            Ui.Line($"[dim]Nothing to move from {Ui.Esc(sourceDir)}[/]");
            return;
        }

        Ui.Info($"Moving {entries.Count} item(s) to {destinationDir}");

        if (DryRun)
        {
            Ui.Line($"[dim]pymv -gip {Ui.Esc(sourceDir)}/* {Ui.Esc(destinationDir)}[/]");
            return;
        }

        Directory.CreateDirectory(destinationDir);

        var pymvArgs = new List<string> { "/opt/anaconda3/envs/util/bin/pymv", "-gip" };
        pymvArgs.AddRange(entries);
        pymvArgs.Add(destinationDir);

        Proc.Run("/opt/anaconda3/envs/util/bin/python", pymvArgs);
    }

    /// The rsync log every laptop backup writes, restarted each run.
    public static string CopyLog => Path.Combine(Paths.ShortcutState, "copy_log.txt");

    /// The exclude list shared by the home-directory backups.
    public static string HomeExcludes =>
        Path.Combine(Paths.Backups, "RSYNC_EXCLUDE", "rsync-homedir-local.txt");

    /// Add --dry-run to a script, wired into Sys.DryRun.
    public static Args WithDryRun(Args args)
        => args.Flag("-n", "--dry-run", "Print the commands instead of running them");

    /// Call right after parsing to apply the flag.
    public static void Apply(ArgVals values)
    {
        DryRun = values.Flag("dry-run");
        if (DryRun) Ui.Line("[bold yellow]Dry run — no changes will be made.[/]");
    }

    /// Render a command the way you would have to type it, so a dry run can be
    /// read straight across against the original shell script. Anything a shell
    /// would interpret gets quoted — note that the real call passes these through
    /// ArgumentList, so nothing is ever actually glob-expanded.
    private static string Format(string exe, IEnumerable<string> args)
        => exe + " " + string.Join(' ', args.Select(Quote));

    private static string Quote(string arg)
    {
        if (arg.Length == 0) return "''";
        bool needsQuoting = arg.Any(c => char.IsWhiteSpace(c) || c is '*' or '?' or '[' or ']'
                                          or '{' or '}' or '$' or '`' or '"' or '\\' or ';' or '&');
        return needsQuoting ? "'" + arg.Replace("'", @"'\''", StringComparison.Ordinal) + "'" : arg;
    }
}

// =============================================================================
//  Pass — the `pass` password store, shared by otp-copy, password-copy and
//  password-show.
// =============================================================================
internal static class PassStore
{
    public static string Root => Path.Combine(Paths.Home, ".password-store");

    /// Entry names under a subtree of the store ("password", "otp"), relative and
    /// without the .gpg suffix — i.e. exactly the names `pass` itself expects.
    public static IReadOnlyList<string> Entries(string subdirectory)
    {
        var root = Path.Combine(Root, subdirectory);
        if (!Directory.Exists(root)) return [];

        var entries = Directory.EnumerateFiles(root, "*.gpg", SearchOption.AllDirectories)
            .Select(path => Path.GetRelativePath(root, path))
            .Select(name => name[..^".gpg".Length])
            .ToList();

        entries.Sort(StringComparer.Ordinal);
        return entries;
    }
}

// =============================================================================
//  Num — replaces num2words.
//
//  Must work on BigInteger: password-generate reports the brute-force attempt
//  count for a 130-bit password, which is a 40-digit number.
// =============================================================================
internal static class Num
{
    private static readonly string[] Ones =
    [
        "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine",
        "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen",
        "seventeen", "eighteen", "nineteen"
    ];

    private static readonly string[] Tens =
    [
        "", "", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety"
    ];

    /// Short scale, matching num2words' English output.
    private static readonly string[] Scales =
    [
        "", "thousand", "million", "billion", "trillion", "quadrillion", "quintillion",
        "sextillion", "septillion", "octillion", "nonillion", "decillion", "undecillion",
        "duodecillion", "tredecillion", "quattuordecillion", "quindecillion",
        "sexdecillion", "septendecillion", "octodecillion", "novemdecillion", "vigintillion"
    ];

    public static string ToWords(long n) => ToWords(new BigInteger(n));

    /// num2words(n) for English, including the British "and" in "one hundred and one"
    /// and the ", " between scale groups.
    public static string ToWords(BigInteger n)
    {
        if (n.IsZero) return "zero";

        var prefix = string.Empty;
        if (n.Sign < 0) { prefix = "minus "; n = -n; }

        // Break into groups of three digits, least significant first.
        var groups = new List<int>();
        var thousand = new BigInteger(1000);
        while (!n.IsZero)
        {
            groups.Add((int)(n % thousand));
            n /= thousand;
        }

        if (groups.Count > Scales.Length)
            return prefix + "a number too large to name";

        var parts = new List<string>();
        for (int i = groups.Count - 1; i >= 0; i--)
        {
            if (groups[i] == 0) continue;   // num2words omits empty groups entirely
            var words = ThreeDigits(groups[i]);
            parts.Add(i == 0 ? words : $"{words} {Scales[i]}");
        }

        if (parts.Count == 1) return prefix + parts[0];

        // num2words joins groups with ", ", except that a final units group below
        // one hundred is attached with " and ":
        //     1,002,069 -> "one million, two thousand and sixty-nine"
        //     1,000,100 -> "one million, one hundred"
        bool trailingAnd = groups[0] is > 0 and < 100;
        var head = string.Join(", ", parts.Take(parts.Count - 1));
        return prefix + head + (trailingAnd ? " and " : ", ") + parts[^1];
    }

    private static string ThreeDigits(int n)
    {
        int hundreds = n / 100, rest = n % 100;

        if (hundreds == 0) return TwoDigits(rest);
        if (rest == 0) return $"{Ones[hundreds]} hundred";
        return $"{Ones[hundreds]} hundred and {TwoDigits(rest)}";
    }

    private static string TwoDigits(int n)
    {
        if (n < 20) return Ones[n];
        int tens = n / 10, unit = n % 10;
        return unit == 0 ? Tens[tens] : $"{Tens[tens]}-{Ones[unit]}";
    }

    /// get-passphrase-strength's seconds_to_printable(): report only the largest
    /// unit that is non-zero.
    public static string HumanizeSeconds(double seconds) => HumanizeSeconds(BigFloor(seconds));

    /// The exact-arithmetic form. Brute-force times run to tens of digits, where
    /// a double has already thrown away everything below the sixteenth.
    public static string HumanizeSeconds(BigInteger secs)
    {
        var minutes = secs / 60;
        var hours = minutes / 60;
        var days = hours / 24;
        var years = days / 365;

        if (years.IsZero && days.IsZero && hours.IsZero && minutes.IsZero)
            return $"{ToWords(secs)} seconds";
        if (years.IsZero && days.IsZero && hours.IsZero) return $"{ToWords(minutes)} minutes";
        if (years.IsZero && days.IsZero) return $"{ToWords(hours)} hours";
        if (years.IsZero) return $"{ToWords(days)} days";
        return $"{ToWords(years)} years";
    }

    /// math.floor() for values far beyond long.MaxValue.
    public static BigInteger BigFloor(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value)) return BigInteger.Zero;
        return new BigInteger(Math.Floor(value));
    }
}

// =============================================================================
//  Args — a small argparse-compatible parser.
//
//  Not Spectre.Console.Cli: that is a separate package whose stable line lags
//  Spectre.Console, its CommandApp + settings-class model fights top-level
//  statements, and it has no equivalent for the str2bool / nargs='?' option
//  that media-length-move relies on.
//
//  Supports: -s V, -sV, --speed V, --speed=V, clustered short flags (-de),
//  the "--" terminator, and positionals interleaved with options.
// =============================================================================
internal sealed class Args
{
    private sealed class Spec
    {
        public string Short = string.Empty;      // "-s" (may be empty)
        public string Long = string.Empty;       // "--speed"
        public string Key = string.Empty;        // "speed"
        public string Help = string.Empty;
        public bool IsFlag;
        public bool Required;
        public string? Default;
        public string[]? Choices;
        public bool ValueOptional;               // nargs='?'
        public string? ConstValue;               // the value used when nargs='?' is bare
    }

    private readonly string _prog;
    private readonly string _description;
    private readonly List<Spec> _specs = [];
    private readonly List<(string Name, string Help)> _positionals = [];
    private (string Name, int Min, string Help)? _rest;

    public Args(string prog, string description = "")
    {
        _prog = prog;
        _description = description;
    }

    /// argparse action='store_true'.
    public Args Flag(string shortName, string longName, string help = "")
    {
        _specs.Add(new Spec
        {
            Short = shortName, Long = longName, Key = Key(longName),
            Help = help, IsFlag = true, Default = "false"
        });
        return this;
    }

    /// A valued option. `valueOptional` + `constValue` implement nargs='?'.
    public Args Opt(string shortName, string longName, string? @default = null,
                    bool required = false, string help = "", string[]? choices = null,
                    bool valueOptional = false, string? constValue = null)
    {
        _specs.Add(new Spec
        {
            Short = shortName, Long = longName, Key = Key(longName),
            Help = help, Required = required, Default = @default,
            Choices = choices, ValueOptional = valueOptional, ConstValue = constValue
        });
        return this;
    }

    /// A single required positional.
    public Args Pos(string name, string help = "")
    {
        _positionals.Add((name, help));
        return this;
    }

    /// argparse nargs='+' — everything left over.
    public Args Rest(string name, int min = 1, string help = "")
    {
        _rest = (name, min, help);
        return this;
    }

    public ArgVals Parse(string[] argv)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        var present = new HashSet<string>(StringComparer.Ordinal);
        var loose = new List<string>();

        foreach (var spec in _specs)
            if (spec.Default is not null) values[spec.Key] = spec.Default;

        bool optionsDone = false;

        for (int i = 0; i < argv.Length; i++)
        {
            var token = argv[i];

            if (optionsDone || !LooksLikeOption(token))
            {
                loose.Add(token);
                continue;
            }

            if (token == "--") { optionsDone = true; continue; }

            if (token is "-h" or "--help") { Console.WriteLine(Usage); Environment.Exit(0); }

            if (token.StartsWith("--", StringComparison.Ordinal))
            {
                string name = token, inline = string.Empty;
                bool hasInline = false;

                int eq = token.IndexOf('=', StringComparison.Ordinal);
                if (eq > 0)
                {
                    name = token[..eq];
                    inline = token[(eq + 1)..];
                    hasInline = true;
                }

                var spec = _specs.FirstOrDefault(s => s.Long == name)
                           ?? Fail($"unrecognized arguments: {token}");

                Consume(spec, hasInline ? inline : null, argv, ref i, values, present);
                continue;
            }

            // A short token: either a cluster of flags, or an option with its
            // value attached (-s1.5), or an option whose value is the next token.
            for (int c = 1; c < token.Length; c++)
            {
                var shortName = "-" + token[c];
                var spec = _specs.FirstOrDefault(s => s.Short == shortName)
                           ?? Fail($"unrecognized arguments: {shortName}");

                if (spec.IsFlag)
                {
                    Consume(spec, null, argv, ref i, values, present);
                    continue;
                }

                // Everything after this character is the value, if anything is.
                var attached = c + 1 < token.Length ? token[(c + 1)..] : null;
                Consume(spec, attached, argv, ref i, values, present);
                break;
            }
        }

        foreach (var spec in _specs)
            if (spec.Required && !present.Contains(spec.Key))
                Fail($"the following arguments are required: {spec.Short}/{spec.Long}");

        // Positionals first, then whatever remains is the rest-list.
        var positional = new Dictionary<string, string>(StringComparer.Ordinal);
        int taken = 0;
        foreach (var (name, _) in _positionals)
        {
            if (taken >= loose.Count) Fail($"the following arguments are required: {name}");
            positional[name] = loose[taken++];
        }

        var rest = loose.Skip(taken).ToList();
        if (_rest is { } r && rest.Count < r.Min)
            Fail($"the following arguments are required: {r.Name}");
        if (_rest is null && rest.Count > 0)
            Fail($"unrecognized arguments: {string.Join(' ', rest)}");

        return new ArgVals(values, present, positional, rest);
    }

    private static void Consume(Spec spec, string? attached, string[] argv, ref int i,
                         Dictionary<string, string> values, HashSet<string> present)
    {
        present.Add(spec.Key);

        if (spec.IsFlag)
        {
            values[spec.Key] = "true";
            return;
        }

        string? value = attached;

        if (value is null)
        {
            bool nextIsValue = i + 1 < argv.Length && !LooksLikeOption(argv[i + 1]);
            if (nextIsValue) value = argv[++i];
            else if (spec.ValueOptional) value = spec.ConstValue ?? "true";
            else Fail($"argument {spec.Short}/{spec.Long}: expected one argument");
        }

        if (spec.Choices is not null && !spec.Choices.Contains(value))
            Fail($"argument {spec.Short}/{spec.Long}: invalid choice: '{value}' " +
                 $"(choose from {string.Join(", ", spec.Choices.Select(c => $"'{c}'"))})");

        values[spec.Key] = value!;
    }

    /// A leading '-' only starts an option when what follows is not a digit or a
    /// decimal point, so negative numbers stay usable as values.
    private static bool LooksLikeOption(string token)
    {
        if (token.Length < 2 || token[0] != '-') return false;
        if (token == "--") return true;
        return !char.IsDigit(token[1]) && token[1] != '.';
    }

    private static Spec Fail(string message)
    {
        Console.Error.WriteLine($"error: {message}");
        Snd.Error();
        Environment.Exit(2);
        return null!;
    }

    private static string Key(string longName) => longName.TrimStart('-');

    /// argparse-shaped help text.
    public string Usage
    {
        get
        {
            var sb = new StringBuilder();

            var head = new StringBuilder($"usage: {_prog} [-h]");
            foreach (var s in _specs)
            {
                var body = s.IsFlag ? s.Short : $"{s.Short} {s.Key.ToUpperInvariant()}";
                head.Append(s.Required ? $" {body}" : $" [{body}]");
            }
            foreach (var (name, _) in _positionals) head.Append(CultureInfo.InvariantCulture, $" {name}");
            if (_rest is { } r) head.Append(CultureInfo.InvariantCulture, $" {r.Name} [{r.Name} ...]");
            sb.AppendLine(head.ToString());

            if (!string.IsNullOrEmpty(_description))
            {
                sb.AppendLine();
                sb.AppendLine(_description);
            }

            if (_positionals.Count > 0 || _rest is not null)
            {
                sb.AppendLine();
                sb.AppendLine("positional arguments:");
                foreach (var (name, help) in _positionals)
                    sb.AppendLine(CultureInfo.InvariantCulture, $"  {name,-22}{help}");
                if (_rest is { } rr) sb.AppendLine(CultureInfo.InvariantCulture, $"  {rr.Name,-22}{rr.Help}");
            }

            sb.AppendLine();
            sb.AppendLine("options:");
            sb.AppendLine(CultureInfo.InvariantCulture, $"  {"-h, --help",-22}show this help message and exit");
            foreach (var s in _specs)
            {
                var left = s.IsFlag
                    ? $"{s.Short}, {s.Long}"
                    : $"{s.Short} {s.Key.ToUpperInvariant()}, {s.Long} {s.Key.ToUpperInvariant()}";
                var help = s.Help;
                if (s.Choices is not null) help += $" (choices: {string.Join(", ", s.Choices)})";
                if (s.Default is not null && !s.IsFlag) help += $" (default: {s.Default})";

                if (left.Length <= 20) sb.AppendLine(CultureInfo.InvariantCulture, $"  {left,-22}{help}");
                else { sb.AppendLine(CultureInfo.InvariantCulture, $"  {left}"); sb.AppendLine(CultureInfo.InvariantCulture, $"  {"",-22}{help}"); }
            }

            return sb.ToString().TrimEnd();
        }
    }
}

/// The result of Args.Parse.
internal sealed class ArgVals
{
    private readonly Dictionary<string, string> _values;
    private readonly HashSet<string> _present;
    private readonly Dictionary<string, string> _positional;

    internal ArgVals(Dictionary<string, string> values, HashSet<string> present,
                     Dictionary<string, string> positional, List<string> rest)
    {
        _values = values;
        _present = present;
        _positional = positional;
        Rest = rest;
    }

    /// Everything matched by Args.Rest(), in the order given on the command line.
    public IReadOnlyList<string> Rest { get; }

    /// Was this option actually given (as opposed to falling back to its default)?
    public bool Has(string name) => _present.Contains(name.TrimStart('-'));

    public bool Flag(string name) => Str(name, "false") == "true";

    public string Str(string name, string fallback = "")
    {
        name = name.TrimStart('-');
        if (_positional.TryGetValue(name, out var p)) return p;
        return _values.TryGetValue(name, out var v) ? v : fallback;
    }

    public string? StrOrNull(string name)
    {
        name = name.TrimStart('-');
        if (_positional.TryGetValue(name, out var p)) return p;
        return _values.TryGetValue(name, out var v) ? v : null;
    }

    public int Int(string name, int fallback = 0)
        => int.TryParse(Str(name), NumberStyles.Integer, CultureInfo.InvariantCulture, out var v)
            ? v : fallback;

    public double Dbl(string name, double fallback = 0)
        => double.TryParse(Str(name), NumberStyles.Float, CultureInfo.InvariantCulture, out var v)
            ? v : fallback;

    /// argparse's str2bool: "yes/true/t/y/1/on" are true, everything else false.
    public bool Bool(string name, bool fallback = false)
    {
        var value = StrOrNull(name);
        if (value is null) return fallback;
        return value.ToLowerInvariant() is "yes" or "true" or "t" or "y" or "1" or "on";
    }
}
