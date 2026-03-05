#!/bin/bash

# RememberAll Database Download Script
# Downloads the app.db file from the remote server to the local RememberAllBackend folder

set -e # Exit on error

# Load configuration from .env file
if [ ! -f .env ]; then
  echo "❌ Error: .env file not found!"
  echo "Please create a .env file based on .env.example"
  exit 1
fi

source .env

echo "📥 Downloading database from $SERVER_HOST..."

# Define paths
REMOTE_DB_PATH="/portainer/rememberall/data/app.db"
LOCAL_DB_PATH="../RememberAllBackend/app.db"

# Download the database file using scp
scp -i "$SSH_KEY" "$SERVER_USER@$SERVER_HOST:$REMOTE_DB_PATH" "$LOCAL_DB_PATH"

if [ $? -eq 0 ]; then
  echo "✅ Database downloaded successfully!"
  echo "📁 Location: $LOCAL_DB_PATH"
  echo "📊 File size: $(du -h "$LOCAL_DB_PATH" | cut -f1)"
else
  echo "❌ Failed to download database"
  exit 1
fi
