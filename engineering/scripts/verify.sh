#!/usr/bin/env bash
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
suite="${1:-all}"
case "$suite" in
  all|frontend|backend|unit) ;;
  *) echo "Usage: bash engineering/scripts/verify.sh [all|frontend|backend|unit]" >&2; exit 2 ;;
esac

if [[ "$suite" == all || "$suite" == frontend ]]; then
  # Use the project runtime without changing the user's global Node.
  node_version="$(cat "$repo_root/.nvmrc")"
  node_bin="${NVM_DIR:-$HOME/.nvm}/versions/node/v${node_version}/bin"
  if [[ -x "$node_bin/node" ]]; then
    export PATH="$node_bin:$PATH"
  fi
  node -e 'const [major, minor] = process.versions.node.split(".").map(Number);
    if (major < 24 || (major === 24 && minor < 11)) {
      console.error("ForeverPin requires Node 24.11+; run nvm install && nvm use.");
      process.exit(1);
    }'
  cd "$repo_root/engineering/codebase/forever-pin.frontend-services"
  pnpm typecheck
  pnpm test
  pnpm build
fi

if [[ "$suite" != frontend ]]; then
  cd "$repo_root/engineering/codebase/forever-pin.backend-services"
  target="forever-pin.backend-services.slnx"
  if [[ "$suite" == unit ]]; then target="ForeverPin.Tests.Unit"; fi
  # Serial builds avoid shared MSBuild/compiler servers. Test sockets and
  # Docker still require native execution approval in a restricted sandbox.
  dotnet test "$target" --disable-build-servers -m:1 \
    -p:UseSharedCompilation=false --verbosity minimal
fi
