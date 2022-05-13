FROM mcr.microsoft.com/dotnet/sdk:6.0 as build
WORKDIR /app
COPY . .
WORKDIR /app/AmadeusProvider

RUN dotnet restore
RUN dotnet build

FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build /app/AmadeusProvider/bin/Debug/net6.0 .

ENTRYPOINT [ "dotnet", "AmadeusProvider.dll" ]