# API do JP Help Desk — imagem em dois estágios.
# Build (a partir da raiz do repositório):  docker build -t jphelpdesk-api backend/JpHelpDesk.Api

# ---------- 1. Build: SDK completo, só para compilar ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Primeiro só o .csproj: o restore fica em cache e só roda de novo quando os pacotes mudam.
COPY JpHelpDesk.Api.csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish --configuration Release --no-restore --output /app /p:UseAppHost=false

# ---------- 2. Runtime: só o ASP.NET Core, sem SDK nem código-fonte ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

# Usuário sem privilégios de root, já existente na imagem oficial.
USER $APP_UID

# A imagem oficial já escuta na porta 8080 (ASPNETCORE_HTTP_PORTS=8080).
EXPOSE 8080
ENTRYPOINT ["dotnet", "JpHelpDesk.Api.dll"]
