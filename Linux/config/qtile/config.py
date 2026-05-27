from typing import List  # noqa: F401

import os
import re
# import socket
import subprocess

from libqtile import bar, layout, widget, hook, qtile
from libqtile import images
from libqtile.config import Click, Drag, Group, Key, Match, Screen
from libqtile.images import Img
from libqtile.lazy import lazy
from libqtile.log_utils import logger
from libqtile.widget import base
from libqtile.widget.battery import BatteryState
# from libqtile.utils import guess_terminal

myhome = os.path.expanduser('~')

mod = "mod4"
alt = "mod1"
mod2 = "control"
mod3 = "shift"

# terminal = guess_terminal()
myTerm = "xfce4-terminal"	# My terminal of choice
#myBrowser = "firefox"	# My browser of choice
myBrowser = "flatpak run io.gitlab.librewolf-community"

keys = [
    # Switch between windows
    Key([mod], "h", lazy.layout.left(), desc="Move focus to left"),
    Key([mod], "l", lazy.layout.right(), desc="Move focus to right"),
    Key([mod], "j", lazy.layout.down(), desc="Move focus down"),
    Key([mod], "k", lazy.layout.up(), desc="Move focus up"),
    Key([mod], "space", lazy.layout.next(), desc="Move window focus to other window"),

    # Move windows between left/right columns or move up/down in current stack.
    # Moving out of range in Columns layout will create new column.
    Key([mod, "shift"], "h", lazy.layout.shuffle_left(), desc="Move window to the left"),
    Key([mod, "shift"], "l", lazy.layout.shuffle_right(), desc="Move window to the right"),
    Key([mod, "shift"], "j", lazy.layout.shuffle_down(), desc="Move window down"),
    Key([mod, "shift"], "k", lazy.layout.shuffle_up(), desc="Move window up"),

    # Grow windows. If current window is on the edge of screen and direction
    # will be to screen edge - window would shrink.
    Key([mod, "control"], "h", lazy.layout.grow_left(), desc="Grow window to the left"),
    Key([mod, "control"], "l", lazy.layout.grow_right(), desc="Grow window to the right"),
    Key([mod, "control"], "j", lazy.layout.grow_down(), desc="Grow window down"),
    Key([mod, "control"], "k", lazy.layout.grow_up(), desc="Grow window up"),
    Key([mod], "n", lazy.layout.normalize(), desc="Reset all window sizes"),

    # Toggle between split and unsplit sides of stack.
    # Split = all windows displayed
    # Unsplit = 1 window displayed, like Max layout, but still with
    # multiple stack panes
    Key([mod, "shift"], "Return", lazy.layout.toggle_split(), desc="Toggle between split and unsplit sides of stack"),

    # Toggle between different layouts as defined below
    Key([mod], "Tab", lazy.next_layout(), desc="Toggle between layouts"),
    Key([mod], "w", lazy.window.kill(), desc="Kill focused window"),

    Key([mod, "control"], "r", lazy.restart(), desc="Restart Qtile"),
    #Key([mod, "control"], "q", lazy.shutdown(), desc="Shutdown Qtile"),

    Key([mod], "m", lazy.layout.maximize(), desc='Toggle window between minimum and maximum sizes'),
    Key([mod, "shift"], "f", lazy.window.toggle_floating(), desc='Toggle floating'),
    Key([mod], "f", lazy.window.toggle_fullscreen(), desc='Toggle fullscreen'),

    # My shortcuts
    Key([mod], "t", lazy.spawn(myTerm+f" --disable-server --initial-title 'xfce4-terminal' -e '/opt/anaconda3/envs/xonsh/bin/python /opt/anaconda3/envs/xonsh/bin/xonsh'"), desc="Launch terminal"),
    Key([mod, "shift"], "v", lazy.spawn(f"/opt/VVV-1.5.0-x86_64/vvv-start.sh"), desc="Launch VVV"),
    Key([mod], "Return", lazy.spawn(myTerm+f" --disable-server --initial-title 'Ranger' -e '/opt/anaconda3/envs/xonsh/bin/python /opt/anaconda3/envs/xonsh/bin/xonsh /usr/local/bin/ranger-open'"), desc="Launch Ranger"),
    Key([mod], "b", lazy.spawn(myBrowser), desc='My Browser' ),
    #Key([mod, "shift"], "c", lazy.spawn(myTerm+" --disable-server -e 'flatpak run com.github.miguelmota.Cointop'"), desc='Cointop' ),
    Key([mod], "d", lazy.spawn(f"/opt/anaconda3/envs/util/bin/python {myhome}/.local/bin/clipboard-convert-text"), desc="Save clipboard to text"),
    Key([mod], "y", lazy.spawn(f"/opt/anaconda3/envs/util/bin/python {myhome}/.local/bin/clipboard-insert-link"), desc="Insert URLs in a text file"),
    Key([mod], "v", lazy.spawn(f"/opt/anaconda3/envs/util/bin/python {myhome}/.local/bin/clipboard-youtube-save"), desc="Save YouTube URLs in a text file"),
    # Key([mod, "shift"], "m", lazy.spawn(f"bash {myhome}/Software/CMapTools/bin/CmapTools"), desc="Launch Cmap"),
    Key([mod], "g", lazy.spawn("thunar"), desc="Launch Thunar"),

    ## Rofi
    Key([mod], "r", lazy.spawn("rofi -show drun -show-icons -dpi 1"), desc='Run Rofi Application Launcher'),
    Key([alt], "Tab", lazy.spawn("rofi -show window -dpi 1"), desc='Run Rofi Window Switcher'),
    Key([mod], "e", lazy.spawn(f"/opt/anaconda3/envs/util/bin/python /opt/anaconda3/envs/util/bin/rofimoji --action copy --skin-tone 'moderate'"), desc='Run Rofi emoji picker'),
    Key([mod], "c", lazy.spawn("rofi -modi 'clipboard:/usr/local/bin/greenclip print' -show clipboard -run-command '{cmd}' -dpi 1"), desc='Run Greenclip in Rofi'),

    ## Volume
    Key([], "XF86AudioMute", lazy.spawn("amixer -D pipewire sset Master toggle")),
    Key([], "XF86AudioLowerVolume", lazy.spawn("amixer -D pipewire sset Master 1%-")),
    Key([], "XF86AudioRaiseVolume", lazy.spawn("amixer -D pipewire sset Master 1%+")),
    Key([mod], "a", lazy.spawn("amixer set Master 15%")),

    ## Scratchpads
    Key([mod2, mod3], "a", lazy.spawn(myTerm+f" --disable-server -e 'nvim {myhome}/Productivity_System/TODO.md'"), desc="Launch TODO List"),
    Key([mod2, mod3], "y", lazy.spawn(myTerm+f" --disable-server -e 'nvim {myhome}/Backups/youtube.txt'"), desc="Launch Youtube Download List"),
]

