FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 5000

RUN mkdir -p /var/www/images
RUN chmod 755 /var/www/images

RUN mkdir -p /var/log/quizzler
RUN chmod 755 /var/log/quizzler

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Quizzler-Backend.csproj", "."]
RUN dotnet restore "./Quizzler-Backend.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "Quizzler-Backend.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Quizzler-Backend.csproj" -c Release -o /app/publish /p:UseApp=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Quizzler-Backend.dll"]