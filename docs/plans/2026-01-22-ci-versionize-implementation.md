# CI Versionize + NuGet Publish Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Add GitHub Actions CI to validate PRs and a release workflow that runs `versionize` and publishes NuGet packages when a PR merged into `main` has the `release` label.

**Architecture:** Two workflows: (1) CI on PRs to run restore/build/test. (2) Release workflow on push to `main` that checks if the merged PR has label `release`; if yes, runs `versionize`, pushes the version commit + tag, and publishes the selected packages to NuGet using `NUGET_API_KEY`.

**Tech Stack:** GitHub Actions, dotnet CLI, versionize, NuGet.

### Task 1: Add CI workflow (build + test)

**Files:**
- Create: `.github/workflows/ci.yml`

**Step 1: Write the failing test**
Skip (workflow only).

**Step 2: Implement minimal workflow**
Create workflow that triggers on PRs to `main` and runs:
- `dotnet restore`
- `dotnet build -c Release --no-restore`
- `dotnet test -c Release --no-build --nologo`

**Step 3: Run workflow locally**
Skip.

**Step 4: Commit**
```bash
git add .github/workflows/ci.yml
git commit -m "ci: add build and test workflow"
```

### Task 2: Add release workflow (label-gated versionize + publish)

**Files:**
- Create: `.github/workflows/release.yml`

**Step 1: Write the failing test**
Skip (workflow only).

**Step 2: Implement release workflow**
Workflow triggers on push to `main` and:
- Uses GitHub API to find the merged PR for the push SHA.
- Checks labels include `release`.
- If not labeled, exits successfully.
- If labeled:
  - Install `versionize` tool (`dotnet tool install --global versionize`).
  - Run `versionize --skip-dirty`.
  - Push version commit + tag back to `main`.
  - `dotnet pack -c Release -o ./artifacts` for:
    - `src/LillyQuest.Core/LillyQuest.Core.csproj`
    - `src/LillyQuest.Engine/LillyQuest.Engine.csproj`
    - `src/LillyQuest.Rendering/LillyQuest.Rendering.csproj`
    - `src/LillyQuest.Scripting.Lua/LillyQuest.Scripting.Lua.csproj`
  - `dotnet nuget push ./artifacts/*.nupkg --api-key ${{ secrets.NUGET_API_KEY }} --source https://api.nuget.org/v3/index.json`

**Step 3: Commit**
```bash
git add .github/workflows/release.yml
git commit -m "ci: add release workflow with versionize and nuget publish"
```

### Task 3: Add release docs

**Files:**
- Create: `docs/plans/2026-01-22-ci-versionize-notes.md`

**Step 1: Write the doc**
Document required secrets, label flow, and how to trigger a release.

**Step 2: Commit**
```bash
git add docs/plans/2026-01-22-ci-versionize-notes.md
git commit -m "docs: add release workflow notes"
```

### Task 4: Full verification

**Step 1: Run tests**
Run: `dotnet test --nologo`
Expected: PASS.

**Step 2: Commit any remaining changes**
```bash
git status -s
```
