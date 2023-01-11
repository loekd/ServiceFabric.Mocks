FROM ubuntu:16.04 AS build

RUN echo 'debconf debconf/frontend select Noninteractive' | debconf-set-selections

RUN apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 417A0893

RUN apt-get update && \
    apt-get install --assume-yes curl apt-transport-https

ADD https://packages.microsoft.com/config/ubuntu/16.04/packages-microsoft-prod.deb packages-microsoft-prod.deb
RUN dpkg -i packages-microsoft-prod.deb

RUN apt-get update && \
    apt-get download servicefabric && \
    dpkg -x servicefabric*.deb .

# deleting unnecessary files to reduce image size
RUN find "/opt/microsoft/servicefabric/bin/Fabric/Fabric.Code" -name "*.exe" -type f -delete && \
    find "/opt/microsoft/servicefabric/bin/Fabric/Fabric.Code" -name "*.pdb" -type f -delete && \
    find "/opt/microsoft/servicefabric/bin/Fabric/Fabric.Code" -name "SFBlockStoreService" -type f -delete && \
    find "/opt/microsoft/servicefabric/bin/Fabric/Fabric.Code" -name "__FabricSystem_App*" -type d -exec rm -rf {} +;

RUN mkdir /etc/servicefabric && \
    echo -n /home/sfuser/sfdevcluster/data > /etc/servicefabric/FabricDataRoot && \
    echo -n /home/sfuser/sfdevcluster/data/log > /etc/servicefabric/FabricLogRoot && \
    echo -n /opt/microsoft/servicefabric/bin > /etc/servicefabric/FabricBinRoot && \
    echo -n /opt/microsoft/servicefabric/bin/Fabric/Fabric.Code > /etc/servicefabric/FabricCodePath

FROM ubuntu:16.04

RUN echo 'debconf debconf/frontend select Noninteractive' | debconf-set-selections

RUN apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 417A0893 && \
    apt-get update && \
    apt-get install --assume-yes apt-transport-https libssh2-1 libxml2 cgroup-bin liblttng-ust0 libssl-dev curl
    
ADD https://packages.microsoft.com/config/ubuntu/16.04/packages-microsoft-prod.deb packages-microsoft-prod.deb
RUN dpkg -i packages-microsoft-prod.deb

RUN apt-get update && \
    apt-get install -y dotnet-sdk-6.0

COPY --from=build /etc/servicefabric /etc/servicefabric
COPY --from=build /opt/microsoft/servicefabric/bin/Fabric/Fabric.Code /opt/microsoft/servicefabric/bin/Fabric/Fabric.Code

ENV LD_LIBRARY_PATH=/opt/microsoft/servicefabric/bin/Fabric/Fabric.Code