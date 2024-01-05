#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["Tgs.GameRecord.SBO/Tgs.GameRecord.SBO.csproj", "Tgs.GameRecord.SBO/"]
COPY ["Tgs.GameRecord.SBO.Core/Tgs.GameRecord.SBO.Core.csproj", "Tgs.GameRecord.SBO.Core/"]
COPY ["Tgs.GameRecord.SBO.Common/Tgs.GameRecord.SBO.Common.csproj", "Tgs.GameRecord.SBO.Common/"]
RUN dotnet restore "./Tgs.GameRecord.SBO/./Tgs.GameRecord.SBO.csproj"
COPY . .
WORKDIR "/src/Tgs.GameRecord.SBO"
RUN dotnet build "./Tgs.GameRecord.SBO.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Tgs.GameRecord.SBO.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tgs.GameRecord.SBO.dll"]