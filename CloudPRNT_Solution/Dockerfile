# Get the base image
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

# Copy the csproj and restore all of the NuGet packages
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app

# Copy the build output
COPY --from=build-env /app/out .

# Copy the SQLite database file into the container
#COPY CloudPRNT_Solution.Data.db /app/data/CloudPRNT_Solution.Data.db
VOLUME ["/data"]


# Set the entry point
ENTRYPOINT ["dotnet", "CloudPRNT_Solution.dll"]
