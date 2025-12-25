#!/bin/bash
echo "Testing if backend is running..."
if curl -sSf http://localhost:5140/swagger/index.html > /dev/null 2>&1; then
  echo "✓ Backend is running on http://localhost:5140"
  echo "Testing /api/chat endpoint..."
  curl -sS http://localhost:5140/api/chat \
    -H "Content-Type: application/json" \
    -d '{"question":"test"}' | head -c 200
  echo ""
else
  echo "✗ Backend is NOT running on http://localhost:5140"
  echo "Start it with: dotnet run --project LMoses"
fi
