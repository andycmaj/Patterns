using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Patterns
{
    /// <summary>
    /// A static class containing helper methods for Reflection.
    /// </summary>
    public static class ReflectionHelper
    {
        /// <summary>
        /// Gets the MethodInfo from a
        /// <see cref="Expression{TDelegate}"/> where the expression
        /// invokes a static method.
        /// </summary>
        /// <example>
        /// <c>ReflectionHelper.GetMethod(() => Object.Equals(null, null));</c>
        /// </example>
        private static MethodInfo GetMethod(Expression<Action> method)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }

            var me = method.Body as MethodCallExpression;
            if (me == null)
            {
                throw new ArgumentException(
                    "The lambda expression must be an instance method invocation"
                );
            }
            return me.Method;
        }

        /// <summary>
        /// Gets the GenericMethodDefinition MethodInfo from a
        /// <see cref="Expression{TDelegate}"/> where the expression
        /// invokes a static method.
        /// </summary>
        /// <example>
        /// <c>
        /// ReflectionHelper.GetGenericMethodDefinition(
        ///     () => Enumerable.Empty&lt;object>()
        /// );
        /// </c>
        /// </example>
        public static MethodInfo GetGenericMethodDefinition(Expression<Action> method)
        {
            return GetMethod(method).GetGenericMethodDefinition();
        }
    }
}
