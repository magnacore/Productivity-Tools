// =============================================================================
//  tui.cs — modal dialogs, for the `*-tui` wrapper programs.
//
//  Included with:
//      #:package Terminal.Gui@2.4.17
//      #:include tui.cs
//
//  Kept separate from utilities.cs on purpose. `#:include` compiles a file into
//  the including program, so anything utilities.cs mentions would force
//  Terminal.Gui onto all 88 scripts. Only the handful that put up a real dialog
//  box pay for it.
//
//  These replace the prompt_toolkit shortcuts the xonsh wrappers used
//  (radiolist_dialog, yes_no_dialog, input_dialog). Every one returns a nullable:
//  null means the user cancelled, which the callers treat as "exit quietly",
//  exactly as prompt_toolkit's None did.
//
//  Terminal.Gui needs a real terminal it can measure. When it cannot get one —
//  input arriving down a pipe, a session with no tty, a window too small to lay
//  a dialog out in — it does not degrade, it throws. So every dialog here is
//  tried inside a guard and falls back to the equivalent inline Spectre prompt.
//  The wrapper then still works from a script or a cron job instead of dying.
//
//  Built on the scoped Application.Create() API. The older static Application
//  members (Init/Run/Shutdown/Instance) are obsolete in Terminal.Gui 2.4 and warn
//  on every build.
// =============================================================================

using System.Collections.ObjectModel;
using System.Drawing;
using Terminal.Gui.App;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

// Terminal.Gui and Spectre.Console both define Color, and Attribute collides with
// System.Attribute, so the drawing types are aliased rather than imported.
using TgColor = Terminal.Gui.Drawing.Color;
using TgAttribute = Terminal.Gui.Drawing.Attribute;
using TgScheme = Terminal.Gui.Drawing.Scheme;

internal static class Dlg
{
    /// A dialog needs at least this much room before it is worth attempting.
    private const int MinimumWidth = 40;
    private const int MinimumHeight = 10;

    /// Pick one of a list of options. Returns its index, or null if cancelled.
    ///
    /// A vertical radio list, navigated with the up and down arrows and confirmed
    /// with Enter — the same shape as the prompt_toolkit radiolist_dialog this
    /// replaces.
    public static int? Choose(string title, string message, params string[] options)
    {
        Term.Drain();

        if (TryGui(app => GuiChoose(app, title, message, options), out var choice))
            return choice;

        // --- Inline fallback ---
        if (!Ui.Interactive)
        {
            ReportNoTerminal(title);
            return null;
        }

        Ui.Begin(title);
        var picked = Ui.Select([.. options, "Cancel"], message);
        var index = Array.IndexOf(options, picked);
        return index < 0 ? null : index;
    }

    /// The suite's one "what should happen to the originals?" question.
    ///
    /// Asked in one place because a dozen programs need it and the phrasing had
    /// drifted into five variants. It also states what actually happens, which none
    /// of the old wordings did: answering No does not leave the originals alone, it
    /// files them under ./Original_&lt;kind&gt;/. Either way the file moves — the answer
    /// only chooses trash or sidecar folder. See Fs.HandleOriginal.
    ///
    /// `kind` is the bucket name the base program passes to Fs.HandleOriginal:
    /// "Audio", "Images", "Media", "Texts", "Video".
    ///
    /// Returns null when cancelled, which callers treat as "do nothing at all".
    public static bool? DeleteOriginals(string kind)
    {
        // Built on Choose rather than YesNo so that KEEPING is the resting choice.
        // YesNo lists Yes first, which would put "trash them" one stray Enter away.
        var choice = Dlg.Choose(
            "Original Files",
            $"What should happen to the original files?",
            $"Keep them  -  filed under ./Original_{kind}/",
            "Trash them  -  recoverable, via trash-put");

        return choice switch { 0 => false, 1 => true, _ => null };
    }

    /// A yes/no question. Returns null if cancelled.
    public static bool? YesNo(string title, string message)
    {
        var choice = Choose(title, message, "Yes", "No");
        return choice switch { 0 => true, 1 => false, _ => null };
    }

    /// Ask for a line of text. Returns null if cancelled, which is distinct from
    /// an empty string (deliberately submitting nothing).
    public static string? Input(string title, string message, string initialValue = "")
    {
        Term.Drain();

        if (TryGui(app => GuiInput(app, title, message, initialValue), out var text))
            return text;

        // --- Inline fallback ---
        if (!Ui.Interactive)
        {
            ReportNoTerminal(title);
            return null;
        }

        Ui.Begin(title);
        return Ui.Ask(message, initialValue.Length > 0 ? initialValue : null);
    }

