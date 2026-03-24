#!/bin/bash
set -euo pipefail

# === SETTINGS (use an ACCESS TOKEN instead of deploy token) ===
REPO_NAME="say-podspecs"
HTTPS_URL="https://gitlab.saygames.io/saykit/say-podspecs.git"
SSH_URL="git@gitlab.saygames.io:saykit/say-podspecs.git"

MACHINE="gitlab.saygames.io"

# For a Personal Access Token (PAT): set ACCESS_USERNAME to the GitLab username of the token owner.
# For a Project/Group Access Token: set ACCESS_USERNAME to the bot's username shown by GitLab.
ACCESS_USERNAME="saykitios"   # <- REQUIRED
ACCESS_TOKEN="glpat-ZBC2dTRQkJGN6zATyceB"   # <- REQUIRED (PAT / Project / Group Access Token)

NETRC="$HOME/.netrc"
REPO_DIR="$HOME/.cocoapods/repos/$REPO_NAME"

# === .netrc handling (validate both username and token) ===
rewrite_netrc=false
if grep -q "machine $MACHINE" "$NETRC" 2>/dev/null; then
  # Extract current login for this machine
  cur_login=$(awk -v m="$MACHINE" '
    $1=="machine" && $2==m {found=1}
    found && $1=="login" {print $2; exit}
  ' "$NETRC")

  # Extract current password for this machine
  cur_pass=$(awk -v m="$MACHINE" '
    $1=="machine" && $2==m {found=1}
    found && $1=="password" {print $2; exit}
  ' "$NETRC")

  # Compare values
  if [ "${cur_login:-}" != "$ACCESS_USERNAME" ] || [ "${cur_pass:-}" != "$ACCESS_TOKEN" ]; then
    rewrite_netrc=true
  fi
else
  rewrite_netrc=true
fi

if [ "$rewrite_netrc" = true ]; then
  # Remove old block for this machine (if exists) and keep the rest
  tmpfile=$(mktemp)
  awk -v m="$MACHINE" '
    $1=="machine" && $2==m {skip=1}
    skip && $1=="machine" {skip=0}
    !skip {print}
  ' "$NETRC" 2>/dev/null >"$tmpfile" || true

  mv "$tmpfile" "$NETRC"

  # Write the new block
  {
    echo "machine $MACHINE"
    echo "  login $ACCESS_USERNAME"
    echo "  password $ACCESS_TOKEN"
  } >> "$NETRC"

  chmod 600 "$NETRC"
  echo "🔄 Updated/added entry for $MACHINE in $NETRC"
else
  echo "✅ Entry for $MACHINE in $NETRC is up to date"
fi

# === CocoaPods repo handling ===
if [ -d "$REPO_DIR/.git" ]; then
  CUR_URL="$(git -C "$REPO_DIR" remote get-url origin 2>/dev/null || echo "")"
  if [ "$CUR_URL" = "$HTTPS_URL" ]; then
    echo "✅ $REPO_NAME already uses HTTPS ($CUR_URL)"
  elif [ "$CUR_URL" = "$SSH_URL" ] || [[ "$CUR_URL" == git@* ]]; then
    echo "🔁 Switching $REPO_NAME from SSH to HTTPS"
    git -C "$REPO_DIR" remote set-url origin "$HTTPS_URL"
    git -C "$REPO_DIR" fetch --all --prune
  else
    echo "⚠️ Unexpected origin ($CUR_URL). Reinstalling $REPO_NAME"
    "$PODPATH" repo remove "$REPO_NAME" || true
    "$PODPATH" repo add "$REPO_NAME" "$HTTPS_URL"
  fi
else
  echo "➕ Adding $REPO_NAME via HTTPS"
  "$PODPATH" repo add "$REPO_NAME" "$HTTPS_URL"
fi

# Update only this repo (faster than updating all)
"$PODPATH" repo update "$REPO_NAME"

echo "🎉 Done — now you can run 'pod install'"
