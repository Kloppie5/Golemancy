using Golemancy.Models;
using Mono.Cecil;

namespace Golemancy.Services
{
    public static class MonoStructureMapper
    {
        public record FieldLayout(string Name, string TypeName, int Offset);
        public record ClassLayout(string Name, List<FieldLayout> Fields);

        /// <summary>
        /// Builds an approximate field layout for a class using Mono metadata.
        /// Note: Does not attach to process — just calculates relative offsets.
        /// </summary>
        public static ClassLayout? BuildClassLayout(string assemblyPath, string className)
        {
            var asm = AssemblyDefinition.ReadAssembly(assemblyPath);
            var type = asm.MainModule.Types.FirstOrDefault(t => t.FullName == className);
            if (type == null) return null;

            var fields = new List<FieldLayout>();
            int offset = 0;

            foreach (var f in type.Fields)
            {
                int size = GetFieldSize(f.FieldType);
                fields.Add(new FieldLayout(f.Name, f.FieldType.FullName, offset));
                offset += size;
            }

            return new ClassLayout(type.FullName, fields);
        }

        /// <summary>
        /// Rough size estimation for common value types.
        /// Reference types are treated as pointer-size (4 bytes for 32-bit).
        /// </summary>
        private static int GetFieldSize(TypeReference type)
        {
            return type.FullName switch
            {
                "System.Boolean" => 1,
                "System.Byte" => 1,
                "System.SByte" => 1,
                "System.Char" => 2,
                "System.Int16" => 2,
                "System.UInt16" => 2,
                "System.Int32" => 4,
                "System.UInt32" => 4,
                "System.Single" => 4,
                "System.Int64" => 8,
                "System.UInt64" => 8,
                "System.Double" => 8,
                _ => 4 // default to pointer size for 32-bit refs
            };
        }

        public static ObjectSnapshot ReadObjectFromMemory(
            string processName,
            string assemblyPath,
            string className,
            IntPtr baseAddress)
        {
            var layout = BuildClassLayout(assemblyPath, className);
            if (layout == null)
                throw new InvalidOperationException($"Class {className} not found in {assemblyPath}.");

            var process = System.Diagnostics.Process.GetProcessesByName(processName).FirstOrDefault()
                ?? throw new InvalidOperationException($"Process '{processName}' not found.");

            var fields = new List<Golemancy.Models.FieldValue>();

            foreach (var field in layout.Fields)
            {
                IntPtr addr = baseAddress + field.Offset;
                var bytes = MemoryReader.ReadMemory(process.Handle, addr, GetFieldSizeByTypeName(field.TypeName)) ?? Array.Empty<byte>();
                var value = InterpretBytes(bytes, field.TypeName);
                fields.Add(new Golemancy.Models.FieldValue(field.Name, field.TypeName, value));
            }

            return new Golemancy.Models.ObjectSnapshot(layout.Name, fields);
        }

        private static int GetFieldSizeByTypeName(string typeName)
        {
            return typeName switch
            {
                "System.Boolean" => 1,
                "System.Byte" => 1,
                "System.SByte" => 1,
                "System.Char" => 2,
                "System.Int16" => 2,
                "System.UInt16" => 2,
                "System.Int32" => 4,
                "System.UInt32" => 4,
                "System.Single" => 4,
                "System.Int64" => 8,
                "System.UInt64" => 8,
                "System.Double" => 8,
                _ => 4
            };
        }

        private static object? InterpretBytes(byte[] bytes, string typeName)
        {
            if (bytes.Length == 0) return null;

            return typeName switch
            {
                "System.Boolean" => bytes[0] != 0,
                "System.Byte" => bytes[0],
                "System.SByte" => (sbyte)bytes[0],
                "System.Int16" => BitConverter.ToInt16(bytes, 0),
                "System.UInt16" => BitConverter.ToUInt16(bytes, 0),
                "System.Int32" => BitConverter.ToInt32(bytes, 0),
                "System.UInt32" => BitConverter.ToUInt32(bytes, 0),
                "System.Single" => BitConverter.ToSingle(bytes, 0),
                "System.Int64" => BitConverter.ToInt64(bytes, 0),
                "System.UInt64" => BitConverter.ToUInt64(bytes, 0),
                "System.Double" => BitConverter.ToDouble(bytes, 0),
                _ => $"0x{BitConverter.ToUInt32(bytes, 0):X8}" // pointer display
            };
        }
    }
