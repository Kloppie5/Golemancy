using System.Runtime.InteropServices;

namespace Golemancy;

[StructLayout(LayoutKind.Explicit)]
public struct MonoInternalHashTable
{
    [FieldOffset(0x00)]
    public uint /* GHashFunc */ hash_func;
    [FieldOffset(0x04)]
    public uint /* MonoInternalHashKeyExtractFunc */ key_extract;
    [FieldOffset(0x08)]
    public uint /* MonoInternalHashNextValueFunc */ next_value;
    [FieldOffset(0x0C)]
    public int size;
    [FieldOffset(0x10)]
    public int num_entries;
    [FieldOffset(0x14)]
    public uint /* gpointer* */ table;
}