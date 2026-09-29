FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copier sy restaurer ny dependencies
COPY ["AttendanceManagement/AttendanceManagement.csproj", "AttendanceManagement/"]
RUN dotnet restore "AttendanceManagement/AttendanceManagement.csproj"

# Copier ny rakitra rehetra sy build
COPY . .
WORKDIR "/src/AttendanceManagement"
RUN dotnet publish "AttendanceManagement.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Image final de production
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
ENTRYPOINT ["dotnet", "AttendanceManagement.dll"]