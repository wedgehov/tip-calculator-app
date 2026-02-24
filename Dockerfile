# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0-bookworm-slim AS build

# Install Bun
RUN apt-get update && \
    apt-get install -y curl unzip && \
    curl -fsSL https://bun.sh/install | bash && \
    ln -s /root/.bun/bin/bun /usr/local/bin/bun

WORKDIR /app

# Copy dotnet tool manifest and restore tools (Fable)
COPY .config/dotnet-tools.json ./.config/
RUN dotnet tool restore

# Copy application source (including bun.lock when present)
COPY . .

# Restore JavaScript dependencies with Bun
RUN bun install

# Build the application
RUN bun run build

# Stage 2: Serve the application using Nginx
FROM nginx:1.27-alpine AS final

# Copy build output from the build stage
COPY --from=build /app/dist /usr/share/nginx/html

# Expose port 80 and start Nginx
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