# Run xprop | grep WM_CLASS | awk '{print $4}' in terminal to find wm_class
groups = [Group("1", layout='treetab', matches=[Match(wm_class=re.compile(r"^(Station|Ferdium|fr.handbrake.ghb|thunderbird-esr|Transmission-gtk)$"))]),
          Group("2", layout='bsp'),
          Group("3", layout='bsp', matches=[Match(wm_class=re.compile(r"^(firefox-esr|librewolf)$"))]),
          Group("4", layout='max'),
          Group("5", layout='bsp'),
          Group("6", layout='bsp'),
          Group("7", layout='bsp'),
          Group("8", layout='bsp'),
          Group("9", layout='bsp'),
          Group("0", layout='floating')]

def toscreen(qtile, group_name):
    if group_name  == qtile.current_screen.group.name:
        qtile.current_screen.set_group(qtile.current_screen.previous_group)
    else:
        for i in range(len(qtile.groups)):
            if group_name == qtile.groups[i].name:
                qtile.current_screen.set_group(qtile.groups[i])
                break

for i in groups:
    keys.extend([
        # mod1 + letter of group = switch to group
        #Key([mod], i.name, lazy.group[i.name].toscreen()),
        # switch to group with ability to go to previous group if pressed again
        Key([mod], i.name, lazy.function(toscreen, i.name)),

        # mod1 + shift + letter of group = switch to & move focused window to group
        Key([mod, "shift"], i.name, lazy.window.togroup(i.name)),
    ])

