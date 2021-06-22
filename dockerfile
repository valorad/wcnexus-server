# ============================================================
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS builder

WORKDIR /src

ADD ./WCNexus.App ./

RUN dotnet restore \
&& dotnet publish -c Release -o /dist

# ============================================================
FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine AS runner

ARG BASE_DIR=/workspace/www/wcnexus.com

WORKDIR ${BASE_DIR}

COPY --from=builder /dist .

RUN chown 1000:1000 -R ${BASE_DIR}

VOLUME ${BASE_DIR}"/appsettings.Production.yaml"
VOLUME ${BASE_DIR}"/secrets.yaml"

EXPOSE 5000

CMD ["dotnet", "WCNexus.App.dll"]