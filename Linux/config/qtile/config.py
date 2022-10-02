from typing import List  # noqa: F401

import os
import re
import socket
import subprocess

from libqtile import bar, layout, widget, hook, qtile
from libqtile.config import Click, Drag, Group, Key, Match, Screen
from libqtile.lazy import lazy
# from libqtile.utils import guess_terminal


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

    Key([mod], "m", lazy.layout.maximize(), desc='Toggle window between minimum and maximum sizes'),
	Key([mod, "shift"], "f", lazy.window.toggle_floating(), desc='Toggle floating'),
	Key([mod], "f", lazy.window.toggle_fullscreen(), desc='Toggle fullscreen'),
	
	# My shortcuts
	Key([mod], "t", lazy.spawn(myTerm+" -e /home/manuj/anaconda3/envs/xonsh/bin/xonsh"), desc="Launch terminal"),
    Key([mod, "shift"], "v", lazy.spawn(myTerm+" -e bash /home/manuj/Software/VVV-1.4.0-x86_64/vvv-start.sh"), desc="Launch VVV"),
    Key([mod], "Return", lazy.spawn(myTerm+" -e /home/manuj/Bin/ranger-open"), desc="Launch Ranger"),
	Key([mod], "b", lazy.spawn(myBrowser), desc='My Browser' ),
    Key([mod, "shift"], "c", lazy.spawn(myTerm+" -e flatpak run com.github.miguelmota.Cointop"), desc='Cointop' ),
    Key([mod], "d", lazy.spawn("/home/manuj/Bin/clipboard-convert-text"), desc="Save clipboard to text"),

    ## Rofi
    Key([mod], "r", lazy.spawn("rofi -show drun -show-icons"), desc='Run Rofi Application Launcher'),
    Key([alt], "Tab", lazy.spawn("rofi -show window"), desc='Run Rofi Window Switcher'),
    Key([mod], "e", lazy.spawn("/home/manuj/anaconda3/envs/util/bin/rofimoji --action copy --skin-tone 'moderate'"), desc='Run Rofi emoji picker'),
    Key([mod], "c", lazy.spawn("rofi -modi 'clipboard:~/.local/bin/greenclip print' -show clipboard -run-command '{cmd}'"), desc='Run Greenclip in Rofi'),
	
    ## Volume
	Key([], "XF86AudioMute", lazy.spawn("amixer -D pipewire sset Master toggle")),
    Key([], "XF86AudioLowerVolume", lazy.spawn("amixer -D pipewire sset Master 1%-")),
    Key([], "XF86AudioRaiseVolume", lazy.spawn("amixer -D pipewire sset Master 1%+")),

    ## Scratchpads
    Key([mod2, mod3], "a", lazy.spawn(myTerm+" -e sh -c 'sleep 0.1 && nvim /home/manuj/Productivity_System/TODO.txt'"), desc="Launch TODO List"),
    Key([mod2, mod3], "y", lazy.spawn(myTerm+" -e sh -c 'sleep 0.1 && nvim /home/manuj/Backups/youtube.txt'"), desc="Launch Youtube Download List"),
]

# Run xprop | grep WM_CLASS | awk '{print $4}' in terminal to find wm_class
groups = [Group("1", layout='treetab', matches=[Match(wm_class=["Ferdium", "Ghb", "Thunderbird", "Transmission-gtk"])]),
          Group("2", layout='bsp'),
          Group("3", layout='bsp', matches=[Match(wm_class=["Firefox"])]),
          Group("4", layout='max'),
          Group("5", layout='bsp'),
          Group("6", layout='bsp'),
          Group("7", layout='bsp'),
          Group("8", layout='bsp'),
          Group("9", layout='bsp'),
          Group("0", layout='floating')]
# groups = [Group(i) for i in "123456789"]

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
                "margin": 0, # Gap wetween windows
                "border_focus": "#4f76c7",
                "border_normal": "#3d3f4b"
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
    font='RobotoMono Nerd Font',
    fontsize=13,
    padding=0,
    # background=colors[2]
)
extension_defaults = widget_defaults.copy()

def open_bpytop():
	qtile.cmd_spawn(myTerm+" -e bpytop")

widget_padding = 0
seperator_padding = 5
icon_font_size = 15
bar_size = 24

