FROM swisschains/dotnet:2.2-aspnetcore-runtime
WORKDIR /app
COPY . .
EXPOSE 5000
ENTRYPOINT ["dotnet", "Lykke.Job.SlackNotifications.dll"]
