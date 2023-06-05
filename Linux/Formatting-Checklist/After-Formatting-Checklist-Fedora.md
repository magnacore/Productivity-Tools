=====================================================================
= Make fedora updates fast
=====================================================================

sudo vim /etc/dnf/dnf.conf

fastestmirror=True
max_parallel_downloads=10

=====================================================================
= Update fedora
=====================================================================

sudo dnf check-update
sudo dnf upgrade --refresh

=====================================================================
= Fedora Snapper Setup
=====================================================================

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
systemctl start crond.service

systemctl status crond.service

Manual backup
sudo snapper --config root create --description "My Message" --cleanup-algorithm timeline

This may disable scanning of home
sudo nvim /etc/updatedb.conf
a) PRUNE_BIND_MOUNTS = "no" ;  man page says the default is no, but Fedora's /etc/updatedb.conf sets it to "yes" which is the central problem in this bug.
b) PRUNEPATHS ; man page says the default is no paths are skipped, but Fedora's /etc/updatedb.conf has quite a long list, but you can add more, e.g. /.snapshots This will speed up locate
c) sudo updatedb

=====================================================================
= Install anaconda
=====================================================================

Install anaconda, do not use sudo

conda create --name qtile
conda create --name xonsh
conda create --name util

Do not create quant it will be cloned later

Install pip in all environments
conda install -c anaconda pip

conda create --name go
conda install -c conda-forge go

=====================================================================
= Setup anaconda environments
=====================================================================
Go to respective environments and install

QTILE:
The pip cache is cleared (remove ~/.cache/pip, if it exists)
pip install xcffib wheel
pip install --no-cache --upgrade --no-build-isolation cairocffi

Old Method:
pip install --no-cache-dir xcffib
pip install --no-cache-dir cairocffi
pip install --no-cache-dir qtile

Delete keyboard shortcut for window manager and keyboards

https://github.com/qtile/qtile/issues/994

Delete keyboard shortcut for window manager and keyboards

sudo apt install neovim

sudo nvim /etc/xdg/xfce4/xfconf/xfce-perchannel-xml/xfce4-session.xml

To make the change on a global scope for all users which do not have this setting already in there private xfce configuration, alter
This is the xfce4-session.xml that does not start the panel, desktop and wm. 3 lines each were deleted for each of them.

conda install -c conda-forge psutil

=====================================================================

QUANT:

conda create --name quant --clone base
If we get an error that package is corrupted, delete all files in the /home/manuj/anaconda3/pkgs folder and the quant env folder and retry.
/home/manuj/anaconda3/envs/quant/bin/pip install vectorbt
/home/manuj/anaconda3/envs/quant/bin/pip install pandas-ta
/home/manuj/anaconda3/envs/quant/bin/pip install nsepy

conda install -c anaconda pylint
conda install -c conda-forge black
pip install isort

=====================================================================

XONSH:
conda install -c conda-forge xonsh
conda install -c conda-forge num2words
conda install -c conda-forge google-cloud-sdk
conda install -c conda-forge google-cloud-texttospeech
conda install -c anaconda nltk
conda install -c conda-forge rich
conda install -c conda-forge pypdf2
conda install -c anaconda pillow
conda install -c conda-forge playsound

Force update only if installed version is not recent. Do this after running the above commands:
conda install -c conda-forge 'google-cloud-texttospeech>=2'

No conda:
/home/manuj/anaconda3/envs/xonsh/bin/pip install simple-term-menu

Do not make Xonsh the default shell. Flatpak apps will not appear in rofi, alacritty terminal will not read colors from alacritty.yaml and zramctl command will fail.

Installing Playsound:
playsound was installed in xonsh
https://stackoverflow.com/questions/70508775/error-could-not-build-wheels-for-pycairo-which-is-required-to-install-pyprojec
sudo dnf install gobject-introspection-devel cairo-gobject-devel
pip install PyGObject

=====================================================================

UTIL:

