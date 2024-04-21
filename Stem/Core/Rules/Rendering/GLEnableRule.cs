using OpenTK.Graphics.OpenGL4;

namespace Stem.Rules.Rendering;

public class GLEnableRule : RenderRule
{
    protected override Archetype Archetype => Archetype.NoRead;

    private EnableCap EnableCap { get; init; }

    public GLEnableRule(EnableCap enableCap)
    {
        EnableCap = enableCap;
    }

    protected override void OnTick(int[] entities, IEntityStore store, TickTime time)
    {
        GL.Enable(EnableCap);
    }
}
public class GLDisableRule : RenderRule
{
    protected override Archetype Archetype => Archetype.NoRead;

    private EnableCap EnableCap { get; init; }

    public GLDisableRule(EnableCap enableCap)
    {
        EnableCap = enableCap;
    }

    protected override void OnTick(int[] entities, IEntityStore store, TickTime time)
    {
        GL.Disable(EnableCap);
    }
}
