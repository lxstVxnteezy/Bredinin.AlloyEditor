FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5030

ENV ASPNETCORE_URLS=http://+:5030
ENV TZ=Europe/Moscow

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Bredinin.AlloyEditor.Gateway/Bredinin.AlloyEditor.Gateway.csproj", "Bredinin.AlloyEditor.Gateway/"]
RUN dotnet restore "./Bredinin.AlloyEditor.Gateway/Bredinin.AlloyEditor.Gateway.csproj"
COPY . .
WORKDIR "/src/Bredinin.AlloyEditor.Gateway"
RUN dotnet build "./Bredinin.AlloyEditor.Gateway.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Этот этап используется для публикации проекта службы, который будет скопирован на последний этап
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Bredinin.AlloyEditor.Gateway.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Этот этап используется в рабочей среде или при запуске из VS в обычном режиме (по умолчанию, когда конфигурация отладки не используется)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Bredinin.AlloyEditor.Gateway.dll"]