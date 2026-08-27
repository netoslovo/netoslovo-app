FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Directory.Packages.props", "./"]
COPY ["Directory.Build.props", "./"]
COPY ["src/api/WordoGuessr.WebAPI/WordoGuessr.WebAPI.csproj", "src/api/WordoGuessr.WebAPI/"]
COPY ["src/api/Auth/WordoGuessr.Auth.API/WordoGuessr.Auth.API.csproj", "src/api/Auth/WordoGuessr.Auth.API/"]
COPY ["src/api/Auth/WordoGuessr.Auth.App/WordoGuessr.Auth.App.csproj", "src/api/Auth/WordoGuessr.Auth.App/"]
COPY ["src/api/Auth/WordoGuessr.Auth.Contract/WordoGuessr.Auth.Contract.csproj", "src/api/Auth/WordoGuessr.Auth.Contract/"]
COPY ["src/api/Auth/WordoGuessr.Auth.Domain/WordoGuessr.Auth.Domain.csproj", "src/api/Auth/WordoGuessr.Auth.Domain/"]
COPY ["src/api/Auth/WordoGuessr.Auth.Infra/WordoGuessr.Auth.Infra.csproj", "src/api/Auth/WordoGuessr.Auth.Infra/"]
COPY ["src/api/Common/WordoGuessr.Common.App/WordoGuessr.Common.App.csproj", "src/api/Common/WordoGuessr.Common.App/"]
COPY ["src/api/Common/WordoGuessr.Common.Dto/WordoGuessr.Common.Dto.csproj", "src/api/Common/WordoGuessr.Common.Dto/"]
COPY ["src/api/Email/WordoGuessr.Email.App/WordoGuessr.Email.App.csproj", "src/api/Email/WordoGuessr.Email.App/"]
COPY ["src/api/Email/WordoGuessr.Email.Contract/WordoGuessr.Email.Contract.csproj", "src/api/Email/WordoGuessr.Email.Contract/"]
COPY ["src/api/Email/WordoGuessr.Email.Domain/WordoGuessr.Email.Domain.csproj", "src/api/Email/WordoGuessr.Email.Domain/"]
COPY ["src/api/Email/WordoGuessr.Email.Infra/WordoGuessr.Email.Infra.csproj", "src/api/Email/WordoGuessr.Email.Infra/"]
COPY ["src/api/Game/WordoGuessr.Game.API/WordoGuessr.Game.API.csproj", "src/api/Game/WordoGuessr.Game.API/"]
COPY ["src/api/Common/WordoGuessr.API.BuildingBlocks/WordoGuessr.API.BuildingBlocks.csproj", "src/api/Common/WordoGuessr.API.BuildingBlocks/"]
COPY ["src/api/Game/WordoGuessr.Game.App/WordoGuessr.Game.App.csproj", "src/api/Game/WordoGuessr.Game.App/"]
COPY ["src/api/Words/WordoGuessr.Words.Contract/WordoGuessr.Words.Contract.csproj", "src/api/Words/WordoGuessr.Words.Contract/"]
COPY ["src/api/Common/WordoGuessr.Common.Domain/WordoGuessr.Common.Domain.csproj", "src/api/Common/WordoGuessr.Common.Domain/"]
COPY ["src/api/Game/WordoGuessr.Game.Contract/WordoGuessr.Game.Contract.csproj", "src/api/Game/WordoGuessr.Game.Contract/"]
COPY ["src/api/Game/WordoGuessr.Game.Domain/WordoGuessr.Game.Domain.csproj", "src/api/Game/WordoGuessr.Game.Domain/"]
COPY ["src/api/Game/WordoGuessr.Game.Dto/WordoGuessr.Game.Dto.csproj", "src/api/Game/WordoGuessr.Game.Dto/"]
COPY ["src/api/Game/WordoGuessr.Game.Infra/WordoGuessr.Game.Infra.csproj", "src/api/Game/WordoGuessr.Game.Infra/"]
COPY ["src/api/Game/WordoGuessr.Game.ReadModels/WordoGuessr.Game.ReadModels.csproj", "src/api/Game/WordoGuessr.Game.ReadModels/"]
COPY ["src/api/Sagas/WordoGuessr.Sagas.API/WordoGuessr.Sagas.API.csproj", "src/api/Sagas/WordoGuessr.Sagas.API/"]
COPY ["src/api/Sagas/WordoGuessr.Sagas.App/WordoGuessr.Sagas.App.csproj", "src/api/Sagas/WordoGuessr.Sagas.App/"]
COPY ["src/api/Sagas/WordoGuessr.Sagas.Contract/WordoGuessr.Sagas.Contract.csproj", "src/api/Sagas/WordoGuessr.Sagas.Contract/"]
COPY ["src/api/Sagas/WordoGuessr.Sagas.Domain/WordoGuessr.Sagas.Domain.csproj", "src/api/Sagas/WordoGuessr.Sagas.Domain/"]
COPY ["src/api/Sagas/WordoGuessr.Sagas.Dto/WordoGuessr.Sagas.Dto.csproj", "src/api/Sagas/WordoGuessr.Sagas.Dto/"]
COPY ["src/api/Sagas/WordoGuessr.Sagas.Infra/WordoGuessr.Sagas.Infra.csproj", "src/api/Sagas/WordoGuessr.Sagas.Infra/"]
COPY ["src/api/Words/WordoGuessr.Words.App/WordoGuessr.Words.App.csproj", "src/api/Words/WordoGuessr.Words.App/"]
COPY ["src/api/Words/WordoGuessr.Words.Infra/WordoGuessr.Words.Infra.csproj", "src/api/Words/WordoGuessr.Words.Infra/"]
RUN dotnet restore "./src/api/WordoGuessr.WebAPI/WordoGuessr.WebAPI.csproj"
COPY . .
WORKDIR "/src/src/api/WordoGuessr.WebAPI"
RUN dotnet run -c Release --no-launch-profile --no-restore -- codegen write

FROM build AS publish
RUN dotnet publish "./WordoGuessr.WebAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WordoGuessr.WebAPI.dll"]
