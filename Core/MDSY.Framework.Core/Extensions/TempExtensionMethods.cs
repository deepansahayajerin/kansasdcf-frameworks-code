using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MDSY.Framework.Buffer.Interfaces;

namespace MDSY.Framework.Core
{
    /// <summary>
    /// Extension methods put in place to help conversion from buffer 2.0 to buffer 3.0. These should ultimately be refactored out. 
    /// </summary>
    public static class TempExtensionMethods
    {
        /// <summary>
        /// Stands in for Record.DBSBufferReplace method;
        /// </summary>
        /// <param name="instance">The IRecord instance for buffer replacement</param>
        /// <param name="oldValue">The old buffer value</param>
        /// <param name="newValue">The new buffer value</param>
        /// <param name="startIndex">The buffer starting position</param>
        /// <param name="count">The number of bytes to be replaced</param>
        public static void DBSBufferReplace(this IRecord instance, string oldValue, string newValue, int startIndex, int count)
        {
            if (String.IsNullOrEmpty(oldValue))
                throw new ArgumentNullException("oldValue is null or empty.", "oldValue");
            if (String.IsNullOrEmpty(newValue))
                throw new ArgumentNullException("newValue is null or empty.", "newValue");
            if (oldValue.Length != newValue.Length)
                throw new ArgumentException("oldValue and newValue must be the same length.");

            var oldBytes = ByteTransformer.ToBytes(oldValue);
            var newBytes = ByteTransformer.ToBytes(newValue);

            instance.DBSBufferReplace(oldBytes, newBytes, startIndex, count);
        }

        /// <summary>
        /// Stands in for Record.DBSBufferReplace method;
        /// </summary>
        /// <param name="instance">The IRecord instance for buffer replacement</param>
        /// <param name="newValue">The new buffer value</param>
        /// <param name="startIndex">The buffer starting position</param>
        /// <param name="count">The number of bytes to be replaced</param>
        public static void DBSBufferReplace(this IRecord instance, string newValue, int startIndex, int count)
        {
            var oldBytes = instance.Buffer.ReadBytes();
            var newBytes = ByteTransformer.ToBytes(newValue);
            instance.DBSBufferReplace(oldBytes, newBytes, startIndex, count);
        }

        /// <summary>
        /// Replaces, within a subrange of this instance, all occurrences of a specified byte pattern
        /// with another specified value.
        /// </summary>
        /// <param name="instance">The IRecord instance for buffer replacement</param>
        /// <param name="oldValue">The old buffer value</param>
        /// <param name="newValue">The new buffer value</param>
        /// <param name="startIndex">The buffer starting position</param>
        /// <param name="count">The number of bytes to be replaced</param>
        public static void DBSBufferReplace(this IRecord instance, byte[] oldValue, byte[] newValue, int startIndex, int count)
        {
            if (oldValue == null || oldValue.Length == 0)
                throw new ArgumentException("oldValue is null or empty.", "oldValue");
            if (newValue == null || newValue.Length == 0)
                throw new ArgumentException("newValue is null or empty.", "newValue");

            if (oldValue.Length != newValue.Length)
                throw new ArgumentException("oldValue and newValue must be the same length.");

            if (count < 0)
                throw new ArgumentOutOfRangeException("count", "count cannot be less than zero.");
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException("startIndex", "startIndex cannot be less than zero.");

            var indexes = instance.Buffer.FindAllIndexes(oldValue, startIndex, count);

            for (int i = 0; i < indexes.Length; i++)
            {
                int idx = indexes[i] + startIndex;
                instance.Buffer.WriteBytes(newValue, idx, newValue.Length);
            }
        }

        /// <summary>
        /// Returns all positions where the byte pattern has been found
        /// </summary>
        /// <param name="instance">The IDataBuffer instance to be searched</param>
        /// <param name="pattern">The byte pattern to search for</param>
        /// <param name="index">The buffer starting position</param>
        /// <param name="count">The length of the bytes to return</param>
        /// <returns></returns>
        public static Int32[] FindAllIndexes(this IDataBuffer instance, byte[] pattern, int index, int count)
        {
            byte[] subset = new byte[count];
            System.Buffer.BlockCopy(instance.ReadBytes(index, count), index, subset, 0, count);
            return AllIndexesQuery(subset, pattern).ToArray();
        }

        /// <summary>
        /// Returns all positions where the byte pattern has been found
        /// </summary>
        /// <param name="instance">The instance to be searched</param>
        /// <param name="pattern">The byte pattern to search for</param>
        /// <returns></returns>
        public static IEnumerable<Int32> AllIndexesQuery(this byte[] instance, byte[] pattern)
        {
            return Enumerable
                        .Range(0, instance.Length - pattern.Length + 1)
                        .Where(i => pattern
                                .Select((byt, idx) => new { idx, byt })
                                .All(ptn => instance[i + ptn.idx] == ptn.byt));
        }

        /// <summary>
        /// Returns the field by name
        /// </summary>
        /// <param name="instance">The Irecord instance</param>
        /// <param name="name">The name of the field</param>
        /// <returns></returns>
        public static IField GetFieldByName(this IRecord instance, string name)
        {
            return instance.GetElementByNameNested(name) as IField;
        }


    }

    public static class TupleListExtensions
    {
        public static void Add<T1, T2>(this IList<Tuple<T1, T2>> list,
                T1 item1, T2 item2)
        {
            list.Add(Tuple.Create(item1, item2));
        }

        public static void Add<T1, T2, T3>(this IList<Tuple<T1, T2, T3>> list,
                T1 item1, T2 item2, T3 item3)
        {
            list.Add(Tuple.Create(item1, item2, item3));
        }

    }
}

