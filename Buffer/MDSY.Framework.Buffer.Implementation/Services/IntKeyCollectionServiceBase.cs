using System;
using System.Collections.Generic;
using System.Linq;
using MDSY.Framework.Buffer.Unity;
using MDSY.Framework.Buffer.Interfaces;
using Unity;
using MDSY.Framework.Buffer.Common;

namespace MDSY.Framework.Buffer.Implementation
{
    /// <summary>
    /// Defines and implements the collection of integer keys for framework services.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    internal abstract class IntKeyCollectionServiceBase<TValue> :
        CollectionServiceBase<Int32, TValue>
        //ICollectionServiceBase<Int32, TValue>
            where TValue : class
    {
        #region protected methods
        protected override int GetNewKeyValue(TValue item)
        {
            // don't reuse key values; this will keep incrementing new keys.
            var keys = Items.Keys.Select(i => i);
            int result = keys.Count() > 0 ? keys.Max() : -1;
            return ++result;
        }
        #endregion

    }
}
