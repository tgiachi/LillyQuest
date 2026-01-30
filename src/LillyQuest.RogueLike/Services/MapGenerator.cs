using GoRogue.MapGeneration;
using LillyQuest.Core.Primitives;
using LillyQuest.RogueLike.Components;
using LillyQuest.RogueLike.GameObjects;
using LillyQuest.RogueLike.Interfaces.Services;
using LillyQuest.RogueLike.Maps;
using LillyQuest.RogueLike.Maps.Tiles;
using LillyQuest.RogueLike.Services.Loaders;
using LillyQuest.RogueLike.Types;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;
using Serilog;

namespace LillyQuest.RogueLike.Services;

public class MapGenerator : IMapGenerator
{
    private readonly ILogger _logger = Log.ForContext<MapGenerator>();

    private readonly TerrainService _terrainService;

    public MapGenerator(TerrainService terrainService)
    {
        _terrainService = terrainService;
    }

    public async Task<LyQuestMap> GenerateMapAsync()
    {
        var map = new LyQuestMap(300, 300)
        {
            Name = "Test Map",
        };
        var generator = new Generator(300, 300);
        var floorId = "floor";
        var wallId = "wall";

        generator.ConfigAndGenerateSafe(
            g =>
            {
                g.AddSteps(DefaultAlgorithms.RectangleMapSteps());
            }
        );

        generator.Generate();

        var wallFloorValues = generator.Context.GetFirst<ISettableGridView<bool>>("WallFloor");

        foreach (var pos in wallFloorValues.Positions())
        {
            var isFloor = wallFloorValues[pos];
            var terrainId = wallFloorValues[pos] ? floorId : wallId;
            var terrainGameObject = new TerrainGameObject(pos);

            if (_terrainService.TryGetTerrain(terrainId, out var terrain))
            {
                terrainGameObject.IsWalkable = isFloor;
                terrainGameObject.IsTransparent = isFloor;
                terrainGameObject.Tile = new VisualTile(
                    terrain.Id,
                    terrain.TileSymbol,
                    terrain.TileFgColor,
                    terrain.TileBgColor
                );

                map.SetTerrain(terrainGameObject);
            }
        }

        var freePosition = map.WalkabilityView
                              .Positions()
                              .FirstOrDefault(p => map.WalkabilityView[p]);

        var player = new CreatureGameObject(freePosition)
        {
            Tile = new VisualTile("player", "@", LyColor.White, LyColor.Transparent)
        };

        var simpleTorch = new ItemGameObject(new Point(10, 10))
        {
            Tile = new VisualTile("torch", "t", LyColor.Yellow, LyColor.Transparent)
        };
        simpleTorch.GoRogueComponents.Add(
            new LightSourceComponent(
                radius: 4,
                startColor: LyColor.Yellow,
                endColor: LyColor.Black
            )
        );
        simpleTorch.GoRogueComponents.Add(
            new LightBackgroundComponent(
                startBackground: LyColor.Orange,
                endBackground: LyColor.Transparent
            )
        );
        simpleTorch.GoRogueComponents.Add(
            new AnimationComponent(
                intervalSeconds: 1.0,
                onAnimationTrigger: () => simpleTorch.Tile.Symbol = simpleTorch.Tile.Symbol == "T" ? "t" : "T"
            )
        );

        var flickerTorch = new ItemGameObject(new Point(8, 6))
        {
            Tile = new VisualTile("torch", "t", LyColor.Yellow, LyColor.Transparent)
        };
        flickerTorch.GoRogueComponents.Add(
            new LightSourceComponent(
                radius: 4,
                startColor: LyColor.Yellow,
                endColor: LyColor.Black
            )
        );
        flickerTorch.GoRogueComponents.Add(
            new LightBackgroundComponent(
                startBackground: LyColor.Orange,
                endBackground: LyColor.Transparent
            )
        );
        flickerTorch.GoRogueComponents.Add(
            new LightFlickerComponent(
                mode: LightFlickerMode.Deterministic,
                intensity: 0.4f,
                radiusJitter: 1f,
                frequencyHz: 6f,
                seed: 86
            )
        );

        map.AddEntity(player);
        map.AddEntity(simpleTorch);
        map.AddEntity(flickerTorch);

        _logger.Information("Map generated");

        return map;
    }
}
