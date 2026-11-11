
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Copy project file
COPY ["src/Werewolf.Server/Werewolf.Server.csproj", "src/Werewolf.Server/"]

# Restore dependencies
RUN dotnet restore "src/Werewolf.Server/Werewolf.Server.csproj"

# Copy seluruh source code
COPY . .

# Publish application
WORKDIR /src/src/Werewolf.Server

RUN dotnet publish "Werewolf.Server.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 10000

ENTRYPOINT ["sh", "-c", "dotnet Werewolf.Server.dll --urls http://0.0.0.0:${PORT:-10000}"]
