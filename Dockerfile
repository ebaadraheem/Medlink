FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

COPY ["MedLink.Web/MedLink.Web.csproj", "MedLink.Web/"]
COPY ["MedLink.Presenter/MedLink.Presenter.csproj", "MedLink.Presenter/"]
COPY ["MedLink.Model/MedLink.Model.csproj", "MedLink.Model/"]
RUN dotnet restore "MedLink.Web/MedLink.Web.csproj"

COPY . .
WORKDIR "/src/MedLink.Web"
RUN dotnet publish "MedLink.Web.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MedLink.Web.dll"]