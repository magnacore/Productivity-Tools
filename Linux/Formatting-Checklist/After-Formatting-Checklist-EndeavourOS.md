Endeavour OS

Choose nvidia option during install
Install EndeavourOS and choose online xfce - this is also the default
Choose btrfs file system
Enable swap with hibernation
update mirrors
update system: yay

install timeshift : yay timeshift
1,2 (to install timeshift and timeshift-autosnap)
remove dependencies after install
for differences select N to show no diffs

sudo pacman -S grub-btrfs (manual snapshots will not appear until we update grub config)
sudo grub-mkconfig -o /boot/grub/grub.cfg (if we boot into this snapshot it will be read only)

Take Snapshot manually and comment it "OS Install"

Setup Nvidia
nvidia-installer-check : it will tell which command to run to install the driver
driver is already set up as well, but nvidia-settings will not be available, but nvidia-smi will be available. Running the installer will add nvidia-settings.

https://discovery.endeavouros.com/nvidia/optimus-manager-for-nvidia/2021/03/

yay -S optimus-manager
yay -S optimus-manager-qt

sudo pacman -S nvim

sudo nvim /etc/optimus-manager/optimus-manager.conf
set startup_mode to hybrid

Alt+f3 and run optimus manager and make it run at startup

Take snapshot and call it "After Nvidia Setup"

optimus-manager --status
optimus-manager --switch nvidia --no-confirm
optimus-manager --switch integrated --no-confirm
optimus-manager --switch hybrid --no-confirm

---------------------------------------------------------------------

Install anaconda, do not use sudo
conda create --name qtile
conda create --name xonsh
conda create --name util
conda create --name ranger
conda create --name go
quant (dont create this now, it will be cloned later)

Install pip in all environments
conda install -c anaconda pip

conda install -c conda-forge go

---------------------------------------------------------------------

Go to respective environments and install
pip install qtile

There was an error in installing qtile, it was solved using:
The pip cache is cleared (remove ~/.cache/pip, if it exists)
pip uninstall cairocffi
pip install --no-cache-dir cairocffi[xcb]

## For HiDPI
https://wiki.archlinux.org/title/HiDPIhttps://wiki.archlinux.org/title/HiDPIhttps://wiki.archlinux.org/title/HiDPI

xfce appearance > scale 2 (do this before enabling qtile, wallpaper directory must be present)

qt env variables in xonshrc (already set no need to set)
$QT_AUTO_SCREEN_SCALE_FACTOR=0
$QT_SCALE_FACTOR=2

rofi -dpi 1 flag (already set in qtile config, no need to set)

---

pip install ranger-fm (master branch was used)

---

conda install -c conda-forge xonsh

make xonsh default - was giving a problem was it was not made default

---------------------------------------------------------------------

copy laptop data
pgp, fonts, icons copy

---------------------------------------------------------------------

in xfce keyboard, restore numlock on startup
session and startup : lock screen before sleep
Application autostart : enable clipman to start automatically
in logout do not save sessions
Enable system sounds

---------------------------------------------------------------------

copy all configs to their respective folders

---------------------------------------------------------------------

# pacman
when a bin option is available in yay use that as the package will be ready made, example gitahead

I added ~/.local/bin in Xonsh path already (no need to do)

Pistol and Hugo-extended are already built and can be copied to ~/.local/bin from ~/Software/bin

