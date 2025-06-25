# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0-bookworm-slim AS build
WORKDIR /src

# Copy everything into the container
COPY . .

# Restore dependencies
RUN dotnet restore DataGEMS.Gateway.sln

# Build and publish the app
RUN dotnet publish DataGEMS.Gateway.Api/DataGEMS.Gateway.Api.csproj -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim AS runtime
WORKDIR /app

# Copy published app from build stage
COPY --from=build /app/publish .

RUN rm -f /app/*.Development.json

# Set entry point
ENTRYPOINT ["dotnet", "DataGEMS.Gateway.Api.dll"]