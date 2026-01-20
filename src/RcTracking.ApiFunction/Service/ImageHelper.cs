using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace RcTracking.ApiFunction.Service
{
    public static class ImageHelper
    {
        public static Image ResizeImage(Image image)
        {
            var thumbnail = image.GetThumbnailImage(300, 300, () => false, IntPtr.Zero);
            return thumbnail;
        }

        public static string ToBase64String(Image image)
        {
            using var ms = new MemoryStream();
            image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            var imageBytes = ms.ToArray();
            return Convert.ToBase64String(imageBytes);
        }
    }
}
