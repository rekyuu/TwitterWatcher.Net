FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["TwitterWatcher/TwitterWatcher.csproj", "TwitterWatcher/"]
RUN dotnet restore "TwitterWatcher/TwitterWatcher.csproj"
COPY . .
WORKDIR "/src/TwitterWatcher"
RUN dotnet publish "TwitterWatcher.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TwitterWatcher.dll"]
