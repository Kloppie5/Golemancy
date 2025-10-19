using Golemancy.Models;
using Mono.Cecil;

namespace Golemancy.Services
{
    public static class MonoInspector
    {
        public static IEnumerable<string> GetClasses(string assemblyPath)
        {
            var asm = AssemblyDefinition.ReadAssembly(assemblyPath);
            return asm.MainModule.Types
                .Where(t => t.IsClass && !t.FullName.StartsWith("<"))
                .Select(t => t.FullName);
        }

        public static MonoClassInfo? GetClassInfo(string assemblyPath, string className)
        {
            var asm = AssemblyDefinition.ReadAssembly(assemblyPath);
            var type = asm.MainModule.Types.FirstOrDefault(t => t.FullName == className);
            if (type == null) return null;

            var fields = type.Fields.Select(f => new MonoFieldInfo(
                f.Name,
                f.FieldType.FullName,
                f.MetadataToken
            )).ToList();

            var methods = type.Methods.Select(m => new MonoMethodInfo(
                m.Name,
                $"{m.ReturnType.Name} {m.Name}({string.Join(", ", m.Parameters.Select(p => p.ParameterType.Name))})"
            )).ToList();

            return new MonoClassInfo(type.FullName, fields, methods);
        }

        public static IEnumerable<MonoClassInfo> DumpAssembly(string assemblyPath)
        {
            var asm = AssemblyDefinition.ReadAssembly(assemblyPath);
            return asm.MainModule.Types
                .Where(t => t.IsClass && !t.FullName.StartsWith("<"))
                .Select(t => new MonoClassInfo(
                    t.FullName,
                    t.Fields.Select(f => new MonoFieldInfo(
                        f.Name,
                        f.FieldType.FullName,
                f.MetadataToken
                    )).ToList(),
                    t.Methods.Select(m => new MonoMethodInfo(
                        m.Name,
                        $"{m.ReturnType.Name} {m.Name}({string.Join(", ", m.Parameters.Select(p => p.ParameterType.Name))})"
                    )).ToList()
                ));
        }
    }
}