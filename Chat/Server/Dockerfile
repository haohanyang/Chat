FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Chat/Server/Chat.Server.csproj", "Chat/Server/"]
COPY ["Chat/Client/Chat.Client.csproj", "Chat/Client/"]
COPY ["Chat/Shared/Chat.Shared.csproj", "Chat/Shared/"]

RUN dotnet restore "Chat/Server/Chat.Server.csproj"
COPY . .

RUN curl -fsSL https://deb.nodesource.com/setup_16.x | bash - \
    && apt-get install -y nodejs

ARG b2c_authority
ARG b2c_clientid
ENV AZADB2C_AUTHORITY $b2c_authority
ENV AZADB2C_CLIENTID $b2c_clientid

RUN npm ci && npm run build

WORKDIR "/src/Chat/Server"
RUN dotnet build "Chat.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Chat.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Chat.Server.dll"]