sudo pacman -S ffmpeg
sudo pacman -S alacritty
sudo pacman -S fzf
sudo pacman -S highlight atool mediainfo
sudo pacman -S libcaca
sudo pacman -S poppler
sudo pacman -S w3m
sudo pacman -S fd
sudo pacman -S mlocate
sudo pacman -S imagemagick
sudo pacman -S mpv (do not use flatpak)
sudo pacman -S syncthing
sudo pacman -S ncdu
sudo pacman -S bpytop
sudo pacman -S virtualbox select virtualbox-host-modules-arch
sudo pacman -S obs-studio (use flatpak - have not tried virtual camera in flatpak)
sudo pacman -S calibre (ranger will use this so we do not use flatpak) if not working use: (sudo -v && wget -nv -O- https://download.calibre-ebook.com/linux-installer.sh | sudo sh /dev/stdin )
sudo pacman -S ffmpegthumbnailer (ffmpegthumbnailer is needed for ranger thumbnail for videos)
sudo pacman -S rofi
sudo pacman -S xdotool xsel
sudo pacman -S cmus
sudo pacman -S sxiv
sudo pacman -S zathura zathura-pdf-mupdf
sudo pacman -S neovim
sudo pacman -S calcurse
sudo pacman -S git
sudo pacman -S nodejs
sudo pacman -S bleachbit
sudo pacman -S simplescreenrecorder
sudo pacman -S trash-cli
sudo pacman -S veracrypt
sudo pacman -S zoxide (install system wide, not in environment)
sudo pacman -S feh
sudo pacman -S pass pass-otp zbar
sudo pacman -S catfish
sudo pacman -S pass pass-otp
sudo pacman -S gnome-disk-utility
sudo pacman -S xclip
sudo pacman -S odt2txt
sudo pacman -S pandoc pandoc-crossref
sudo pacman -S texlive-most

---------------------------------------------------------------------

# yay

yay gitahead (use bin option)
yay visual-studio-code-bin
yay ferdi
yay dragon-drag-and-drop (it has been renamed to dragon-drag-and-drop)
yay go-ipfs-git (try to get a non git stable release)
yay -S brave-bin
yay preload (5th option in AUR)
yay stacer
yay mkvtoolnix mkvtoolnix-gui
yay w3m-imgcat
yay rofi-greenclip
yay sioyek-git
yay python-xlsx2csv

---------------------------------------------------------------------

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

To change the venv
:CocCommand then fuzzy search for python.setInterpreter and choose the venv.

The following fix was not required for the latest master branch
File ranger/core/actions.py, line 459, in
filenames = [f.path for f in files]
change the line to
filenames = [f if isinstance(f, str) else f.path for f in files]

---------------------------------------------------------------------

# FLATPAK
Note: flatpak commands can be run by ranger, even if they are not available in terminal. See ksnip as an example. But if a 3rd part software is executing terminal command then we should install terminal version.

sudo pacman -Syu
sudo pacman -S flatpak

flatpak remote-add --if-not-exists flathub https://flathub.org/repo/flathub.flatpakrepo

flatpak install flathub com.github.tchx84.Flatseal
flatpak install flathub org.libreoffice.LibreOffice
flatpak install flathub org.keepassxc.KeePassXC
flatpak install flathub nz.mega.MEGAsync
flatpak install flathub org.signal.Signal
flatpak install flathub fr.handbrake.ghb
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
flatpak install flathub com.valvesoftware.Steam
flatpak install flathub com.discordapp.Discord
flatpak install flathub net.agalwood.Motrix
flatpak install flathub org.fedoraproject.MediaWriter
flatpak install flathub com.transmissionbt.Transmission
flatpak install flathub io.lbry.lbry-app
flatpak install flathub com.obsproject.Studio (NVME was not working, but an update of flatpaks fixed it automatically)

---------------------------------------------------------------------

# PIP/Conda Install

# util
# YoutubeDL using pip in conda environment util:
# /home/manuj/anaconda3/envs/util/bin/pip install youtube-dl
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

---

# qtile
For qtile memory module
/home/manuj/anaconda3/envs/qtile/bin/pip install psutil

---

# quant
conda create --name quant --clone base
If we get an error that package is corrupted, delete all files in the /home/manuj/anaconda3/pkgs folder and the quant env folder and retry.
/home/manuj/anaconda3/envs/quant/bin/pip install vectorbt
/home/manuj/anaconda3/envs/quant/bin/pip install pandas-ta
/home/manuj/anaconda3/envs/quant/bin/pip install nsepy

conda install -c anaconda pylint
conda install -c conda-forge black
pip install isort

---------------------------------------------------------------------

firefox login

---------------------------------------------------------------------

AppImages
Sourcetrail
ImageMosaic

---

Hugo : Download the binary from github page and put it in ~/.local/bin

# As Needed
Avidemux
jdk by redhat
CMap
Recoll

---

eos-welcome --enable

~/.Xresources
Xft.dpi: 192

--------------------------

# Problems
## Show stoppers

# Annoyances
qtile not scaling
imagemagick-perl (could not install)
zathura-pdf-mupdf zathura-pdf-poppler (they are in conflict but pdf is opening)
yay autokey-qt (did not build)

yay appimagelauncher (did not build)
then double click a app image in thunar, it will ask for a directory to be set. Set to appimage directory.
integrate and run

---
