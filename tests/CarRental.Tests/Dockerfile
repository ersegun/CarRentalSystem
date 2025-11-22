FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["tests/CarRental.Tests/CarRental.Tests.csproj", "tests/CarRental.Tests/"]
COPY ["src/CarRental.Application/CarRental.Application.csproj", "src/CarRental.Application/"]
COPY ["src/CarRental.Domain/CarRental.Domain.csproj", "src/CarRental.Domain/"]
COPY ["src/CarRental.Repository/CarRental.Repository.csproj", "src/CarRental.Repository/"]
RUN dotnet restore "tests/CarRental.Tests/CarRental.Tests.csproj"
COPY . .
WORKDIR "/src/tests/CarRental.Tests"
RUN dotnet build "./CarRental.Tests.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CarRental.Tests.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CarRental.Tests.dll"]
