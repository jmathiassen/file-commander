#!/bin/bash
# File Commander (fcom) - Quick Start Script

echo "==================================="
echo "  File Commander (fcom) - Phase 1"
echo "==================================="
echo ""

PROJECT_DIR="/home/jmathias/RiderProjects/File Commander/File Commander"
BIN_PATH="$PROJECT_DIR/bin/Debug/net8.0/File Commander"

# Check if binary exists, if not build it
if [ ! -f "$BIN_PATH" ]; then
    echo "Building File Commander..."
    cd "$PROJECT_DIR"
    dotnet build --configuration Debug

    if [ $? -ne 0 ]; then
        echo "Build failed! Please check errors above."
        exit 1
    fi
fi

echo "Starting File Commander..."
echo ""

# Run with optional directory argument
if [ -n "$1" ]; then
    "$BIN_PATH" "$1"
else
    "$BIN_PATH"
fi

