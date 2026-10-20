# syntax=docker/dockerfile:1

# ForeverPin's two images from one file: `management` (the API serving the Vue app) and `redirect` (the scan host).
# The build context MUST be engineering/codebase/, which holds both code directories.
#
#   docker build --build-arg APP_VERSION=0.10.99 --build-arg VCS_REF=$(git rev-parse HEAD) --target management \
#     -f engineering/deployment/Dockerfile -t foreverpin-management:local engineering/codebase
#
# The images hold no environment: a target's settings file (/app/appsettings.Target.json) supplies it at run time.

# ---- The Vue workspace ----
FROM node:24.11.0-alpine AS spa
RUN npm install --global pnpm@10.33.2
WORKDIR /src/forever-pin.frontend-services
COPY forever-pin.frontend-services/package.json forever-pin.frontend-services/pnpm-lock.yaml forever-pin.frontend-services/pnpm-workspace.yaml ./
COPY forever-pin.frontend-services/apps/web/package.json ./apps/web/package.json
RUN pnpm install --frozen-lockfile
COPY forever-pin.frontend-services/ ./
# The build's version from the shared publish workflow (X.Y.Z or a pre-release); unset in a local build. Declared
# after the dependency layers, since a changed build argument misses the cache of every RUN after it.
ARG APP_VERSION
RUN pnpm build

# ---- The .NET hosts ----
# Stays in global.json's 10.0.3xx band; the floating 10.0 tag moved to 10.0.4xx, which the pin refuses.
FROM mcr.microsoft.com/dotnet/sdk:10.0.302 AS source
WORKDIR /src
COPY forever-pin.backend-services/ ./

# MSBuild reads APP_VERSION from the environment; Directory.Build.props turns it into the product version. VCS_REF is
# the full commit SHA the shared publish workflow passes; the build appends it to the informational version.
FROM source AS management-build
ARG TARGETARCH
ARG APP_VERSION
ARG VCS_REF
COPY --from=spa /src/forever-pin.frontend-services/apps/web/dist ./ForeverPin.Api/wwwroot
RUN case "$TARGETARCH" in amd64) rid=linux-x64 ;; arm64) rid=linux-arm64 ;; *) exit 1 ;; esac \
    && dotnet publish ForeverPin.Api/ForeverPin.Api.csproj -c Release -o /app -r "$rid" --self-contained false -p:BuildSpa=false \
       -p:SourceRevisionId="$VCS_REF"

FROM source AS redirect-build
ARG TARGETARCH
ARG APP_VERSION
ARG VCS_REF
RUN case "$TARGETARCH" in amd64) rid=linux-x64 ;; arm64) rid=linux-arm64 ;; *) exit 1 ;; esac \
    && dotnet publish ForeverPin.Redirect.Api/ForeverPin.Redirect.Api.csproj -c Release -o /app -r "$rid" --self-contained false \
       -p:SourceRevisionId="$VCS_REF"

# ---- Runtime images ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
RUN apt-get update && apt-get install -y --no-install-recommends curl libfontconfig1 \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
USER app
EXPOSE 8080
HEALTHCHECK --interval=15s --timeout=5s --start-period=60s --retries=4 \
    CMD curl --fail --silent http://localhost:8080/health || exit 1

FROM runtime AS redirect
USER root
RUN mkdir -p /data/geo && chown app:app /data/geo
USER app
ENV IP_LOCATION_DIRECTORY=/data/geo
COPY --from=redirect-build /app ./
ENTRYPOINT ["dotnet", "ForeverPin.Redirect.Api.dll"]

FROM runtime AS management
USER root
RUN mkdir -p /data/keys && chown app:app /data/keys
USER app
ENV DataProtection__KeyPath=/data/keys
COPY --from=management-build /app ./
ENTRYPOINT ["dotnet", "ForeverPin.Api.dll"]
