FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5020

ENV ASPNETCORE_URLS=http://+:5020
ENV TZ=Europe/Moscow

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Bredinin.AlloyEditor.Identity.Service/Bredinin.AlloyEditor.Identity.Service.csproj", "Bredinin.AlloyEditor.Identity.Service/"]
RUN dotnet restore "./Bredinin.AlloyEditor.Identity.Service/Bredinin.AlloyEditor.Identity.Service.csproj"
COPY . .
WORKDIR "/src/Bredinin.AlloyEditor.Identity.Service"
RUN dotnet build "./Bredinin.AlloyEditor.Identity.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "./Bredinin.AlloyEditor.Identity.Service.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Bredinin.AlloyEditor.Identity.Service.dll"]