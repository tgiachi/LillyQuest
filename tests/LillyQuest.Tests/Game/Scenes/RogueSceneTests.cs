using System.Linq;
using LillyQuest.Core.Data.Assets.Tiles;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Core.Primitives;
using LillyQuest.Core.Types;
using LillyQuest.Engine.Data.Input;
using LillyQuest.Engine.Interfaces.Services;
using LillyQuest.Engine.Interfaces.Managers;
using LillyQuest.Engine.Interfaces.Screens;
using LillyQuest.Game.Scenes;
using LillyQuest.RogueLike.GameObjects;
using LillyQuest.RogueLike.Interfaces.Services;
using LillyQuest.RogueLike.Maps;
using LillyQuest.RogueLike.Maps.Tiles;
using LillyQuest.RogueLike.Systems;
using LillyQuest.RogueLike.Types;
using NUnit.Framework;
using SadRogue.Primitives;

namespace LillyQuest.Tests.Game.Scenes;

public class RogueSceneTests
{
    [Test]
    public void RogueScene_UsesMapRenderSystem()
    {
        var screenManager = new FakeScreenManager();
        var map = BuildTestMap();
        var mapGenerator = new FakeMapGenerator(map);
        var tilesetManager = new FakeTilesetManager();
        var shortcutService = new FakeShortcutService();
        var actionService = new FakeActionService();
        var scene = new RogueScene(screenManager, mapGenerator, tilesetManager, shortcutService, actionService);

        scene.OnLoad();

        var mapRenderSystem = scene.GetSceneGameObjects().OfType<MapRenderSystem>().Single();

        Assert.That(mapRenderSystem.HasMap(map), Is.True);
    }

    [Test]
    public void RogueScene_RegistersViewportUpdateSystem()
    {
        var screenManager = new FakeScreenManager();
        var map = BuildTestMap();
        var mapGenerator = new FakeMapGenerator(map);
        var tilesetManager = new FakeTilesetManager();
        var shortcutService = new FakeShortcutService();
        var actionService = new FakeActionService();
        var scene = new RogueScene(screenManager, mapGenerator, tilesetManager, shortcutService, actionService);

        scene.OnLoad();

        Assert.That(scene.GetSceneGameObjects().OfType<ViewportUpdateSystem>().Any(), Is.True);
    }

    private static LyQuestMap BuildTestMap()
    {
        var map = new LyQuestMap(10, 10);

        for (var x = 0; x < map.Width; x++)
        {
            for (var y = 0; y < map.Height; y++)
            {
                var terrain = new TerrainGameObject(new Point(x, y), isWalkable: true, isTransparent: true)
                {
                    Tile = new VisualTile("floor", "A", LyColor.Black, LyColor.White)
                };

                map.SetTerrain(terrain);
            }
        }

        var player = new CreatureGameObject(new Point(1, 1))
        {
            Tile = new VisualTile("player", "@", LyColor.Transparent, LyColor.White)
        };

        map.AddEntity(player);

        return map;
    }

    private sealed class FakeMapGenerator : IMapGenerator
    {
        private readonly LyQuestMap _map;

        public FakeMapGenerator(LyQuestMap map)
        {
            _map = map;
        }

        public Task<LyQuestMap> GenerateMapAsync()
            => Task.FromResult(_map);
    }

    private sealed class FakeScreenManager : IScreenManager
    {
        private readonly List<IScreen> _screenStack = new();

        public IScreen? FocusedScreen => _screenStack.Count > 0 ? _screenStack[^1] : null;
        public IReadOnlyList<IScreen> ScreenStack => _screenStack;
        public IScreen? RootScreen => _screenStack.Count > 0 ? _screenStack[0] : null;

        public bool DispatchKeyPress(KeyModifierType modifier, IReadOnlyList<Silk.NET.Input.Key> keys)
            => false;

        public bool DispatchKeyRelease(KeyModifierType modifier, IReadOnlyList<Silk.NET.Input.Key> keys)
            => false;

        public bool DispatchKeyRepeat(KeyModifierType modifier, IReadOnlyList<Silk.NET.Input.Key> keys)
            => false;

