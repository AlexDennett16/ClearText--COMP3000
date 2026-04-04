#!/usr/bin/env bash
# RUN via ./update_python_server.sh from gRPC/ to regenerate Python gRPC stubs after grammar.proto changes
echo "=== Updating Python gRPC server stubs ==="

# Resolve script location even if called via symlink
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Assume script is in gRPC/ → repo root is one level up
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
cd "$REPO_ROOT" || {
  echo "ERROR: Could not move to repo root"
  exit 1
}

echo "Repo root: $REPO_ROOT"

# Prefer python.exe on Windows Git Bash
if command -v python >/dev/null 2>&1; then
  PYTHON=python
elif command -v python3 >/dev/null 2>&1; then
  PYTHON=python3
else
  echo "ERROR: Python not found in PATH"
  exit 1
fi

echo "Using Python: $PYTHON"

# Verify grpc_tools exists
$PYTHON -m grpc_tools.protoc --version >/dev/null 2>&1 || {
  echo "ERROR: grpc_tools not installed in this Python environment"
  echo "Run: pip install grpcio grpcio-tools"
  exit 1
}

# Generate Python gRPC code
$PYTHON -m grpc_tools.protoc \
  -I gRPC \
  --python_out=Backend \
  --grpc_python_out=Backend \
  gRPC/grammar.proto

echo "Python gRPC stubs regenerated successfully"