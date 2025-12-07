# Work Order Service Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5011

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["WorkOrderService.csproj", "./"]
RUN dotnet restore "WorkOrderService.csproj"
COPY . .
RUN dotnet build "WorkOrderService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WorkOrderService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WorkOrderService.dll"]
