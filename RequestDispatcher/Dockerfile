FROM mcr.microsoft.com/dotnet/sdk:6.0 as build
WORKDIR /app
COPY . .
WORKDIR /app/RequestDispatcher

RUN dotnet restore
RUN dotnet build

FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build /app/RequestDispatcher/bin/Debug/net6.0 .

ENTRYPOINT [ "dotnet", "RequestDispatcher.dll" ]