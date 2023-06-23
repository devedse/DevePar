#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0-bullseye-slim-amd64 AS build
WORKDIR /src
COPY ["DevePar.ConsoleApp/DevePar.ConsoleApp.csproj", "DevePar.ConsoleApp/"]
COPY ["DevePar/DevePar.csproj", "DevePar/"]
RUN dotnet restore "DevePar.ConsoleApp/DevePar.ConsoleApp.csproj"
COPY . .
WORKDIR "/src/DevePar.ConsoleApp"
RUN dotnet build "DevePar.ConsoleApp.csproj" -c Release -o /app/build

FROM build AS publish
ARG BUILD_VERSION
ARG VER=${BUILD_VERSION:-1.0.0}
RUN dotnet publish "DevePar.ConsoleApp.csproj" -c Release -o /app/publish /p:Version=$VER /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DevePar.ConsoleApp.dll"]