# syntax=docker/dockerfile:1
FROM node:24.11.0-alpine AS spa
RUN npm install --global pnpm@10.33.2
WORKDIR /src/forever-pin.frontend-services
COPY forever-pin.frontend-services/package.json forever-pin.frontend-services/pnpm-lock.yaml ./
RUN pnpm install --frozen-lockfile
COPY forever-pin.frontend-services/ ./
RUN pnpm build

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS source
WORKDIR /src
COPY forever-pin.backend-services/ ./

FROM source AS management-build
ARG TARGETARCH
COPY --from=spa /src/forever-pin.backend-services/ForeverPin.Api/wwwroot ./ForeverPin.Api/wwwroot
RUN case "$TARGETARCH" in amd64) rid=linux-x64 ;; arm64) rid=linux-arm64 ;; *) exit 1 ;; esac \
    && dotnet publish ForeverPin.Api/ForeverPin.Api.csproj -c Release -o /app -r "$rid" --self-contained false -p:BuildSpa=false

FROM source AS redirect-build
ARG TARGETARCH
RUN case "$TARGETARCH" in amd64) rid=linux-x64 ;; arm64) rid=linux-arm64 ;; *) exit 1 ;; esac \
    && dotnet publish ForeverPin.Redirect.Api/ForeverPin.Redirect.Api.csproj -c Release -o /app -r "$rid" --self-contained false

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
COPY --from=redirect-build /app ./
ENTRYPOINT ["dotnet", "ForeverPin.Redirect.Api.dll"]

FROM runtime AS management
USER root
RUN mkdir -p /data/keys && chown app:app /data/keys
USER app
ENV DataProtection__KeyPath=/data/keys
COPY --from=management-build /app ./
ENTRYPOINT ["dotnet", "ForeverPin.Api.dll"]
