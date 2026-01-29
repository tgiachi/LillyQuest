using LillyQuest.Core.Data.Contexts;
using LillyQuest.Core.Graphics.Rendering2D;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Core.Primitives;
using LillyQuest.Engine.Interfaces.Managers;
using LillyQuest.Engine.Interfaces.Systems;
using LillyQuest.Engine.Types;

namespace LillyQuest.Engine.Systems;

public class ScreenSystem : ISystem, IDisposable
{
    public uint Order => 160;
    public string Name => "Screen system";
    public SystemQueryType QueryType => SystemQueryType.Renderable;

    private SpriteBatch? _spriteBatch;

    private readonly IScreenManager _screenManager;
    private readonly EngineRenderContext _renderContext;
    private readonly IShaderManager _shaderManager;
    private readonly ITextureManager _textureManager;
    private readonly IFontManager _fontManager;

    public ScreenSystem(
        IScreenManager screenManager,
        EngineRenderContext renderContext,
        IShaderManager shaderManager,
        IFontManager fontManager,
        ITextureManager textureManager
    )
    {
        _screenManager = screenManager;
        _renderContext = renderContext;
        _shaderManager = shaderManager;
        _fontManager = fontManager;
        _textureManager = textureManager;
    }

    public void Initialize()
    {
        _spriteBatch = new(_renderContext, _shaderManager, _fontManager, textureManager: _textureManager);
    }

    public void ProcessEntities(GameTime gameTime, IGameEntityManager entityManager)
    {
        var spriteBatch = _spriteBatch ?? throw new InvalidOperationException("ScreenSystem not initialized.");
        spriteBatch.Begin();

        _screenManager.Render(spriteBatch, _renderContext);

        spriteBatch.End();
    }

    public void Dispose()
    {
        _spriteBatch?.Dispose();
        _spriteBatch = null;
        GC.SuppressFinalize(this);
    }
}
