FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5019

ENV ASPNETCORE_URLS=http://+:5019
ENV TZ=Europe/Moscow

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Bredinin.AlloyEditor.WebApi/Bredinin.AlloyEditor.WebApi.csproj", "Bredinin.AlloyEditor.WebApi/"]
RUN dotnet restore "./Bredinin.AlloyEditor.WebApi/Bredinin.AlloyEditor.WebApi.csproj"
COPY . .
WORKDIR "/src/Bredinin.AlloyEditor.WebApi"
RUN dotnet build "./Bredinin.AlloyEditor.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "./Bredinin.AlloyEditor.WebApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Bredinin.AlloyEditor.WebApi.dll"]