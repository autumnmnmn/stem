using System.Collections.Specialized;

namespace Stem.Internal;

internal struct EntityMetadata
{
    public int id;
    public BitVector32 aspectMask; // IDEA: Create a bigger bitvector type.
}
