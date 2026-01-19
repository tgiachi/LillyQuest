using System.Numerics;
using System.Text;
using FontStashSharp;
using FontStashSharp.Interfaces;
using LillyQuest.Core.Data.Contexts;
using LillyQuest.Core.Graphics.OpenGL.Buffers;
using LillyQuest.Core.Graphics.OpenGL.Resources;
using LillyQuest.Core.Graphics.Text;
using LillyQuest.Core.Interfaces.Assets;
using LillyQuest.Core.Primitives;
using LillyQuest.Core.Types;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Shader = LillyQuest.Core.Graphics.OpenGL.Resources.Shader;

namespace LillyQuest.Core.Graphics.Rendering2D;

public class SpriteBatch : IFontStashRenderer2, IDisposable
{
    private const int INITIAL_SPRITE_COUNT = 2048;
    private const int GROWTH_FACTOR = 2;

    private readonly GL _gl;
    private readonly EngineRenderContext _renderContext;
    public EngineRenderContext RenderContext => _renderContext;
    private BufferObject<VertexPositionColorTexture> _vertexBuffer;
    private BufferObject<short> _indexBuffer;
    private readonly VertexArrayObject<short> _vao;
    private VertexPositionColorTexture[] _vertexData;
    private short[] _indexData;
    private object _lastTexture;
    private int _vertexIndex;
    private int _currentCapacity;
    private readonly List<SpriteBatchItem> _sortedBatch;

    private readonly FontSharpTexture2DManager _textureManager;
    private readonly IFontManager _fontManager;
    private readonly ITextureManager? _assetTextureManager;
    private readonly Texture2D _shapeTexture;
    private readonly bool _ownsShapeTexture;

    public ITexture2DManager TextureManager => _textureManager;
    public bool IsActive { get; private set; }

    public BatchingMode BeginMode { get; private set; }

    public Shader ShaderProgram { get; }

    public int TextureUniform => ShaderProgram.GetUniformLocation("TextureSampler");

    public Rectangle<int> Viewport { get; set; } = new(0, 0, 1200, 800);

    public unsafe SpriteBatch(
        EngineRenderContext context,
        IShaderManager shaderManager,
        IFontManager fontManager,
        string shaderName = "texture2d",
        BatchingMode batchingMode = BatchingMode.Deferred,
        ITextureManager? textureManager = null
    )
    {
        _renderContext = context;
        _gl = context.Gl;
        context.Window.Resize += OnResize;
        ShaderProgram = shaderManager.GetShader(shaderName);
        _fontManager = fontManager;
        BeginMode = batchingMode;
        _textureManager = new(_gl);
        _assetTextureManager = textureManager;

        if (textureManager != null)
        {
            _shapeTexture = textureManager.DefaultWhiteTexture;
            _ownsShapeTexture = false;
        }
        else
        {
            var whitePixel = new byte[] { 255, 255, 255, 255 };
            _shapeTexture = new(_gl, whitePixel, 1, 1);
            _ownsShapeTexture = true;
        }
        _sortedBatch = new();
        _currentCapacity = INITIAL_SPRITE_COUNT;

        InitializeBuffers();

        _vao = new(_gl, sizeof(VertexPositionColorTexture), _indexBuffer);
        _vao.Bind();

        var location = ShaderProgram.GetAttribLocation("a_position");
        _vao.VertexAttribPointer(location, 3, VertexAttribPointerType.Float, false, 0);

        location = ShaderProgram.GetAttribLocation("a_color");
        _vao.VertexAttribPointer(location, 4, VertexAttribPointerType.UnsignedByte, true, 12);

        location = ShaderProgram.GetAttribLocation("a_texCoords0");
        _vao.VertexAttribPointer(location, 2, VertexAttribPointerType.Float, false, 16);

        Viewport = new(0, 0, context.Window.Size.X, context.Window.Size.Y);
    }

    ~SpriteBatch()
        => Dispose(false);

