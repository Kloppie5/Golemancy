using System.Runtime.InteropServices;

namespace Golemancy;

[StructLayout(LayoutKind.Explicit)]
public struct MonoImage32
{
    [FieldOffset(0x00)]
    public int /* int */ ref_count;
    [FieldOffset(0x14)]
    public int /* char* */ name;
    [FieldOffset(0x18)]
    public int /* char* */ filename;
    [FieldOffset(0x1C)]
    public int /* char* */ assembly_name;
    [FieldOffset(0x20)]
    public int /* char* */ module_name;
    [FieldOffset(0x24)]
    public int time_date_stamp;
    [FieldOffset(0x28)]
    public int /* char* */ version;
    [FieldOffset(0x2C)]
    public short md_version_major;
    [FieldOffset(0x2E)]
    public short md_version_minor;
    [FieldOffset(0x30)]
    public int /* char* */ guid;

    [FieldOffset(0x35C)]
    public MonoInternalHashTable class_cache;
}

public partial class ProcessManager
{
    public int MonoImageGetMonoClassByName ( int image, string @namespace, string className )
    {
        return CallMonoFunctionUnsafe<int>("mono_class_from_name", image, @namespace, className);
    }
}