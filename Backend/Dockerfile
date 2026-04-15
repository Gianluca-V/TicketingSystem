FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["TicketingSystem.Api/TicketingSystem.Api.csproj", "TicketingSystem.Api/"]
COPY ["TicketingSystem.Application/TicketingSystem.Application.csproj", "TicketingSystem.Application/"]
COPY ["TicketingSystem.Infrastructure/TicketingSystem.Infrastructure.csproj", "TicketingSystem.Infrastructure/"]
COPY ["TicketingSystem.Domain/TicketingSystem.Domain.csproj", "TicketingSystem.Domain/"]
RUN dotnet restore "TicketingSystem.Api/TicketingSystem.Api.csproj"
COPY . .
WORKDIR "/src/TicketingSystem.Api"
RUN dotnet build "TicketingSystem.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TicketingSystem.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TicketingSystem.Api.dll"]
