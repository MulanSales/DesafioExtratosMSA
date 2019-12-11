# Dockerfile - Deploy heroku container registry

# 0: Get dotnet sdk image and expose port
FROM microsoft/dotnet:2.1-sdk AS publish
WORKDIR /app
EXPOSE 80
EXPOSE 443

# 1: Copy csproj to current dir and restore
WORKDIR /src
COPY src/Services/Releases/Releases.API/*.csproj ./Services/Releases/Releases.API/
COPY src/Libraries/CommonLibrary/*.csproj ./Libraries/CommonLibrary/
COPY src/Libraries/Events/*.csproj ./Libraries/Events/
RUN dotnet restore "Services/Releases/Releases.API/Releases.API.csproj"

# 2: Copy everything, build and publish
COPY ./src .
WORKDIR /src/Services/Releases/Releases.API
RUN dotnet publish "Releases.API.csproj" -c Release -o /out

# 3: Run app
FROM microsoft/dotnet:2.1-aspnetcore-runtime AS runtime
COPY --from=publish /out .
ENTRYPOINT [ "dotnet", "Releases.API.dll" ]