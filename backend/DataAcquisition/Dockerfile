FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["DataAcquisition.csproj", "./"]
RUN dotnet restore "DataAcquisition.csproj"
COPY . .
RUN dotnet build "DataAcquisition.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DataAcquisition.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DataAcquisition.dll"]
