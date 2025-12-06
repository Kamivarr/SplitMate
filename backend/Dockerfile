# build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# kopiuje csproj i restore
COPY backend/SplitMate.Api.csproj backend/
RUN dotnet restore backend/SplitMate.Api.csproj

# kopiuje resztę i publikuj
COPY . .
WORKDIR /src/backend
RUN dotnet publish -c Release -o /app/publish

# runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80
ENTRYPOINT ["dotnet", "SplitMate.Api.dll"]
