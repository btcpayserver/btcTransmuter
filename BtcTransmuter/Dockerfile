FROM microsoft/dotnet:2.2-sdk  AS base
WORKDIR /app
EXPOSE 5000
EXPOSE 5001

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY ["BtcTransmuter/BtcTransmuter.csproj", "BtcTransmuter/"]
RUN dotnet restore "BtcTransmuter/BtcTransmuter.csproj"
COPY . .
WORKDIR "/src/BtcTransmuter"

ENTRYPOINT ["dotnet", "run"]