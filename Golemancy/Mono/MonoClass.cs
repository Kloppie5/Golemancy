using System.Runtime.InteropServices;

namespace Golemancy;

[StructLayout(LayoutKind.Explicit)]
public struct MonoClass32 {
    [FieldOffset(0x00)]
    public int /* MonoClass* */ element_class;
    [FieldOffset(0x04)]
    public int /* MonoClass* */ cast_class;
    [FieldOffset(0x08)]
    public int /* MonoClass** */ supertypes;
    [FieldOffset(0x0C)]
    public ushort idepth;
    [FieldOffset(0x0E)]
    public byte rank;
    [FieldOffset(0x0F)]
    public byte child_class;
    [FieldOffset(0x10)]
    public int instance_size;

    [FieldOffset(0x20)]
    public int /* MonoClass32* */ parent;
    [FieldOffset(0x24)]
    public int /* MonoClass32* */ nested_in;
    [FieldOffset(0x28)]
    public int /* MonoImage32* */ image;
    [FieldOffset(0x2C)]
    public int /* char* */ name;

    [FieldOffset(0x30)]
    public uint /* char* */ name_space;

    [FieldOffset(0x34)]
    public uint type_token;
    [FieldOffset(0x38)]
    public int vtable_size;

    [FieldOffset(0x5C)]
    public int sizes;
    [FieldOffset(0x60)]
    public int /* MonoClassField32*  */ fields;
    [FieldOffset(0x64)]
    public int /* MonoMethod32**  */ methods;

    [FieldOffset(0x7C)]
    public int /* MonoClassRuntimeInfo32* */ runtime_info;
}

public partial class ProcessManager
{
    public int MonoClassGetMonoVTable ( int monoDomain, int @class )
    {
        return CallMonoFunctionUnsafe<int>("mono_class_vtable", monoDomain, @class);
    }
}