        public bool DispatchMouseDown(int x, int y, IReadOnlyList<Silk.NET.Input.MouseButton> buttons)
            => false;

        public bool DispatchMouseMove(int x, int y)
            => false;

        public bool DispatchMouseUp(int x, int y, IReadOnlyList<Silk.NET.Input.MouseButton> buttons)
            => false;

        public bool DispatchMouseWheel(int x, int y, float delta)
            => false;

        public void PopScreen()
        {
            if (_screenStack.Count > 0)
            {
                _screenStack.RemoveAt(_screenStack.Count - 1);
            }
        }

        public void PopScreen(IScreen screen)
        {
            if (_screenStack.Count > 0 && ReferenceEquals(_screenStack[^1], screen))
            {
                _screenStack.RemoveAt(_screenStack.Count - 1);
            }
        }

        public void PushScreen(IScreen screen)
            => _screenStack.Add(screen);

        public void RemoveScreen(IScreen screen)
            => _screenStack.Remove(screen);

        public void Render(LillyQuest.Core.Graphics.Rendering2D.SpriteBatch spriteBatch, LillyQuest.Core.Data.Contexts.EngineRenderContext renderContext) { }

        public void SetRootScreen(IScreen? screen)
        {
            _screenStack.Clear();
            if (screen != null)
            {
                _screenStack.Add(screen);
            }
        }

        public void Update(GameTime gameTime) { }
    }

    private sealed class FakeTilesetManager : ITilesetManager
    {
        public void Dispose() { }

        public IReadOnlyDictionary<string, Tileset> GetAllTilesets()
            => new Dictionary<string, Tileset>();

        public Tileset GetTileset(string name)
            => throw new KeyNotFoundException();

        public bool HasTileset(string name)
            => false;

        public void LoadTileset(string name, string filePath, int tileWidth, int tileHeight, int spacing, int margin)
            => throw new NotSupportedException();

        public void LoadTileset(string name, Span<byte> content, int tileWidth, int tileHeight, int spacing, int margin)
            => throw new NotSupportedException();

        public bool TryGetTileset(string name, out Tileset tileset)
        {
            tileset = null!;
            return false;
        }

        public void UnloadTileset(string name)
            => throw new NotSupportedException();
    }

    private sealed class FakeShortcutService : IShortcutService
    {
        public InputContextType GetCurrentContext()
            => InputContextType.Global;

        public void HandleKeyPress(KeyModifierType modifier, IReadOnlyList<Silk.NET.Input.Key> keys) { }

        public void HandleKeyRelease(KeyModifierType modifier, IReadOnlyList<Silk.NET.Input.Key> keys) { }

        public void HandleKeyRepeat(KeyModifierType modifier, IReadOnlyList<Silk.NET.Input.Key> keys) { }

        public bool PopContext()
            => false;

        public void PushContext(InputContextType context) { }

        public bool RegisterShortcut(string actionName, Action action, InputContextType context, string shortcut, ShortcutTriggerType trigger)
            => true;

        public bool RegisterShortcut(string actionName, InputContextType context, string shortcut, ShortcutTriggerType trigger)
            => true;

        public void SetContext(InputContextType context) { }

        public bool UnregisterShortcut(string actionName, InputContextType context, string shortcut, ShortcutTriggerType trigger)
            => true;
    }

    private sealed class FakeActionService : IActionService
    {
        private readonly Dictionary<string, Action> _actions = new();

        public bool Execute(string actionName)
        {
            if (_actions.TryGetValue(actionName, out var action))
            {
                action();
                return true;
            }

            return false;
        }

        public bool HasAction(string actionName)
            => _actions.ContainsKey(actionName);

        public bool IsActionInUse(string actionName)
            => false;

        public void MarkActionInUse(string actionName) { }

        public void MarkActionReleased(string actionName) { }

        public void RegisterAction(string actionName, Action action)
            => _actions[actionName] = action;

        public bool UnregisterAction(string actionName)
            => _actions.Remove(actionName);
    }
}
