using System;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace Manisero.StreamProcessingModel.Utils
{
    public static class MethodInfoExtensions
    {
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
