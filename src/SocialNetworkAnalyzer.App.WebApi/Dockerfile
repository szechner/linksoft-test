FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SocialNetworkAnalyzer.App.WebApi/SocialNetworkAnalyzer.App.WebApi.csproj", "SocialNetworkAnalyzer.App.WebApi/"]
RUN dotnet restore "SocialNetworkAnalyzer.App.WebApi/SocialNetworkAnalyzer.App.WebApi.csproj"
COPY . .
WORKDIR "/src/SocialNetworkAnalyzer.App.WebApi"
RUN dotnet build "SocialNetworkAnalyzer.App.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "SocialNetworkAnalyzer.App.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SocialNetworkAnalyzer.App.WebApi.dll"]
