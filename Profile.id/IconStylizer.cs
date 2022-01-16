using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;


namespace Profile.id
{
    public static class IconStylizer
    {
        private const int ThicknessDivisor = 5;

        private static Icon ConvertToIcon(Bitmap bitmap)
        {
            bitmap.MakeTransparent(Color.Snow);
            var icH = bitmap.GetHicon();
            var icon = Icon.FromHandle(icH);
            return icon;
        }

        public static Icon ChangeColor(Icon icon, Color color)
        {
            var bitmap = icon.ToBitmap();
            var thickness = Math.Min(bitmap.Height, bitmap.Width) / ThicknessDivisor;
            for (var x = 0; x < bitmap.Width; x++)
            {
                for (var y = bitmap.Height - 1 - thickness; y < bitmap.Height; y++)
                {
                    bitmap.SetPixel(x, y, color);
                }
            }

            var newIcon = ConvertToIcon(bitmap);
            return newIcon;
        }
    }
}
