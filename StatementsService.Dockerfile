# Dockerfile - Deploy heroku container registry

# 0: Get dotnet sdk image and expose port
FROM microsoft/dotnet:2.1-sdk AS publish
WORKDIR /app
EXPOSE 80
EXPOSE 443

# 1: Copy csproj to current dir and restore
WORKDIR /src
COPY src/Services/Statements/Statements.API/*.csproj ./Services/Statements/Statements.API/
COPY src/Libraries/CommonLibrary/*.csproj ./Libraries/CommonLibrary/
COPY src/Libraries/Events/*.csproj ./Libraries/Events/
RUN dotnet restore "Services/Statements/Statements.API/Statements.API.csproj"

# 2: Copy everything, build and publish
COPY ./src .
WORKDIR /src/Services/Statements/Statements.API
RUN dotnet publish "Statements.API.csproj" -c Release -o /out

# 3: Run app
FROM microsoft/dotnet:2.1-aspnetcore-runtime AS runtime
COPY --from=publish /out .
ENTRYPOINT [ "dotnet", "Statements.API.dll" ]