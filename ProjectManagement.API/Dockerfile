# syntax=docker/dockerfile:1.7

# -------- Build stage --------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files first for better layer caching.
COPY ProjectManagement.Domain/ProjectManagement.Domain.csproj ProjectManagement.Domain/
COPY ProjectManagement.Application/ProjectManagement.Application.csproj ProjectManagement.Application/
COPY ProjectManagement.Infrastructure/ProjectManagement.Infrastructure.csproj ProjectManagement.Infrastructure/
COPY ProjectManagement.API/ProjectManagement.API.csproj ProjectManagement.API/

RUN dotnet restore ProjectManagement.API/ProjectManagement.API.csproj

# Copy the rest of the sources and publish.
COPY ProjectManagement.Domain/ ProjectManagement.Domain/
COPY ProjectManagement.Application/ ProjectManagement.Application/
COPY ProjectManagement.Infrastructure/ ProjectManagement.Infrastructure/
COPY ProjectManagement.API/ ProjectManagement.API/ 

RUN dotnet publish ProjectManagement.API/ProjectManagement.API.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# -------- Runtime stage --------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ProjectManagement.API.dll"]
