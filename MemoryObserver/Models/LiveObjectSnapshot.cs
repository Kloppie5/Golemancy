namespace Golemancy.Models
{
    public record FieldValue(string Name, string TypeName, object? Value);
    public record ObjectSnapshot(string ClassName, List<FieldValue> Fields);
}
