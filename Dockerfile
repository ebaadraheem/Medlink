
# 1. Use the official .NET 8 ASP.NET runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# 2. Use the SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 3. Copy the project files and restore dependencies
COPY ["MedLink.Web/MedLink.Web.csproj", "MedLink.Web/"]
COPY ["MedLink.Presenter/MedLink.Presenter.csproj", "MedLink.Presenter/"]
COPY ["MedLink.Model/MedLink.Model.csproj", "MedLink.Model/"]
RUN dotnet restore "MedLink.Web/MedLink.Web.csproj"

# 4. Copy the rest of the code and build
COPY . .
WORKDIR "/src/MedLink.Web"
RUN dotnet build "MedLink.Web.csproj" -c Release -o /app/build
RUN dotnet publish "MedLink.Web.csproj" -c Release -o /app/publish

# 5. Finalize the image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MedLink.Web.dll"]