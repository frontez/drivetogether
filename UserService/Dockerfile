FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
ARG buildtime_variable=default_value
ENV ASPNETCORE_HTTP_PORTS=8000
WORKDIR /app
EXPOSE 8000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["UserService.csproj", "UserService/"]
RUN dotnet restore "./UserService/UserService.csproj"

WORKDIR "/src/UserService"
COPY . .

RUN dotnet build "./UserService.csproj" -c $BUILD_CONFIGURATION -o /app/build 

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./UserService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UserService.dll"]