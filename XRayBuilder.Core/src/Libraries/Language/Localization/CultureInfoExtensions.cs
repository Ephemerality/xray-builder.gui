using System.Globalization;
using System.Threading;

namespace XRayBuilder.Core.Libraries.Language.Localization
{
    public static class CultureInfoExtensions
    {
        public static void SetAsThreadCulture(this CultureInfo cultureInfo)
        {
            Thread.CurrentThread.CurrentUICulture = cultureInfo;
        }
    }
}