# Monokai Pro (default) palette — single source of truth.
BG      = "#2D2A2E"  # panel background
BG2     = "#403E41"  # secondary background (widget banding)
FG      = "#FCFCFA"  # foreground (text / icons)
RED     = "#FF6188"  # red accent (urgent)
PURPLE  = "#AB9DF2"  # purple accent
CYAN    = "#78DCE8"  # cyan accent (focus / active)
YELLOW  = "#FFD866"  # yellow accent
GREY    = "#939293"  # dim grey (inactive text)
GREY_DK = "#5B595C"  # muted grey (unfocused window border)

layout_theme = {"border_width": 2, # Window highlight width
                "margin": 0, # Gap between windows
                "border_focus": CYAN,
                "border_normal": GREY_DK
                }

layouts = [
    layout.Bsp(**layout_theme),
    layout.Max(),
    layout.TreeTab(
        bg_color = BG,
        active_bg = CYAN,
        active_fg = BG,
        inactive_bg = BG2,
        inactive_fg = GREY,
        border_width = 2,
    ),
]

# Indexed view of the palette for the bar widgets that reference colors[N].
# 0=bg 1=bg2 2=fg 3=red 4=purple 5=cyan 6=yellow 7=grey
colors = [BG, BG2, FG, RED, PURPLE, CYAN, YELLOW, GREY]

widget_defaults = dict(
    font='RobotoMono Nerd Font',
    fontsize=13,
    padding=0,
    # background=colors[2]
)
extension_defaults = widget_defaults.copy()

def open_bpytop():
    qtile.spawn(myTerm+" --disable-server -e bpytop")

widget_padding = 0
seperator_padding = 5
icon_font_size = 15
bar_size = 32
# Non-Nerd font for rotated text in the vertical bars. RobotoMono Nerd Font has
# tall metrics (room for icon glyphs) that leave plain text off-centre once
# rotated; a normal-metric mono font centres cleanly. Icons/glyphs keep the
# Nerd font via widget_defaults.
text_font = "Liberation Mono"


# --- Vertical-bar widget variants -------------------------------------------
# qtile refuses to place a widget in a vertical bar unless it declares
# ORIENTATION_BOTH, and several widgets additionally size/centre themselves
# using bar.height (which is the full screen height in a vertical bar). The
# subclasses below add vertical support while behaving identically to their
# parents in a horizontal bar. The image-based ones mirror qtile's own
# widget.Image (resize by bar.width / blit with height= when vertical).
# These copy/adapt qtile-internal methods; tested against qtile 0.31.0 —
# re-verify them after a qtile upgrade.

class VolumeVertical(widget.Volume):
    """Volume widget (text mode) that is also allowed in a vertical bar."""

    orientations = base.ORIENTATION_BOTH


