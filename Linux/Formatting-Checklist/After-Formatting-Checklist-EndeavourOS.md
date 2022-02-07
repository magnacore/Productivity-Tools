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
for differences select N

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

Add these environment variables:
export QT_AUTO_SCREEN_SCALE_FACTOR=0
export QT_SCALE_FACTOR=2

===

=====================================================================

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

=====================================================================

Go to respective environments and install
pip install qtile

There was an error in installing qtile, it was solved using:
The pip cache is cleared (remove ~/.cache/pip, if it exists)
pip uninstall cairocffi
pip install --no-cache-dir cairocffi[xcb]

===
sudo pacman -S zoxide (install system wide, not in environment)
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

# pacman yay
when a bin option is available in yay use that as the package will be ready made, example gitahead

for dragon and ueberzug, which was installed using "make install"
sudo dnf install gtk3-devel
If the above does not work try also
sudo dnf groupinstall "Development Tools" "Development Libraries"
I added ~/.local/bin in Xonsh path already (no need to do)

Dragon, Pistol and Hugo-extended are already built and can be copied to ~/.local/bin from ~/Software/bin

Just run `make` to compile dragon and get an executable you can run immediately or put where you like. To install, run `make install`, which will put it into ~/.local/bin by default.

sudo pacman -S

ffmpeg
alacritty
fzf
sudo pacman -S highlight atool mediainfo
yay w3m-imgcat
libcaca
poppler
w3m
yay mkvtoolnix mkvtoolnix-gui
fd
mlocate
imagemagick
imagemagick-perl (could not install)
mpv (do not use flatpak)
syncthing
yay autokey-gtk (did not build)
ncdu
bpytop
virtualbox select virtualbox-host-modules-arch
yay stacer
obs-studio
calibre (ranger will use this so we do not use flatpak)
ffmpegthumbnailer (ffmpegthumbnailer is needed for ranger thumbnail for videos)
rofi
yay ttf-emojione-color (not installing but rofimoji was working without it)
xdotool xsel
cmus
sxiv
zathura
zathura-pdf-mupdf zathura-pdf-poppler (they are in conflict)
neovim
calcurse
git
nodejs
yay preload (5th option in AUR)
bleachbit
simplescreenrecorder
trash-cli
yay gitahead (default option)
yay visual-studio-code-bin
yay ferdi

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

yay -S brave-bin

=====================================================================

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
flatpak install flathub io.github.gitahead.GitAhead (look for a fork)
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

flatpak install flathub io.lbry.lbry-app
=====================================================================

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

=====================================================================

sudo pacman -S veracrypt
yay go-ipfs-git
flatpak install flathub com.transmissionbt.Transmission
firefox login

yay appimagelauncher (did not build)
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
CMap
Recoll

pass
pass-otp
passmenu
zbarimg

===

Can you run rofi -h for the detected dpi and check the dpi setting?

eos-welcome --enable

~/.Xresources
Xft.dpi: 192

https://wiki.archlinux.org/title/HiDPIhttps://wiki.archlinux.org/title/HiDPIhttps://wiki.archlinux.org/title/HiDPI

yay dragon-drag-and-drop (it has been renamed to dragon-drag-and-drop)

--------------------------

rofimoji --selector-args '-dpi 1'

# Problems
## Show stoppers

# Annoyances
qtile not scaling
imagemagick-perl (could not install)
zathura-pdf-mupdf zathura-pdf-poppler (they are in conflict but pdf is opening)

yay appimagelauncher (not building)
yay autokey-qt (did not build)
