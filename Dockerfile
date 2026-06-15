FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY MonthSpendings/MonthSpendings.csproj MonthSpendings/
COPY Domain/Domain.csproj Domain/
COPY Application/Application.csproj Application/
COPY Infrastructure/Infrastructure.csproj Infrastructure/
COPY EnableBanking/EnableBanking/EnableBanking.csproj EnableBanking/EnableBanking/
COPY MonthSpendings.sln ./

RUN dotnet restore MonthSpendings/MonthSpendings.csproj \
    --disable-parallel \
    --force \
    --ignore-failed-sources

COPY . .

RUN dotnet publish MonthSpendings/MonthSpendings.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "MonthSpendings.dll"]