# Copyright (c) 2010 Aldo Cortesi
# Copyright (c) 2010, 2014 dequis
# Copyright (c) 2012 Randall Ma
# Copyright (c) 2012-2014 Tycho Andersen
# Copyright (c) 2012 Craig Barnes
# Copyright (c) 2013 horsik
# Copyright (c) 2013 Tao Sauvage
#
# Permission is hereby granted, free of charge, to any person obtaining a copy
# of this software and associated documentation files (the "Software"), to deal
# in the Software without restriction, including without limitation the rights
# to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
# copies of the Software, and to permit persons to whom the Software is
# furnished to do so, subject to the following conditions:
#
# The above copyright notice and this permission notice shall be included in
# all copies or substantial portions of the Software.
#
# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
# IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
# FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
# AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
# LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
# OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
# SOFTWARE.

from typing import List  # noqa: F401

import os
import re
import socket
import subprocess

from libqtile import bar, layout, widget, hook
from libqtile.config import Click, Drag, Group, Key, Match, Screen
from libqtile.lazy import lazy
from libqtile.utils import guess_terminal


mod = "mod4"
alt = "mod1"
mod2 = "control"
mod3 = "shift"

# terminal = guess_terminal()
myTerm = "alacritty"	# My terminal of choice
myBrowser = "firefox"	# My browser of choice

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
    Key([mod, "control"], "q", lazy.shutdown(), desc="Shutdown Qtile"),
	Key([mod], "c", lazy.spawncmd(), desc="Spawn a command using a prompt widget"),
	
	# My shortcuts
	Key([mod], "t", lazy.spawn(myTerm+" -e /home/manuj/anaconda3/envs/xonsh/bin/xonsh"), desc="Launch terminal"),
    # Key([mod], "Return", lazy.spawn(myTerm+" -e /home/manuj/Bin/ranger-open"), desc="Launch Ranger"),	
    Key([mod], "Return", lazy.spawn(myTerm+" -e /home/manuj/Bin/ranger-open-beta"), desc="Launch Ranger"),	
	Key([mod], "b", lazy.spawn(myBrowser), desc='My Browser' ),

    ## Rofi
    Key([mod], "r", lazy.spawn("rofi -show drun -show-icons"), desc='Run Rofi Application Launcher'),
    Key([alt], "Tab", lazy.spawn("rofi -show window"), desc='Run Rofi Window Switcher'),
    Key([mod], "e", lazy.spawn("/home/manuj/anaconda3/envs/util/bin/rofimoji --action copy --skin-tone 'moderate'"), desc='Run Rofi emoji picker'),
	
	Key([mod], "m", lazy.layout.maximize(), desc='Toggle window between minimum and maximum sizes'),
	Key([mod, "shift"], "f", lazy.window.toggle_floating(), desc='Toggle floating'),
	Key([mod], "f", lazy.window.toggle_fullscreen(), desc='Toggle fullscreen'),
	
	Key([], "XF86AudioMute", lazy.spawn("amixer -D pipewire sset Master toggle")),
    Key([], "XF86AudioLowerVolume", lazy.spawn("amixer -D pipewire sset Master 1%-")),
    Key([], "XF86AudioRaiseVolume", lazy.spawn("amixer -D pipewire sset Master 1%+")),

    Key([mod2, mod3], "a", lazy.spawn(myTerm+" -e sh -c 'sleep 0.1 && nvim /home/manuj/Productivity_System/TODO.txt'"), desc="Launch TODO List"),
    Key([mod2, mod3], "y", lazy.spawn(myTerm+" -e sh -c 'sleep 0.1 && nvim /home/manuj/Backups/youtube.txt'"), desc="Launch TODO List"),
]

groups = [Group(i) for i in "123456789"]

