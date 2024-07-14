
namespace Golemancy;

class AssemblyBuilder
{
    public static AssemblyBuilder Start()
    {
        return new AssemblyBuilder();
    }

    public List<List<byte>> code = [];
    public byte[] Finalize ( ) {
        byte[] result = code.SelectMany(x => x).ToArray();
        code.Clear();
        return result;
    }

    public enum Register : byte {
        EAX = 0,
        ECX = 1,
        EDX = 2,
        EBX = 3,
        ESP = 4,
        EBP = 5,
        ESI = 6,
        EDI = 7
    }

    public AssemblyBuilder MOVi64r ( Register r, long imm ) {
        List<byte> line =
        [
            0x48, // REX.W
            (byte)(0xB8 + (byte)r), // o270 + r
            .. BitConverter.GetBytes(imm),
        ];
        code.Add(line);
        return this;
    }
    public AssemblyBuilder MOVr0m64 (long address ) {
        List<byte> line =
        [
            0x48, // REX.W
            0xA3, // o243 + r0
            .. BitConverter.GetBytes(address),
        ];
        code.Add(line);
        return this;
    }

    public AssemblyBuilder MOVi32r ( Register r, int imm ) {
        List<byte> line =
        [
            (byte)(0xB8 + (byte)r), // o270 + r
            .. BitConverter.GetBytes(imm),
        ];
        code.Add(line);
        return this;
    }
    public AssemblyBuilder MOVr0m32 (int address ) {
        List<byte> line =
        [
            0xA3, // o243 + r0
            .. BitConverter.GetBytes(address),
        ];
        code.Add(line);
        return this;
    }

    public AssemblyBuilder ADDi8r ( byte r, sbyte imm ) {
        List<byte> line =
        [
            0x83, // o203
            (byte)(0xC0 + r), // o300 + r
            (byte)imm,
        ];
        code.Add(line);
        return this;
    }

    public AssemblyBuilder PUSHi32 (int imm ) {
        List<byte> line =
        [
            0x68, // o150
            .. BitConverter.GetBytes(imm),
        ];
        code.Add(line);
        return this;
    }

    public AssemblyBuilder RET ( ) {
        List<byte> line =
        [
            0xC3, // o303
        ];
        code.Add(line);
        return this;
    }

    public AssemblyBuilder CALLr ( Register r ) {
        List<byte> line =
        [
            0xFF, // o377
            (byte)(0xD0 + (byte)r), // o320 + r
        ];
        code.Add(line);
        return this;
    }
}