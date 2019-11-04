using System;
using XRayBuilder.Core.Libraries.Primitives.Extensions;

namespace XRayBuilder.Core.Libraries.Language.Pluralization
{
    public sealed class PluralFormatProvider : IFormatProvider, ICustomFormatter
    {
        public object GetFormat(Type formatType) => this;

        public string Format(string format, object arg, IFormatProvider formatProvider)
            => arg + " " + format.Plural((int) arg);
    }
}