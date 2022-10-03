using System;
using System.Collections.Generic;
using System.Linq;

namespace MDSY.Framework.Buffer.Common
{
    /// <summary>
    /// Base class for objects which will be passed into Initialize() methods 
    /// of objects which implement IInitializable(T).
    /// </summary>
    /// <remarks>
    /// <para>One of the weak points of Dependency Injection (DI) is that there is always a 'coupling point' 
    /// somewhere in the implementation of Interface--Implementater.
    /// See <see cref="MDSY.Framework.Buffer.Interfaces.IInitializeable"/> for more.</para>
    /// <para>
    /// IInitializable(T) has a type constraint of <c>where T: InitializeArgs</c>, 
    /// thus forcing any object which will serve as initialization arguments to descend from InitializeArgs. 
    /// </para>
    /// </remarks>
    public abstract class InitializeArgs
    {

    }


}
