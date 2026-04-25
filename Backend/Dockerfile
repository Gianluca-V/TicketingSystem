FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Backend/TicketingSystem.Api/TicketingSystem.Api.csproj", "Backend/TicketingSystem.Api/"]
COPY ["Backend/TicketingSystem.Application/TicketingSystem.Application.csproj", "Backend/TicketingSystem.Application/"]
COPY ["Backend/TicketingSystem.Infrastructure/TicketingSystem.Infrastructure.csproj", "Backend/TicketingSystem.Infrastructure/"]
COPY ["Backend/TicketingSystem.Domain/TicketingSystem.Domain.csproj", "Backend/TicketingSystem.Domain/"]

RUN dotnet restore "Backend/TicketingSystem.Api/TicketingSystem.Api.csproj"

COPY . .

WORKDIR "/src/Backend/TicketingSystem.Api"
RUN dotnet build "TicketingSystem.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TicketingSystem.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TicketingSystem.Api.dll"]
