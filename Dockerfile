FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY backend.csproj .
RUN dotnet restore backend.csproj

COPY . .
RUN dotnet publish backend.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Disable config file-change watching: constrained containers (e.g. Render free tier)
# have a low inotify instance cap, and the FileSystemWatcher used to hot-reload
# appsettings.json crashes the process at boot once that cap is hit.
ENV DOTNET_hostBuilder__reloadConfigOnChange=false

ENTRYPOINT ["dotnet", "backend.dll"]
