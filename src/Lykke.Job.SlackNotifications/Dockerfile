FROM microsoft/dotnet:2.1.0-aspnetcore-runtime
WORKDIR /app
COPY . .
EXPOSE 5000
ENTRYPOINT ["dotnet", "Lykke.Job.SlackNotifications.dll"]
