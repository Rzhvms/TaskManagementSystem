FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY AuthService/AuthService.csproj AuthService/
COPY HealthChecksLibrary/HealthChecksLibrary.csproj HealthChecksLibrary/

RUN dotnet restore AuthService/AuthService.csproj

COPY AuthService/ AuthService/
COPY HealthChecksLibrary/ HealthChecksLibrary/

WORKDIR /src/AuthService

RUN dotnet publish "AuthService.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 5000

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5000

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AuthService.dll"]
