#!/bin/bash
#
# Setup Git Hooks for LillyQuest
# This script installs pre-commit hook that runs tests before commit
#

set -e

SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
HOOKS_DIR="$SCRIPT_DIR/.git/hooks"
PRE_COMMIT_HOOK="$HOOKS_DIR/pre-commit"

echo "🔧 Setting up Git hooks for LillyQuest..."

# Check if .git directory exists
if [ ! -d "$SCRIPT_DIR/.git" ]; then
    echo "❌ Error: .git directory not found. Are you in the repository root?"
    exit 1
fi

# Create hooks directory if it doesn't exist
mkdir -p "$HOOKS_DIR"

# Backup existing pre-commit hook if it exists
if [ -f "$PRE_COMMIT_HOOK" ]; then
    echo "⚠️  Existing pre-commit hook found. Creating backup..."
    mv "$PRE_COMMIT_HOOK" "$PRE_COMMIT_HOOK.backup.$(date +%Y%m%d_%H%M%S)"
fi

# Create pre-commit hook
cat > "$PRE_COMMIT_HOOK" << 'EOF'
#!/bin/sh
#
# Pre-commit hook that runs dotnet test before allowing commit
#

echo "Running tests before commit..."

# Run dotnet test
dotnet test --no-restore --verbosity quiet

# Capture exit code
TEST_EXIT_CODE=$?

# If tests failed, prevent commit
if [ $TEST_EXIT_CODE -ne 0 ]; then
    echo ""
    echo "❌ Tests failed! Commit aborted."
    echo "Fix the failing tests before committing."
    exit 1
fi

echo "✅ All tests passed!"
exit 0
EOF

# Make the hook executable
chmod +x "$PRE_COMMIT_HOOK"

echo "✅ Pre-commit hook installed successfully!"
echo ""
echo "The hook will:"
echo "  - Run 'dotnet test' before each commit"
echo "  - Block commits if tests fail"
echo "  - Can be bypassed with 'git commit --no-verify' (not recommended)"
echo ""
echo "To test the hook manually, run:"
echo "  .git/hooks/pre-commit"
