using System.Runtime.InteropServices;

namespace Golemancy;

[StructLayout(LayoutKind.Explicit)]
public struct MonoVTable32 {
    [FieldOffset(0x00)]
    public int /* MonoClass* */ klass;
    [FieldOffset(0x04)]
    public int /* void* */ gc_descr;
    [FieldOffset(0x08)]
    public int /* MonoDomain* */ domain;
}

public partial class ProcessManager
{
    public int VTableGetStaticFieldData ( int vtable )
    {
        return CallMonoFunctionUnsafe<int>("mono_vtable_get_static_field_data", vtable);
    }
}