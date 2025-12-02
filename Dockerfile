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

# 1)docker login
# 2)docker tag reportauts-api jerh011/reportauts-api:latest
# 3)docker push jerh011/reportauts-api:latest
# 4)docker build -t reportauts-api .
# 5)docker run -d -p 5000:5000 --name reportauts-container reportauts-api  
# 6)docker pull jerh011/reportauts-api:latest 
