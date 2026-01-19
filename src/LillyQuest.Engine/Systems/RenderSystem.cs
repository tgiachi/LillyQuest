using LillyQuest.Core.Graphics.Rendering2D;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Interfaces.Features;
using LillyQuest.Engine.Interfaces.Managers;
using LillyQuest.Engine.Interfaces.Systems;
using LillyQuest.Engine.Systems.Base;

namespace LillyQuest.Engine.Systems;

/// <summary>
/// System that manages rendering of all IRenderFeature instances.
/// Creates and maintains the main SpriteBatch used for 2D rendering.
/// </summary>
public class RenderSystem : BaseSystem, IRenderSystem
{
    private readonly IShaderManager _shaderManager;
    private readonly IFontManager _fontManager;
    private readonly ITextureManager _textureManager;

    private SpriteBatch? _spriteBatch;

    /// <summary>
    /// Gets the main SpriteBatch used for rendering.
    /// </summary>
    public SpriteBatch SpriteBatch
    {
        get => _spriteBatch ?? throw new InvalidOperationException("SpriteBatch not initialized. Call Initialize() first.");
        private set => _spriteBatch = value;
    }

    /// <summary>
    /// Creates a new RenderSystem with priority 100 (late execution).
    /// </summary>
    public RenderSystem(
        IGameEntityManager entityManager,
        IShaderManager shaderManager,
        IFontManager fontManager,
        ITextureManager textureManager
    )
        : base("Render System", 100, entityManager)
    {
        _shaderManager = shaderManager;
        _fontManager = fontManager;
        _textureManager = textureManager;
    }

    /// <summary>
    /// Renders all IRenderFeature instances sorted by RenderOrder.
    /// </summary>
    public void Render(GameTime gameTime)
    {
        // Query all render features
        var features = EntityManager.QueryOfType<IRenderFeature>()
                                    .Where(f => f.IsEnabled)
                                    .OrderBy(f => f.RenderOrder)
                                    .ToList();

        if (features.Count == 0)
        {
            return;
        }

        // Begin rendering
        SpriteBatch.Begin();

        // Render all features
        foreach (var feature in features)
        {
            feature.Render(SpriteBatch, gameTime);
        }

        // End rendering
        SpriteBatch.End();
    }

    /// <summary>
    /// Disposes the SpriteBatch on shutdown.
    /// </summary>
    public override void Shutdown()
    {
        SpriteBatch?.Dispose();
        base.Shutdown();
    }

    /// <summary>
    /// Initializes the SpriteBatch with the render context.
    /// </summary>
    protected override void OnInitialize()
    {
        SpriteBatch = new(
            RenderContext,
            _shaderManager,
            _fontManager,
            textureManager: _textureManager
        );
    }
}
