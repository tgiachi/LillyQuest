# LillyQuest

LillyQuest is a C# engine for building roguelike/roguelite open-world games. It is built on Silk.NET and currently ships with an OpenGL renderer, with room to evolve the rendering backend in the future.

This is a hobby project. A core goal is to keep momentum by publishing a weekly devlog article on <https://orivega.io>.

## Goals and Features

- Data-driven engine design using JSON.
- AOT-friendly architecture (aspirational).
- Planned Lua scripting for gameplay logic.
- Modular engine structure split across core, rendering, and engine layers.

## Tech Stack

- C# / .NET (net10.0)
- Silk.NET
- OpenGL renderer (current)
- JSON-driven data pipeline
- Lua scripting (planned)

## Project Structure

The codebase is organized into modular layers:

### LillyQuest.Core
Foundation layer providing low-level engine primitives:
- OpenGL rendering abstractions (shaders, textures, sprite batching)
- Asset management (textures, fonts, tilesets, audio)
- Audio system (OpenAL with MP3 and Ogg Vorbis support)
- Input abstractions and data structures
- Embedded resource pipeline

### LillyQuest.Engine
High-level engine layer built on Core:
- Feature-based Entity architecture
- System pipeline (Update, FixedUpdate, Render)
- Screen and UI management (stacking, focus, pixel-perfect rendering)
- Scene lifecycle and transitions
- Animation and tween system
- ImGui integration for debug tooling
- Windowing and bootstrap orchestration

### LillyQuest.Scripting.Lua
Lua scripting integration via MoonSharp (planned for gameplay logic).

### LillyQuest.Game
Executable game application demonstrating engine capabilities:
- Game-specific scenes and entities
- Tileset editor and surface rendering examples
- Debug panels and development tools

### LillyQuest.Tests
Comprehensive test suite covering Core and Engine components.

## Build and Run

Prerequisites:

- .NET SDK 10.0

Build:

```bash
dotnet build LillyQuest.slnx
```

Run (game project):

```bash
dotnet run --project src/LillyQuest.Game/LillyQuest.Game.csproj
```

## Project Board

- Trello: <https://trello.com/b/XvjwuWqE/lillyquest>

## Devlog

Weekly updates are published on:

- <https://orivega.io>

Feedback in the comments helps keep momentum.

## Contributing

Issues and PRs are welcome, but please open a discussion first since this is a hobby project and priorities may change.

## Philosophy / FAQ

**Why a custom engine?**
Learning, long-term flexibility, and the joy of building the stack end-to-end.

**Is this production-ready?**
No. This is an experimental engine under active development.

**Will other renderers be supported?**
OpenGL is the current backend; other renderers may be explored later.

## Development Philosophy

**On AI-Assisted Development**

This project uses AI tools (Claude, Codex, etc.) with intentionality and restraint. AI assists with:
- Planning and architectural brainstorming
- Test generation and coverage analysis
- Bug discovery and code review
- Documentation drafting

However, core implementation is hand-crafted. Programming is an art form—the discipline of thinking through problems, making deliberate design choices, and writing clean, maintainable code cannot be delegated. AI is a tool for augmenting human judgment, not replacing it.

The code you read here reflects human reasoning and intentional design, not automated generation.

## License

GPLv3. See `LICENSE` for details.
