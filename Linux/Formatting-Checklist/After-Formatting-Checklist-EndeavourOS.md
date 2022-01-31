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

===
