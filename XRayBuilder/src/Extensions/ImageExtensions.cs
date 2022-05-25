using System.Drawing;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using Image = SixLabors.ImageSharp.Image;

namespace XRayBuilderGUI.Extensions;

public static class ImageExtensions
{
    /// <summary>
    /// https://gist.github.com/vurdalakov/00d9471356da94454b372843067af24e
    /// </summary>
    public static Bitmap ToBitmap<TPixel>(this Image<TPixel> image) where TPixel : unmanaged, IPixel<TPixel> => ToBitmap((Image) image);

    public static Bitmap ToBitmap(this Image image)
    {
        using var memoryStream = new MemoryStream();
        var imageEncoder = image.GetConfiguration().ImageFormatsManager.FindEncoder(PngFormat.Instance);
        image.Save(memoryStream, imageEncoder);

        memoryStream.Seek(0, SeekOrigin.Begin);

        return new Bitmap(memoryStream);
    }

    /// <summary>
    /// https://gist.github.com/vurdalakov/00d9471356da94454b372843067af24e
    /// </summary>
    public static Image ToImageSharpImage(this Bitmap bitmap)
    {
        using var memoryStream = new MemoryStream();
        bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);

        memoryStream.Seek(0, SeekOrigin.Begin);

        return Image.Load(memoryStream);
    }

    /// <summary>
    /// Determines if <paramref name="image1"/> and <paramref name="image2"/> are pixel-perfect equals
    /// </summary>
    public static bool IsPixelMatch(this Bitmap image1, Bitmap image2)
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