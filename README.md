# LillyQuest

LillyQuest is a C# engine for building roguelike/roguelite open-world games. It is built on Silk.NET and currently ships with an OpenGL renderer, with room to evolve the rendering backend in the future.

This is a hobby project. A core goal is to keep momentum by publishing a weekly devlog article on https://orivega.io.

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

- Trello: https://trello.com/b/XvjwuWqE/lillyquest

## Devlog

Weekly updates are published on:
- https://orivega.io

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

## License

GPLv3. See `LICENSE` for details.
