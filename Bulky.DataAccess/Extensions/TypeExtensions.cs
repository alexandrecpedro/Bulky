using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Bulky.DataAccess.Extensions;

public static class TypeExtensions
{
    public static string GetPrimaryKeyName(this Type type)
    {
        var keyAttribute = type.GetProperties()
            //.Select(entity => entity.GetCustomAttribute<KeyAttribute>())
            .FirstOrDefault(entity => entity.GetCustomAttribute<KeyAttribute>() != null);

        return keyAttribute?.Name ?? "Id";
    }
}
