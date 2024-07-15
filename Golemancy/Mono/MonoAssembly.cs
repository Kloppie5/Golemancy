using System.Runtime.InteropServices;

namespace Golemancy;

[StructLayout(LayoutKind.Explicit)]
public struct MonoAssembly
{
    [FieldOffset(0x00)]
    public int /* gint32 */ ref_count;
    [FieldOffset(0x04)]
    public int /* char* */ basedir;
    [FieldOffset(0x08)]
    public int /* char* */ monoAssemblyNameDOTname;
    [FieldOffset(0x48)]
    public int /* MonoImage* */ image;
}

public partial class ProcessManager
{
    public string MonoAssemblyGetName (int assembly )
    {
        MonoAssembly assemblyStruct = ReadUnsafe<MonoAssembly>(assembly);
        return ReadUnsafeUTF8String(assemblyStruct.monoAssemblyNameDOTname);
    }
    public int MonoAssemblyGetMonoImage (int assembly )
    {
        MonoAssembly assemblyStruct = ReadUnsafe<MonoAssembly>(assembly);
        return assemblyStruct.image;
    }
}