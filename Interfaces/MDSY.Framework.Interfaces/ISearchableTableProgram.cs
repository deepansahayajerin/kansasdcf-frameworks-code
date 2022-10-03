using System;
using System.Collections.Generic;
using System.Linq;

namespace MDSY.Framework.Interfaces
{
    /// <summary>Defines a legacy program object which manages a searchable table of data.
    /// </summary>
    public interface ISearchableTableProgram : ILegacyProgram
    {
        /// <summary>
        /// Adds the specified <paramref name="value"/> to the program's data 
        /// table using the given <paramref name="key"/>.
        /// </summary>
        void Add(string key, string value);
        /// <summary>
        /// Searches the program's data table for the value with the given <paramref name="key"/> 
        /// and returns the value, if found.</summary>
        string Search(string key);

        /// <summary>
        /// Causes the program's data table to be output for display.
        /// </summary>
        void Display();
        //        string Display();

        /// <summary>
        /// Adds all the elements of the given dictionary to the program's data table.
        /// </summary>
        /// <param name="keyedValues">An IDictionary&lt;&gt; implementation containing 
        /// keys and values to be added.</param>
        void Load(IDictionary<string, string> keyedValues);
    }
}

