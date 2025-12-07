FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ScadaCore.csproj", "./"]
RUN dotnet restore "ScadaCore.csproj"
COPY . .
RUN dotnet build "ScadaCore.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ScadaCore.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ScadaCore.dll"]
