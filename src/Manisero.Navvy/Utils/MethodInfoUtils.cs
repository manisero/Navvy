using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace Manisero.Navvy.Utils
{
    internal static class MethodInfoUtils
    {
        [System.Diagnostics.DebuggerStepThrough]
        public static async Task InvokeAsGenericAsync(
            this MethodInfo method,
            object target,
            Type[] typeArguments,
            params object[] arguments)
            => await method.InvokeAsGeneric<Task>(target, typeArguments, arguments);

        [System.Diagnostics.DebuggerStepThrough]
        public static TResult InvokeAsGeneric<TResult>(
            this MethodInfo method,
            object target,
            Type[] typeArguments,
            params object[] arguments)
        {
            try
            {
                return (TResult)method.MakeGenericMethod(typeArguments)
                                      .Invoke(target, arguments);
            }
            catch (TargetInvocationException exception)
            {
                ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
                throw;
            }
        }
    }
}