    // -------------------------------------------------------------------------

    private static int? GuiChoose(IApplication app, string title, string message, string[] options)
    {
        int? answer = null;

        // ListView, not MessageBox: a message box lays its choices out as a row of
        // buttons, which navigate with Tab and Left/Right, so the up and down
        // arrows do nothing. This is a vertical list whose SelectedItem tracks the
        // arrow keys, matching the prompt_toolkit radiolist_dialog it replaces.
        var list = new ListView
        {
            X = 1,
            Y = 2,
            Width = Dim.Fill(2),
            Height = options.Length,
        };

        list.SetSource(new ObservableCollection<string>(options));

        // Start on the first entry, so one Down moves to the second rather than
        // merely entering the list.
        list.SelectedItem = 0;

        RunDialog(app, title, message, list,
                  onAccept: () => answer = list.SelectedItem,
                  height: options.Length + 7);

        return answer;
    }

    private static string? GuiInput(IApplication app, string title, string message, string initialValue)
    {
        string? answer = null;

        var field = new TextField
        {
            X = 1,
            Y = 2,
            Width = Dim.Fill(2),
            Text = initialValue,
        };

        RunDialog(app, title, message, field,
                  onAccept: () => answer = field.Text,
                  height: 9);

        return answer;
    }

    /// Lay out one modal: a prompt, the control that answers it, and Ok/Cancel.
    ///
    /// `onAccept` runs only on Ok or Enter, so a cancel leaves the caller's
    /// answer untouched at null.
    private static void RunDialog(IApplication app, string title, string message,
                                  View input, Action onAccept, int height)
    {
        var dialog = new Dialog
        {
            Title = title,
            Width = Dim.Percent(70),
            Height = Math.Min(height, Math.Max(MinimumHeight, app.Screen.Height - 4)),
        };

        var prompt = new Label { X = 1, Y = 0, Text = message };

        var ok = new Button { Text = "_Ok", IsDefault = true, X = Pos.Center() - 8, Y = Pos.AnchorEnd(1) };
        ok.Accepting += (_, e) =>
        {
            onAccept();
            e.Handled = true;
            app.RequestStop(dialog);
        };

        var cancel = new Button { Text = "_Cancel", X = Pos.Center() + 2, Y = Pos.AnchorEnd(1) };
        cancel.Accepting += (_, e) =>
        {
            e.Handled = true;
            app.RequestStop(dialog);
        };

        dialog.Add(prompt, input, ok, cancel);

        // Focus the control being answered, not the buttons, so the arrow keys go
        // where the user expects the moment the dialog opens.
        //
        // Hooked to Initialized rather than called here: a Dialog gives focus to
        // its default button as it comes up, which would override a SetFocus made
        // before Run and leave the arrow keys doing nothing.
        dialog.Initialized += (_, _) => input.SetFocus();

        app.Run(dialog);
        dialog.Dispose();
    }

