#!/bin/bash

# RememberAll Frontend Deployment Script
# Automatically deploys the frontend to the remote server

set -e  # Exit on error

# Load configuration from .env file
if [ ! -f .env ]; then
  echo "❌ Error: .env file not found!"
  echo "Please create a .env file based on .env.example"
  exit 1
fi

source .env

echo "🚀 Starting deployment to $SERVER_HOST..."

# Execute remote commands
ssh -i "$SSH_KEY" "$SERVER_USER@$SERVER_HOST" bash << 'REMOTE_SCRIPT'
  set -e
  
  PROJECT_ROOT="/portainer/rememberall"
  
  echo "📍 Navigating to project root..."
  cd "$PROJECT_ROOT" || exit 1
  
  echo "📂 Current directory: $(pwd)"
  
  echo "� Backing up database..."
  TIMESTAMP=$(date +%Y-%m-%d_%H%M%S)
  cp "$PROJECT_ROOT/data/app.db" "$PROJECT_ROOT/data/app.db.bak.$TIMESTAMP"
  echo "✅ Database backed up to: data/app.db.bak.$TIMESTAMP"
  
  echo "�🛑 Stopping containers..."
  docker compose down
  
  echo "📥 Pulling latest changes..."
  git pull
  
  echo "🏗️  Rebuilding and starting containers..."
  docker compose up -d --build
  
  echo "✅ Deployment complete!"
  echo "📊 Container status:"
  docker compose ps

REMOTE_SCRIPT

echo "✨ Deployment finished successfully!"
