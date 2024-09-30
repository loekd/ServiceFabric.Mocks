FROM ubuntu:20.04 

RUN echo "servicefabric servicefabric/accepted-eula-ga select true" | debconf-set-selections
RUN echo "servicefabricsdkcommon servicefabricsdkcommon/accepted-eula-ga select true" | debconf-set-selections
ENV LD_LIBRARY_PATH=/opt/microsoft/servicefabric/bin/Fabric/Fabric.Code

RUN apt-get update && apt-get install apt-utils apt-transport-https ca-certificates gnupg2 software-properties-common curl -y

ADD https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb packages-microsoft-prod.deb
RUN dpkg -i packages-microsoft-prod.deb

ADD https://packages.microsoft.com/keys/msopentech.asc msopentech.asc
RUN apt-key add ./msopentech.asc

ADD https://download.docker.com/linux/ubuntu/gpg gpg
RUN apt-key add ./gpg

RUN add-apt-repository "deb [arch=amd64] https://download.docker.com/linux/ubuntu focal stable"
RUN apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 0xB1998361219BD9C9
#RUN apt-add-repository "deb http://repos.azul.com/azure-only/zulu/apt stable main"
RUN curl -O https://cdn.azul.com/zulu/bin/zulu-repo_1.0.0-2_all.deb && apt-get install ./zulu-repo_1.0.0-2_all.deb 
RUN apt-get update && apt-get install zulu11-jdk servicefabricsdkcommon dotnet-sdk-6.0 dotnet-sdk-7.0 dotnet-sdk-8.0 servicefabric -y
