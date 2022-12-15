FROM ubuntu:18.04

RUN wget -q https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/packages-microsoft-prod.deb && 
    dpkg -i packages-microsoft-prod.deb &&
    curl -fsSL https://packages.microsoft.com/keys/msopentech.asc | sudo apt-key add - &&
    curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo apt-key add -

RUN add-apt-repository "deb [arch=amd64] https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable" &&
    apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 0xB1998361219BD9C9 &&
    apt-add-repository "deb http://repos.azul.com/azure-only/zulu/apt stable main" &&
    apt-get update

RUN apt-get install servicefabricsdkcommon