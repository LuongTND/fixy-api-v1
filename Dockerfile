# ===== BUILD STAGE =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# copy solution + all projects
COPY . .

# restore
RUN dotnet restore FIXY.sln

# publish API project
RUN dotnet publish API/API.csproj -c Release -o /app/publish

# ===== RUNTIME STAGE =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

# copy published output
COPY --from=build /app/publish .

# expose port (Render sẽ override PORT)
EXPOSE 8080

# start app
ENTRYPOINT ["dotnet", "API.dll"]