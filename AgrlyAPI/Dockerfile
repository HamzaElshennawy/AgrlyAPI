# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Use SDK 8 to match project target (net8.0). Delete repo global.json before build to avoid SDK version mismatch.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# copy csproj and restore dependencies
COPY ["AgrlyAPI/AgrlyAPI.csproj", "AgrlyAPI/"]
RUN dotnet restore "./AgrlyAPI/AgrlyAPI.csproj"

# copy rest of source
COPY . .

# Remove global.json from build context if present to avoid SDK resolution errors
RUN rm -f /src/global.json || true

WORKDIR "/src/AgrlyAPI"
RUN dotnet build "AgrlyAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

# publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AgrlyAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AgrlyAPI.dll"]