using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace XRayBuilder.Core.Libraries.Images.Extensions
{
    public static class BitmapExtensions
    {
        public static Image ToGrayscale3(this Image original)
        {
            //create a blank bitmap the same size as original
            var newBitmap = new Bitmap(original.Width, original.Height);

            //get a graphics object from the new image
            var g = Graphics.FromImage(newBitmap);

            //create the grayscale ColorMatrix
            var colorMatrix = new ColorMatrix(
                new[]
                {
                    new [] {.3f, .3f, .3f, 0, 0},
                    new [] {.59f, .59f, .59f, 0, 0},
                    new [] {.11f, .11f, .11f, 0, 0},
                    new [] {0f, 0f, 0f, 1f, 0f},
                    new [] {0f, 0f, 0f, 0f, 1f}
                });

            //create some image attributes
            var attributes = new ImageAttributes();

            //set the color matrix attribute
            attributes.SetColorMatrix(colorMatrix);

            //draw the original image on the new image
            //using the grayscale color matrix
            g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
                0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);

            //dispose the Graphics object
            g.Dispose();

            return newBitmap;
        }

        public static string ToBase64(this Image image, ImageFormat format)
        {
            using var ms = new MemoryStream();
            image.Save(ms, format);
            return Convert.ToBase64String(ms.ToArray());
        }

        /// <summary>
        ///     Resize the image to the specified width and height.
        ///     https://stackoverflow.com/a/10323949
        ///     InterpolationMode HighQualityBicubic introducing artifacts on edge of resized images
        ///     https://stackoverflow.com/a/8312531
        /// </summary>
        /// <param name="original">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <param name="fill">Fill the resized image keeping aspect ratio.</param>
        /// <returns>The resized image.</returns>
        public static Image Scale(this Image original, int width, int height, bool fill)
        {
            #region Calculations

            var sourceWidth = original.Width;
            var sourceHeight = original.Height;
            const int sourceX = 0;
            const int sourceY = 0;
            double destX = 0;
            double destY = 0;

            double nScale;

            var nScaleW = width / (double)sourceWidth;
            var nScaleH = height / (double) sourceHeight;
            if (!fill)
            {
                nScale = Math.Min(nScaleH, nScaleW);
            }
            else
            {
                nScale = Math.Max(nScaleH, nScaleW);
                destY = (height - sourceHeight * nScale) / 2;
                destX = (width - sourceWidth * nScale) / 2;
            }

            var destWidth = (int) Math.Round(sourceWidth * nScale);
            var destHeight = (int) Math.Round(sourceHeight * nScale);

            #endregion

            Bitmap bmPhoto;
            try
            {
                bmPhoto = new Bitmap(destWidth + (int) Math.Round(2 * destX), destHeight + (int) Math.Round(2 * destY));
            }
            catch (Exception)
            {
                return new Bitmap(original);
            }

            using var grPhoto = Graphics.FromImage(bmPhoto);
            grPhoto.CompositingMode = CompositingMode.SourceCopy;
            grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;
            grPhoto.PixelOffsetMode = PixelOffsetMode.HighQuality;
            grPhoto.CompositingQuality = CompositingQuality.HighQuality;
            grPhoto.SmoothingMode = SmoothingMode.HighQuality;

            var to = new Rectangle((int) Math.Round(destX), (int) Math.Round(destY), destWidth, destHeight);
            var from = new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight);

            using var wrapMode = new ImageAttributes();
            wrapMode.SetWrapMode(WrapMode.TileFlipXY);
            grPhoto.DrawImage(original, to, 0, 0, from.Width, from.Height, GraphicsUnit.Pixel, wrapMode);

            return bmPhoto;
        }
    }
}