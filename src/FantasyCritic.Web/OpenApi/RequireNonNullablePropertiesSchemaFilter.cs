using NJsonSchema.Generation;

namespace FantasyCritic.Web.OpenApi;

/// <summary>
/// Marks non-nullable properties as required in the schema so that generated clients
/// produce non-optional types for those fields.
/// </summary>
public class RequireNonNullablePropertiesSchemaProcessor : ISchemaProcessor
{
    public void Process(SchemaProcessorContext context)
    {
        var schema = context.Schema;
        foreach (var (name, prop) in schema.Properties)
        {
            if (!prop.IsNullableRaw.GetValueOrDefault() && !schema.RequiredProperties.Contains(name))
            {
                schema.RequiredProperties.Add(name);
            }
        }
    }
}
