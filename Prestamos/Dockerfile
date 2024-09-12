#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Prestamos/Prestamos.csproj", "Prestamos/"]
RUN dotnet restore "./Prestamos/./Prestamos.csproj"
COPY . .
WORKDIR "/src/Prestamos"
RUN dotnet build "./Prestamos.csproj" -c $BUILD_CONFIGURATION -o /app/build

#ENV DefaultConnection="Server=192.168.1.148,1433;Database=Sistema.Gestion.Nomina;User Id=sa;Password=123;TrustServerCertificate=True;"


FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Prestamos.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV DefaultConnection="Server=servidorssgn.database.windows.net;Database=Sistema.Gestion.Nomina;User Id=adminsql;Password=Alegre@soto2024;TrustServerCertificate=True;"

ENTRYPOINT ["dotnet", "Prestamos.dll"]

#FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
#WORKDIR /app
#
#
#COPY *.csproj ./
#RUN dotnet restore
#
#
#COPY . ./
#RUN dotnet publish -c Release -o out
#
#FROM mcr.microsoft.com/dotnet/aspnet:8.0
#WORKDIR /app
#COPY --from=build-env /app/out .
#
#EXPOSE 80
#
#ENTRYPOINT ["dotnet", "Prestamos.dll"]