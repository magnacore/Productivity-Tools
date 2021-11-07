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
mod1 = "alt"
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
	Key([mod], "r", lazy.spawncmd(), desc="Spawn a command using a prompt widget"),
	
	# My shortcuts
	Key([mod], "t", lazy.spawn(myTerm+" -e /home/manuj/anaconda3/envs/xonsh/bin/xonsh"), desc="Launch terminal"),
    Key([mod], "Return", lazy.spawn(myTerm+" -e /home/manuj/Bin/ranger-open"), desc="Launch terminal"),
    # Key([mod], "Return", lazy.spawn(myTerm), desc="Launch terminal"),
	# TODO Key([mod, "shift"], "Return", lazy.spawn("rofi -p 'Run: '"), desc='Run Launcher'),
	Key([mod], "b", lazy.spawn(myBrowser), desc='My Browser' ),
	
	Key([mod], "m", lazy.layout.maximize(), desc='toggle window between minimum and maximum sizes'),
	Key([mod, "shift"], "f", lazy.window.toggle_floating(), desc='toggle floating'),
	Key([mod], "f", lazy.window.toggle_fullscreen(), desc='toggle fullscreen'),
	
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

layout_theme = {"border_width": 2,
                "margin": 1,
                "border_focus": "e1acff",
                "border_normal": "1D2330"
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

colors = [["#282c34", "#282c34"], # panel background
          ["#3d3f4b", "#434758"], # background for current screen tab
          ["#ffffff", "#ffffff"], # font color for group names
          ["#ff5555", "#ff5555"], # border line color for current tab
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


screens = [
    Screen(
        top=bar.Bar(
            [
                # Logo
				widget.Sep(linewidth = 0, padding = 6, foreground = colors[2], background = colors[0]),
				widget.Image(filename = "~/.config/qtile/icons/python-white.png", scale = "False", mouse_callbacks = {'Button1': open_terminal}),
				
                # Layout Icon
                widget.Sep(linewidth = 0, padding = 6, foreground = colors[2], background = colors[0]),
				widget.CurrentLayoutIcon(custom_icon_paths = [os.path.expanduser("~/.config/qtile/icons")], foreground = colors[0], background = colors[4], padding = 0, scale = 0.7),
                widget.CurrentLayout(),
                
                # Groupbox
                widget.Sep(linewidth = 0, padding = 6, foreground = colors[2], background = colors[0]),
                widget.GroupBox(),
                
                # Prompt
                widget.Sep(linewidth = 0, padding = 6, foreground = colors[2], background = colors[0]),
                widget.Prompt(),
                
                # Window Name
                widget.Sep(linewidth = 0, padding = 6, foreground = colors[2], background = colors[0]),
                widget.WindowName(),
                
                # Unknown
                widget.Sep(linewidth = 0, padding = 6, foreground = colors[2], background = colors[0]),
                widget.Chord(
                    chords_colors={
                        'launch': ("#ff0000", "#ffffff"),
                    },
                    name_transform=lambda name: name.upper(),
                ),
				
                # Temperature
                widget.TextBox(text = " 🌡", padding = 2, foreground = colors[2], background = colors[4], fontsize = 11),
				widget.ThermalSensor(foreground = colors[2],background = colors[4], threshold = 90, padding = 5),
				
                # Ram
                widget.TextBox(text = " 🖬", foreground = colors[2], background = colors[4], padding = 0, fontsize = 14),
				# TODO Install bpytop and open it when clicked
				# widget.Memory(foreground = colors[2], background = colors[4], mouse_callbacks = {'Button1': lambda: qtile.cmd_spawn(myTerm + ' -e bpytop')}, padding = 5),
				widget.Memory(format = '{MemUsed: .0f}{mm}/{MemTotal: .0f}{mm}', measure_mem='G', update_interval = 1, fontsize = 14, foreground = colors[2], background = colors[4]),

                # Disk
                widget.TextBox(text = " 🖬", foreground = colors[2], background = colors[4], padding = 0, fontsize = 14),
				widget.DF(format = '{p} ({uf}{m}|{r:.0f}%)', visible_on_warn=False, fontsize = 14, foreground = colors[2], background = colors[4]),

                # Volume
				widget.TextBox( text = " 🔊", foreground = colors[5], background = colors[1],  mouse_callbacks = {'Button1': lambda qtile: qtile.cmd_spawn(myTerm + ' -e pulsemixer')}, padding = 0, fontsize = 12),
                widget.Volume(fontsize = 14, 
                fmt = '{} ', 
                foreground = colors[5], 
                background = colors[1], 
                padding = 5,
                # mute_command = 'amixer -D pipewire sset Master toggle'.split(),
                # volume_up_command = 'amixer -D pipewire sset Master 1%+'.split(),
                # volume_down_command = 'amixer -D pipewire sset Master 1%-'.split(),
                get_volume_command = 'amixer -D pipewire get Master'.split()),

                # Battery
                widget.TextBox(text = " 🌡", foreground = colors[2],background = colors[4],  mouse_callbacks = {'Button1': lambda qtile: qtile.cmd_spawn(myTerm + ' -e pulsemixer')}, padding = 0, fontsize = 12),
                widget.Battery(fontsize = 14, foreground = colors[2],background = colors[4], padding = 5, charge_char='C', discharge_char='D', full_char='F', notify_below=5),
                widget.BatteryIcon(foreground = colors[2],background = colors[4], padding = 5),

                # Wallpaper
                # widget.Wallpaper(directory='~/Pictures/Wallpapers/', random_selection=True, wallpaper_command=['xwallpaper', '--zoom']),
                widget.Wallpaper(directory='~/Pictures/Wallpapers/', random_selection=True, wallpaper_command=['feh', '--bg-fill'], label=' 🖬 '),
                
                # System Tray
                widget.Systray(),

                # Clock
                # widget.Clock(format='%Y-%m-%d %a %I:%M %p'),
                widget.Clock(format='%d-%m-%Y %a %I:%M %p'),
                
                # Logout
                # widget.QuickExit(),
                # widget.Image(filename = "~/.config/qtile/icons/python.png", scale = "False", mouse_callbacks = {'Button1': lambda: qtile.cmd_spawn(myTerm)}),
            ],
            24,
        ),
    ),
]

# Drag floating layouts.
mouse = [
    Drag([mod], "Button1", lazy.window.set_position_floating(),
         start=lazy.window.get_position()),
    Drag([mod], "Button3", lazy.window.set_size_floating(),
         start=lazy.window.get_size()),
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
