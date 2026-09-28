FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["AttendanceManagement/AttendanceManagement.csproj", "AttendanceManagement/"]
RUN dotnet restore "AttendanceManagement/AttendanceManagement.csproj"

COPY . .
WORKDIR "/src/AttendanceManagement"
RUN dotnet publish "AttendanceManagement.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "AttendanceManagement.dll"]