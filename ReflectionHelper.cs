using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TONWallet
{
    public class ReflectionHelper
    {
        public static object CreateInstance(Type type, object[]? constructorArgs)
        {
            try
            {
                // Get the constructor of the type with the specified parameter types
                ConstructorInfo constructor = type.GetConstructor(GetParameterTypes(constructorArgs));

                // Create an instance using the constructor and the provided arguments
                object instance = constructor.Invoke(constructorArgs);

                return instance;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating instance: {ex.Message}");
                return null;
            }
        }

        public static Type[] GetParameterTypes(object[]? args)
        {
            if (args is null) return new Type[0];
            Type[] parameterTypes = new Type[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                parameterTypes[i] = args[i].GetType();
            }
            return parameterTypes;
        }

        public static async Task InvokeRunMethod(object instance)
        {
            // Get the Run() method of the instance's type
            MethodInfo runMethod = instance.GetType().GetMethod("Run");

            // Invoke the Run() method on the instance
            var task = (Task)runMethod.Invoke(instance, null);
            await task;
        }
    }
}
