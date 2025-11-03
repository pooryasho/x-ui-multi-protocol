#!/bin/bash

if [[ -f /etc/os-release ]]; then
    source /etc/os-release
    release=$ID
else 
    source /usr/lib/os-release
    release=$ID
fi  
rm -rf x-ui-multi-protocol

if [[ "${release}" == "debian" ]]; then
apt install git
git clone https://github.com/pooryasho/x-ui-multi-protocol.git
  cd x-ui-multi-protocol/x-ui-multi-protocol
  wget https://packages.microsoft.com/config/debian/10/packages-microsoft-prod.deb -O packages-microsoft-prod.deb 
  dpkg -i packages-microsoft-prod.deb 
  rm packages-microsoft-prod.deb 
  apt-get update 
  sudo add-apt-repository ppa:dotnet/backports
  apt-get install -y dotnet-sdk-8.0 
  apt-get install -y aspnetcore-runtime-8.0 
  apt-get install -y dotnet-runtime-8.0 
  apt install libc6 
  dotnet publish -c Release -o /etc/x-ui-multi-protocol
elif [[ "${release}" == "centos" ]]; then 
dnf install wget
dnf install git
git clone https://github.com/pooryasho/x-ui-multi-protocol.git
 cd x-ui-multi-protocol/x-ui-multi-protocol
  sudo dnf install dotnet-sdk-8.0
  sudo dnf install aspnetcore-runtime-8.0
  sudo dnf install dotnet-runtime-8.0
  dotnet publish -c Release -o /etc/x-ui-multi-protocol
elif [[ "${release}" == "fedora" ]]; then
dnf install wget
dnf install git
git clone https://github.com/pooryasho/x-ui-multi-protocol.git
 cd x-ui-multi-protocol/x-ui-multi-protocol
  sudo dnf install dotnet-sdk-8.0
  sudo dnf install aspnetcore-runtime-8.0
  sudo dnf install dotnet-runtime-8.0
  dotnet publish -c Release -o /etc/x-ui-multi-protocol
elif [[ "${release}" == "ubuntu" ]]; then

declare repo_version=$(if command -v lsb_release &> /dev/null; then lsb_release -r -s; else grep -oP '(?<=^VERSION_ID=).+' /etc/os-release | tr -d '"'; fi)
apt install git
git clone https://github.com/pooryasho/x-ui-multi-protocol.git
cd x-ui-multi-protocol/x-ui-multi-protocol
wget https://packages.microsoft.com/config/ubuntu/$repo_version/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb
apt-get update 
apt-get install -y dotnet-sdk-8.0
apt-get update
apt-get install -y aspnetcore-runtime-8.0
apt-get update
apt-get install -y dotnet-runtime-8.0
apt-get update
if [[ "${repo_version}" == "22.04" ]]; then
sudo apt-get purge dotnet-sdk-8.0
sudo rm -f /etc/apt/sources.list.d/microsoft-prod.list && sudo apt update
sudo apt install -y dotnet8
fi
  dotnet publish -c Release -o /etc/x-ui-multi-protocol

 else
 apt install git
git clone https://github.com/pooryasho/x-ui-multi-protocol.git
  wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
  chmod +x ./dotnet-install.sh
  ./dotnet-install.sh --channel 8.0
  apt update
  apt install -y dotnet8
  cd x-ui-multi-protocol/x-ui-multi-protocol
  dotnet publish -c Release -o /etc/x-ui-multi-protocol
fi

  
 
  cd /etc/systemd/system/
  wget https://raw.githubusercontent.com/pooryasho/x-ui-multi-protocol/master/x-ui-multi-protocol.service
   cp /etc/x-ui/x-ui.db /etc/x-ui/backup.db
  systemctl daemon-reload
  systemctl start x-ui-multi-protocol
systemctl enable x-ui-multi-protocol
systemctl restart x-ui-multi-protocol
