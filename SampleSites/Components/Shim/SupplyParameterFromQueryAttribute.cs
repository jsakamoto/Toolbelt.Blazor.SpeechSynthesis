#if NET5_0
using System;

namespace Microsoft.AspNetCore.Components
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    sealed class SupplyParameterFromQueryAttribute : Attribute
    {
    }
}

#endif