    public void Begin(BatchingMode batchingMode = BatchingMode.Deferred)
    {
        IsActive = true;
        BeginMode = batchingMode;
        _vertexIndex = 0;
        _lastTexture = null;

        _gl.Disable(EnableCap.DepthTest);
        _gl.Enable(EnableCap.Blend);
        _gl.BlendFunc(BlendingFactor.One, BlendingFactor.OneMinusSrcAlpha);

        ShaderProgram.Use();
        ShaderProgram.SetUniform("TextureSampler", 0);

        var transform = Matrix4x4.CreateOrthographicOffCenter(
            Viewport.Origin.X,
            Viewport.Origin.X + Viewport.Size.X,
            Viewport.Origin.Y + Viewport.Size.Y,
            Viewport.Origin.Y,
            0,
            -1
        );
        ShaderProgram.SetUniform("MatrixTransform", transform);

        _vao.Bind();
        _vertexBuffer.Bind();

        if (BeginMode == BatchingMode.SortByTexture)
        {
            _sortedBatch.Clear();
        }
        else if (BeginMode == BatchingMode.SortByDepthThenTexture)
        {
            _sortedBatch.Clear();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Draw a sprite with basic parameters.
    /// </summary>
    public void Draw(object texture, Vector2 position, Vector2 size, LyColor color, float depth = 0f)
    {
        Draw(texture, position, size, color, 0f, Vector2.Zero, depth);
    }

    /// <summary>
    /// Draw a sprite with basic parameters.
    /// </summary>
    public void Draw(object texture, Vector2 position, Vector2 size, FSColor color, float depth = 0f)
    {
        Draw(texture, position, size, ToDarkLyColor(color), depth);
    }

    /// <summary>
    /// Draw a sprite with position, size, color, and rotation.
    /// </summary>
    public void Draw(object texture, Vector2 position, Vector2 size, LyColor color, float rotation, float depth = 0f)
    {
        Draw(texture, position, size, color, rotation, size / 2f, depth);
    }

    /// <summary>
    /// Draw a sprite with position, size, color, and rotation.
    /// </summary>
    public void Draw(object texture, Vector2 position, Vector2 size, FSColor color, float rotation, float depth = 0f)
    {
        Draw(texture, position, size, ToDarkLyColor(color), rotation, depth);
    }

    /// <summary>
    /// Draw a sprite with position, size, color, rotation, and origin.
    /// </summary>
    public void Draw(
        object texture,
        Vector2 position,
        Vector2 size,
        LyColor color,
        float rotation,
        Vector2 origin,
        float depth = 0f
    )
    {
        var rect = new Rectangle<float>(0, 0, 1, 1);
        Draw(texture, position, size, color, rotation, origin, rect, depth);
    }

    /// <summary>
    /// Draw a sprite with position, size, color, rotation, and origin.
    /// </summary>
    public void Draw(
        object texture,
        Vector2 position,
        Vector2 size,
        FSColor color,
        float rotation,
        Vector2 origin,
        float depth = 0f
    )
    {
        Draw(texture, position, size, ToDarkLyColor(color), rotation, origin, depth);
    }

    /// <summary>
    /// Draw a sprite with all parameters including source rectangle and depth.
    /// </summary>
    public void Draw(
        object texture,
        Vector2 position,
        Vector2 size,
        LyColor color,
        float rotation,
        Vector2 origin,
        Rectangle<float> sourceRect,
        float depth
    )
    {
        var fsColor = ToFsColor(color);
        var topLeft = new Vector3(position.X - origin.X, position.Y - origin.Y, depth);
        var topRight = new Vector3(position.X - origin.X + size.X, position.Y - origin.Y, depth);
        var bottomLeft = new Vector3(position.X - origin.X, position.Y - origin.Y + size.Y, depth);
        var bottomRight = new Vector3(position.X - origin.X + size.X, position.Y - origin.Y + size.Y, depth);

        if (rotation != 0f)
        {
            var cos = MathF.Cos(rotation);
            var sin = MathF.Sin(rotation);

            topLeft = RotatePoint(topLeft, origin, cos, sin);
            topRight = RotatePoint(topRight, origin, cos, sin);
            bottomLeft = RotatePoint(bottomLeft, origin, cos, sin);
            bottomRight = RotatePoint(bottomRight, origin, cos, sin);
        }

        var tlVertex = new VertexPositionColorTexture(topLeft, fsColor, new(sourceRect.Origin.X, sourceRect.Origin.Y));
        var trVertex = new VertexPositionColorTexture(
            topRight,
            fsColor,
            new(sourceRect.Origin.X + sourceRect.Size.X, sourceRect.Origin.Y)
        );
        var blVertex = new VertexPositionColorTexture(
            bottomLeft,
            fsColor,
            new(sourceRect.Origin.X, sourceRect.Origin.Y + sourceRect.Size.Y)
        );
        var brVertex = new VertexPositionColorTexture(
            bottomRight,
            fsColor,
            new(sourceRect.Origin.X + sourceRect.Size.X, sourceRect.Origin.Y + sourceRect.Size.Y)
        );

        DrawQuad(texture, ref tlVertex, ref trVertex, ref blVertex, ref brVertex);
    }

    /// <summary>
    /// Draw a sprite with all parameters including source rectangle and depth.
    /// </summary>
    public void Draw(
        object texture,
        Vector2 position,
        Vector2 size,
        FSColor color,
        float rotation,
        Vector2 origin,
        Rectangle<float> sourceRect,
        float depth
    )
    {
        Draw(texture, position, size, ToDarkLyColor(color), rotation, origin, sourceRect, depth);
    }

    /// <summary>
    /// Draws a circle outline using the default white texture.
    /// </summary>
    public void DrawCircle(
        Vector2 center,
        float radius,
        LyColor color,
        int segments = 32,
        float thickness = 5f,
        float depth = 0f
    )
    {
        if (radius <= 0f || segments < 3)
        {
            return;
        }

        var angleStep = MathF.Tau / segments;
        var prev = center + new Vector2(radius, 0f);

        for (var i = 1; i <= segments; i++)
        {
            var angle = angleStep * i;
            var next = center + new Vector2(MathF.Cos(angle) * radius, MathF.Sin(angle) * radius);
            DrawLine(prev, next, color, thickness, depth);
            prev = next;
        }
    }

    /// <summary>
    /// Draws a circle outline using the default white texture.
    /// </summary>
    public void DrawCircle(
        Vector2 center,
        float radius,
        FSColor color,
        int segments = 32,
        float thickness = 5f,
        float depth = 0f
    )
    {
        DrawCircle(center, radius, ToDarkLyColor(color), segments, thickness, depth);
    }

    /// <summary>
    /// Draws a filled circle using the default white texture.
    /// </summary>
    public void DrawCircleFilled(Vector2 center, float radius, LyColor color, float depth = 0f)
    {
        if (radius <= 0f)
        {
            return;
        }

        var radiusSquared = radius * radius;

        for (var y = -radius; y <= radius; y += 1f)
        {
            var x = MathF.Sqrt(radiusSquared - y * y);
            var start = new Vector2(center.X - x, center.Y + y);
            var end = new Vector2(center.X + x, center.Y + y);
            DrawLine(start, end, color, 1f, depth);
        }
    }

    /// <summary>
    /// Draws a filled circle using the default white texture.
    /// </summary>
    public void DrawCircleFilled(Vector2 center, float radius, FSColor color, float depth = 0f)
    {
        DrawCircleFilled(center, radius, ToDarkLyColor(color), depth);
    }

    /// <summary>
    /// Draws text using a named font and size.
    /// </summary>
    public float DrawFont(
        string fontName,
        int size,
        string text,
        Vector2 position,
        LyColor color,
        float rotation,
        Vector2 origin,
        Vector2? scale,
        float layerDepth,
        float characterSpacing,
        float lineSpacing,
        TextStyle textStyle,
        FontSystemEffect effect,
        int effectAmount
    )
    {
        ArgumentNullException.ThrowIfNull(fontName);
        ArgumentNullException.ThrowIfNull(text);

        var font = _fontManager.GetFont(fontName, size);
        var fsColor = ToFsColor(color);

        return font.DrawText(
            this,
            text,
            position,
            fsColor,
            rotation,
            origin,
            scale,
            layerDepth,
            characterSpacing,
            lineSpacing,
            textStyle,
            effect,
            effectAmount
        );
    }

    /// <summary>
    /// Draws text using a named font, size, and color.
    /// </summary>
    public float DrawFont(string fontName, int size, string text, Vector2 position, LyColor color)
        => DrawFont(
            fontName,
            size,
            text,
            position,
            color,
            0f,
            Vector2.Zero,
            Vector2.One * 2,
            0f,
            0f,
            0f,
            TextStyle.None,
            FontSystemEffect.None,
            0
        );

    /// <summary>
    /// Draws text using a named font and size with per-character colors.
    /// </summary>
    public float DrawFont(
        string fontName,
        int size,
        string text,
        Vector2 position,
        LyColor[] colors,
        float rotation,
        Vector2 origin,
        Vector2? scale,
        float layerDepth,
        float characterSpacing,
        float lineSpacing,
        TextStyle textStyle,
        FontSystemEffect effect,
        int effectAmount
    )
    {
        ArgumentNullException.ThrowIfNull(fontName);
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(colors);

        var font = _fontManager.GetFont(fontName, size);
        var fsColors = ToFsColors(colors);

        return font.DrawText(
            this,
            text,
            position,
            fsColors,
            rotation,
            origin,
            scale,
            layerDepth,
            characterSpacing,
            lineSpacing,
            textStyle,
            effect,
            effectAmount
        );
    }

    /// <summary>
    /// Draws text using a named font and size.
    /// </summary>
    public float DrawFont(
        string fontName,
        int size,
        StringSegment text,
        Vector2 position,
        LyColor color,
        float rotation,
        Vector2 origin,
        Vector2? scale,
        float layerDepth,
        float characterSpacing,
        float lineSpacing,
        TextStyle textStyle,
        FontSystemEffect effect,
        int effectAmount
    )
    {
        ArgumentNullException.ThrowIfNull(fontName);

        var font = _fontManager.GetFont(fontName, size);
        var fsColor = ToFsColor(color);

        return font.DrawText(
            this,
            text,
            position,
            fsColor,
            rotation,
            origin,
            scale,
            layerDepth,
            characterSpacing,
            lineSpacing,
            textStyle,
            effect,
            effectAmount
        );
    }

    /// <summary>
    /// Draws text using a named font and size with per-character colors.
    /// </summary>
    public float DrawFont(
        string fontName,
        int size,
        StringSegment text,
        Vector2 position,
        LyColor[] colors,
        float rotation,
        Vector2 origin,
        Vector2? scale,
        float layerDepth,
        float characterSpacing,
        float lineSpacing,
        TextStyle textStyle,
        FontSystemEffect effect,
        int effectAmount
    )
    {
        ArgumentNullException.ThrowIfNull(fontName);
        ArgumentNullException.ThrowIfNull(colors);

        var font = _fontManager.GetFont(fontName, size);
        var fsColors = ToFsColors(colors);

        return font.DrawText(
            this,
            text,
            position,
            fsColors,
            rotation,
            origin,
            scale,
            layerDepth,
            characterSpacing,
            lineSpacing,
            textStyle,
            effect,
            effectAmount
        );
    }

    /// <summary>
    /// Draws text using a named font and size.
    /// </summary>
    public float DrawFont(
        string fontName,
        int size,
        StringBuilder text,
        Vector2 position,
        LyColor color,
        float rotation,
        Vector2 origin,
        Vector2? scale,
        float layerDepth,
        float characterSpacing,
        float lineSpacing,
        TextStyle textStyle,
        FontSystemEffect effect,
        int effectAmount
    )
    {
        ArgumentNullException.ThrowIfNull(fontName);
        ArgumentNullException.ThrowIfNull(text);

        var font = _fontManager.GetFont(fontName, size);
        var fsColor = ToFsColor(color);

        return font.DrawText(
            this,
            text,
            position,
            fsColor,
            rotation,
            origin,
            scale,
            layerDepth,
            characterSpacing,
            lineSpacing,
            textStyle,
            effect,
            effectAmount
        );
    }

    /// <summary>
    /// Draws text using a named font and size with per-character colors.
    /// </summary>
    public float DrawFont(
        string fontName,
        int size,
        StringBuilder text,
        Vector2 position,
        LyColor[] colors,
        float rotation,
        Vector2 origin,
        Vector2? scale,
        float layerDepth,
        float characterSpacing,
        float lineSpacing,
        TextStyle textStyle,
        FontSystemEffect effect,
        int effectAmount
    )
    {
        ArgumentNullException.ThrowIfNull(fontName);
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(colors);

        var font = _fontManager.GetFont(fontName, size);
        var fsColors = ToFsColors(colors);

        return font.DrawText(
            this,
            text,
            position,
            fsColors,
            rotation,
            origin,
            scale,
            layerDepth,
            characterSpacing,
            lineSpacing,
            textStyle,
            effect,
            effectAmount
        );
    }

    /// <summary>
    /// Draws text using a bitmap font atlas.
    /// </summary>
    public void DrawFontBmp(
        string fontName,
        string text,
        Vector2 position,
        int size,
        LyColor color,
        float depth = 0f
    )
    {
        ArgumentNullException.ThrowIfNull(fontName);
        ArgumentNullException.ThrowIfNull(text);

        var font = _fontManager.GetBitmapFont(fontName);
        var glyphHeight = size > 0 ? size : font.TileHeight;
        var aspect = font.TileWidth / (float)font.TileHeight;
        var glyphWidth = size > 0 ? glyphHeight * aspect : font.TileWidth;

        DrawFontBmp(fontName, text, position, new Vector2(glyphWidth, glyphHeight), color, depth);
    }

    /// <summary>
    /// Draws text using a bitmap font atlas.
    /// </summary>
    public void DrawFontBmp(
        string fontName,
        string text,
        Vector2 position,
        Vector2 size,
        LyColor color,
        float depth = 0f
    )
    {
        ArgumentNullException.ThrowIfNull(fontName);
        ArgumentNullException.ThrowIfNull(text);

        var font = _fontManager.GetBitmapFont(fontName);
        var fsColor = ToFsColor(color);

        var glyphSize = size;

        if (glyphSize.X <= 0f || glyphSize.Y <= 0f)
        {
            glyphSize = new(font.TileWidth, font.TileHeight);
        }

        var scaleX = glyphSize.X / font.TileWidth;
        var scaleY = glyphSize.Y / font.TileHeight;
        var spacingX = font.Spacing * scaleX;
        var spacingY = font.Spacing * scaleY;

        var cursor = position;

        foreach (var ch in text)
        {
            if (ch == '\r')
            {
                continue;
            }

            if (ch == '\n')
            {
                cursor.X = position.X;
                cursor.Y += glyphSize.Y + spacingY;

                continue;
            }

            if (!font.TryGetGlyphIndex(ch, out var glyphIndex) || glyphIndex < 0 || glyphIndex >= font.GlyphCount)
            {
                cursor.X += glyphSize.X + spacingX;

                continue;
            }

            var column = glyphIndex % font.Columns;
            var row = glyphIndex / font.Columns;

            var u = column * font.TileWidth / (float)font.Texture.Width;
            var v = row * font.TileHeight / (float)font.Texture.Height;
            var uSize = font.TileWidth / (float)font.Texture.Width;
            var vSize = font.TileHeight / (float)font.Texture.Height;

            var sourceRect = new Rectangle<float>(u, v, uSize, vSize);

            Draw(
                font.Texture,
                cursor,
                glyphSize,
                fsColor,
                0f,
                Vector2.Zero,
                sourceRect,
                depth
            );

            cursor.X += glyphSize.X + spacingX;
        }
    }

    /// <summary>
    /// Draws a line between two points using the default white texture.
    /// </summary>
    public void DrawLine(Vector2 start, Vector2 end, LyColor color, float thickness = 5f, float depth = 0f)
    {
        var direction = end - start;
        var length = direction.Length();

        if (length <= 0f)
        {
            return;
        }

        var rotation = MathF.Atan2(direction.Y, direction.X);
        var size = new Vector2(length, thickness);
        var origin = new Vector2(0f, thickness / 2f);

        Draw(_shapeTexture, start, size, color, rotation, origin, depth);
    }

    /// <summary>
    /// Draws a line between two points using the default white texture.
    /// </summary>
    public void DrawLine(Vector2 start, Vector2 end, FSColor color, float thickness = 5f, float depth = 0f)
    {
        DrawLine(start, end, ToDarkLyColor(color), thickness, depth);
    }

    /// <summary>
    /// Draws a polygon outline using the default white texture.
    /// </summary>
    public void DrawPolygon(IReadOnlyList<Vector2> points, LyColor color, float thickness = 5f, float depth = 0f)
    {
        ArgumentNullException.ThrowIfNull(points);

        if (points.Count < 2)
        {
            return;
        }

        for (var i = 0; i < points.Count - 1; i++)
        {
            DrawLine(points[i], points[i + 1], color, thickness, depth);
        }

        if (points.Count > 2)
        {
            DrawLine(points[^1], points[0], color, thickness, depth);
        }
    }

    /// <summary>
    /// Draws a polygon outline using the default white texture.
    /// </summary>
    public void DrawPolygon(IReadOnlyList<Vector2> points, FSColor color, float thickness = 5f, float depth = 0f)
    {
        DrawPolygon(points, ToDarkLyColor(color), thickness, depth);
    }

    /// <summary>
    /// Draws a polygon outline using the default white texture.
    /// </summary>
    public void DrawPolygon(ReadOnlySpan<Vector2> points, LyColor color, float thickness = 5f, float depth = 0f)
    {
        if (points.Length < 2)
        {
            return;
        }

        for (var i = 0; i < points.Length - 1; i++)
        {
            DrawLine(points[i], points[i + 1], color, thickness, depth);
        }

        if (points.Length > 2)
        {
            DrawLine(points[^1], points[0], color, thickness, depth);
        }
    }

    /// <summary>
    /// Draws a polygon outline using the default white texture.
    /// </summary>
    public void DrawPolygon(ReadOnlySpan<Vector2> points, FSColor color, float thickness = 5f, float depth = 0f)
    {
        DrawPolygon(points, ToDarkLyColor(color), thickness, depth);
    }

    /// <summary>
    /// Draws a filled polygon using triangle fan triangulation (assumes convex polygon).
    /// </summary>
    public void DrawPolygonFilled(IReadOnlyList<Vector2> points, LyColor color, float depth = 0f)
    {
        ArgumentNullException.ThrowIfNull(points);

        if (points.Count < 3)
        {
            return;
        }

        var origin = points[0];

        for (var i = 1; i < points.Count - 1; i++)
        {
            DrawTriangleFilled(origin, points[i], points[i + 1], color, depth);
        }
    }

    /// <summary>
    /// Draws a filled polygon using triangle fan triangulation (assumes convex polygon).
    /// </summary>
    public void DrawPolygonFilled(IReadOnlyList<Vector2> points, FSColor color, float depth = 0f)
    {
        DrawPolygonFilled(points, ToDarkLyColor(color), depth);
    }

    /// <summary>
    /// Draws a filled polygon using triangle fan triangulation (assumes convex polygon).
    /// </summary>
    public void DrawPolygonFilled(ReadOnlySpan<Vector2> points, LyColor color, float depth = 0f)
    {
        if (points.Length < 3)
        {
            return;
        }

        var origin = points[0];

        for (var i = 1; i < points.Length - 1; i++)
        {
            DrawTriangleFilled(origin, points[i], points[i + 1], color, depth);
        }
    }

    /// <summary>
    /// Draws a filled polygon using triangle fan triangulation (assumes convex polygon).
    /// </summary>
    public void DrawPolygonFilled(ReadOnlySpan<Vector2> points, FSColor color, float depth = 0f)
    {
        DrawPolygonFilled(points, ToDarkLyColor(color), depth);
    }

    public void DrawQuad(
        object texture,
        ref VertexPositionColorTexture topLeft,
        ref VertexPositionColorTexture topRight,
        ref VertexPositionColorTexture bottomLeft,
        ref VertexPositionColorTexture bottomRight
    )
    {
        if (!IsActive)
        {
            throw new InvalidOperationException("Begin() must be called before drawing");
        }

        EnsureBufferCapacity(4);

        if (BeginMode == BatchingMode.Immediate)
        {
            FlushBuffer();
            _vertexData[_vertexIndex++] = topLeft;
            _vertexData[_vertexIndex++] = topRight;
            _vertexData[_vertexIndex++] = bottomLeft;
            _vertexData[_vertexIndex++] = bottomRight;
            FlushBuffer();
        }
        else if (BeginMode == BatchingMode.SortByTexture || BeginMode == BatchingMode.SortByDepthThenTexture)
        {
            _sortedBatch.Add(new(texture, topLeft.Position.Z, topLeft, topRight, bottomLeft, bottomRight));
        }
        else if (BeginMode == BatchingMode.OnTheFly)
        {
            if (_lastTexture != texture)
            {
                FlushBuffer();
            }
            _vertexData[_vertexIndex++] = topLeft;
            _vertexData[_vertexIndex++] = topRight;
            _vertexData[_vertexIndex++] = bottomLeft;
            _vertexData[_vertexIndex++] = bottomRight;
            _lastTexture = texture;
        }
        else // Deferred
        {
            if (_lastTexture != texture)
            {
                FlushBuffer();
            }
            _vertexData[_vertexIndex++] = topLeft;
            _vertexData[_vertexIndex++] = topRight;
            _vertexData[_vertexIndex++] = bottomLeft;
            _vertexData[_vertexIndex++] = bottomRight;
            _lastTexture = texture;
        }
    }

    /// <summary>
    /// Draw raw vertex data.
    /// </summary>
    public void DrawRaw(
        object texture,
        VertexPositionColorTexture[] vertices,
        int vertexCount
    )
    {
        if (!IsActive)
        {
            throw new InvalidOperationException("Begin() must be called before drawing");
        }

        if (vertexCount > vertices.Length || vertexCount % 4 != 0)
        {
            throw new ArgumentException("Vertex count must be a multiple of 4 and not exceed array length");
        }

        EnsureBufferCapacity(vertexCount);

        if (_lastTexture != texture && BeginMode != BatchingMode.Immediate)
        {
            FlushBuffer();
        }

        Array.Copy(vertices, 0, _vertexData, _vertexIndex, vertexCount);
        _vertexIndex += vertexCount;
        _lastTexture = texture;

        if (BeginMode == BatchingMode.Immediate)
        {
            FlushBuffer();
        }
    }

    /// <summary>
    /// Draw raw vertex data with transformation matrix.
    /// </summary>
    public void DrawRaw(
        object texture,
        VertexPositionColorTexture[] vertices,
        int vertexCount,
        Matrix4x4 transform
    )
    {
        var transformedVertices = new VertexPositionColorTexture[vertexCount];

        for (var i = 0; i < vertexCount; i++)
        {
            var pos = Vector3.Transform(vertices[i].Position, transform);
            transformedVertices[i] = new(pos, vertices[i].Color, vertices[i].TextureCoordinate);
        }

        DrawRaw(texture, transformedVertices, vertexCount);
    }

    /// <summary>
    /// Draws a filled rectangle using the default white texture.
    /// </summary>
    public void DrawRectangle(Vector2 position, Vector2 size, LyColor color, float depth = 0f)
    {
        Draw(_shapeTexture, position, size, color, depth);
    }

    /// <summary>
    /// Draws a filled rectangle using the default white texture.
    /// </summary>
    public void DrawRectangle(Vector2 position, Vector2 size, FSColor color, float depth = 0f)
    {
        DrawRectangle(position, size, ToDarkLyColor(color), depth);
    }

    /// <summary>
    /// Draws a rectangle outline using the default white texture.
    /// </summary>
    public void DrawRectangleOutline(
        Vector2 position,
        Vector2 size,
        LyColor color,
        float thickness = 5f,
        float depth = 0f
    )
    {
        var topLeft = position;
        var topRight = new Vector2(position.X + size.X, position.Y);
        var bottomLeft = new Vector2(position.X, position.Y + size.Y);
        var bottomRight = new Vector2(position.X + size.X, position.Y + size.Y);

        DrawLine(topLeft, topRight, color, thickness, depth);
        DrawLine(topRight, bottomRight, color, thickness, depth);
        DrawLine(bottomRight, bottomLeft, color, thickness, depth);
        DrawLine(bottomLeft, topLeft, color, thickness, depth);
    }

    /// <summary>
    /// Draws a rectangle outline using the default white texture.
    /// </summary>
    public void DrawRectangleOutline(Vector2 position, Vector2 size, FSColor color, float thickness = 5f, float depth = 0f)
    {
        DrawRectangleOutline(position, size, ToDarkLyColor(color), thickness, depth);
    }

    /// <summary>
    /// Draw a texture by asset name with basic parameters.
    /// </summary>
    public void DrawTexture(string textureName, Vector2 position, Vector2 size, LyColor color, float depth = 0f)
    {
        ArgumentNullException.ThrowIfNull(textureName);

        if (_assetTextureManager == null)
        {
            throw new InvalidOperationException("Texture manager is not available for name-based drawing.");
        }

        var texture = _assetTextureManager.GetTexture(textureName);
        Draw(texture, position, size, color, depth);
    }

    /// <summary>
    /// Draw a texture by asset name with position, size, color, and rotation.
    /// </summary>
    public void DrawTexture(
        string textureName,
        Vector2 position,
        Vector2 size,
        LyColor color,
        float rotation,
        float depth = 0f
    )
    {
        ArgumentNullException.ThrowIfNull(textureName);

        if (_assetTextureManager == null)
        {
            throw new InvalidOperationException("Texture manager is not available for name-based drawing.");
        }

        var texture = _assetTextureManager.GetTexture(textureName);
        Draw(texture, position, size, color, rotation, depth);
    }

    /// <summary>
    /// Draw a texture by asset name with position, size, color, rotation, and origin.
    /// </summary>
    public void DrawTexture(
        string textureName,
        Vector2 position,
        Vector2 size,
        LyColor color,
        float rotation,
        Vector2 origin,
        float depth = 0f
    )
    {
        ArgumentNullException.ThrowIfNull(textureName);

        if (_assetTextureManager == null)
        {
            throw new InvalidOperationException("Texture manager is not available for name-based drawing.");
        }

        var texture = _assetTextureManager.GetTexture(textureName);
        Draw(texture, position, size, color, rotation, origin, depth);
    }

    /// <summary>
    /// Draws a triangle outline using the default white texture.
    /// </summary>
    public void DrawTriangle(
        Vector2 p1,
        Vector2 p2,
        Vector2 p3,
        LyColor color,
        float thickness = 5f,
        float depth = 0f
    )
    {
        DrawLine(p1, p2, color, thickness, depth);
        DrawLine(p2, p3, color, thickness, depth);
        DrawLine(p3, p1, color, thickness, depth);
    }

    /// <summary>
    /// Draws a triangle outline using the default white texture.
    /// </summary>
    public void DrawTriangle(
        Vector2 p1,
        Vector2 p2,
        Vector2 p3,
        FSColor color,
        float thickness = 5f,
        float depth = 0f
    )
    {
        DrawTriangle(p1, p2, p3, ToDarkLyColor(color), thickness, depth);
    }

    /// <summary>
    /// Draws a filled triangle using the default white texture.
    /// </summary>
    public void DrawTriangleFilled(Vector2 p1, Vector2 p2, Vector2 p3, LyColor color, float depth = 0f)
    {
        var fsColor = ToFsColor(color);
        var v1 = new VertexPositionColorTexture(new(p1.X, p1.Y, depth), fsColor, Vector2.Zero);
        var v2 = new VertexPositionColorTexture(new(p2.X, p2.Y, depth), fsColor, Vector2.Zero);
        var v3 = new VertexPositionColorTexture(new(p3.X, p3.Y, depth), fsColor, Vector2.Zero);
        var v4 = v3;

        DrawQuad(_shapeTexture, ref v1, ref v2, ref v3, ref v4);
    }

    /// <summary>
    /// Draws a filled triangle using the default white texture.
    /// </summary>
    public void DrawTriangleFilled(Vector2 p1, Vector2 p2, Vector2 p3, FSColor color, float depth = 0f)
    {
        DrawTriangleFilled(p1, p2, p3, ToDarkLyColor(color), depth);
    }

    public void End()
    {
        if (!IsActive)
        {
            return;
        }

        if (BeginMode == BatchingMode.SortByTexture || BeginMode == BatchingMode.SortByDepthThenTexture)
        {
            FlushSortedBatch();
        }
        else
        {
            FlushBuffer();
        }

        _gl.Enable(EnableCap.DepthTest);
        IsActive = false;
        _lastTexture = null;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        _vao.Dispose();
        _vertexBuffer.Dispose();
        _indexBuffer.Dispose();
        ShaderProgram.Dispose();

        if (_ownsShapeTexture)
        {
            _shapeTexture.Dispose();
        }
    }

    private void EnsureBufferCapacity(int vertexCount)
    {
        var requiredCapacity = (_vertexIndex + vertexCount) / 4;

        if (requiredCapacity > _currentCapacity)
        {
            var newCapacity = _currentCapacity * GROWTH_FACTOR;

            while (newCapacity < requiredCapacity)
            {
                newCapacity *= GROWTH_FACTOR;
            }

            FlushBuffer();

            _currentCapacity = newCapacity;
            _vertexBuffer.Dispose();
            _indexBuffer.Dispose();
            InitializeBuffers();

            _vao.Bind();
            _vertexBuffer.Bind();
        }
    }

    private unsafe void FlushBuffer()
    {
        if (_vertexIndex == 0 || _lastTexture == null)
        {
            _vertexIndex = 0;

            return;
        }

        _vertexBuffer.SetData(_vertexData, 0, _vertexIndex);

        var texture = (Texture2D)_lastTexture;
        texture.Bind();

        _gl.DrawElements(PrimitiveType.Triangles, (uint)(_vertexIndex * 6 / 4), DrawElementsType.UnsignedShort, null);
        _vertexIndex = 0;
    }

    private void FlushSortedBatch()
    {
        if (_sortedBatch.Count == 0)
        {
            return;
        }

        var sortedItems = BeginMode == BatchingMode.SortByDepthThenTexture
                              ? _sortedBatch.OrderBy(x => x.Depth).ThenBy(x => x.Texture).ToList()
                              : _sortedBatch.OrderBy(x => x.Texture).ToList();

        foreach (var item in sortedItems)
        {
            var tlVertex = item.TopLeft;
            var trVertex = item.TopRight;
            var blVertex = item.BottomLeft;
            var brVertex = item.BottomRight;

            if (_lastTexture != item.Texture)
            {
                FlushBuffer();
                _lastTexture = item.Texture;
            }

            EnsureBufferCapacity(4);
            _vertexData[_vertexIndex++] = tlVertex;
            _vertexData[_vertexIndex++] = trVertex;
            _vertexData[_vertexIndex++] = blVertex;
            _vertexData[_vertexIndex++] = brVertex;
        }

        FlushBuffer();
    }

    private static short[] GenerateIndexArray(int maxIndices)
    {
        var result = new short[maxIndices];

        for (int i = 0,
                 j = 0;
             i < maxIndices;
             i += 6, j += 4)
        {
            result[i] = (short)j;
            result[i + 1] = (short)(j + 1);
            result[i + 2] = (short)(j + 2);
            result[i + 3] = (short)(j + 3);
            result[i + 4] = (short)(j + 2);
            result[i + 5] = (short)(j + 1);
        }

        return result;
    }

    private void InitializeBuffers()
    {
        var maxVertices = _currentCapacity * 4;
        var maxIndices = _currentCapacity * 6;

        _vertexBuffer = new(_gl, maxVertices, BufferTargetARB.ArrayBuffer, true);
        _indexBuffer = new(_gl, maxIndices, BufferTargetARB.ElementArrayBuffer, false);
        _vertexData = new VertexPositionColorTexture[maxVertices];
        _indexData = GenerateIndexArray(maxIndices);
        _indexBuffer.SetData(_indexData, 0, _indexData.Length);
    }

    private void OnResize(Vector2D<int> obj)
    {
        Viewport = new(0, 0, obj.X, obj.Y);
    }

    private static Vector3 RotatePoint(Vector3 point, Vector2 origin, float cos, float sin)
    {
        var x = point.X - origin.X;
        var y = point.Y - origin.Y;

        return new(
            origin.X + (x * cos - y * sin),
            origin.Y + (x * sin + y * cos),
            point.Z
        );
    }

    private static LyColor ToDarkLyColor(FSColor color)
        => new(color.A, color.R, color.G, color.B);

    private static FSColor ToFsColor(LyColor color)
        => new(color.R, color.G, color.B, color.A);

    private static FSColor[] ToFsColors(ReadOnlySpan<LyColor> colors)
    {
        var result = new FSColor[colors.Length];

        for (var i = 0; i < colors.Length; i++)
        {
            result[i] = ToFsColor(colors[i]);
        }

        return result;
    }
}
