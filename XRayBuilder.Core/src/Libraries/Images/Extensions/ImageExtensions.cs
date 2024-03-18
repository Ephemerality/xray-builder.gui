using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;

namespace XRayBuilder.Core.Libraries.Images.Extensions
{
    public static class ImageExtensions
    {
        /// <summary>
        /// https://gist.github.com/vurdalakov/00d9471356da94454b372843067af24e
        /// </summary>
        public static byte[] ToArray<TPixel>(this Image<TPixel> image, IImageFormat imageFormat) where TPixel : unmanaged, IPixel<TPixel>
        {
            using var memoryStream = new MemoryStream();
            var imageEncoder = image.Configuration.ImageFormatsManager.GetEncoder(imageFormat);
            image.Save(memoryStream, imageEncoder);
            return memoryStream.ToArray();
        }

        public static Image Base64ToImage(this string base64String) => Image.Load(Convert.FromBase64String(base64String));
    }
}