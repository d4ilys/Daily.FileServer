#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Daily.FileServer.API/Daily.FileServer.API.csproj", "Daily.FileServer.API/"]
RUN dotnet restore "Daily.FileServer.API/Daily.FileServer.API.csproj"
COPY . .
WORKDIR "/src/Daily.FileServer.API"
RUN dotnet build "Daily.FileServer.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Daily.FileServer.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Daily.FileServer.API.dll"]



