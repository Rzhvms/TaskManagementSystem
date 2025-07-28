FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY TaskService/TaskService.csproj TaskService/
COPY HealthChecksLibrary/HealthChecksLibrary.csproj HealthChecksLibrary/

RUN dotnet restore TaskService/TaskService.csproj

COPY TaskService/ TaskService/
COPY HealthChecksLibrary/ HealthChecksLibrary/

WORKDIR /src/TaskService

RUN dotnet publish "TaskService.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 5000

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5000

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TaskService.dll"]
