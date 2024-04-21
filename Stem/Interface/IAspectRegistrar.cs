using Stem.Internal;

namespace Stem;

public interface IAspectRegistrar
{
    public void Register<T>(AspectType aspectType = AspectType.Sparse) where T : struct;
}
