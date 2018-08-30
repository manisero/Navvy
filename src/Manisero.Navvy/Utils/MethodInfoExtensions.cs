using System;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace Manisero.Navvy.Utils
{
    internal static class MethodInfoExtensions
    {
        [System.Diagnostics.DebuggerStepThrough]
        public static void InvokeAsGeneric(
            this MethodInfo method,
            object target,
            Type[] typeArguments,
            params object[] arguments)
            => method.InvokeAsGeneric<object>(target, typeArguments, arguments);

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
