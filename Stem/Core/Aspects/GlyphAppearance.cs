namespace Stem.Aspects;

public struct GlyphAppearance
{
    public int glyphIndex = '?';

    public float depth = 0.5f, scale = 16.0f;

    public Position2D offset = (0, 0);

    public GlyphAppearance() {}
}

public struct UIGlyphAppearance 
{
    public int glyphIndex = '?';

    public float depth = 0.5f, scale = 16.0f;

    public Position2D offset = (0, 0);

    public UIGlyphAppearance() {}
}