    /// Bring the full-screen UI up, run one dialog, and always tear it down —
    /// leaving the terminal usable even if the dialog throws.
    ///
    /// Returns false when the graphical path is unavailable, so the caller can
    /// fall back rather than fail.
    private static bool TryGui<T>(Func<IApplication, T?> body, out T? result)
    {
        result = default;

        // No terminal at all means there is nothing to draw on. This one is
        // expected — under a pipe or cron there is nothing to report.
        if (!Ui.Interactive) return false;

        // Measure the terminal BEFORE Terminal.Gui is initialised.
        //
        // This ordering is the whole ballgame. Application.Init asks the terminal
        // for its size and cursor position by writing escape queries (CSI 18 t,
        // CSI 6 n) and reading the answers back off stdin. If we then decide not
        // to run a dialog, those answers are still sitting unread in the input
        // buffer — and the inline Spectre prompt we fall back to reads them as if
        // they were keystrokes. Its key parser desynchronises, arrow keys arrive
        // as uninterpreted "^[OB" text, and the list becomes unnavigable.
        //
        // Console.WindowWidth is an ioctl: it writes nothing and reads nothing, so
        // asking it first cannot disturb anything. Terminal.Gui is only started
        // once we are committed to using it.
        var (width, height) = TerminalSize();
        if (width < MinimumWidth || height < MinimumHeight)
        {
            Fallback($"terminal is {width}x{height}, need at least {MinimumWidth}x{MinimumHeight}");
            return false;
        }

        Term.KeyboardTaken();

        IApplication? app = null;
        bool started = false;
        try
        {
            app = Application.Create();
            app.Init(null);
            started = true;

            // Terminal.Gui works out the screen size by writing a query to the
            // terminal and reading the reply. Under some launchers — a file
            // manager's shell, a pty whose input it does not own — that reply
            // never comes back, and it is left believing the screen is 0x0. It
            // then starts up, clears the display and draws nothing at all.
            //
            // Console.WindowWidth already told us the real size without asking the
            // terminal anything, so hand it over. Only when its own answer is
            // unusable, so a correct detection is never overridden.
            if (app.Screen.Width <= 0 || app.Screen.Height <= 0)
                app.Screen = new Rectangle(0, 0, width, height);

            ApplyMonokai();
            result = body(app);
            return true;
        }
        catch (Exception ex)
        {
            // Deliberately broad: any driver, layout or theming failure inside
            // Terminal.Gui should degrade to the inline prompt, never kill the
            // program the user actually asked to run.
            //
            // But it must not do so silently. A quiet fallback here once made a
            // real layout bug look like a deliberate design choice.
            Fallback($"{ex.GetType().Name}: {ex.Message}");
            return false;
        }
        finally
        {
            // The terminal must be handed back whatever happened above.
            try { app?.Dispose(); }
            catch (Exception) { }

            // Terminal.Gui ran, so its query replies may still be queued. Discard
            // anything pending rather than let the inline prompt read it as input.
            if (started) DrainInput();
        }
    }

    /// The terminal's size, without asking the terminal anything.
    private static (int Width, int Height) TerminalSize()
    {
        try
        {
            return (Console.WindowWidth, Console.WindowHeight);
        }
        catch (Exception ex) when (ex is IOException or ArgumentOutOfRangeException)
        {
            // No console attached; the Ui.Interactive check above normally catches
            // this first, so treat it as unusable and fall back.
            return (0, 0);
        }
    }

    /// Throw away buffered input, so leftover escape-sequence replies cannot be
    /// mistaken for keystrokes by whatever prompts next.
    private static void DrainInput()
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

    /// Say why the dialog was not used, so an inline prompt appearing where a
    /// dialog was expected is self-explaining rather than a mystery.
    ///
    /// Set TUI_QUIET=1 to suppress, or TUI_TRACE=1 for the full stack.
    private static void Fallback(string reason)
    {
        if (Environment.GetEnvironmentVariable("TUI_QUIET") == "1") return;

        Ui.Line($"[yellow]Dialog unavailable ({Ui.Esc(reason)}); using inline prompt.[/]");

        if (Environment.GetEnvironmentVariable("TUI_TRACE") == "1")
            Ui.Line($"[dim]{Ui.Esc(Environment.StackTrace)}[/]");
    }

    /// There is no terminal to ask on. Say so once, plainly; the caller then
    /// treats the answer as a cancellation.
    private static void ReportNoTerminal(string title)
    {
        Ui.Err($"{title}: needs a terminal to ask this question.");
        Ui.Line("[dim]Run the underlying command directly, or use this wrapper's " +
                "bypass flags if it has them (see --help).[/]");
    }

    /// The Monokai palette the prompt_toolkit dialogs used: #272822 ground,
    /// #f8f8f2 text. Applied best-effort — a failure to theme is not a reason to
    /// refuse to show the dialog.
    private static void ApplyMonokai()
    {
        try
        {
            var background = new TgColor(0x27, 0x28, 0x22);   // Monokai ground
            var foreground = new TgColor(0xf8, 0xf8, 0xf2);   // Monokai text
            var accent = new TgColor(0xa6, 0xe2, 0x2e);       // Monokai green, for hotkeys
            var muted = new TgColor(0x75, 0x71, 0x5e);        // Monokai comment grey

            var scheme = new TgScheme
            {
                Normal = new TgAttribute(foreground, background),
                Focus = new TgAttribute(background, foreground),
                HotNormal = new TgAttribute(accent, background),
                HotFocus = new TgAttribute(accent, foreground),
                Disabled = new TgAttribute(muted, background),
            };

            var schemes = Terminal.Gui.Configuration.SchemeManager.GetSchemesForCurrentTheme();
            schemes["Dialog"] = scheme;
            schemes["Base"] = scheme;
        }
        catch (Exception)
        {
            // Deliberately broad: colours are decoration, and any drift in the
            // theming API must not stop the dialog appearing.
        }
    }
}
