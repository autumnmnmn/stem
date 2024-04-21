using OpenTK.Graphics.OpenGL4;

namespace Stem.Rules.Rendering;

public class ClearColorRule : RenderRule
{
    protected override Archetype Archetype => Archetype.NoRead;

    private readonly (float r, float g, float b) __clearColor;

    public ClearColorRule(float r = 0f, float g = 0f, float b = 0f)
    {
        __clearColor = new(r, g, b);
    }

    protected override void OnTick(int[] entities, IEntityStore store, TickTime time)
    {
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
    }

    protected internal override void Setup(IEntityStore store)
    {
        GL.ClearColor(__clearColor.r, __clearColor.g, __clearColor.b, 1f);
        base.Setup(store);
    }
}