screens = [
    Screen(
        top=bar.Bar(
            [                		
                # Layout Icon
                widget.CurrentLayoutIcon(padding = widget_padding, scale = 0.9, background = colors[0]),
                widget.Sep(linewidth = 0, padding = seperator_padding, background = colors[0]),
                widget.CurrentLayout(foreground = colors[2], background = colors[0]),
                
                # Groupbox
                widget.GroupBox(active = colors[2], background = colors[0], inactive = colors[1], disable_drag = True),
                
                # # Prompt
                # widget.Sep(linewidth = 0, padding = 5, foreground = colors[2], background = colors[0]),
                # widget.Prompt(),
                
                # Window Name
                widget.WindowName(foreground = colors[2], background = colors[0]),
                
                widget.Chord(
                    chords_colors={
                        'launch': ("#f92672", "#f8f8f2"),
                    },
                    name_transform=lambda name: name.upper(),
                ),
				
                # Temperature
                widget.TextBox(text = "🌡️", padding = widget_padding, background = colors[1], fontsize = icon_font_size),
				widget.ThermalSensor(foreground = colors[2], background = colors[1], threshold = 90, padding = widget_padding),
                widget.Sep(linewidth = 0, padding = seperator_padding, foreground = colors[2], background = colors[1]),

                # CPU
                widget.Sep(linewidth = 0, padding = seperator_padding, foreground = colors[2], background = colors[0]),
                widget.TextBox(text = "🧠", padding = widget_padding, background = colors[0], fontsize = icon_font_size, mouse_callbacks = {'Button1': open_bpytop}),
                widget.Sep(linewidth = 0, padding = seperator_padding, foreground = colors[2], background = colors[0]),
                widget.CPU(foreground = colors[2], background = colors[0], mouse_callbacks = {'Button1': open_bpytop}),
                widget.Sep(linewidth = 0, padding = seperator_padding, foreground = colors[2], background = colors[0]),
				
                # Ram
                widget.Sep(linewidth = 0, padding = seperator_padding, background = colors[1]),
                widget.TextBox(text = "🐏", background = colors[1], padding = widget_padding, fontsize = icon_font_size, mouse_callbacks = {'Button1': open_bpytop}),
				widget.Sep(linewidth = 0, padding = seperator_padding, background = colors[1]),
                widget.Memory(format = '{MemUsed:.0f}{mm}/{MemTotal:.0f}{mm}', measure_mem='G', foreground = colors[2], background = colors[1], mouse_callbacks = {'Button1': open_bpytop}),
                widget.Sep(linewidth = 0, padding = seperator_padding, background = colors[1]),

                # Disk
                widget.Sep(linewidth = 0, padding = seperator_padding, foreground = colors[2], background = colors[0]),
                widget.TextBox(text = "🗄️", background = colors[0], padding = widget_padding, fontsize = icon_font_size, mouse_callbacks = {'Button1': open_bpytop}),
				widget.Sep(linewidth = 0, padding = seperator_padding, foreground = colors[2], background = colors[0]),
                widget.DF(format = '{p}({uf}{m}|{r:.0f}%)', visible_on_warn=False, foreground = colors[2], background = colors[0], mouse_callbacks = {'Button1': open_bpytop}),
                widget.Sep(linewidth = 0, padding = seperator_padding, foreground = colors[2], background = colors[0]),

                # Volume
				widget.Sep(linewidth = 0, padding = seperator_padding, background = colors[1]),
                widget.TextBox(text = "🔊", background = colors[1], mouse_callbacks = {'Button1': lambda : qtile.cmd_spawn(myTerm+" -e alsamixer")}, padding = widget_padding, fontsize = icon_font_size),
                widget.Sep(linewidth = 0, padding = seperator_padding, background = colors[1]),
                widget.Volume(
                fmt = '{} ',
                foreground = colors[2],
                background = colors[1],
                padding = widget_padding,
                # mute_command = 'amixer -D pipewire sset Master toggle'.split(),
                # volume_up_command = 'amixer -D pipewire sset Master 1%+'.split(),
                # volume_down_command = 'amixer -D pipewire sset Master 1%-'.split(),
                get_volume_command = 'amixer -D pipewire get Master'.split()),

                # Battery
                widget.Sep(linewidth = 0, padding = 2, foreground = colors[2], background = colors[0]),
                widget.BatteryIcon(foreground = colors[2],background = colors[0], padding = widget_padding),
                widget.Sep(linewidth = 0, padding = seperator_padding, foreground = colors[2], background = colors[0]),
                widget.Battery(foreground = colors[2],background = colors[0], padding = widget_padding, charge_char='⏫', discharge_char='⏬', full_char='🔋', notify_below=5),
                widget.Sep(linewidth = 0, padding = 5, foreground = colors[2], background = colors[0]),

                # Wallpaper
                widget.Sep(linewidth = 0, padding = seperator_padding, background = colors[1]),
                widget.Wallpaper(directory='~/Pictures/Wallpapers/', random_selection=True, wallpaper_command=['feh', '--bg-fill'], label='🌄', fontsize = icon_font_size, background = colors[1]),
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
                widget.Clock(format='%d-%m-%Y %a %I:%M %p', foreground = colors[2], background = colors[0], mouse_callbacks = {'Button1': lambda : qtile.cmd_spawn(myTerm+" -e sh -c 'sleep 0.1 && calcurse'")}),
                widget.Sep(linewidth = 0, padding = seperator_padding, foreground = colors[2], background = colors[0]),

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
    processes = [
        [myBrowser],
        "flatpak run fr.handbrake.ghb".split(),
        "flatpak run org.mozilla.Thunderbird".split(),
        f"/home/manuj/anaconda3/envs/qtile/bin/qtile run-cmd --group 2 {myTerm} -e /home/manuj/anaconda3/envs/xonsh/bin/xonsh".split(),
        f"/home/manuj/anaconda3/envs/qtile/bin/qtile run-cmd --group 4 {myTerm} -e /home/manuj/Bin/ranger-open".split(),
        "/usr/bin/syncthing serve --no-browser --logfile=default".split(),
        f"/home/manuj/anaconda3/envs/qtile/bin/qtile run-cmd --group 1 {myTerm} -e cmus".split(),
        "/home/manuj/.local/bin/greenclip daemon".split(),
    ]

    for p in processes:
        subprocess.Popen(p)

# XXX: Gasp! We're lying here. In fact, nobody really uses or cares about this
# string besides java UI toolkits; you can see several discussions on the
# mailing lists, GitHub issues, and other WM documentation that suggest setting
# this string if your java app doesn't work correctly. We may as well just lie
# and say that we're a working one by default.
#
# We choose LG3D to maximize irony: it is a 3D non-reparenting WM written in
# java that happens to be on java's whitelist.
wmname = "LG3D"
