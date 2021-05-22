FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80

ENV ASPNETCORE_URLS=http://+:80

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["Antiques-Auction-WebApp.csproj", "./"]
RUN dotnet restore "Antiques-Auction-WebApp.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "Antiques-Auction-WebApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Antiques-Auction-WebApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Antiques-Auction-WebApp.dll"]
