TODO:

delete clipman startup

nms and dragon need to be recompiled

zoxide version is too old : Zoxode conda with simlink in user local bin???

gpg folder structure setup

=====================================================================

- Install MX
compress=zstd,noatime,space_cache=v2,ssd,discard=async

MX Tweak > Config Options > disable single click
Appearance Theme Arc Dark, icons tango

=====================================================================

Install anaconda, do not use sudo
bash <Anaconda Installer>

sudo apt update
sudo apt upgrade

conda create --name qtile
conda create --name xonsh
conda create --name util
conda create --name gcp

Install pip in all environments
conda install -c anaconda pip

=====================================================================

QTILE:
The pip cache is cleared (remove ~/.cache/pip, if it exists)
pip install --no-cache-dir xcffib
pip install --no-cache-dir cairocffi
pip install --no-cache-dir qtile

Delete keyboard shortcut for window manager and keyboards

sudo apt install neovim

sudo nvim /etc/xdg/xfce4/xfconf/xfce-perchannel-xml/xfce4-session.xml

To make the change on a global scope for all users which do not have this setting already in there private xfce configuration, alter
This is the xfce4-session.xml that does not start the panel, desktop and wm. 3 lines each were deleted for each of them.

<?xml version="1.0" encoding="UTF-8"?>

<channel name="xfce4-session" version="1.0">
  <property name="general" type="empty">
    <property name="FailsafeSessionName" type="string" value="Failsafe"/>
    <property name="LockCommand" type="string" value=""/>
  </property>
  <property name="sessions" type="empty">
    <property name="Failsafe" type="empty">
      <property name="IsFailsafe" type="bool" value="true"/>
      <property name="Count" type="int" value="5"/>
      <property name="Client0_Priority" type="int" value="15"/>
      <property name="Client0_PerScreen" type="bool" value="false"/>
      <property name="Client1_Command" type="array">
        <value type="string" value="xfsettingsd"/>
      </property>
      <property name="Client1_Priority" type="int" value="20"/>
      <property name="Client1_PerScreen" type="bool" value="false"/>
      <property name="Client2_Priority" type="int" value="25"/>
      <property name="Client2_PerScreen" type="bool" value="false"/>
      <property name="Client3_Command" type="array">
        <value type="string" value="Thunar"/>
        <value type="string" value="--daemon"/>
      </property>
      <property name="Client3_Priority" type="int" value="30"/>
      <property name="Client3_PerScreen" type="bool" value="false"/>
      <property name="Client4_Priority" type="int" value="35"/>
      <property name="Client4_PerScreen" type="bool" value="false"/>
    </property>
  </property>
</channel>

copy qtile to
/home/mankind/anaconda3/envs/qtile/bin/

copy qtile-launch to
/usr/local/bin/
chmod 755 qtile-launch

QTile.desktop file (created once using Application startup GUI) was moved to
etc/xgd/autostart/
chmod 644 QTile.desktop

Flow:
qtile.desktop starts qtile-launch which is in /usr/local/bin and this in turn starts qtile. Make sure the permissions are correct else scripts will not launch

=====================================================================

in xfce keyboard, restore numlock on startup
session and startup : lock screen before sleep
Application autostart : disable clipman to start automatically

Copy terminal settings to home .config

copy .fonts
copy wallpapers

=====================================================================

Conda one time init

copy conda to
/home/$USER/anaconda3/bin/

copy conda-init to /usr/local/bin
chmod 755 conda-init

Conda.desktop - put in /etc/xgd/autostart - same place where we started qtile
chmod 644 Conda.desktop

=====================================================================

conda install -c conda-forge xonsh

Do not make Xonsh the default shell.

=====================================================================

copy software folder

Copy software
Move binaries from ~/Software/bin to ~/.local/bin

=====================================================================

# APT

sudo apt install ffmpeg fzf caca-utils highlight atool w3m w3m-img poppler-utils mediainfo mkvtoolnix mkvtoolnix-gui fd-find mlocate imagemagick libimage-magick-perl mpv syncthing ncdu bpytop virtualbox stacer obs-studio calibre ffmpegthumbnailer rofi xdotool xsel cmus sxiv zathura zathura-pdf-poppler zathura-djvu zathura-ps calcurse git bleachbit simplescreenrecorder feh pass pass-extension-otp zbar-tools pandoc tesseract-ocr ufw figlet vagrant cpu-x lzip build-essential zoxide trash-cli libx11-dev libxext-dev veracrypt clamav clamtk fonts-noto-color-emoji -y

VS Code:
installed using mx package manager popular applications
https://linuxize.com/post/how-to-install-visual-studio-code-on-debian-10/


=====================================================================

# FLATPAK
flatpak remote-add --if-not-exists flathub https://flathub.org/repo/flathub.flatpakrepo

flatpak install flathub com.github.tchx84.Flatseal org.keepassxc.KeePassXC org.signal.Signal fr.handbrake.ghb com.github.Murmele.Gittyup org.gimp.GIMP us.zoom.Zoom com.skype.Client com.github.xournalpp.xournalpp net.codeindustry.MasterPDFEditor org.ksnip.ksnip net.christianbeier.Gromit-MPX com.github.miguelmota.Cointop org.shotcut.Shotcut org.blender.Blender org.inkscape.Inkscape com.discordapp.Discord net.agalwood.Motrix io.lbry.lbry-app com.github.alexhuntley.Plots org.gaphor.Gaphor com.usebottles.bottles fyi.zoey.TeX-Match md.obsidian.Obsidian org.ferdium.Ferdium org.gnome.meld com.brave.Browser net.jami.Jami org.gnome.seahorse.Application com.valvesoftware.Steam -y

