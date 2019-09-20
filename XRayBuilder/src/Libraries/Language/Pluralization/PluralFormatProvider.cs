using System;
using XRayBuilderGUI.Libraries.Primitives.Extensions;

namespace XRayBuilderGUI.Libraries.Language.Pluralization
{
    public sealed class PluralFormatProvider : IFormatProvider, ICustomFormatter
    {
        public object GetFormat(Type formatType) => this;

        public string Format(string format, object arg, IFormatProvider formatProvider)
            => arg + " " + format.Plural((int) arg);
    }
}