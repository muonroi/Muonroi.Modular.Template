FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Host/Muonroi.Modular.Host/Muonroi.Modular.Host.csproj", "src/Host/Muonroi.Modular.Host/"]
COPY ["src/Shared/Muonroi.Modular.Shared/Muonroi.Modular.Shared.csproj", "src/Shared/Muonroi.Modular.Shared/"]
COPY ["src/Shared/Muonroi.Modular.Kernel/Muonroi.Modular.Kernel.csproj", "src/Shared/Muonroi.Modular.Kernel/"]
COPY ["src/Modules/Catalog/Muonroi.Modular.Modules.Catalog.csproj", "src/Modules/Catalog/Muonroi.Modular.Modules.Catalog.csproj"]
COPY ["src/Modules/Identity/Muonroi.Modular.Modules.Identity.csproj", "src/Modules/Identity/Muonroi.Modular.Modules.Identity.csproj"]
RUN dotnet restore "./src/Host/Muonroi.Modular.Host/Muonroi.Modular.Host.csproj"
COPY . .
WORKDIR "/src/src/Host/Muonroi.Modular.Host"
RUN dotnet build "./Muonroi.Modular.Host.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Muonroi.Modular.Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Muonroi.Modular.Host.dll"]
