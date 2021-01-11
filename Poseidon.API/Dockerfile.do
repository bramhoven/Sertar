FROM mcr.microsoft.com/dotnet/aspnet:3.1

WORKDIR /app

COPY bin/Release/netcoreapp3.1/ ./
COPY appsettings.json ./
COPY NLog.config ./

EXPOSE 80

ENTRYPOINT ["dotnet", "Poseidon.Api.dll"]