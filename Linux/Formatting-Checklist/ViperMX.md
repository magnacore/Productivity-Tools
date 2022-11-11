- Install MX
compress=zstd,noatime,space_cache=v2,ssd,discard=async

sudo apt update
sudo apt upgrade
flatpak update (flatpak is preinstalled - no need to activate it)

MX Tweak > Config Options > disable single click
Appearance Theme Arc Dark, icons tango

=====================================================================

Install anaconda, do not use sudo
bash <Anaconda Installer>

conda create --name qtile
conda create --name xonsh
conda create --name util

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

copy qtile to
/home/mankind/anaconda3/envs/qtile/bin/

copy qtile-launch to
/usr/local/bin/
chmod 744 qtile-launch

QTile.desktop file was moved to
etc/xgd/autostart/
chmod 644 QTile.desktop

=====================================================================

in xfce keyboard, restore numlock on startup
session and startup : lock screen before sleep
Application autostart : disable clipman to start automatically

Copy terminal settings to home .config
copy
/home/mankind/.config/xfce4
to skel

copy .fonts to skel

anaconda3 to skel

set thunar to list view and copy .config/thunar to skel

=====================================================================

Conda one time init

copy conda to
/home/$USER/anaconda3/bin/

copy .firstlogin to
/etc/skel/.firstlogin
and home

copy conda-init to /usr/local/bin
chmod 755 conda-init

=====================================================================

conda install -c conda-forge xonsh

Do not make Xonsh the default shell. Flatpak apps will not appear in rofi, alacritty terminal will not read colors from alacritty.yaml and zramctl command will fail.

=====================================================================

copy wallpapers

copy software folder

move bin to local bin and skel

=====================================================================

Just run `make` to compile dragon and get an executable you can run immediately or put where you like. To install, run `make install`, which will put it into ~/.local/bin by default.
nms and dragon need to be recompiled
/lib/x86_64-linux-gnu/libc.so.6: version `GLIBC_2.34' not found (required by ./nms)` for nms and dragon
ueberzug, which was installed using "make install"

zoxide version is too old

=====================================================================

# APT

sudo apt install ffmpeg
sudo apt install fzf
sudo apt install caca-utils highlight atool w3m w3m-img poppler-utils mediainfo
sudo apt install mkvtoolnix mkvtoolnix-gui
sudo apt install fd-find
sudo apt install mlocate
sudo apt install imagemagick libimage-magick-perl
sudo apt install mpv
sudo apt install syncthing
sudo apt install ncdu
sudo apt install bpytop
sudo apt install virtualbox
sudo apt install stacer
sudo apt install obs-studio
sudo apt install calibre
sudo apt install ffmpegthumbnailer
sudo apt install rofi
sudo apt install xdotool xsel
sudo apt install cmus
sudo apt install sxiv
sudo apt install zathura zathura-pdf-poppler zathura-djvu zathura-ps
sudo apt install calcurse
sudo apt install git
sudo apt install bleachbit
sudo apt install simplescreenrecorder
sudo apt install feh
sudo apt install pass pass-extension-otp zbar-tools
sudo apt install pandoc
sudo apt install tesseract-ocr
sudo apt install ufw
sudo apt install figlet
sudo apt install vagrant
sudo apt install cpu-x
sudo apt install lzip
sudo apt install build-essential
sudo apt install zoxide
sudo apt install trash-cli

VS Code:
installed using mx package manager popular applications

=====================================================================

# FLATPAK
flatpak remote-add --if-not-exists flathub https://flathub.org/repo/flathub.flatpakrepo

