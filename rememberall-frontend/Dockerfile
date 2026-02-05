# Build stage
FROM node:20-alpine AS build

WORKDIR /app

# Copy package files
COPY package.json pnpm-lock.yaml pnpm-workspace.yaml ./

# Install pnpm and dependencies
RUN npm install -g pnpm && pnpm install --frozen-lockfile

# Copy source code
COPY . .

# Build the SvelteKit application
RUN pnpm run build

# Runtime stage
FROM node:20-alpine AS runtime

WORKDIR /app

# Copy package files
COPY package.json pnpm-lock.yaml pnpm-workspace.yaml ./

# Install pnpm and production dependencies only
RUN npm install -g pnpm && pnpm install --frozen-lockfile --prod

# Copy built application from build stage
COPY --from=build /app/build ./build

EXPOSE 3000

ENV NODE_ENV=production

CMD ["node", "build/index.js"]
