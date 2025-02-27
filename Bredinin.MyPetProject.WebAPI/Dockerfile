FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Bredinin.MyPetProject.WebAPI/Bredinin.MyPetProject.WebAPI.csproj", "Bredinin.MyPetProject.WebAPI/"]
RUN dotnet restore "Bredinin.MyPetProject.WebAPI/Bredinin.MyPetProject.WebAPI.csproj"
COPY . .
WORKDIR "/src/Bredinin.MyPetProject.WebAPI"
RUN dotnet build "Bredinin.MyPetProject.WebAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Bredinin.MyPetProject.WebAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Bredinin.MyPetProject.WebAPI.dll"]
