// SPDX-License-Identifier: MIT
#if NET462 || NETSTANDARD2_0

namespace System.Diagnostics.CodeAnalysis;

[AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
[ExcludeFromCodeCoverage]
internal sealed class MaybeNullWhenAttribute : Attribute
{
    public bool ReturnValue { get; }

    public MaybeNullWhenAttribute(bool returnValue)
    {
        ReturnValue = returnValue;
    }
}

#endif
