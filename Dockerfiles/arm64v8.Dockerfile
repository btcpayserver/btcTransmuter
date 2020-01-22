FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
RUN apt-get update \
	&& apt-get install -qq --no-install-recommends qemu qemu-user-static qemu-user binfmt-support
WORKDIR /src
COPY . .
WORKDIR "/src/BtcTransmuter"
RUN dotnet restore
RUN dotnet publish -c Release -o /app


FROM mcr.microsoft.com/dotnet/core/aspnet:3.1.0-buster-slim-arm64v8  AS runtime
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT false
COPY --from=build /usr/bin/qemu-aarch64-static /usr/bin/qemu-aarch64-static
RUN apt-get update && apt-get install -y --no-install-recommends iproute2 openssh-client \
    && rm -rf /var/lib/apt/lists/* 

ENV LC_ALL en_US.UTF-8
ENV LANG en_US.UTF-8
WORKDIR /app
EXPOSE 80
EXPOSE 443
WORKDIR /app
COPY --from=publish /app .

ENV TRANSMUTER_Database="Data Source=data/btctransmuter.db;"
ENV TRANSMUTER_DatabaseType="sqlite"
ENV TRANSMUTER_DataProtectionDir="data"

ENTRYPOINT ["dotnet", "BtcTransmuter.dll"]