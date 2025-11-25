# 1. Imagem de Construção (SDK do .NET 8)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# 2. Copiar os arquivos de projeto (csproj) e restaurar dependências
# Isso aproveita o cache do Docker para ser mais rápido
COPY src/Ecommerce.Domain/*.csproj ./src/Ecommerce.Domain/
COPY src/Ecommerce.Application/*.csproj ./src/Ecommerce.Application/
COPY src/Ecommerce.Infrastructure/*.csproj ./src/Ecommerce.Infrastructure/
COPY src/Ecommerce.API/*.csproj ./src/Ecommerce.API/
# Se tiver testes, copie também, ou ignore
# COPY tests/Ecommerce.Tests/*.csproj ./tests/Ecommerce.Tests/

# Restaura as dependências
RUN dotnet restore src/Ecommerce.API/Ecommerce.API.csproj

# 3. Copiar todo o resto do código e compilar (Build & Publish)
COPY . .
RUN dotnet publish src/Ecommerce.API/Ecommerce.API.csproj -c Release -o /app/out

# 4. Imagem de Execução (Apenas o Runtime, mais leve)
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Expõe a porta 8080 (padrão do .NET 8 no Docker)
EXPOSE 8080

# Comando para iniciar a API
ENTRYPOINT ["dotnet", "Ecommerce.API.dll"]