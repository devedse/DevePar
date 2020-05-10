# Stage 1
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS builder
WORKDIR /source

# caches restore result by copying csproj file separately
#COPY /NuGet.config /source/
COPY /DevePar/*.csproj /source/DevePar/
COPY /DevePar.ConsoleApp/*.csproj /source/DevePar.ConsoleApp/
COPY /DevePar.Tests/*.csproj /source/DevePar.Tests/
COPY /DevePar.sln /source/
RUN ls
RUN dotnet restore

# copies the rest of your code
COPY . .
RUN dotnet build --configuration Release
RUN dotnet test --configuration Release ./DevePar.Tests/DevePar.Tests.csproj
RUN dotnet publish ./DevePar.ConsoleApp/DevePar.ConsoleApp.csproj --output /app/ --configuration Release

# Stage 2
FROM mcr.microsoft.com/dotnet/core/runtime:3.1-alpine
WORKDIR /app
COPY --from=builder /app .
ENTRYPOINT ["dotnet", "DevePar.ConsoleApp.dll"]