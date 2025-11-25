FROM mcr.microsoft.com/dotnet/aspnet:8.0-nanoserver-1809 AS base
WORKDIR /app
EXPOSE 5013

ENV ASPNETCORE_URLS=http://+:5013

FROM mcr.microsoft.com/dotnet/sdk:8.0-nanoserver-1809 AS build
ARG configuration=Release
WORKDIR /src
COPY ["src/Ecommerce.API/Ecommerce.API.csproj", "src/Ecommerce.API/"]
RUN dotnet restore "src\Ecommerce.API\Ecommerce.API.csproj"
COPY . .
WORKDIR "/src/src/Ecommerce.API"
RUN dotnet build "Ecommerce.API.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "Ecommerce.API.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ecommerce.API.dll"]
