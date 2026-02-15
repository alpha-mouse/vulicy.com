# --- 1. Build UI (Node.js) ---
FROM node:22-alpine AS ui-build
WORKDIR /src/Vulicy.UI
COPY Vulicy.UI/package*.json ./
RUN npm install
COPY Vulicy.UI/ ./
# This will output to ../Vulicy.Web/wwwroot as per vite.config.js
RUN npm run build

# --- 2. Build Backend (.NET SDK) ---
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
RUN apk add --no-cache icu-libs
ARG COMMIT_HASH
ARG COMMIT_TIME
WORKDIR /src

# Copy project files for restore
COPY Vulicy.slnx ./
COPY Vulicy.Web/*.csproj ./Vulicy.Web/
COPY Vulicy.DB/*.csproj ./Vulicy.DB/
COPY Vulicy.Domain/*.csproj ./Vulicy.Domain/
COPY Vulicy.Services/*.csproj ./Vulicy.Services/
RUN dotnet restore Vulicy.Web/Vulicy.Web.csproj

# Copy all source code
COPY . .

# Copy UI build artifacts into the Web project's wwwroot
COPY --from=ui-build /src/Vulicy.Web/wwwroot ./Vulicy.Web/wwwroot

# Create version.json in wwwroot
RUN COMMIT_TIME_UTC=$(date -u -d "@${COMMIT_TIME}" +%Y-%m-%dT%H:%M:%SZ) && \
    echo "{\"commitHash\": \"${COMMIT_HASH}\", \"commitTime\": \"${COMMIT_TIME_UTC}\", \"buildTime\": \"$(date -u +%Y-%m-%dT%H:%M:%SZ)\"}" > ./Vulicy.Web/wwwroot/version.json

# Publish
RUN dotnet publish Vulicy.Web/Vulicy.Web.csproj -c Release -o /app/publish /p:UseAppHost=false

# --- 3. Final Runtime (ASP.NET) ---
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Vulicy.Web.dll"]
