FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY FeedbackMap/FeedbackMap.csproj FeedbackMap/
RUN dotnet restore FeedbackMap/FeedbackMap.csproj
COPY FeedbackMap/ FeedbackMap/
RUN dotnet publish FeedbackMap/FeedbackMap.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "FeedbackMap.dll"]
