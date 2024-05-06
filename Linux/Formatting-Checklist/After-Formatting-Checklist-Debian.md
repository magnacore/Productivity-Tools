=====================================================================
= Preinstall
=====================================================================

Install Sprial Linux

sudo apt udate && sudo apt upgrade

autoremove twice

copy laptop data
pgp, fonts, icons, gpg, boto, password-store

=====================================================================
= Apt packages
=====================================================================

git
git-lfs
fzf
caca-utils
highlight
atool
ffmpeg
ffmpegthumbnailer
w3m
w3m-img
imagemagick
libimage-magick-perl
mpv
poppler-utils
mediainfo
mkvtoolnix
mkvtoolnix-gui
fd-find
syncthing
ncdu
bpytop
stacer
calibre
rofi
xdotool
xsel
cmus
sxiv
zathura
zathura-pdf-poppler (replace with zathura-pdf-mupdf)
zathura-djvu
zathura-ps
zathura-cb
neovim
calcurse
bleachbit
simplescreenrecorder
feh
pass
pass-otp
zbar-tools
texlive pandoc
tesseract-ocr
ufw
figlet
cpu-x
lzip
tango-icon-theme
#clamav
#clamtk
pwgen
trash-cli
plocate
efibootmgr
flatpak
p7zip
zoxide (gets an old version that does not support ranger, so a binary was downloaded using curl from github)
qdirstat
lm-sensors
libnotify-bin
steghide
paperkey
qrencode
gobject-introspection
ssss
oathtool
pinentry-tty
# texlive-scheme-full
greybird-gtk-theme
tango-icon-theme
# elementary-xfce-icon-theme
# elementary-icon-theme
odt2txt
build-essential
mupdf
libcrack2
sioyek

For distrobox:
podman
uidmap
slirp4netns
distrobox

For ueberzugcpp:
libchafa0
libopencv-core406
libopencv-imgcodecs406
libopencv-imgproc406
libopencv-videoio406
libspdlog1.10-fmt9
libtbb12
libvips42
libxcb-res0

For Virtual Machine manager:
qemu-kvm
qemu-system
qemu-utils
python3
python3-pip
libvirt-clients
libvirt-daemon-system
bridge-utils
virtinst
libvirt-daemon
virt-manager
qemu-system-gui
gir1.2-spiceclientgtk-3.0

For Veracrypt:
libwxgtk3.2-1
pcscd

For portmaster:
libnetfilter-queue1

=====================================================================
= Flatpaks
=====================================================================

com.github.tchx84.Flatseal
org.libreoffice.LibreOffice
org.keepassxc.KeePassXC
org.signal.Signal
fr.handbrake.ghb
com.github.Murmele.Gittyup
org.gimp.GIMP
us.zoom.Zoom
com.skype.Client
org.mozilla.Thunderbird
com.github.xournalpp.xournalpp
net.codeindustry.MasterPDFEditor
org.ksnip.ksnip
net.christianbeier.Gromit-MPX
org.blender.Blender
org.inkscape.Inkscape
com.valvesoftware.Steam
net.davidotek.pupgui2
com.discordapp.Discord
org.gaphor.Gaphor
com.usebottles.bottles
fyi.zoey.TeX-Match
md.obsidian.Obsidian
org.ferdium.Ferdium
org.gnome.meld
com.brave.Browser
net.jami.Jami
org.gnome.seahorse.Application
com.obsproject.Studio
org.torproject.torbrowser-launcher
net.sapples.LiveCaptions
network.loki.Session
org.filezillaproject.Filezilla
net.sourceforge.liferea
org.kde.kleopatra3

=====================================================================
= Fonts
=====================================================================
fonts-adf-baskervald
fonts-adf-gillius
fonts-adf-ikarius
fonts-adf-irianis
fonts-adf-switzera
fonts-adf-tribun
fonts-adf-verana
fonts-anonymous-pro
fonts-arapey
fonts-arkpandora
fonts-cantarell
fonts-roboto
fonts-b612
fonts-bajaderka
fonts-beteckna
fonts-bwht
fonts-cabin
fonts-cabinsketch
fonts-cantarell
fonts-cardo
fonts-century-catalogue
fonts-cherrybomb
fonts-comfortaa
fonts-comic-neue
fonts-compagnon
fonts-courier-prime
fonts-dancingscript
fonts-dejavu
fonts-deva
fonts-dosis
fonts-ebgaramond
fonts-ebgaramond-extra
fonts-ecolier-court
fonts-essays1743
fonts-fantasma
fonts-fantasque-sans
fonts-fanwood
fonts-firacode
fonts-fork-awesome

GREEK:
fonts-gfs-bodoni-classic
fonts-gfs-complutum
fonts-gfs-didot
fonts-gfs-didot-classic
fonts-gfs-neohellenic
fonts-gfs-porson
fonts-gfs-solomos

