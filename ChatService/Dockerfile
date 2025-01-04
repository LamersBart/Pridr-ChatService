# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release

# Set working directory for build
WORKDIR /src

# Copy only the project files first to leverage caching
COPY ChatService.csproj ./
RUN dotnet restore

# Copy the source files explicitly
COPY ./ ./

# Build the application
RUN dotnet build "ChatService.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Stage 2: Publish the application
FROM build AS publish
ARG BUILD_CONFIGURATION=Release

RUN dotnet publish "ChatService.csproj" -c $BUILD_CONFIGURATION -o /app/publish --no-restore

# Stage 3: Create a runtime image with a non-root user
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Add a non-root user and group
RUN addgroup --system appgroup && \
    adduser --system --ingroup appgroup appuser

# Set working directory
WORKDIR /app

# Copy the published files
COPY --from=publish /app/publish .

# Set permissions for the non-root user
RUN chown -R appuser:appgroup /app

# Switch to the non-root user
USER appuser

# Expose ports
EXPOSE 8080
EXPOSE 8081

# Entry point
ENTRYPOINT ["dotnet", "ChatService.dll"]
