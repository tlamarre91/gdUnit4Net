#!/bin/bash
# Script to rebuild and pack local gdUnit4Net packages
# Usage: ./rebuild-local.sh

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

echo "🔨 Building and packing gdUnit4Net local packages..."

# Clean previous builds
echo "  → Cleaning previous packages"
rm -rf local-packages/*.nupkg

# Clean build artifacts
echo "  → Cleaning build artifacts (bin/obj)"
find . -type d \( -name "bin" -o -name "obj" \) -exec rm -rf {} + 2>/dev/null || true

# Restore all dependencies first
echo "  → Restoring NuGet packages"
dotnet restore --force-evaluate

# Build and pack the test adapter
echo "  → Building gdUnit4.test.adapter"
cd TestAdapter
dotnet clean -c Release
dotnet pack -c Release -o ../local-packages

# Build and pack the API
echo "  → Building gdUnit4.api"
cd ../Api
dotnet clean -c Release
dotnet pack -c Release -o ../local-packages

# Build and pack the analyzers
echo "  → Building gdUnit4.analyzers"
cd ../Analyzers
dotnet clean -c Release
dotnet pack -c Release -o ../local-packages

cd "$SCRIPT_DIR"

echo ""
echo "✅ Local packages built successfully:"
ls -lh local-packages/*.nupkg