fonts-glasstty
fonts-gnutypewriter
fonts-goudybookletter
fonts-gujr
fonts-guru
fonts-hack
fonts-havana
fonts-hermit
fonts-humor-sans
fonts-inconsolata
fonts-inter
fonts-jetbrains-mono
fonts-junction
fonts-junicode
fonts-kaushanscript
fonts-knda
fonts-league-mono
fonts-league-spartan
fonts-levien-museum
fonts-liberation2
fonts-lindenhill
fonts-linuxlibertine
fonts-lobstertwo
fonts-lohit-beng-assamese
fonts-lohit-beng-bengali
fonts-lohit-deva-marathi
fonts-lohit-deva-nepali
fonts-lohit-orya
fonts-lohit-taml
fonts-lohit-telu
fonts-millimetre
fonts-mlym
fonts-monofur
fonts-monoid
fonts-mononoki
fonts-mplus
fonts-nafees
fonts-noto
fonts-noto-color-emoji (This makes the color emojis appear in terminal)
fonts-ocr-b
fonts-oflb-euterpe
fonts-oldstandard
fonts-open-sans
fonts-opendin
fonts-opendyslexic
fonts-orya
fonts-osifont
fonts-paratype
fonts-prociono
fonts-proggy
fonts-quattrocento
fonts-quicksand
fonts-radisnoir
fonts-rampart
fonts-roadgeek
fonts-roboto
fonts-routed-gothic
fonts-rufscript
fonts-samyak
fonts-sil-andika
fonts-sil-galatia
fonts-sil-gentium
fonts-sora
fonts-tiresias
fonts-tomsontalks
fonts-tuffy
fonts-vollkorn
fonts-yanone-kaffeesatz

=====================================================================
= Manual
=====================================================================

sudo dpkg -i sample_file.deb

App image launcher was installed using ? downloaded from github then double click an app image in thunar, it will ask for a directory to be set. Set to appimage directory. Integrate and run.

IPFS was installed using ? downloaded from github page, but check there is appimage also and there may be other installers as well later.
We can also download the IPFS Desktop appimage from github. Make is executable. run it ./<name>. AppImage launcher will ask for integration. Approve it and it will automatically move the appimage to ~/Applications. After that we can launch IPFS from rofi.

sudo apt-get install texlive-xetex texlive-fonts-recommended texlive-plain-generic

portmaster

ueberzugcpp by downloading its deb

vscodium by following instructions from website

veracrypt

=====================================================================
= Install anaconda
=====================================================================

Install anaconda, do not use sudo
Update base if prompted
let conda init run so we can use environments in bash

Set mamba solver
conda install -n base conda-libmamba-solver
conda config --set solver libmamba

conda create --name qtile
conda create --name xonsh
conda create --name util

Do not create quant it will be cloned later

Install pip in all environments
conda install anaconda::pip

=====================================================================
= Setup anaconda environments
=====================================================================
Go to respective environments and install

QTILE:
The pip cache is cleared (remove ~/.cache/pip, if it exists)
pip install xcffib wheel
pip install --no-cache --upgrade --no-build-isolation cairocffi
pip install --no-cache-dir qtile
https://github.com/qtile/qtile/issues/994

Setup qtile as per github instructions

To make the change on a global scope for all users which do not have this setting already in there private xfce configuration, alter
sudo nvim /etc/xdg/xfce4/xfconf/xfce-perchannel-xml/xfce4-session.xml
Do not start the panel, desktop and wm. 3 lines each were deleted for each of them.

conda install conda-forge::psutil

=====================================================================

QUANT:

conda create --name quant --clone base
If we get an error that package is corrupted, delete all files in the /home/manuj/anaconda3/pkgs folder and the quant env folder and retry.
/home/manuj/anaconda3/envs/quant/bin/pip install vectorbt
/home/manuj/anaconda3/envs/quant/bin/pip install pandas-ta
/home/manuj/anaconda3/envs/quant/bin/pip install nsepy

conda install -c anaconda pylint
conda install -c conda-forge black
conda install -c conda-forge numpy-financial
pip install isort

=====================================================================

XONSH:
conda install conda-forge::xonsh
conda install conda-forge::num2words
conda install conda-forge::google-cloud-sdk
conda install conda-forge::google-cloud-texttospeech (does not require google cloud sdk)
conda install conda-forge::rich
conda install conda-forge::pypdf2
conda install conda-forge::pillow
conda install conda-forge::unidecode
conda install conda-forge::prompt_toolkit
conda install conda-forge::ebooklib

Force update only if installed version is not recent. Do this after running the above commands:
conda install -c conda-forge 'google-cloud-texttospeech>=2'

No conda:
/home/manuj/anaconda3/envs/xonsh/bin/pip install simple-term-menu

Do not make Xonsh the default shell. Flatpak apps will not appear in rofi, alacritty terminal will not read colors from alacritty.yaml and zramctl command will fail.

=====================================================================

UTIL:

Note: for scripts which are using python like pdf-split-1, we are importing a path like this:
#!/home/manuj/anaconda3/envs/util/bin/python3
The imports must be installed in the same environment from which we are importing python, in this case util
conda install conda-forge::pypdf2

conda install conda-forge::yt-dlp
conda install conda-forge::pyperclip
conda install conda-forge::qrcode
conda install conda-forge::libwebp
conda install conda-forge::rich
conda install conda-forge::ffmpeg-normalize
conda install conda-forge::prompt_toolkit

conda not available:
/home/manuj/anaconda3/envs/util/bin/pip install pycp
/home/manuj/anaconda3/envs/util/bin/pip install passphraseme
/home/manuj/anaconda3/envs/util/bin/pip install rofimoji

=====================================================================
= Wiki
=====================================================================

sudo apt -t bookworm-backports update
sudo apt -t bookworm-backports upgrade

GPG permissions
In bash:
chown -R $(whoami) ~/.gnupg/
find ~/.gnupg -type f -exec chmod 600 {} \; # Set 600 for files
find ~/.gnupg -type d -exec chmod 700 {} \; # Set 700 for directories
