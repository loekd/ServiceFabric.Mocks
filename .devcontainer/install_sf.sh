#!/bin/bash

echo "servicefabric servicefabric/accepted-eula-ga select true" | sudo debconf-set-selections
echo "servicefabricsdkcommon servicefabricsdkcommon/accepted-eula-ga select true" | sudo debconf-set-selections

apt-get install apt-transport-https -y
wget -q https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb

sudo curl -fsSL https://packages.microsoft.com/keys/msopentech.asc | sudo apt-key add -

sudo curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo apt-key add -

sudo add-apt-repository "deb [arch=amd64] https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable"

sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 0xB1998361219BD9C9
sudo apt-add-repository "deb http://repos.azul.com/azure-only/zulu/apt stable main"

sudo apt-get update

sudo apt-get install servicefabricsdkcommon