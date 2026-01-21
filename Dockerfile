# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["OrderProcessingSystem.sln", "./"]
COPY ["src/OrderProcessingSystem.Domain/OrderProcessingSystem.Domain.csproj", "src/OrderProcessingSystem.Domain/"]
COPY ["src/OrderProcessingSystem.Application/OrderProcessingSystem.Application.csproj", "src/OrderProcessingSystem.Application/"]
COPY ["src/OrderProcessingSystem.Infrastructure/OrderProcessingSystem.Infrastructure.csproj", "src/OrderProcessingSystem.Infrastructure/"]
COPY ["src/OrderProcessingSystem.Api/OrderProcessingSystem.Api.csproj", "src/OrderProcessingSystem.Api/"]
COPY ["tests/OrderProcessingSystem.UnitTests/OrderProcessingSystem.UnitTests.csproj", "tests/OrderProcessingSystem.UnitTests/"]

RUN dotnet restore

# Copy everything else and build the release
COPY . .
WORKDIR "/src/src/OrderProcessingSystem.Api"
RUN dotnet build -c Release -o /app/build

# Publish the app
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 80
EXPOSE 443
ENV ENABLE_SWAGGER=true
ENTRYPOINT ["dotnet", "OrderProcessingSystem.Api.dll"]
