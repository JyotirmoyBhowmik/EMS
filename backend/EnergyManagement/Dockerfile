# Energy Management Service Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5010

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["EnergyManagement.csproj", "./"]
RUN dotnet restore "Energy Management.csproj"
COPY . .
RUN dotnet build "EnergyManagement.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EnergyManagement.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EnergyManagement.dll"]
