using GoRogue.MapGeneration;
using LillyQuest.Core.Primitives;
using LillyQuest.RogueLike.Components;
using LillyQuest.RogueLike.GameObjects;
using LillyQuest.RogueLike.Interfaces.Services;
using LillyQuest.RogueLike.Maps;
using LillyQuest.RogueLike.Maps.Tiles;
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
        var map = new LyQuestMap(300, 300);
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
            Tile = new VisualTile("player", "@", LyColor.Transparent, LyColor.White)
        };

        var simpleTorch = new ItemGameObject(new Point(10, 10))
        {
            Tile = new VisualTile("torch", "t", LyColor.Transparent, LyColor.Yellow)
        };
        simpleTorch.GoRogueComponents.Add(new LightSourceComponent(
            radius: 4,
            startColor: LyColor.Yellow,
            endColor: LyColor.Black
        ));

        map.AddEntity(player);
        map.AddEntity(simpleTorch);

        return map;
    }
}
