FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src
RUN apk add --no-cache clang zlib-static
COPY src/Mittens/Mittens.csproj .
RUN dotnet restore
COPY src/Mittens/ .
RUN dotnet publish -c Release -o /publish

FROM mcr.microsoft.com/dotnet/runtime-deps:10.0-alpine AS runtime
WORKDIR /app
COPY --from=build /publish .
EXPOSE 5070
ENV ASPNETCORE_URLS=http://0.0.0.0:5070
ENTRYPOINT ["./Mittens"]
