using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FittingStraightLine
{
    unsafe public static class ImageProcesser
    {
        public static void Process(Bitmap bitmap, string name = "")
        {
            ClearHighlighting(bitmap, name);
            UserFunction(bitmap, name);
        }

        private static void ClearHighlighting(Bitmap bitmap, string name = "")
        {
            var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height)
                , ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            for (int j = 0; j < data.Height; ++j)
            {
                for (int i = 0; i < data.Width; ++i)
                {
                    var color = (byte*)data.Scan0 + j * data.Stride + i * 3;
                    if(color[0] == 0xfe)
                    {

                    }
                    else
                    {

                    }
                    switch (color[0])
                    {
                        case 0x20:
                            color[0] = color[1] = color[2] = 0x00;
                            break;
                        case 0xe0:
                            color[0] = color[1] = color[2] = 0xfe;
                            break;
                        case 0xfe:
                        case 0x00:
                            break;
                        default:
                            color[0] = color[1] = color[2] = 0xfe;
                            break;
                    }
                }
            }
            for (int i = data.Width / 2 - 40; i < data.Width; ++i)
            {
                var color = (byte*)data.Scan0 + 29 * data.Stride + i * 3;
                if (color[0] == 0x00)
                {
                    color[0] = color[1] = color[2] = 0xfe;
                    break;
                }
            }
            bitmap.UnlockBits(data);
        }

        private static void UserFunction(Bitmap bitmap, string name = "")
        {
            bitmap.RotateFlip(RotateFlipType.Rotate180FlipX);
            var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height)
                , ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            var rightBorder = new int[data.Height];
            var leftBorder = new int[data.Height];
            var middleLine = new int[data.Height];
            var leftSlope = new int[data.Height];
            var rightSlope = new int[data.Height];
            var leftZero = new int[data.Height];
            var rightZero = new int[data.Height];
            var middleSlope = new int[data.Height];
            var middleZero = new int[data.Height];

            var correctLeftRow = -1;
            var correctRightRow = -1;

            var borderSearchStart = data.Width / 2;

            for (int i = 0; i < data.Height; ++i)
            {
                rightBorder[i] = data.Width - 1;
                rightZero[i] = data.Width - 1;
                middleZero[i] = data.Width / 2;
            }

            for (int row = 0; row < data.Height; ++row)
            {
                for (int col = borderSearchStart - 1; col >= 0; --col)
                {
                    var color = (byte*)data.Scan0 + row * data.Stride + col * 3;
                    if (color[0] == 0x00 && color[3] == 0xff)
                    {
                        leftBorder[row] = col + 1;
                        break;
                    }
                }

                for (int col = borderSearchStart; col < data.Width - 1; ++col)
                {
                    var color = (byte*)data.Scan0 + row * data.Stride + col * 3;
                    if (color[0] == 0xff && color[3] == 0x00)
                    {
                        rightBorder[row] = col;
                        break;
                    }
                }

                if (row >= 4)
                {
                    double leftSlopeX = 0, leftSlopeA = 0, leftSlopeB = 0;
                    double rightSlopeX = 0, rightSlopeA = 0, rightSlopeB = 0;
                    double middleSlopeX = 0, middleSlopeA = 0, middleSlopeB = 0;
                    for (int col = row - 4; col <= row; ++col)
                    {
                        leftSlopeX += leftBorder[col];
                        leftSlopeA += col * leftBorder[col];
                        rightSlopeX += rightBorder[col];
                        rightSlopeA += col * rightBorder[col];
                        middleSlopeX += middleLine[col];
                        middleSlopeA += col * middleLine[col];
                    }
                    leftSlopeB = (row - 2) * leftSlopeX;
                    rightSlopeB = (row - 2) * rightSlopeX;
                    middleSlopeB = (row - 2) * middleSlopeX;

                    leftSlope[row] = (int)((leftSlopeA - leftSlopeB) / 10.0);
                    leftZero[row] = (int)((leftSlopeX / 5) - leftSlope[row] * (row - 2));
                    rightSlope[row] = (int)((rightSlopeA - rightSlopeB) / 10.0);
                    rightZero[row] = (int)((rightSlopeX / 5) - rightSlope[row] * (row - 2));
                    middleSlope[row] = (int)((middleSlopeA - middleSlopeB) / 10.0);
                    middleZero[row] = (int)((middleSlopeX / 5) - middleSlope[row] * (row - 2));
                }

                if (row > 5)
                {
                    if (leftBorder[row] == 0 || correctLeftRow != -1
                        || leftSlope[row - 1] == 0 ? leftSlope[row] < 0 : Math.Abs(leftSlope[row] / leftSlope[row - 1]) > 2)
                    {
                        if (correctLeftRow == -1)
                        {
                            correctLeftRow = row - 5;
                            while (correctLeftRow > 0 && leftSlope[correctLeftRow] == 0) { correctLeftRow--; }
                        }
                        leftBorder[row] = row * leftSlope[correctLeftRow] + leftZero[correctLeftRow];
                    }
                    if (rightBorder[row] == data.Width - 1 || correctRightRow != -1
                        || rightSlope[row - 1] == 0 ? rightSlope[row] > 0 : Math.Abs(rightSlope[row] / rightSlope[row - 1]) > 2)
                    {
                        if (correctRightRow == -1)
                        {
                            correctRightRow = row - 5;
                        }
                        rightBorder[row] = row * rightSlope[correctRightRow] + rightZero[correctRightRow];
                    }
                }

                {
                    borderSearchStart = middleLine[row] = (leftBorder[row] + rightBorder[row]) / 2;
                    byte* color;
                    color = (byte*)data.Scan0 + row * data.Stride + middleLine[row] * 3;
                    color[0] = 0xfe;
                    color[1] = color[2] = 0;
                    color = (byte*)data.Scan0 + row * data.Stride + leftBorder[row] * 3;
                    color[0] = 0xfe;
                    color[1] = color[2] = 0;
                    color = (byte*)data.Scan0 + row * data.Stride + rightBorder[row] * 3;
                    color[0] = 0xfe;
                    color[1] = color[2] = 0;
                }
            }

            bitmap.UnlockBits(data);
            bitmap.RotateFlip(RotateFlipType.Rotate180FlipX);
        }
    }
}
