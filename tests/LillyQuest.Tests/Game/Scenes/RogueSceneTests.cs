using LillyQuest.Core.Data.Assets.Tiles;
using LillyQuest.Core.Data.Contexts;
using LillyQuest.Core.Graphics.Rendering2D;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Core.Primitives;
using LillyQuest.Core.Types;
using LillyQuest.Engine.Data.Input;
using LillyQuest.Engine.Interfaces.Managers;
using LillyQuest.Engine.Interfaces.Screens;
using LillyQuest.Engine.Interfaces.Services;
using LillyQuest.Game.Scenes;
using LillyQuest.RogueLike.GameObjects;
using LillyQuest.RogueLike.Interfaces.Services;
using LillyQuest.RogueLike.Maps;
using LillyQuest.RogueLike.Maps.Tiles;
using LillyQuest.RogueLike.Systems;
using LillyQuest.RogueLike.Types;
using SadRogue.Primitives;
using Silk.NET.Input;

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
        var worldManager = new FakeWorldManager();
        var scene = new RogueScene(screenManager, mapGenerator, tilesetManager, shortcutService, actionService, null!, null!, null!, null!, null!, worldManager);

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
        var worldManager = new FakeWorldManager();
        var scene = new RogueScene(screenManager, mapGenerator, tilesetManager, shortcutService, actionService, null!, null!, null!, null!, null!, worldManager);

        scene.OnLoad();

        Assert.That(scene.GetSceneGameObjects().OfType<ViewportUpdateSystem>().Any(), Is.True);
    }

    [Test]
    public void RogueScene_SetsCurrentMapOnWorldManager()
    {
        var screenManager = new FakeScreenManager();
        var map = BuildTestMap();
        var mapGenerator = new FakeMapGenerator(map);
        var tilesetManager = new FakeTilesetManager();
        var shortcutService = new FakeShortcutService();
        var actionService = new FakeActionService();
        var worldManager = new FakeWorldManager();
        var scene = new RogueScene(screenManager, mapGenerator, tilesetManager, shortcutService, actionService, null!, null!, null!, null!, null!, worldManager);

        scene.OnLoad();

        Assert.That(worldManager.CurrentMap, Is.EqualTo(map));
    }

    [Test]
    public void RogueScene_ActionsUseWorldManagerCurrentMap()
    {
        var screenManager = new FakeScreenManager();
        var map1 = BuildTestMap();
        var map2 = BuildTestMap();
        var mapGenerator = new FakeMapGenerator(map1);
        var tilesetManager = new FakeTilesetManager();
        var shortcutService = new FakeShortcutService();
        var actionService = new FakeActionService();
        var worldManager = new FakeWorldManager();
        var scene = new RogueScene(screenManager, mapGenerator, tilesetManager, shortcutService, actionService, null!, null!, null!, null!, null!, worldManager);

        scene.OnLoad();

        worldManager.CurrentMap = map2;

        var player1 = map1.Entities.GetLayer((int)MapLayer.Creatures).First().Item as CreatureGameObject;
        var player2 = map2.Entities.GetLayer((int)MapLayer.Creatures).First().Item as CreatureGameObject;

        Assert.That(player1, Is.Not.Null);
        Assert.That(player2, Is.Not.Null);

        actionService.Execute("up");

        Assert.That(player1!.Position, Is.EqualTo(new Point(1, 1)));
        Assert.That(player2!.Position, Is.EqualTo(new Point(1, 0)));
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

    private sealed class FakeWorldManager : IWorldManager
    {
        private readonly List<IMapHandler> _handlers = new();
        private LyQuestMap _currentMap = null!;

        public event IWorldManager.OnCurrentMapChangedHandler? OnCurrentMapChanged;

        public LyQuestMap CurrentMap
        {
            get => _currentMap;
            set
            {
                var oldMap = _currentMap;
                _currentMap = value;

                if (oldMap != null)
                {
                    foreach (var handler in _handlers)
                    {
                        handler.OnMapUnregistered(oldMap);
                    }
                }

                foreach (var handler in _handlers)
                {
                    handler.OnMapRegistered(_currentMap);
                }

                foreach (var handler in _handlers)
                {
                    handler.OnCurrentMapChanged(oldMap, _currentMap);
                }

                OnCurrentMapChanged?.Invoke(oldMap, _currentMap);
            }
        }

        public LyQuestMap OverworldMap { get; set; } = null!;

        public void RegisterMapHandler(IMapHandler handler)
        {
            if (!_handlers.Contains(handler))
            {
                _handlers.Add(handler);
            }
        }

        public void UnregisterMapHandler(IMapHandler handler)
            => _handlers.Remove(handler);

        public Task GenerateMapAsync()
            => Task.CompletedTask;
    }

    private sealed class FakeScreenManager : IScreenManager
    {
        private readonly List<IScreen> _screenStack = new();

        public IScreen? FocusedScreen => _screenStack.Count > 0 ? _screenStack[^1] : null;
        public IReadOnlyList<IScreen> ScreenStack => _screenStack;
        public IScreen? RootScreen => _screenStack.Count > 0 ? _screenStack[0] : null;

        public bool DispatchKeyPress(KeyModifierType modifier, IReadOnlyList<Key> keys)
            => false;

        public bool DispatchKeyRelease(KeyModifierType modifier, IReadOnlyList<Key> keys)
            => false;

        public bool DispatchKeyRepeat(KeyModifierType modifier, IReadOnlyList<Key> keys)
            => false;

        public bool DispatchMouseDown(int x, int y, IReadOnlyList<MouseButton> buttons)
            => false;

        public bool DispatchMouseMove(int x, int y)
            => false;

        public bool DispatchMouseUp(int x, int y, IReadOnlyList<MouseButton> buttons)
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

        public void Render(SpriteBatch spriteBatch, EngineRenderContext renderContext) { }

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

        public void HandleKeyPress(KeyModifierType modifier, IReadOnlyList<Key> keys) { }

        public void HandleKeyRelease(KeyModifierType modifier, IReadOnlyList<Key> keys) { }

        public void HandleKeyRepeat(KeyModifierType modifier, IReadOnlyList<Key> keys) { }

        public bool PopContext()
            => false;

        public void PushContext(InputContextType context) { }

        public bool RegisterShortcut(string actionName, Action action, InputContextType context, string shortcut, ShortcutTriggerType trigger, int repeatDelayMs = 0)
            => true;

        public bool RegisterShortcut(string actionName, InputContextType context, string shortcut, ShortcutTriggerType trigger, int repeatDelayMs = 0)
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
