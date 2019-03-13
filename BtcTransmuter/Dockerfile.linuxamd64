FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY ["BtcTransmuter/BtcTransmuter.csproj", "BtcTransmuter/"]
RUN dotnet restore "BtcTransmuter/BtcTransmuter.csproj"
COPY . .
WORKDIR "/src/BtcTransmuter"
RUN dotnet build "BtcTransmuter.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "BtcTransmuter.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENV ConnectionStrings_Database="Data Source=data/btctransmuter.db;"

ENTRYPOINT ["/app/entrypoint.sh"]