# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copiar la solución y los proyectos
COPY ApiReportaUTS.sln ./
COPY *.csproj ./

# Restaurar dependencias explícitamente de la solución
RUN dotnet restore ApiReportaUTS.sln

# Copiar todo el código
COPY . ./

# Publicar la solución
RUN dotnet publish ApiReportaUTS.sln -c Release -o out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Variables de entorno (puedes sobreescribirlas al correr el contenedor)
ENV ASPNETCORE_ENVIRONMENT=Development
ENV CONNECTION_STRING="Host=ep-nameless-surf-a8knfktc-pooler.eastus2.azure.neon.tech;Database=neondb;Username=neondb_owner;Password=npg_OkU8t6yXZLDu;SSL Mode=VerifyFull;Channel Binding=Require;"
ENV JWT_SECRET="superSecretKeysuperSecretKeyssuperSecretKeyssuperSecretKeyssuperSecretKeysuperSecretKey"
ENV RT_SECRET="superSecretKeysuperSecretKeyssuperSecretKeyssuperSecretKeyssuperSecretKeysuperSecretKey"

# Copiar los archivos publicados desde la etapa de build
COPY --from=build /app/out ./

EXPOSE 5000
ENTRYPOINT ["dotnet", "ReportaUTS.dll"]
