using FreeSql.DataAnnotations;
using System.Reflection;
using System.Security.Principal;

namespace Gubon.Gateway.Store.FreeSql
{
    public class SyncStructure
    {

        public static Type[] GetTypesByTableAttribute()
        {
            List<Type> tableAssembies = new List<Type>();
            foreach (Type type in Assembly.GetAssembly(typeof(SyncStructure)).GetExportedTypes())
                foreach (Attribute attribute in type.GetCustomAttributes())
                    if (attribute is TableAttribute tableAttribute)
                        if (tableAttribute.DisableSyncStructure == false)
                            tableAssembies.Add(type);

            return tableAssembies.ToArray();
        }

        public static void Sync(IFreeSql fsql)
        {
            fsql.CodeFirst.SyncStructure(GetTypesByTableAttribute());
        }

    }
}