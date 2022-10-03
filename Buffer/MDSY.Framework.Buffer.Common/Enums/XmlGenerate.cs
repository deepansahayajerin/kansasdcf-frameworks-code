using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDSY.Framework.Buffer.Common
{
    public enum XmlGenerate
    {
        CountIn,
        WithAttributes,
        WithEncoding,
        WithXmlDeclaration,
        NamespaceIs,
        NamespacePrefixIs,
        NameOf,
        Suppress,
        TypeOf
    }

    public enum TypeOf
    {
        IsAttribute,
        IsElement
    }

    public enum When
    {
        Zero,
        Zeroes,
        Zeros,
        Space,
        Spaces,
        LowValue,
        LowValues,
        HighValue,
        HighValues
    }

    public enum Every
    {
        None,
        Numeric,
        NumericAttribute,
        NumericContent,
        NumericElement,
        NonNumeric,
        NonNumericAttribute,
        NonNumericContent,
        NonNumericElement,
        Attribute,
        Content,
        Element
    }
}
