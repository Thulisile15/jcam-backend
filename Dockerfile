FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["JCAM CONNECT/JCAM CONNECT.csproj", "JCAM CONNECT/"]
RUN dotnet restore "JCAM CONNECT/JCAM CONNECT.csproj"
COPY . .
WORKDIR "/src/JCAM CONNECT"
RUN dotnet build "JCAM CONNECT.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "JCAM CONNECT.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "JCAM CONNECT.dll"]