for i in groups:
    keys.extend([
        # mod1 + letter of group = switch to group
        Key([mod], i.name, lazy.group[i.name].toscreen(), desc="Switch to group {}".format(i.name)),

        # mod1 + shift + letter of group = switch to & move focused window to group
        Key([mod, "shift"], i.name, lazy.window.togroup(i.name, switch_group=True), desc="Switch to & move focused window to group {}".format(i.name)),
        # Or, use below if you prefer not to switch to that group.
        # # mod1 + shift + letter of group = move focused window to group
        # Key([mod, "shift"], i.name, lazy.window.togroup(i.name),
        #     desc="move focused window to group {}".format(i.name)),
    ])

layout_theme = {"border_width": 2, # Window lighlight width
                "margin": 1, # Gap wetween windows
                "border_focus": "#4f76c7",
                "border_normal": "#1D2330"
                }

layouts = [
    # layout.Columns(border_focus_stack=['#d75f5f', '#8f3d3d'], border_width=4),
    layout.Bsp(**layout_theme),
    # Try more layouts by unleashing below layouts.
    # layout.Stack(num_stacks=2),
    layout.Max(),
    # layout.Matrix(),
    # layout.MonadTall(),
    # layout.MonadWide(),
    # layout.RatioTile(),
    # layout.Tile(),
    layout.TreeTab(),
    # layout.VerticalTile(),
    # layout.Zoomy(**layout_theme),
]

colors = [["#272822", "#272822"], # panel background
          ["#3d3f4b", "#434758"], # background for current screen tab
          ["#f8f8f2", "#f8f8f2"], # font color for group names
          ["#f92672", "#f92672"], # border line color for current tab
          ["#74438f", "#74438f"], # border line color for 'other tabs' and color for 'odd widgets'
          ["#4f76c7", "#4f76c7"], # color for the 'even widgets'
          ["#e1acff", "#e1acff"], # window name
          ["#ecbbfb", "#ecbbfb"]] # backbround for inactive screens

widget_defaults = dict(
    font='sans',
    fontsize=12,
    padding=3,
    # background=colors[2]
)
extension_defaults = widget_defaults.copy()

def open_terminal():
	qtile.cmd_spawn('alacritty')

padding = 5
icon_font_size = 15
bar_size = 24