flatpak install flathub com.github.tchx84.Flatseal
flatpak install flathub org.keepassxc.KeePassXC
flatpak install flathub org.signal.Signal
flatpak install flathub fr.handbrake.ghb
flatpak install flathub com.github.Murmele.Gittyup
flatpak install flathub org.gimp.GIMP
flatpak install flathub us.zoom.Zoom
flatpak install flathub com.skype.Client
flatpak install flathub org.mozilla.Thunderbird
flatpak install flathub com.github.xournalpp.xournalpp
flatpak install flathub net.codeindustry.MasterPDFEditor
flatpak install flathub org.ksnip.ksnip
flatpak install flathub net.christianbeier.Gromit-MPX
flatpak install flathub com.github.miguelmota.Cointop
flatpak install flathub org.shotcut.Shotcut
flatpak install flathub org.blender.Blender
flatpak install flathub org.inkscape.Inkscape
flatpak install flathub com.discordapp.Discord
flatpak install flathub net.agalwood.Motrix
flatpak install flathub io.lbry.lbry-app
flatpak install flathub com.github.alexhuntley.Plots
flatpak install flathub org.gaphor.Gaphor
flatpak install flathub com.usebottles.bottles
flatpak install flathub fyi.zoey.TeX-Match
flatpak install flathub md.obsidian.Obsidian
flatpak install flathub org.ferdium.Ferdium
flatpak install flathub org.gnome.meld
flatpak install flathub com.brave.Browser
flatpak install flathub net.jami.Jami
flatpak install flathub org.gnome.seahorse.Application
flatpak install flathub com.valvesoftware.Steam

=====================================================================

## Vim installation
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

copy all configs to their respective folders

=====================================================================

# PIP/Conda Install

## util
### YoutubeDL using pip in conda environment util:
/home/manuj/anaconda3/envs/util/bin/pip install -U yt-dlp

/home/manuj/anaconda3/envs/util/bin/pip install pycp

Note: for scripts which are using python like pdf-split-1, we are importing a path like this:
#!/home/manuj/anaconda3/envs/util/bin/python3
The imports must be installed in the same environment from which we are importing python, in this case util
conda install -c conda-forge pypdf2
conda install -c conda-forge tqdm

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
/home/manuj/anaconda3/envs/qtile/bin/pip install psutil

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
/home/manuj/anaconda3/envs/xonsh/bin/pip install tabulate
/home/manuj/anaconda3/envs/xonsh/bin/pip install simple-term-menu
/home/manuj/anaconda3/envs/xonsh/bin/pip install nltk
num2words

=====================================================================

# RPM

greenclip was downloaded using the https://github.com/erebe/greenclip/releases and put in ~/.local/bin
Qtile config is already modified to use it

sudo dnf install code did not work, install as per instructions on fedora on VSCode website

Fredi was installed using rpm download

VeraCrypt was installed by downloading rpm

App image launcher was installed using rpm downloaded from github then double click a app image in thunar, it will ask for a directory to be set. Set to appimage directory. Integrate and run.

IPFS was installed using rpm downloaded from github page, but check there is appimage also and there may be other installers as well later.
sudo rpm -i sample_file.rpm

We can also download the IPFS Desktop appimage from github. Make is executable. run it ./<name>. AppImage launcher will ask for integration. Approve it and it will automatically move the appimage to ~/Applications. After that we can launch IPFS from rofi.

firefox login

transmission was already installed

Move binaries from ~/Software/bin to ~/.local/bin
Update the binaries by downloading them from GitHub

=====================================================================

AppImages
Sourcetrail
ImageMosaic

=======

Hugo : Download the binary from github page and put it in ~/.local/bin

# As Needed
Avidemux
jdk by redhat
CMap
Recoll

===

Gnome:
cipboard https://extensions.gnome.org/extension/779/clipboard-indicator/
Gnome sushi sudo apt-get install gnome-sushi
gnome tweak, enable weekdays

===

Debian:
rofi:
sudo apt install fonts-emojione rofi xdotool xsel

sudo apt-get install texlive-xetex texlive-fonts-recommended texlive-plain-generic
sudo apt install tig

===

pip install ranger-fm

Java was already installed. Check if its working
sudo apt install java-1.8.0-openjdk.x86_64
sudo alternatives --config java

sudo apt install texlive-scheme-full???

sudo apt autoremove
sudo apt clean
flatpak uninstall --unused
conda clean --all

setup firewall
pgp test
disable hidden files in thunar config


- reorder the git repo as viperos folder
- create a script that will move all the files to the right folder
- set the correct permissions automatically
- Cleanup everything after install



======

TODO:

Python simlink
Zoxode conda with simlink in user local bin???
Psutil
why audio is not playing? it was playing earler system sounds were playing - try youtube
rofi emoji not working - maybe its not installed
when i deleted a file in ranger, it gave an error
