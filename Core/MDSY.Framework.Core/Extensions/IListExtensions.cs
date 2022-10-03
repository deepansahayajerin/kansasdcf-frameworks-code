using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace MDSY.Framework.Core
{
    /// <summary>
    /// Implements some of the List&lt;T&gt; extension methods that are not implemented
    /// in IList&lt;T&gt;. 
    /// </summary>
    public static class IListExtensions
    {
        /// <summary>
        /// Inserts the elements of the specified collection to the front of the IList&lt;T&gt;. 
        /// </summary>
        /// <param name="source">The collection whose elements should be inserted to the front of the IList&lt;T&gt;. 
        /// The collection itself cannot be null, but it can contain elements 
        /// that are null, if type T is a reference type.
        /// </param>
        /// <exception cref="System.ArgumentNullException">collection is null.</exception>
        public static void InsertRange<T>(this IList<T> instance, int index, IEnumerable<T> source)
        {
            var arr = source.ToArray();
            for (int i = arr.Length - 1; i >= 0; i--)
            {
                instance.Insert(index, arr[i]);
            }
        }

        /// <summary>
        /// Sets actions for each instance
        /// </summary>
        /// <typeparam name="T">The type of instance for action</typeparam>
        /// <param name="instance">The object instance</param>
        /// <param name="action">The action for the instance</param>
        public static void ForEach<T>(this IEnumerable<T> instance, Action<T> action)
        {
            foreach (T item in instance)
            {
                action(item);
            }
        }

        /// <summary>
        /// Adds the elements of the specified collection to the end of the IList&lt;T&gt;. 
        /// </summary>
        /// <param name="source">The collection whose elements should be added to the end of the IList&lt;T&gt;. 
        /// The collection itself cannot be null, but it can contain elements 
        /// that are null, if type T is a reference type.
        /// </param>
        /// <exception cref="System.ArgumentNullException">collection is null.</exception>
        public static void AddRange<T>(this IList<T> instance, IEnumerable<T> source)
        {
            source.ForEach(instance.Add);
        }
    }
}
