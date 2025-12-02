# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Dependencias
COPY GestionAgenda.csproj .
RUN dotnet restore

# Todo el proyecto y compila
COPY . .
RUN dotnet publish -c Release -o /app/publish



FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080


ENTRYPOINT ["dotnet", "GestionAgenda.dll"]
