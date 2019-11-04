using System;

namespace XRayBuilder.Core.Libraries.Language.Pluralization
{
    public static class PluralUtil
    {
        public static string Pluralize(FormattableString formattable) => formattable.ToString(new PluralFormatProvider());
    }
}