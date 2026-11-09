
# Stage 1: Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

# Stage 2: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["RaceDayMVC.csproj", "./"]

RUN dotnet restore "RaceDayMVC.csproj"

COPY . .

RUN dotnet build "RaceDayMVC.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/build

# Stage 3: Publish the application
FROM build AS publish

RUN dotnet publish "RaceDayMVC.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false

# Stage 4: Final container
FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "RaceDayMVC.dll"]
