FROM mcr.microsoft.com/dotnet/sdk:8.0

ARG BUILD_CONFIGURATION=Debug
WORKDIR /src

# Install dotnet-ef
RUN dotnet tool install -g dotnet-ef
ENV PATH="${PATH}:/root/.dotnet/tools"

# Copy and restore
COPY ["Quivi.OAuth2/Quivi.OAuth2.csproj", "Quivi.OAuth2/"]
RUN dotnet restore "./Quivi.OAuth2/Quivi.OAuth2.csproj"

# Copy the rest of the files
COPY . .

# Set working dir and build
WORKDIR /src/Quivi.OAuth2

ENTRYPOINT sh -c "dotnet ef database update -c OAuthDbContext && dotnet ef database update -c QuiviContext"