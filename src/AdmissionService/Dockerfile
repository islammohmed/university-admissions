FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/AdmissionService/AdmissionService.csproj", "AdmissionService/"]
COPY ["src/Shared.Contracts/Shared.Contracts.csproj", "Shared.Contracts/"]
RUN dotnet restore "AdmissionService/AdmissionService.csproj"
COPY src/ .
WORKDIR "/src/AdmissionService"
RUN dotnet build "AdmissionService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AdmissionService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AdmissionService.dll"]
