using System;
using System.Reflection;

namespace RandomStartMod.Compat
{
    // Keep reflection and failure handling independent of RimWorld so the boundary
    // can be tested without loading the game or any optional mod assemblies.
    internal sealed class OptionalModApi
    {
        private readonly string name;
        private readonly Action<string> warn;
        private bool disabled;

        internal OptionalModApi(string name, Action<string> warn)
        {
            this.name = name;
            this.warn = warn;
        }

        internal bool TryRun(Action operation)
        {
            if (disabled)
                return false;

            try
            {
                operation();
                return true;
            }
            catch (Exception error)
            {
                disabled = true;
                warn($"[Random Start] {name} integration failed and is disabled until restart: "
                    + error.GetBaseException().Message
                    + " Check that the installed mod version is compatible. Other integrations remain enabled.");
                return false;
            }
        }

        internal static Type RequireType(string fullName)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(fullName, throwOnError: false);
                if (type != null)
                    return type;
            }
            throw new TypeLoadException($"Optional mod type '{fullName}' was not found.");
        }

        internal static FieldInfo RequireField(Type type, string name, bool isStatic, Type fieldType = null)
        {
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic
                | (isStatic ? BindingFlags.Static : BindingFlags.Instance);
            FieldInfo field = type.GetField(name, flags);
            if (field == null || (fieldType != null && !fieldType.IsAssignableFrom(field.FieldType)))
                throw new MissingFieldException(type.FullName, name);
            return field;
        }

        internal static MethodInfo RequireMethod(Type type, string name, Type returnType, params Type[] parameters)
        {
            MethodInfo method = type.GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic
                | BindingFlags.Instance, null, parameters, null);
            if (method == null || method.ReturnType != returnType)
                throw new MissingMethodException(type.FullName, name);
            return method;
        }
    }
}
