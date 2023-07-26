FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Chat/Client/Chat.Client.csproj", "Chat/Client/"]
COPY ["Chat/Shared/Chat.Shared.csproj", "Chat/Shared/"]
RUN dotnet restore "Chat/Client/Chat.Client.csproj"
COPY . .

RUN curl -fsSL https://deb.nodesource.com/setup_16.x | bash - \
    && apt-get install -y nodejs
RUN npm ci && npm run mock

WORKDIR "/src/Chat/Client"
RUN dotnet build "Chat.Client.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Chat.Client.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Chat.Client.dll"]