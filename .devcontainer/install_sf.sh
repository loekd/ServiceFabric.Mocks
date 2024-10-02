#!/bin/bash

echo "servicefabric servicefabric/accepted-eula-ga select true" |  debconf-set-selections
echo "servicefabricsdkcommon servicefabricsdkcommon/accepted-eula-ga select true" |  debconf-set-selections


DEBIAN_FRONTEND=noninteractive
LD_LIBRARY_PATH=/opt/microsoft/servicefabric/bin/Fabric/Fabric.Code



wget -q https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb packages-microsoft-prod.deb
dpkg -i packages-microsoft-prod.deb
rm -f packages-microsoft-prod.deb

wget -q https://packages.microsoft.com/keys/msopentech.asc msopentech.asc
apt-key add ./msopentech.asc
rm -f msopentech.asc



wget -q https://download.docker.com/linux/ubuntu/gpg gpg
apt-key add ./gpg
rm -f gpg

add-apt-repository "deb [arch=amd64] https://download.docker.com/linux/ubuntu focal stable" -y
apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 0xB1998361219BD9C9
curl -O https://cdn.azul.com/zulu/bin/zulu-repo_1.0.0-2_all.deb && apt-get install ./zulu-repo_1.0.0-2_all.deb 
rm -f zulu-repo_1.0.0-2_all.deb

# apt-get install apt-transport-https -y
# wget -q https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb
# dpkg -i packages-microsoft-prod.deb
# rm -f packages-microsoft-prod.deb

# curl -fsSL https://packages.microsoft.com/keys/msopentech.asc |  apt-key add -

# curl -fsSL https://download.docker.com/linux/ubuntu/gpg |  apt-key add -

# apt-get install software-properties-common -y
# apt-key adv --keyserver keyserver.ubuntu.com --recv-keys 7EA0A9C3F273FCD8
# add-apt-repository "deb [arch=amd64] https://download.docker.com/linux/ubuntu oracular stable" -y


# apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 0xB1998361219BD9C9
# apt install gnupg ca-certificates curl -y
# curl -s https://repos.azul.com/azul-repo.key | gpg --dearmor -o /usr/share/keyrings/azul.gpg
# echo "deb [signed-by=/usr/share/keyrings/azul.gpg] https://repos.azul.com/zulu/deb stable main" |  tee /etc/apt/sources.list.d/zulu.list
# rm -f azul.gpg

apt-get update

apt-get install servicefabricsdkcommon -y