FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY NotificationService/NotificationService.csproj NotificationService/
COPY HealthChecksLibrary/HealthChecksLibrary.csproj HealthChecksLibrary/

RUN dotnet restore NotificationService/NotificationService.csproj

COPY NotificationService/ NotificationService/
COPY HealthChecksLibrary/ HealthChecksLibrary/

WORKDIR /src/NotificationService

RUN dotnet publish "NotificationService.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 5000

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5000

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "NotificationService.dll"]
