using MDSY.Framework.Buffer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace MDSY.Framework.Buffer
{
    /// <summary>
    /// Extension methods for IArray(of T)-descendants. 
    /// </summary>
    public static class IArrayExtensions
    {
        /// <summary>
        /// Evaluates each element in the given <paramref name="instance"/> IArray(of T) using the specified <paramref name="criteria"/>.
        /// </summary>
        /// <typeparam name="T">The IBufferElement-implementing type of which the given <paramref name="instance"/> is an array.</typeparam>
        /// <param name="instance">The array object.</param>
        /// <param name="criteria">The Func(of T, bool) which will process each array element.</param>
        /// <returns><c>true</c> if all elements of <paramref name="instance"/> return <c>true</c> for <paramref name="criteria"/>, 
        /// otherwise, <c>false</c>.</returns>
        public static bool EachMeetsCriteria<T>(this IArray<T> instance, Expression<Func<T, bool>> criteria)
            where T : IBufferElement
        {
            bool result = true;

            Func<T, bool> critFunc = criteria.Compile();

            for (int i = 0; i < instance.ArrayElementCount; i++)
            {
                if (!(critFunc.Invoke(instance[i])))
                {
                    result = false;
                    break;
                }
            }

            return result;
        }
    }
}
