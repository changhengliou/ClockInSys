FROM microsoft/dotnet:1.1-sdk

ENV INSTALL_PATH=/usr/local/app EXECUTEABLE=ReactSpa.dll

WORKDIR ${INSTALL_PATH}

RUN curl -sL https://deb.nodesource.com/setup_8.x | /bin/bash - && \
    apt-get install -y nodejs && \
    adduser --disabled-password app

COPY Controllers ${INSTALL_PATH}/Controllers

COPY Data ${INSTALL_PATH}/Data

COPY Extension ${INSTALL_PATH}/Extension

COPY Utils ${INSTALL_PATH}/Utils

COPY Views ${INSTALL_PATH}/Views

COPY wwwroot ${INSTALL_PATH}/wwwroot

COPY ClientApp ${INSTALL_PATH}/ClientApp

COPY ./appsettings.json ./global.json ./Startup.cs \
    ./Program.cs ./ReactSpa.csproj ./ReactSpa.csproj.user \
    ./webpack.config.js ./webpack.config.vendor.js ./package.json \
    ./ReactSpa.sln ./web.config ${INSTALL_PATH}/

RUN dotnet restore 

RUN dotnet publish -o ${INSTALL_PATH}/publish

COPY ./cert.pfx ${INSTALL_PATH}/publish

USER app

CMD [ "dotnet", "./publish/ReactSpa.dll" ]
