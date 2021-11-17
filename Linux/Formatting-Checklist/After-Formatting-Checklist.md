make fedora download faster:

sudo vim /etc/dnf/dnf.conf

fastestmirror=True
max_parallel_downloads=10

=====================================================================

sudo dnf check-update
sudo dnf upgrade

=====================================================================

Install anaconda, do not use sudo
conda create --name qtile
conda create --name xonsh
conda create --name util
conda create --name quant
conda create --name ranger

Install pip in all environments
conda install -c anaconda pip

=====================================================================

Go to respective environments and install
pip install qtile

There was an error in installing qtile, it was solved using:
The pip cache is cleared (remove ~/.cache/pip, if it exists)
pip uninstall cairocffi 
pip install --no-cache-dir cairocffi[xcb]

===

sudo dnf install zoxide (install system wide, not in environment)
pip install ranger-fm

===

conda install -c conda-forge xonsh

make xonsh default - was giving a problem was it was not made default

=====================================================================

copy laptop data
pgp, fonts, icons copy

=====================================================================

in xfce keyboard, restore numlock on startup
session and startup : lock screen before sleep
Application autostart : enable clipman to start automatically
in logout do not save sessions

=====================================================================

copy all configs to their respective folders

=====================================================================

# DNF

for dragon and ueberzug, which was installed using "make install"
sudo dnf install gtk3-devel
If the above does not work try also
sudo dnf groupinstall "Development Tools" "Development Libraries"
I added ~/.local/bin in Xonsh path already (no need to do)

sudo dnf install https://download1.rpmfusion.org/free/fedora/rpmfusion-free-release-$(rpm -E %fedora).noarch.rpm
sudo dnf install https://download1.rpmfusion.org/nonfree/fedora/rpmfusion-nonfree-release-$(rpm -E %fedora).noarch.rpm

sudo dnf install ffmpeg
sudo dnf install timeshift
sudo dnf install alacritty
sudo dnf install fzf
sudo dnf install caca-utils highlight atool w3m w3m-img poppler-utils mediainfo
sudo dnf install mkvtoolnix mkvtoolnix-gui
sudo dnf install fd-find
sudo dnf install mlocate
sudo dnf install ImageMagick ImageMagick-perl
sudo dnf install mpv (do not use flatpak)
sudo dnf install syncthing
sudo dnf install autokey-gtk
sudo dnf install ncdu
sudo dnf install bpytop
sudo dnf install VirtualBox
sudo dnf install stacer
sudo dnf install obs-studio
sudo dnf install calibre (ranger will use this so we do not use flatpak)
sudo dnf install ffmpegthumbnailer (ffmpegthumbnailer is needed for ranger thumbnail for videos)
sudo dnf install rofi
sudo dnf install eosrei-emojione-fonts xdotool xsel
sudo dnf install cmus
sudo dnf install sxiv
sudo dnf install zathura zathura-pdf-mupdf zathura-pdf-poppler
sudo dnf install neovim
sudo dnf install calcurse
sudo dnf install git

install vim plug
Create an empty file at /home/manuj/.config/nvim/init.vim
.vimrc from home automatically gets copied to it once we start nvim
:PlugInstall

sudo dnf install trash-cli
File ranger/core/actions.py, line 459, in
filenames = [f.path for f in files]
change the line to
filenames = [f if isinstance(f, str) else f.path for f in files]

Brave:
sudo dnf install dnf-plugins-core
sudo dnf config-manager --add-repo https://brave-browser-rpm-release.s3.brave.com/x86_64/
sudo rpm --import https://brave-browser-rpm-release.s3.brave.com/brave-core.asc
sudo dnf install brave-browser

=====================================================================

# FLATPAK
Note: flatpak commands can be run by ranger, even if they are not available in terminal. See ksnip as an example. But if a 3rd part software is executing terminal command then we should install terminal version.

flatpak remote-add --if-not-exists flathub https://flathub.org/repo/flathub.flatpakrepo

flatpak install flathub org.libreoffice.LibreOffice
flatpak install flathub org.keepassxc.KeePassXC
flatpak install flathub nz.mega.MEGAsync
flatpak install flathub org.signal.Signal
flatpak install flathub fr.handbrake.ghb
flatpak install flathub com.ozmartians.VidCutter
flatpak install flathub io.github.gitahead.GitAhead (look for a fork)
flatpak install flathub org.gimp.GIMP
flatpak install flathub us.zoom.Zoom
flatpak install flathub com.skype.Client
flatpak install flathub org.mozilla.Thunderbird
flatpak install flathub com.github.xournalpp.xournalpp
flatpak install flathub net.codeindustry.MasterPDFEditor
flatpak install flathub org.ksnip.ksnip
flatpak install flathub net.christianbeier.Gromit-MPX
flatpak install flathub com.github.tchx84.Flatseal
flatpak install flathub com.github.miguelmota.Cointop
flatpak install flathub org.shotcut.Shotcut
flatpak install flathub org.blender.Blender

=====================================================================

# PIP/Conda Install

# util
# YoutubeDL using pip in conda environment util:
# /home/manuj/anaconda3/envs/util/bin/pip install youtube-dl
/home/manuj/anaconda3/envs/util/bin/pip install -U yt-dlp

/home/manuj/anaconda3/envs/util/bin/pip install pycp

Note: for scripts which are using python like pdf-split-1, we are importing a path like this:
#!/home/manuj/anaconda3/envs/util/bin/python3
The imports must be installed in the same invironment from which we are importing python, in this case util
conda install -c conda-forge pypdf2
conda install -c conda-forge tqdm

/home/manuj/anaconda3/envs/util/bin/pip install rofimoji

/home/manuj/anaconda3/envs/util/bin/pip install ueberzug

conda install -c conda-forge go-ipfs

===

# qtile
For qtile memory module
/home/manuj/anaconda3/envs/qtile/bin/pip install psutil

=====================================================================

# RPM

sudo dnf install code did not work, rpm was downloaded and installed from website

Fredi was installed using rpm download

VaeraCrypt was installed by downloading rpm

IPFS was installed using rpm downloaded from github page, but check there is appimage also and there may be other installers as well later.
sudo rpm -i sample_file.rpm

firefox login

transmission was already installed

App image launcher was installed using rpm downloaded from github
then double click a app image in thunar, it will ask for a directory to be set. Set to appimage directory.
integrate and run

=====================================================================

AppImages
Sourcetrail
ImageMosaic

=======

Hugo : Download the binary from github page and put it in ~/.local/bin

# As Needed
Avidemux
jdk by redhat
bleechbit
CMap
Recoll

pass
pass-otp
passmenu
zbarimg

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
