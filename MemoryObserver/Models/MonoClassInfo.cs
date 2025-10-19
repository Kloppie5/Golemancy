using Mono.Cecil;

namespace Golemancy.Models
{
    public record MonoFieldInfo(string Name, string TypeName, MetadataToken MetadataToken);
    public record MonoMethodInfo(string Name, string Signature);
    public record MonoClassInfo(
        string Name,
        List<MonoFieldInfo> Fields,
        List<MonoMethodInfo> Methods
    );
}
