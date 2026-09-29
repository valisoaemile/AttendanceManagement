FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copier ny rakitra .csproj avy eo amin'ny racine
COPY ["AttendanceManagement.csproj", "./"]
RUN dotnet restore "AttendanceManagement.csproj"

# Copier ny rakitra rehetra sy hanao publish
COPY . .
RUN dotnet publish "AttendanceManagement.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Image final amin'ny execution
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
ENTRYPOINT ["dotnet", "AttendanceManagement.dll"]