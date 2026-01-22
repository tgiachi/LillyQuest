# CI Versionize + NuGet Release Notes

## Overview
Releases are triggered by merging a PR into `main` with the `release` label.

## Requirements
- GitHub secret `NUGET_API_KEY` must be set with a NuGet API key.
- Conventional commits are required for `versionize` to determine version bump.

## Release Flow
1. Open a PR targeting `main`.
2. Add the `release` label to the PR.
3. Merge the PR.
4. On push to `main`, the Release workflow:
   - Finds the merged PR for the commit.
   - Checks for `release` label.
   - Runs `versionize` (updates CHANGELOG and project versions).
   - Pushes the version commit and tag.
   - Packs and publishes NuGet packages:
     - LillyQuest.Core
     - LillyQuest.Engine
     - LillyQuest.Rendering
     - LillyQuest.Scripting.Lua

## CI Flow
PRs to `main` run:
- `dotnet restore`
- `dotnet build -c Release --no-restore`
- `dotnet test -c Release --no-build --nologo`

## Troubleshooting
- If release doesn’t run, confirm the PR has the `release` label.
- If publishing fails, verify `NUGET_API_KEY` is valid and has push permissions.
