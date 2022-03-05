make fedora download faster:

sudo vim /etc/dnf/dnf.conf

fastestmirror=True
max_parallel_downloads=10

=====================================================================

sudo dnf check-update
sudo dnf upgrade --refresh

=====================================================================

Fedora Snapper Setup

lsblk

Note the root partition like nvme0n1p3 (this is home and root)
df -h | grep vda

sudo btrfs subvolume list /

sudo dnf install snapper python3-dnf-plugin-snapper

sudo snapper -c root create-config /
(-c create, root is the name of the config, / is the partition)

sudo btrfs subvolume list /
Now an extra subvolume called .snapshot will be created. Its top level ID is same as root which means its created under the root subvolume.
We want to manually mount so we want to delete this, create a directory and create a subvolume in that directory

sudo btrfs subvolume delete /.snapshots

Now we create a new directory where will will create a new subvolume that we will mount later

sudo mkdir /.snapshots

Now we need to mount the file system so we can create a new subvolume in there

sudo mkdir /mnt/btrfs

Now we need to mount the root partition into this sub directory so we can see the subvolumes

sudo mount /dev/nvme0n1p3 /mnt/btrfs/
OR
sudo mount /dev/mapper/luks-0fd3694f-0b13-4296-a44f-9900b3970f31 /mnt/btrfs/

cd /mnt/btrfs/
ls
home root (we will see 2 subvolumes)

Now we will create a new subvolume and mount it in the directory we created before (.snapshots)
sudo btrfs subvolume create snapshots
ls
cd.. to go back to the mnt directory

sudo umount /mnt/btrfs

sudo rmdir btrfs/

Now we need to create a new mount point into the fstab (file system table)
sudo nvim /etc/fstab/
We will see root and home have the same UUID, copy that
UUID=XXX	/.snapshots	btrfs	subvol=snapshots	0	0

sudo mount -a
we should have no error

sudo btrfs subvolume get-default /

we want to make sure when the system boots the default subvolume will be the root subvolume

sudo btrfs subvolume set-default 258 /
sudo btrfs subvolume get-default /

Do this in bash, not in xonsh otherwise the flags will not get removed
sudo grubby --info=ALL
sudo grubby --update-kernel=ALL --remove-args="rootflags=subvol=root"
OR
sudo grubby --update-kernel=ALL --remove-args="rootflags=subvol=@ rd.luks.uuid=luks-0fd3694f-0b13-4296-a44f-9900b3970f31"
sudo grubby --info=ALL

This should remain
args="ro rhgb quiet"

reboot

sudo btrfs subvolume get-default /

sudo snapper ls
empty

sudo dnf install neofetch

sudo snapper ls
pre and post

Try without ambit first
sudo snapper --ambit classic rollback 1

reboot

sudo snapper ls

sudo snapper rollback 2

sudo snapper delete 4
sudo snapper --config root delete 7-25

systemctl status cronie

sudo nvim /etc/snapper/configs/root
root is the configuration we created before

hourly 3 daily 5 rest zero
cleaup flag will only work if cronie is installed

sudo dnf install cronie

reboot
or
systemctl start crodd.service

systemctl status crond.service

Manual backup
sudo snapper --config root create --description "My Message" --cleanup-algorithm timeline

This may disable scanning of home
sudo nvim /etc/updatedb.conf
a) PRUNE_BIND_MOUNTS = "no" ;  man page says the default is no, but Fedora's /etc/updatedb.conf sets it to "yes" which is the central problem in this bug.
b) PRUNEPATHS ; man page says the default is no paths are skipped, but Fedora's /etc/updatedb.conf has quite a long list, but you can add more, e.g. /.snapshots This will speed up locate
c) sudo updatedb

=====================================================================

Install anaconda, do not use sudo
conda create --name qtile
conda create --name xonsh
conda create --name util
conda create --name ranger
conda create --name go
Do not create quant it will be cloned later

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

sudo dnf install zoxide (install system wide, not in environment)
pip install ranger-fm

===

conda install -c conda-forge xonsh

Do not make Xonsh the default shell. Flatpak apps will not appear in rofi, alacritty terminal will not read colors from alacritty.yaml and zramctl command will fail.

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

Dragon, Pistol and Hugo-extended are already built and can be copied to ~/.local/bin from ~/Software/bin

Just run `make` to compile dragon and get an executable you can run immediately or put where you like. To install, run `make install`, which will put it into ~/.local/bin by default.

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
sudo dnf install nodejs
sudo dnf copr enable elxreno/preload -y && sudo dnf install preload -y
sudo dnf install bleachbit
sudo dnf install dnf-plugin-system-upgrade
sudo dnf install simplescreenrecorder
sudo dnf install feh
sudo dnf pass pass-otp zbar
sudo dnf install pandoc
sudo dnf install texlive-scheme-full

sudo dnf install trash-cli
The following fix was not required for the latest master branch
File ranger/core/actions.py, line 459, in
filenames = [f.path for f in files]
change the line to
filenames = [f if isinstance(f, str) else f.path for f in files]

Brave:
sudo dnf install dnf-plugins-core
sudo dnf config-manager --add-repo https://brave-browser-rpm-release.s3.brave.com/x86_64/
sudo rpm --import https://brave-browser-rpm-release.s3.brave.com/brave-core.asc
sudo dnf install brave-browser

VS Code:
sudo rpm --import https://packages.microsoft.com/keys/microsoft.asc
sudo sh -c 'echo -e "[code]\nname=Visual Studio Code\nbaseurl=https://packages.microsoft.com/yumrepos/vscode\nenabled=1\ngpgcheck=1\ngpgkey=https://packages.microsoft.com/keys/microsoft.asc" > /etc/yum.repos.d/vscode.repo'
dnf check-update
sudo dnf install code

---

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

# FLATPAK
Note: flatpak commands can be run by ranger, even if they are not available in terminal. See ksnip as an example. But if a 3rd part software is executing terminal command then we should install terminal version.

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
flatpak install flathub com.github.alexhuntley.Plots
flatpak install flathub org.gaphor.Gaphor

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

# RPM

greenclip was downloaded using the https://github.com/erebe/greenclip/releases and put in ~/.local/bin
Qtile config is already modified to use it

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
