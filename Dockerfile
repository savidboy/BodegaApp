ARG DOTNET_VERSION=10.0

# ---------- Runtime base ----------
FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS base
WORKDIR /app
EXPOSE 8080

# ---------- Build + Publish ----------
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS publish
WORKDIR /src

COPY ["BodegaApp.csproj", "./"]
RUN dotnet restore "BodegaApp.csproj"

COPY . .
RUN dotnet publish "BodegaApp.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false \
    --no-restore

# ---------- Final image ----------
FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .

USER app

HEALTHCHECK --interval=30s --timeout=5s --start-period=20s --retries=3 \
  CMD ["dotnet", "--info"]

ENTRYPOINT ["dotnet", "BodegaApp.dll"]
