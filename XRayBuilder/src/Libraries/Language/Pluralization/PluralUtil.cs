using System;

namespace XRayBuilderGUI.Libraries.Language.Pluralization
{
    public static class PluralUtil
    {
        [Obsolete("This might be obsolete now, see Pluralize.NET")]
        public static string Pluralize(FormattableString formattable) => formattable.ToString(new PluralFormatProvider());
    }
}