class CurrentLayoutIconVertical(widget.CurrentLayoutIcon):
    """CurrentLayoutIcon that sizes/centres its icon for a vertical bar."""

    orientations = base.ORIENTATION_BOTH

    def _setup_images(self):
        for names in self._get_layout_names():
            layout_name = names[0]
            layouts = dict.fromkeys(names)
            for layout_cls in layouts.keys():
                icon_file_path = self.find_icon_file_path(layout_cls)
                if icon_file_path:
                    break
            else:
                logger.warning('No icon found for layout "%s"', layout_name)
                icon_file_path = self.find_icon_file_path("unknown")

            img = Img.from_path(icon_file_path)
            if self.bar.horizontal:
                img.resize(height=(self.bar.height - 2) * self.scale)
                if img.width > self.length:
                    self.length = img.width + self.actual_padding * 2
            else:
                img.resize(width=(self.bar.width - 2) * self.scale)
                if img.height > self.length:
                    self.length = img.height + self.actual_padding * 2

            self.surfaces[layout_name] = img

        self.icons_loaded = True

    def draw(self):
        if not self.icons_loaded:
            # Fallback to text (handled for both orientations by _TextBox).
            self.text = self.current_layout[0].upper()
            base._TextBox.draw(self)
            return

        try:
            surface = self.surfaces[self.current_layout]
        except KeyError:
            logger.error("No icon for layout %s", self.current_layout)
            return

        self.drawer.clear(self.background or self.bar.background)
        self.drawer.ctx.save()
        self.drawer.ctx.translate(
            (self.width - surface.width) / 2,
            (self.height - surface.height) / 2,
        )
        self.drawer.ctx.set_source(surface.pattern)
        self.drawer.ctx.paint()
        self.drawer.ctx.restore()

        if self.bar.horizontal:
            self.drawer.draw(offsetx=self.offset, offsety=self.offsety, width=self.length)
        else:
            self.drawer.draw(offsety=self.offset, offsetx=self.offsetx, height=self.length)


