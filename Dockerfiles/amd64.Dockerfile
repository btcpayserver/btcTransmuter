FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS build
WORKDIR /src
COPY . .
WORKDIR "/src/BtcTransmuter"

ENV BTCPAY_DATADIR=/datadir
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
RUN dotnet restore
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine AS base
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT false
RUN apk add --no-cache icu-libs openssh-keygen

ENV LC_ALL en_US.UTF-8
ENV LANG en_US.UTF-8
WORKDIR /app
EXPOSE 80
EXPOSE 443
WORKDIR /app
COPY --from=build /app .

ENV TRANSMUTER_Database="Data Source=data/btctransmuter.db;"
ENV TRANSMUTER_DatabaseType="sqlite"
ENV TRANSMUTER_DataProtectionDir="data"

ENV BTCPAY_DATADIR=/datadir
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
ENTRYPOINT ["dotnet", "BtcTransmuter.dll"]