screens = [
    Screen(
        top=bar.Bar(
            [
                # Logo
				# widget.Sep(linewidth = 0, padding = 5, foreground = colors[2], background = colors[0]),
				# widget.Image(filename = "~/.config/qtile/icons/python-white.png", scale = "False", foreground = colors[2], background = colors[0], mouse_callbacks = {'Button1': open_terminal}),
				
                # Layout Icon
                # widget.Sep(linewidth = 0, padding = 5, foreground = colors[2], background = colors[0]),
				widget.CurrentLayoutIcon(padding = padding, scale = 0.9, background = colors[0]),
                widget.CurrentLayout(foreground = colors[2], background = colors[0]),
                
                # Groupbox
                widget.Sep(linewidth = 0, padding = padding, background = colors[0]),
                widget.GroupBox(active = colors[2], background = colors[0], inactive = colors[1], disable_drag = True),
                
                # # Prompt
                # widget.Sep(linewidth = 0, padding = 5, foreground = colors[2], background = colors[0]),
                # widget.Prompt(),
                
                # Window Name
                widget.Sep(linewidth = 0, padding = padding, background = colors[0]),
                widget.WindowName(foreground = colors[2], background = colors[0]),
                
                # Unknown
                widget.Sep(linewidth = 0, padding = padding, background = colors[0]),
                
                widget.Chord(
                    chords_colors={
                        'launch': ("#f92672", "#f8f8f2"),
                    },
                    name_transform=lambda name: name.upper(),
                ),
				
                # Temperature
                widget.TextBox(text = "🔥", padding = padding, background = colors[0], fontsize = icon_font_size),
				widget.ThermalSensor(foreground = colors[2], background = colors[0], threshold = 90, padding = 2),
                widget.Sep(linewidth = 0, padding = padding, foreground = colors[2], background = colors[0]),
				
                # Ram
                widget.TextBox(text = "💾", background = colors[1], padding = padding, fontsize = icon_font_size),
				# TODO Install bpytop and open it when clicked
				# widget.Memory(foreground = colors[2], background = colors[4], mouse_callbacks = {'Button1': lambda: qtile.cmd_spawn(myTerm + ' -e bpytop')}, padding = 5),
				widget.Memory(format = '{MemUsed: .0f}{mm}/{MemTotal: .0f}{mm}', measure_mem='G', foreground = colors[2], background = colors[1]),
                widget.Sep(linewidth = 0, padding = padding, background = colors[1]),

                # Disk
                widget.TextBox(text = "💻", background = colors[0], padding = padding, fontsize = icon_font_size),
				widget.DF(format = '{p} ({uf}{m}|{r:.0f}%)', visible_on_warn=False, foreground = colors[2], background = colors[0]),
                widget.Sep(linewidth = 0, padding = padding, foreground = colors[2], background = colors[0]),

                # Volume
				widget.TextBox( text = "📢", background = colors[1],  mouse_callbacks = {'Button1': lambda qtile: qtile.cmd_spawn(myTerm + ' -e pulsemixer')}, padding = 5, fontsize = icon_font_size),
                widget.Volume(
                fmt = '{} ',
                foreground = colors[2],
                background = colors[1],
                padding = 5,
                # mute_command = 'amixer -D pipewire sset Master toggle'.split(),
                # volume_up_command = 'amixer -D pipewire sset Master 1%+'.split(),
                # volume_down_command = 'amixer -D pipewire sset Master 1%-'.split(),
                get_volume_command = 'amixer -D pipewire get Master'.split()),
                widget.Sep(linewidth = 0, padding = padding, background = colors[1]),

                # Battery
                # widget.TextBox(text = "🔋", foreground = colors[2],background = colors[4],  mouse_callbacks = {'Button1': lambda qtile: qtile.cmd_spawn(myTerm + ' -e pulsemixer')}, padding = 5, fontsize = icon_font_size),
                widget.BatteryIcon(foreground = colors[2],background = colors[0], padding = padding),
                widget.Battery(foreground = colors[2],background = colors[0], padding = padding, charge_char='⏫', discharge_char='⏬', full_char='🔋', notify_below=5),
                # widget.Sep(linewidth = 0, padding = 5, foreground = colors[2], background = colors[0]),

                # Wallpaper
                # widget.Wallpaper(directory='~/Pictures/Wallpapers/', random_selection=True, wallpaper_command=['xwallpaper', '--zoom']),
                widget.Wallpaper(directory='~/Pictures/Wallpapers/', random_selection=True, wallpaper_command=['feh', '--bg-fill'], label=' 🎨 ', fontsize = icon_font_size, background = colors[1]),
                
                # TODO
                # widget.Bluethooth(),
                # widget.CapsNumLockIndicator(),
                # widget.CPU(),
                # widget.CPUGraph(),
                # widget.KhalCalendar(),
                # widget.MemoryGraph(),
                
                # System Tray
                widget.Systray(foreground = colors[2], background = colors[0]),

                # Clock
                widget.Clock(format='%d-%m-%Y %a %I:%M %p', foreground = colors[2], background = colors[0]),
                
                # Logout
                # widget.QuickExit(),
            ],
            bar_size,
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

floating_layout = layout.Floating(float_rules=[
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
    home = os.path.expanduser('~')
    # subprocess.call([home + '/.config/qtile/autostart.sh']) TODO Run startup script

# XXX: Gasp! We're lying here. In fact, nobody really uses or cares about this
# string besides java UI toolkits; you can see several discussions on the
# mailing lists, GitHub issues, and other WM documentation that suggest setting
# this string if your java app doesn't work correctly. We may as well just lie
# and say that we're a working one by default.
#
# We choose LG3D to maximize irony: it is a 3D non-reparenting WM written in
# java that happens to be on java's whitelist.
wmname = "LG3D"
