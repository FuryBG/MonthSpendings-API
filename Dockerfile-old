FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

COPY MonthSpendings/MonthSpendings.csproj MonthSpendings/
COPY Domain/Domain.csproj Domain/
COPY Application/Application.csproj Application/
COPY Infrastructure/Infrastructure.csproj Infrastructure/

RUN dotnet restore MonthSpendings/MonthSpendings.csproj

COPY . .

# Publish
RUN dotnet publish MonthSpendings/MonthSpendings.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "MonthSpendings.dll"]
