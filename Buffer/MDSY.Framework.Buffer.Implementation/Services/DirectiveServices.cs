using System;
using System.Collections.Generic;
using System.Linq;
using MDSY.Framework.Buffer.Unity;
using MDSY.Framework.Buffer.Interfaces;
using Unity;
using MDSY.Framework.Buffer.Common;
//CHADusing Unity.Attributes;

namespace MDSY.Framework.Buffer.Implementation
{
    /// <summary>
    /// Implements the injection interface IDirectiveServices.
    /// </summary>
    [InjectionImplementer(typeof(IDirectiveServices))]
    internal sealed class DirectiveServices : IDirectiveServices
    {
        //Following change made to keep FieldValueMoves thread static - concurrency issue 4921
        [ThreadStatic]
        private static FieldValueMoveType _fieldValueMoves;
        /// <summary>
        /// Specifies which numeric field value move type is in effect.
        /// </summary>
        public FieldValueMoveType FieldValueMoves 
        {
            get { return _fieldValueMoves; }
            set { _fieldValueMoves = value; } 
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectiveServices"/> class.
        /// </summary>
        public DirectiveServices()
        {
            FieldValueMoves = FieldValueMoveType.CobolMoves;
        }
    }

}
