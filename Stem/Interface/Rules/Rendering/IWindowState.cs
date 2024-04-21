namespace Stem.Rules.Rendering;

public interface IWindowState
{
    StemWindow Window { get; }

    void SetWindow(StemWindow window);
}
