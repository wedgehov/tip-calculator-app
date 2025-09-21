# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0-bookworm-slim AS build

# Install Node.js and npm
RUN apt-get update && \
    apt-get install -y curl && \
    curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
    apt-get install -y nodejs

WORKDIR /app

# Copy project files and restore dependencies to leverage Docker layer caching
COPY package.json package-lock.json ./
RUN npm install

# Copy dotnet tool manifest and restore tools (Fable)
COPY .config/dotnet-tools.json ./.config/
RUN dotnet tool restore

# Copy the rest of the application source
COPY . .

# Build the application
RUN npm run build

# Stage 2: Serve the application using Nginx
FROM nginx:1.27-alpine AS final

# Copy build output from the build stage
COPY --from=build /app/dist /usr/share/nginx/html

# Expose port 80 and start Nginx
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]