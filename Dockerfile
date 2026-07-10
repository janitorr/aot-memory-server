FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src
RUN apk add --no-cache clang zlib-static
COPY src/Mittens.Core/Mittens.Core.csproj Mittens.Core/
COPY src/Mittens.Host/Mittens.Host.csproj Mittens.Host/
RUN dotnet restore Mittens.Host/Mittens.Host.csproj
COPY src/Mittens.Core/ Mittens.Core/
COPY src/Mittens.Host/ Mittens.Host/
RUN dotnet publish Mittens.Host/Mittens.Host.csproj -c Release -o /publish

FROM mcr.microsoft.com/dotnet/runtime-deps:10.0-alpine AS runtime
WORKDIR /app
COPY --from=build /publish .
EXPOSE 5070
ENV ASPNETCORE_URLS=http://0.0.0.0:5070
ENTRYPOINT ["./Mittens.Host"]
