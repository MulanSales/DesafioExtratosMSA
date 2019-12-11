# Dockerfile - Deploy heroku container registry

# 0: Get dotnet sdk image and expose port
FROM microsoft/dotnet:2.1-sdk AS publish
WORKDIR /app
EXPOSE 80
EXPOSE 443

# 1: Copy csproj to current dir and restore
WORKDIR /src
COPY src/APIGateway/*.csproj APIGateway/
RUN dotnet restore "APIGateway/APIGateway.csproj"

# 2: Copy everything, build and publish
COPY ./src .
WORKDIR /src/APIGateway
RUN dotnet publish -c Release -o /out

# 3: Run app
FROM microsoft/dotnet:2.1-aspnetcore-runtime AS runtime
WORKDIR /app
COPY --from=publish /out .
CMD ASPNETCORE_URLS=http://*:$PORT dotnet APIGateway.dll