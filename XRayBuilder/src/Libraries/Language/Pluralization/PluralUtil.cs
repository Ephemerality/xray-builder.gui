using System;

namespace XRayBuilderGUI.Libraries.Language.Pluralization
{
    public static class PluralUtil
    {
        public static string Pluralize(FormattableString formattable) => formattable.ToString(new PluralFormatProvider());
    }
}