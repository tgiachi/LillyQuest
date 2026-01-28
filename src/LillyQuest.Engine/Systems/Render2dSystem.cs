using LillyQuest.Core.Data.Contexts;
using LillyQuest.Core.Graphics.Rendering2D;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Interfaces.Features;
using LillyQuest.Engine.Interfaces.Managers;
using LillyQuest.Engine.Systems.Base;
using LillyQuest.Engine.Types;

namespace LillyQuest.Engine.Systems;

public class Render2dSystem : BaseSystem<IRenderableEntity>, IDisposable
{
    private readonly ITextureManager _textureManager;
    private readonly IShaderManager _shaderManager;
    private readonly EngineRenderContext _renderContext;
    private readonly IFontManager _fontManager;

    private SpriteBatch? _spriteBatch;

    public Render2dSystem(
        ITextureManager textureManager,
        IShaderManager shaderManager,
        EngineRenderContext renderContext,
        IFontManager fontManager
    ) : base(150, "Render 2d System", SystemQueryType.Renderable)
    {
        _textureManager = textureManager;
        _shaderManager = shaderManager;
        _renderContext = renderContext;
        _fontManager = fontManager;
    }

    public override void Initialize()
    {
        _spriteBatch = new(_renderContext, _shaderManager, _fontManager, textureManager: _textureManager);
        base.Initialize();
    }

    protected override void ProcessTypedEntities(
        GameTime gameTime,
        IGameEntityManager entityManager,
        IReadOnlyList<IRenderableEntity> typedEntities
    )
    {
        var spriteBatch = _spriteBatch ?? throw new InvalidOperationException("Render2dSystem not initialized.");
        spriteBatch.Begin();

        foreach (var entity in typedEntities)
        {
            entity.Render(spriteBatch, _renderContext);
        }

        spriteBatch.End();
    }

    public void Dispose()
    {
        _spriteBatch?.Dispose();
        _spriteBatch = null;
    }
}