Note: for scripts which are using python like pdf-split-1, we are importing a path like this:
#!/home/manuj/anaconda3/envs/util/bin/python3
The imports must be installed in the same environment from which we are importing python, in this case util
conda install -c conda-forge pypdf2

conda install -c conda-forge yt-dlp (do not install using conda because we get old versions - use pip)
conda install -c conda-forge go-ipfs
conda install -c conda-forge pyperclip
conda install -c conda-forge qrcode
conda install -c conda-forge libwebp
conda install -c conda-forge pypdf2 (check if needed)
conda install -c conda-forge rich
conda install -c conda-forge plyer # use notify-send instead

conda not available:
/home/manuj/anaconda3/envs/util/bin/pip install pycp
/home/manuj/anaconda3/envs/util/bin/pip install passphraseme
/home/manuj/anaconda3/envs/util/bin/pip install rofimoji
/home/manuj/anaconda3/envs/util/bin/pip install ueberzug (ueberzug command is also hardcoded)

(do not install pygobject in util else playsound will break)

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
= DNF
=====================================================================

See kickstart file

sudo dnf groupinstall "Development Tools" "Development Libraries"
I added ~/.local/bin in Xonsh path already (no need to do)

sudo dnf install https://download1.rpmfusion.org/free/fedora/rpmfusion-free-release-$(rpm -E %fedora).noarch.rpm
sudo dnf install https://download1.rpmfusion.org/nonfree/fedora/rpmfusion-nonfree-release-$(rpm -E %fedora).noarch.rpm

VS Code:
sudo rpm --import https://packages.microsoft.com/keys/microsoft.asc
sudo sh -c 'echo -e "[code]\nname=Visual Studio Code\nbaseurl=https://packages.microsoft.com/yumrepos/vscode\nenabled=1\ngpgcheck=1\ngpgkey=https://packages.microsoft.com/keys/microsoft.asc" > /etc/yum.repos.d/vscode.repo'
dnf check-update
sudo dnf install code

Java
sudo dnf install java-1.8.0-openjdk.x86_64 (already in kickstart)
sudo alternatives --config java

=====================================================================
= Vim installation
=====================================================================

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
= Flatpak
=====================================================================

Note: flatpak commands can be run by ranger, even if they are not available in terminal. See ksnip as an example. But if a 3rd part software is executing terminal command then we should install terminal version.

flatpak remote-add --if-not-exists flathub https://flathub.org/repo/flathub.flatpakrepo


=====================================================================
= RPM / Manual
=====================================================================

greenclip was downloaded using the https://github.com/erebe/greenclip/releases and put in ~/.local/bin
Qtile config is already modified to use it

Hugo : Download the binary from github page and put it in ~/.local/bin

Dragon, Pistol and Hugo-extended are already built and can be copied to ~/.local/bin from ~/Software/bin

Just run `make` to compile dragon and get an executable you can run immediately or put where you like. To install, run `make install`, which will put it into ~/.local/bin by default.

App image launcher was installed using rpm downloaded from github then double click a app image in thunar, it will ask for a directory to be set. Set to appimage directory. Integrate and run.

IPFS was installed using rpm downloaded from github page, but check there is appimage also and there may be other installers as well later.
sudo rpm -i sample_file.rpm

We can also download the IPFS Desktop appimage from github. Make is executable. run it ./<name>. AppImage launcher will ask for integration. Approve it and it will automatically move the appimage to ~/Applications. After that we can launch IPFS from rofi.

firefox login

transmission was already installed

Move binaries from ~/Software/bin to ~/.local/bin
Update the binaries by downloading them from GitHub

=====================================================================
= Appimages
=====================================================================

AppImages
Sourcetrail
ImageMosaic

=====================================================================
= Optional
=====================================================================
Recoll

=====================================================================
= Wiki
=====================================================================

Debian:
rofi:
sudo apt install fonts-emojione rofi xdotool xsel

sudo apt-get install texlive-xetex texlive-fonts-recommended texlive-plain-generic
sudo apt install tig
