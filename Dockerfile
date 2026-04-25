# Etapa 1: Construcción
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de proyecto y restaurar
COPY ["Parcial_Vilchez_Cristopher.csproj", "./"]
RUN dotnet restore "Parcial_Vilchez_Cristopher.csproj"

# Copiar todo el código y publicar
COPY . .
RUN dotnet publish "Parcial_Vilchez_Cristopher.csproj" -c Release -o /app/publish

# Etapa 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Variables para Render
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Parcial_Vilchez_Cristopher.dll"]