class BatteryIconVertical(widget.BatteryIcon):
    """BatteryIcon that sizes/centres its icon for a vertical bar, and hides
    itself (zero length) unless the battery is charging or discharging."""

    orientations = base.ORIENTATION_BOTH
    _show = False  # updated per-poll; collapses to zero length when False

    def update(self):
        status = self._battery.update_status()
        show = status.state in (BatteryState.CHARGING, BatteryState.DISCHARGING)
        icon = self._get_icon_key(status)
        if show != self._show or icon != self.current_icon:
            self._show = show
            self.current_icon = icon
            self.bar.draw()  # length may have changed -> relayout the bar

    def setup_images(self):
        d_imgs = images.Loader(self.theme_path)(*self.icon_names)
        for key, img in d_imgs.items():
            if self.bar.horizontal:
                img.resize(height=self.bar.height * self.scale)
            else:
                img.resize(width=self.bar.width * self.scale)
            self.images[key] = img

    def calculate_length(self):
        if not self._show or not self.images:
            return 0
        icon = self.images[self.current_icon]
        if self.bar.horizontal:
            return icon.width + 2 * self.padding
        return icon.height + 2 * self.padding

    def draw(self):
        self.drawer.clear(self.background or self.bar.background)
        if self._show:
            image = self.images[self.current_icon]
            self.drawer.ctx.save()
            if self.bar.horizontal:
                self.drawer.ctx.translate(self.padding, (self.bar.height - image.height) // 2)
            else:
                self.drawer.ctx.translate((self.bar.width - image.width) // 2, self.padding)
            self.drawer.ctx.set_source(image.pattern)
            self.drawer.ctx.paint()
            self.drawer.ctx.restore()
        if self.bar.horizontal:
            self.drawer.draw(offsetx=self.offset, offsety=self.offsety, width=self.length)
        else:
            self.drawer.draw(offsety=self.offset, offsetx=self.offsetx, height=self.length)


class BatteryHideIdle(widget.Battery):
    """Battery text shown only while charging or discharging; hidden otherwise.
    Returning '' makes a text widget collapse to zero length."""

    def poll(self):
        if self._battery.update_status().state not in (
            BatteryState.CHARGING,
            BatteryState.DISCHARGING,
        ):
            return ""
        return super().poll()


class GroupBoxVertical(widget.GroupBox):
    """GroupBox that stacks its group labels vertically for a vertical bar."""

    orientations = base.ORIENTATION_BOTH

    def box_height(self, groups):
        _, height = self.drawer.max_layout_size(
            [self.fmt.format(i.label) for i in groups],
            self.font,
            self.fontsize,
            self.markup,
        )
        return height + self.padding_y * 2 + self.borderwidth * 2

    def calculate_length(self):
        if self.bar.horizontal:
            return super().calculate_length()
        height = self.margin_y * 2 + (len(self.groups) - 1) * self.spacing
        for g in self.groups:
            height += self.box_height([g])
        return height

    def button_press(self, x, y, button):
        if self.bar.horizontal:
            return super().button_press(x, y, button)
        # Hit-test along the y-axis for a vertical stack.
        self.click = y
        base._Widget.button_press(self, x, y, button)

    def get_clicked_group(self):
        if self.bar.horizontal:
            return super().get_clicked_group()
        group = None
        new_height = self.margin_y - self.spacing / 2.0
        height = 0
        for g in self.groups:
            new_height += self.box_height([g]) + self.spacing
            if height <= self.click <= new_height:
                group = g
                break
            height = new_height
        return group

    def drawbox(
        self,
        offset,
        text,
        bordercolor,
        textcolor,
        highlight_color=None,
        width=None,
        rounded=False,
        block=False,
        line=False,
        highlighted=False,
    ):
        if self.bar.horizontal:
            return super().drawbox(
                offset,
                text,
                bordercolor,
                textcolor,
                highlight_color,
                width,
                rounded,
                block,
                line,
                highlighted,
            )

        self.layout.text = self.fmt.format(text)
        self.layout.font_family = self.font
        self.layout.font_size = self.fontsize
        self.layout.colour = textcolor
        # Draw each group as a square box: the layout width is forced to match
        # the box height (capped to the bar width), and the label is centred
        # inside it (TextLayout uses ALIGN_CENTER). This makes the active-group
        # highlight a symmetric square centred across the bar, not a thin sliver.
        side = min(self.layout.height + self.padding_y * 2, self.bar.width - 2)
        self.layout.width = side

        if bordercolor is None:
            border_width = 0
            framecolor = self.background or self.bar.background
        else:
            border_width = self.borderwidth
            framecolor = bordercolor

        framed = self.layout.framed(border_width, framecolor, 0, self.padding_y, highlight_color)
        # Centre each box across the bar's width; stack down the y-axis.
        x = (self.bar.width - framed.width) / 2
        if block and bordercolor is not None:
            framed.draw_fill(x, offset, rounded)
        elif line:
            framed.draw_line(x, offset, highlighted)
        else:
            framed.draw(x, offset, rounded)

    def draw(self):
        if self.bar.horizontal:
            return super().draw()

        self.drawer.clear(self.background or self.bar.background)

        offset = self.margin_y
        for i, g in enumerate(self.groups):
            to_highlight = False
            is_block = self.highlight_method == "block"
            is_line = self.highlight_method == "line"

            bh = self.box_height([g])

            if self.group_has_urgent(g) and self.urgent_alert_method == "text":
                text_color = self.urgent_text
            elif g.windows:
                text_color = self.active
            else:
                text_color = self.inactive

            if g.screen:
                if self.highlight_method == "text":
                    border = None
                    text_color = self.this_current_screen_border
                else:
                    if self.block_highlight_text_color:
                        text_color = self.block_highlight_text_color
                    if self.bar.screen.group.name == g.name:
                        if self.qtile.current_screen == self.bar.screen:
                            border = self.this_current_screen_border
                            to_highlight = True
                        else:
                            border = self.this_screen_border
                    else:
                        if self.qtile.current_screen == g.screen:
                            border = self.other_current_screen_border
                        else:
                            border = self.other_screen_border
            elif self.group_has_urgent(g) and self.urgent_alert_method in (
                "border",
                "block",
                "line",
            ):
                border = self.urgent_border
                if self.urgent_alert_method == "block":
                    is_block = True
                elif self.urgent_alert_method == "line":
                    is_line = True
            else:
                border = None

            self.drawbox(
                offset,
                g.label,
                border,
                text_color,
                highlight_color=self.highlight_color,
                width=None,  # ignored by the vertical drawbox (it computes a square)
                rounded=self.rounded,
                block=is_block,
                line=is_line,
                highlighted=to_highlight,
            )
            offset += bh + self.spacing
        self.drawer.draw(offsety=self.offset, offsetx=self.offsetx, height=self.length)


screens = [
    Screen(
        left=bar.Bar(
            [                		
                # Layout Icon
                CurrentLayoutIconVertical(padding = 4, scale = 0.6, background = colors[0]),
                widget.Sep(linewidth = 0, padding = seperator_padding, background = colors[0]),
                widget.CurrentLayout(font = text_font, foreground = colors[2], background = colors[0]),
                widget.Sep(linewidth = 0, padding = seperator_padding, background = colors[0]),

                # Groupbox
                GroupBoxVertical(
                    active = colors[2],                       # white text, groups with windows
                    inactive = colors[7],                     # dim grey, readable inactive text
                    background = colors[0],
                    highlight_method = "border",
                    this_current_screen_border = colors[2],   # white border = active workspace
                    this_screen_border = colors[2],
                    other_current_screen_border = colors[7],
                    other_screen_border = colors[7],
                    urgent_border = colors[3],
                    borderwidth = 2,
                    rounded = True,
                    disable_drag = True,
                ),
                widget.Sep(linewidth = 0, padding = seperator_padding, background = colors[0]),
                
                # # Prompt
                # widget.Sep(linewidth = 0, padding = 5, foreground = colors[2], background = colors[0]),
                # widget.Prompt(),
                
                # Window Name (stretches to fill; reads from the bottom up)
                widget.WindowName(font = text_font, foreground = colors[2], background = colors[0]),
                widget.Sep(linewidth = 0, padding = 8, background = colors[0]),
            ],
            bar_size,
            opacity=0.85
        ),
        # Right vertical bar: everything else, anchored to the bottom.
        right=bar.Bar(
            [
                widget.Chord(
                    chords_colors={
                        'launch': (RED, FG),
                    },
                    name_transform=lambda name: name.upper(),
                ),

                # Push the system widgets + clock to the bottom of the bar
                widget.Spacer(background = colors[0]),

                # Temperature
                widget.TextBox(text = " 󰈸 ", padding = widget_padding, background = colors[1], fontsize = icon_font_size),
                widget.ThermalSensor(font = text_font, tag_sensor='edge', format='APU:{temp:.0f}{unit}', foreground = colors[2], background = colors[1], threshold = 90, padding = widget_padding),
                widget.Sep(linewidth = 0, padding = seperator_padding, foreground = colors[2], background = colors[1]),

                # CPU
                widget.Sep(linewidth = 0, padding = seperator_padding, foreground = colors[2], background = colors[0]),
                widget.TextBox(text = " ", padding = widget_padding, background = colors[0], fontsize = icon_font_size, mouse_callbacks = {'Button1': open_bpytop}),
                widget.Sep(linewidth = 0, padding = seperator_padding, foreground = colors[2], background = colors[0]),
                widget.CPU(font = text_font, foreground = colors[2], background = colors[0], mouse_callbacks = {'Button1': open_bpytop}),
                widget.Sep(linewidth = 0, padding = seperator_padding, foreground = colors[2], background = colors[0]),

                # Ram
                widget.Sep(linewidth = 0, padding = seperator_padding, background = colors[1]),
                widget.TextBox(text = " ", background = colors[1], padding = widget_padding, fontsize = icon_font_size, mouse_callbacks = {'Button1': open_bpytop}),
                widget.Sep(linewidth = 0, padding = seperator_padding, background = colors[1]),
                widget.Memory(font = text_font, format = '{MemUsed:.0f}{mm}/{MemTotal:.0f}{mm}', measure_mem='G', foreground = colors[2], background = colors[1], mouse_callbacks = {'Button1': open_bpytop}),
                widget.Sep(linewidth = 0, padding = seperator_padding, background = colors[1]),

                # Disk
                widget.Sep(linewidth = 0, padding = seperator_padding, foreground = colors[2], background = colors[0]),
                widget.TextBox(text = " ", background = colors[0], padding = widget_padding, fontsize = icon_font_size, mouse_callbacks = {'Button1': open_bpytop}),
                widget.Sep(linewidth = 0, padding = seperator_padding, foreground = colors[2], background = colors[0]),
                widget.DF(font = text_font, format = '{p}({uf}{m}|{r:.0f}%)', visible_on_warn=False, foreground = colors[2], background = colors[0], mouse_callbacks = {'Button1': open_bpytop}),
                widget.Sep(linewidth = 0, padding = seperator_padding, foreground = colors[2], background = colors[0]),

                # Volume
                widget.Sep(linewidth = 0, padding = seperator_padding, background = colors[1]),
                widget.TextBox(text = "󰕾 ", background = colors[1], mouse_callbacks = {'Button1': lambda : qtile.spawn(myTerm+" --disable-server -e alsamixer")}, padding = widget_padding, fontsize = icon_font_size),
                widget.Sep(linewidth = 0, padding = seperator_padding, background = colors[1]),
                VolumeVertical(
                font = text_font,
                fmt = '{} ',
                foreground = colors[2],
                background = colors[1],
                padding = widget_padding,
                # mute_command = 'amixer -D pipewire sset Master toggle'.split(),
                # volume_up_command = 'amixer -D pipewire sset Master 1%+'.split(),
                # volume_down_command = 'amixer -D pipewire sset Master 1%-'.split(),
                get_volume_command = 'amixer -D pipewire get Master'.split()),

                # Battery (icon + %; the whole block hides unless charging or
                # discharging — separators folded into widget padding so it
                # collapses cleanly, leaving no gap when hidden)
                BatteryIconVertical(foreground = colors[2], background = colors[0], padding = 3, update_interval = 5),
                BatteryHideIdle(foreground = colors[2], background = colors[0], padding = 3, charge_char='󰛃', discharge_char='󰛀', full_char='󱊣', notify_below=5, update_interval = 5),

                # Wallpaper
                widget.Sep(linewidth = 0, padding = seperator_padding, background = colors[1]),
                widget.Wallpaper(directory='~/Pictures/Wallpapers/', random_selection=True, wallpaper_command=['feh', '--bg-fill'], label=' ', fontsize = icon_font_size, background = colors[1]),
                # widget.Wallpaper(directory='~/Pictures/Wallpapers/', random_selection=True, wallpaper_command=['xwallpaper', '--zoom']),
                widget.Sep(linewidth = 0, padding = seperator_padding, background = colors[1]),

                # TODO
                # widget.Bluethooth(),
                # widget.CapsNumLockIndicator(),
                # widget.CPUGraph(),
                # widget.KhalCalendar(),
                # widget.MemoryGraph(),
                
                # System Tray
                widget.Sep(linewidth = 0, padding = seperator_padding, foreground = colors[2], background = colors[0]),
                widget.Systray(foreground = colors[2], background = colors[0]),
                widget.Sep(linewidth = 0, padding = seperator_padding, foreground = colors[2], background = colors[0]),

                # Clock
                widget.Clock(font = text_font, format='%d-%m-%Y %a %I:%M %p', foreground = colors[2], background = colors[0], mouse_callbacks = {'Button1': lambda : qtile.spawn(myTerm+""" --disable-server -e "sh -c 'sleep 0.1 && calcurse'" """)}),
                widget.Sep(linewidth = 0, padding = seperator_padding, foreground = colors[2], background = colors[0]),

            ],
            bar_size,
            opacity=0.85
        ),
    ),
]

# Drag floating layouts.
mouse = [
    Drag([mod], "Button1", lazy.window.set_position_floating(), start=lazy.window.get_position()),
    Drag([mod], "Button3", lazy.window.set_size_floating(), start=lazy.window.get_size()),
    Click([mod], "Button2", lazy.window.bring_to_front())
]

dgroups_key_binder = None
dgroups_app_rules = []  # type: List
follow_mouse_focus = True
bring_front_click = False
cursor_warp = False

floating_layout = layout.Floating(
    border_focus = CYAN,
    border_normal = GREY_DK,
    border_width = 2,
    # Leave notification windows where xfce4-notifyd puts them. Otherwise qtile
    # re-centres any floating window whose edge falls in a bar's reserved zone,
    # which sends right-anchored notifications to the middle of the screen.
    no_reposition_rules=[Match(wm_class="xfce4-notifyd")],
    float_rules=[
    # Run the utility of `xprop` to see the wm class and name of an X client.
    *layout.Floating.default_float_rules,
    Match(wm_class='confirmreset'),  # gitk
    Match(wm_class='makebranch'),  # gitk
    Match(wm_class='maketag'),  # gitk
    Match(wm_class='ssh-askpass'),  # ssh-askpass
    Match(title='branchdialog'),  # gitk
    Match(title='pinentry'),  # GPG key password entry
])

auto_fullscreen = True
focus_on_window_activation = "smart"
reconfigure_screens = True

# If things like steam games want to auto-minimize themselves when losing
# focus, should we respect this or not?
auto_minimize = True

@hook.subscribe.startup_once
def start_once():
    processes = [
        f"/opt/anaconda3/envs/xonsh/bin/python /opt/anaconda3/envs/xonsh/bin/xonsh {myhome}/.local/bin/audio-play {myhome}/.local/bin/oxygen-sound-theme/Oxygen-Sys-Log-In.ogg".split(),
        "/usr/local/bin/greenclip daemon".split(),
        ["qtile", "run-cmd", "--group", "2", f"{myTerm}", "--disable-server", "-e", "/opt/anaconda3/envs/xonsh/bin/python /opt/anaconda3/envs/xonsh/bin/xonsh"],
        ["qtile", "run-cmd", "--group", "4", f"{myTerm}", "--disable-server", "-e", f"/opt/anaconda3/envs/xonsh/bin/python /opt/anaconda3/envs/xonsh/bin/xonsh /usr/local/bin/ranger-open"],
        "/usr/bin/syncthing serve --no-browser --logfile=default".split(),
        f"qtile run-cmd --group 1 {myTerm} --disable-server -e cmus".split(),
        myBrowser.split(),
        #"flatpak run fr.handbrake.ghb".split(),
        "flatpak run org.mozilla.thunderbird_esr".split(),
        "flatpak run org.ferdium.Ferdium".split(),
        "transmission-gtk",
    ]

    for p in processes:
        subprocess.Popen(p)

@hook.subscribe.client_new
def disable_floating(window):
    rules = [
        Match(wm_class="mpv")
    ]

    if any(window.match(rule) for rule in rules):
        window.togroup(qtile.current_group.name)
        window.disable_floating()

# XXX: Gasp! We're lying here. In fact, nobody really uses or cares about this
# string besides java UI toolkits; you can see several discussions on the
# mailing lists, GitHub issues, and other WM documentation that suggest setting
# this string if your java app doesn't work correctly. We may as well just lie
# and say that we're a working one by default.
#
# We choose LG3D to maximize irony: it is a 3D non-reparenting WM written in
# java that happens to be on java's whitelist.
wmname = "LG3D"
