using NJsonSchema;
using NJsonSchema.Generation;

namespace FantasyCritic.Web.OpenApi;

/// <summary>
/// Marks non-nullable scalar properties (int, bool, double, string) as required in the schema
/// so that generated clients produce non-nullable C# types for those fields.
/// </summary>
public class RequireNonNullablePropertiesSchemaProcessor : ISchemaProcessor
{
    public void Process(SchemaProcessorContext context)
    {
        var schema = context.Schema;
        if (schema.Properties.Count == 0)
        {
            return;
        }

        foreach (var (name, prop) in schema.Properties)
        {
            var isScalar = prop.Type.HasFlag(JsonObjectType.Integer)
                || prop.Type.HasFlag(JsonObjectType.Boolean)
                || prop.Type.HasFlag(JsonObjectType.Number)
                || prop.Type.HasFlag(JsonObjectType.String);

            if (isScalar && !prop.IsNullableRaw.GetValueOrDefault() && !schema.RequiredProperties.Contains(name))
            {
                schema.RequiredProperties.Add(name);
            }
        }
    }
}