org.mozilla.Thunderbird is already installed as apt

=====================================================================

copy xnosh to /home/goldust/anaconda3/envs/xonsh/bin/

copy all configs to their respective folders

duplicate the python3 simlink in /usr/bin/ and rename it python

=====================================================================

# PIP/Conda Install

## util
### YoutubeDL using pip in conda environment util:
/home/manuj/anaconda3/envs/util/bin/pip install -U yt-dlp

/home/manuj/anaconda3/envs/util/bin/pip install pycp

conda install -c conda-forge pypdf2

/home/manuj/anaconda3/envs/util/bin/pip install rofimoji

/home/manuj/anaconda3/envs/util/bin/pip install ueberzug

conda install -c conda-forge go-ipfs
conda install -c conda-forge pyperclip

/home/manuj/anaconda3/envs/util/bin/pip install playsound

/home/manuj/anaconda3/envs/util/bin/pip install qrcode

conda install -c conda-forge libwebp

/home/manuj/anaconda3/envs/util/bin/pip3 install passphraseme

===

# qtile
For qtile memory module
conda install -c conda-forge psutil

===

# quant
conda create --name quant --clone base
If we get an error that package is corrupted, delete all files in the /home/manuj/anaconda3/pkgs folder and the quant env folder and retry.
/home/manuj/anaconda3/envs/quant/bin/pip install vectorbt
/home/manuj/anaconda3/envs/quant/bin/pip install pandas-ta
/home/manuj/anaconda3/envs/quant/bin/pip install nsepy

conda install -c anaconda pylint
conda install -c conda-forge black
pip install isort

===

# xonsh
/home/manuj/anaconda3/envs/xonsh/bin/pip install simple-term-menu
/home/manuj/anaconda3/envs/xonsh/bin/pip install nltk
num2words
/home/manuj/anaconda3/envs/util/bin/pip install playsound and pygobjects

===

# gcp
Install GCP via conda

=====================================================================

# Vim installation
install Vim Plug

sh -c 'curl -fLo "${XDG_DATA_HOME:-$HOME/.local/share}"/nvim/site/autoload/plug.vim --create-dirs \
       https://raw.githubusercontent.com/junegunn/vim-plug/master/plug.vim'

Create an empty file at /home/manuj/.config/nvim/init.vim
.vimrc from home automatically gets copied to it once we start nvim
:PlugInstall

To install vim-hexokinase go to
~/.vim/plugged/vim-hexokinas
change to go env from bash shell
make hexokinase

coc installation
cd ~/.vim/plugged/coc.nvim
sudo npm install -g yarn
yarn install
yarn build
/home/manuj/anaconda3/envs/quant/bin/pip install jedi (conda install -c conda-forge jedi)
/home/manuj/anaconda3/envs/quant/bin/pip install pynvim (:checkhealth to see any missing error)
from within nvim
:CocInstall coc-python (this may not be required since we included this in the vimrc)

To change the venv
:CocCommand then fuzzy search for python.setInterpreter and choose the venv.

=====================================================================

# Manual Software Installation

Move binaries from ~/Software/bin to ~/.local/bin

App image launcher was installed using rpm downloaded from github then double click a app image in thunar, it will ask for a directory to be set. Set to appimage directory. Integrate and run.

IPFS was installed using rpm downloaded from github page, but check there is appimage also and there may be other installers as well later.
sudo rpm -i sample_file.rpm

We can also download the IPFS Desktop appimage from github. Make is executable. run it ./<name>. AppImage launcher will ask for integration. Approve it and it will automatically move the appimage to ~/Applications. After that we can launch IPFS from rofi.

Hugo : Download the binary from github page and put it in ~/.local/bin

Java was already installed. Check if its working
sudo apt install java-1.8.0-openjdk.x86_64
sudo alternatives --config java

sudo apt install texlive-scheme-full???

setup firewall

disable hidden files in thunar config

Issues:
Flatpak not taking dark theme
2 apps in bin folder need to be rebuilt

=====================================================================

# Copy to Skel

recopy conda in anaconda as conda init will harcode it again
copy conda to
/home/$USER/anaconda3/bin/

Anaconda
Bin
Software
fonts
qtile
ranger
rofi
sxiv
xfce4 (with terminal)
zathura
greenclip
set thunar to list view and copy .config/thunar to skel
xonshrc
.vimrc
copy .firstlogin to /etc/skel/.firstlogin
.bash_aliases
.alacritty.yml
.xonshrc
.Xresources
mpv
gromit in .var

=====================================================================

# Before making ISO

- reorder the git repo as viperos folder
- create a script that will move all the files to the right folder
- set the correct permissions automatically
- flatpak update (flatpak is preinstalled - no need to activate it)
- update software
- clean anaconda pkgs?
- fedora-update change to debian-update
- test all productivity scripts

sudo apt update
sudo apt upgrade
sudo apt autoremove
sudo apt clean
flatpak uninstall --unused
conda clean --all

- Cleanup everything after install

======

Wiki

https://wiki.xfce.org/howto/other_window_manager

~/.config/autostart (user-specific)
/etc/xdg/autostart/ (system-wide)

Things I will miss if I switch from Fedora:
systemd-oomd (MX Does not support)
ZRam
