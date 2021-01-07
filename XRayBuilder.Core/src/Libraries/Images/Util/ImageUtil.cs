using System;
using System.Drawing;
using System.IO;

namespace XRayBuilder.Core.Libraries.Images.Util
{
    public static class ImageUtil
    {
        public static Image Base64ToImage(string base64String)
        {
            var imageBytes = Convert.FromBase64String(base64String);
            using var ms = new MemoryStream(imageBytes, 0, imageBytes.Length);
            return new Bitmap(ms);
        }

        /// <summary>
        /// Determines if <paramref name="image1"/> and <paramref name="image2"/> are pixel-perfect equals
        /// </summary>
        public static bool AreEqual(Bitmap image1, Bitmap image2)
        {
            if (image1 == null || image2 == null)
                return false;

            if (image1.Width != image2.Width || image1.Height != image2.Height)
                return false;

            for (var i = 0; i < image1.Width; i++)
            {
                for (var j = 0; j < image1.Height; j++)
                {
                    if (image1.GetPixel(i, j) != image2.GetPixel(i, j))
                        return false;
                }
            }

            return true;
        }
    }
}