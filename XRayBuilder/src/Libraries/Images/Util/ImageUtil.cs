using System;
using System.Drawing;
using System.IO;

namespace XRayBuilderGUI.Libraries.Images.Util
{
    public static class ImageUtil
    {
        public static Image Base64ToImage(string base64String)
        {
            var imageBytes = Convert.FromBase64String(base64String);
            using var ms = new MemoryStream(imageBytes, 0, imageBytes.Length);
            return new Bitmap(ms);
        }
    }
}