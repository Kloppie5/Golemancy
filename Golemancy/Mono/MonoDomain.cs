using System.Runtime.InteropServices;

namespace Golemancy;

[StructLayout(LayoutKind.Explicit)]
public struct MonoDomain
{
    [FieldOffset(0x00)]
    public int /* ? */ unknown_0; // -1
    [FieldOffset(0x04)]
    public int /* ? */ unknown_4; // -1
    [FieldOffset(0x08)]
    public int /* ? */ unknown_8; // 0
    [FieldOffset(0x0C)]
    public int /* ? */ unknown_C; // 0
    [FieldOffset(0x10)]
    public int /* ? */ unknown_10; // -1

    [FieldOffset(0x1C)]
    public int /* ?* */ unknown_1C;
    [FieldOffset(0x20)]
    public int /* ?* */ unknown_20;
    [FieldOffset(0x24)]
    public int /* ?* */ unknown_24;
    [FieldOffset(0x28)]
    public int /* ?* */ unknown_28;
    [FieldOffset(0x2C)]
    public int /* ?* */ unknown_2C;
    [FieldOffset(0x30)]
    public int /* ?* */ unknown_30;

    [FieldOffset(0x58)]
    public int /* GSList32* */ domain_assemblies;
    [FieldOffset(0x60)]
    public int /* char* */ friendly_name;
}

public partial class ProcessManager
{
    public int MonoDomainGetMonoAssemblyByName(int domain, string name )
    {
        MonoDomain domainStruct = ReadUnsafe<MonoDomain>(domain);
        int it = domainStruct.domain_assemblies;
        while ( it != 0 ) {
            GSList32 list = ReadUnsafe<GSList32>(it);
            int assembly = list.data;
            string assemblyName = MonoAssemblyGetName(assembly);
            if ( assemblyName == name )
                return assembly;
            it = list.next;
        }
        throw new ArgumentException($"Could not find MonoAssembly '{name}'");
    }
}