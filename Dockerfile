# Etapa 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copiar el proyecto y restaurar dependencias
COPY *.csproj ./
RUN dotnet restore

# Copiar el resto del código y publicar
COPY . . 
RUN dotnet publish -c Release -o /out

# Etapa 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copiar la salida de la etapa de build
COPY --from=build /out ./

# Exponer el puerto en el que Kestrel va a escuchar
EXPOSE 8080

# Arrancar la aplicación
ENTRYPOINT ["dotnet", "GestionAgenda.dll"]
