
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5019

ENV ASPNETCORE_URLS=http://+:5019
ENV TZ=Europe/Moscow


# Этот этап используется для сборки проекта службы
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Bredinin.AlloyEditor.WebApi/Bredinin.AlloyEditor.WebApi.csproj", "Bredinin.AlloyEditor.WebApi/"]
COPY ["Bredinin.AlloyEditor.Core.Authentication/Bredinin.AlloyEditor.Core.Authentication.csproj", "Bredinin.AlloyEditor.Core.Authentication/"]
COPY ["Bredinin.AlloyEditor.Core.Http/Bredinin.AlloyEditor.Core.Http.csproj", "Bredinin.AlloyEditor.Core.Http/"]
COPY ["Bredinin.AlloyEditor.Core.Swagger/Bredinin.AlloyEditor.Core.Swagger.csproj", "Bredinin.AlloyEditor.Core.Swagger/"]
COPY ["Bredinin.AlloyEditor.DAL.Context/Bredinin.AlloyEditor.DAL.Context.csproj", "Bredinin.AlloyEditor.DAL.Context/"]
COPY ["Bredinin.AlloyEditor.Domain/Bredinin.AlloyEditor.Domain.csproj", "Bredinin.AlloyEditor.Domain/"]
COPY ["Bredinin.AlloyEditor.DAL.Core/Bredinin.AlloyEditor.DAL.Core.csproj", "Bredinin.AlloyEditor.DAL.Core/"]
COPY ["Bredinin.AlloyEditor.DAL.Migration/Bredinin.AlloyEditor.DAL.Migration.csproj", "Bredinin.AlloyEditor.DAL.Migration/"]
COPY ["Bredinin.AlloyEditor.Handlers/Bredinin.AlloyEditor.Handlers.csproj", "Bredinin.AlloyEditor.Handlers/"]
COPY ["Bredinin.AlloyEditor.Contracts/Bredinin.AlloyEditor.Contracts.csproj", "Bredinin.AlloyEditor.Contracts/"]
RUN dotnet restore "./Bredinin.AlloyEditor.WebApi/Bredinin.AlloyEditor.WebApi.csproj"
COPY . .
WORKDIR "/src/Bredinin.AlloyEditor.WebApi"
RUN dotnet build "./Bredinin.AlloyEditor.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Этот этап используется для публикации проекта службы, который будет скопирован на последний этап
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Bredinin.AlloyEditor.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Этот этап используется в рабочей среде или при запуске из VS в обычном режиме (по умолчанию, когда конфигурация отладки не используется)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Bredinin.AlloyEditor.WebApi.dll"]