using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FittingStraightLine
{
    unsafe public struct ImageFrame
    {
        public static readonly byte BLACK = 0x00;
        public static readonly byte WHITE = 0xff;

        public BitmapData data;
        public int height;
        public int width;

        public ImageFrame(BitmapData data)
        {
            this.data = data;
            height = data.Height;
            width = data.Width;
        }

        public byte GetColor(int row, int col)
        {
            var color = (byte*)data.Scan0 + row * data.Stride + col * 3;
            return color[0];
        }

        public bool IsWhite(int row, int col)
        {
            return GetColor(row, col) == WHITE;
        }

        public bool IsBlack(int row, int col)
        {
            return GetColor(row, col) == BLACK;
        }

        public void SetColor(int row, int col, byte r, byte g, byte b)
        {
            var color = (byte*)data.Scan0 + row * data.Stride + col * 3;
            color[0] = r;
            color[1] = g;
            color[2] = b;
        }
    }
}
