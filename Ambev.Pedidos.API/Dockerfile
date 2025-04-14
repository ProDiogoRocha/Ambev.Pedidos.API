# Etapa 1: build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY *.sln .
COPY Ambev.Pedidos.API/*.csproj ./Ambev.Pedidos.API/
RUN dotnet restore

COPY . .
WORKDIR /app/Ambev.Pedidos.API
RUN dotnet publish -c Release -o out

# Etapa 2: runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/Ambev.Pedidos.API/out ./

ENTRYPOINT ["dotnet", "Ambev.Pedidos.API.dll"]