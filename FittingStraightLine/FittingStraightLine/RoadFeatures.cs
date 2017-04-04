using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace FittingStraightLine
{
    unsafe public struct RoadFeatures
    {
        public int[] rightBorder;
        public int[] leftBorder;
        public int[] middleLine;
        public int[] leftSlope;
        public int[] rightSlope;
        public int[] leftZero;
        public int[] rightZero;
        public int[] middleSlope;
        public int[] middleZero;

        public static readonly byte BLACK = 0x00;
        public static readonly byte WHITE = 0xff;

        public BitmapData data;

        public RoadFeatures(int height, int width, BitmapData data)
        {
            rightBorder = new int[height];
            leftBorder = new int[height];
            middleLine = new int[height];
            leftSlope = new int[height];
            rightSlope = new int[height];
            leftZero = new int[height];
            rightZero = new int[height];
            middleSlope = new int[height];
            middleZero = new int[height];
            rightBorder = new int[height];

            this.data = data;

            for (int i = 0; i < height; ++i)
            {
                rightBorder[i] = width - 1;
                rightZero[i] = width - 1;
                middleZero[i] = width / 2;
            }
        }

        private byte GetColor(int row, int col)
        {
            var color = (byte*)data.Scan0 + row * data.Stride + col * 3;
            return color[0];
        }

        private void SetColor(int row, int col, byte r, byte g, byte b)
        {
            var color = (byte*)data.Scan0 + row * data.Stride + col * 3;
            color[0] = r;
            color[1] = g;
            color[2] = b;
        }

        public void SearchForBorders()
        {
            var borderSearchStart = data.Width / 2;

            for (int row = 0; row < data.Height; ++row)
            {
                for (int col = borderSearchStart - 1; col >= 0; --col)
                {
                    if (GetColor(row, col) == BLACK && GetColor(row, col + 1) == WHITE)
                    {
                        leftBorder[row] = col + 1;
                        break;
                    }
                }

                for (int col = borderSearchStart; col < data.Width - 1; ++col)
                {
                    var color = (byte*)data.Scan0 + row * data.Stride + col * 3;
                    if (GetColor(row, col) == WHITE && GetColor(row, col + 1) == BLACK)
                    {
                        rightBorder[row] = col;
                        break;
                    }
                }

                borderSearchStart = middleLine[row] = (leftBorder[row] + rightBorder[row]) / 2;
            }
        }

        public void CalculateSlopes()
        {
            for (int row = 4; row < data.Height; ++row)
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
        }

        public bool IsCurve
        {
            get
            {
                int blackCnt = 0;
                for (int row = data.Height - 1; row >= 40; --row)
                {
                    if (GetColor(row, middleLine[row]) == BLACK)
                    {
                        ++blackCnt;
                    }
                }
                if (blackCnt > 5)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public void CompensateCurve()
        {
            int row;
            for (row = data.Height - 1; row > 8; --row)
            {
                if (GetColor(row, middleLine[row]) == WHITE)
                {
                    break;
                }
            }
            var leftBorderNotFoundCnt = 0;
            var rightBorderNotFoundCnt = 0;
            for (int row_ = row; row_ > row - 12; --row_)
            {
                if (leftBorder[row_] == 0)
                {
                    ++leftBorderNotFoundCnt;
                }
                if (rightBorder[row_] == data.Width - 1)
                {
                    ++rightBorderNotFoundCnt;
                }
            }
            if (leftBorderNotFoundCnt > rightBorderNotFoundCnt && leftBorderNotFoundCnt > 6)
            {
                for (int row_ = data.Height - 1; row_ > row; --row_)
                {
                    middleLine[row_] = 0;
                }
                for (int cnt = 0; cnt < 12; ++cnt)
                {
                    middleLine[row - cnt] = cnt * middleLine[row - 12] / 12;
                }
            }
            else if (rightBorderNotFoundCnt > 6)
            {
                for (int row_ = data.Height - 1; row_ > row; --row_)
                {
                    middleLine[row_] = data.Width - 1;
                }
                for (int cnt = 0; cnt < 12; ++cnt)
                {
                    middleLine[row - cnt] = data.Width - 1 - cnt * (data.Width - 1 - middleLine[row - 12]) / 12;
                }
            }
        }

        public void CompensateCrossRoad()
        {
            var leftCompensateStart = data.Height - 1;
            var rightCompensateStart = data.Height - 1;
            var leftCompensateEnd = data.Height - 1;
            var rightCompensateEnd = data.Height - 1;

            {
                int row = 6;
                while (row < data.Height && leftBorder[row] != 0
                    && Math.Abs(leftSlope[row] - leftSlope[row - 1]) < 3) { ++row; }
                leftCompensateStart = row;
                row += 5;
                while (row < data.Height
                    && (leftBorder[row] == 0 || Math.Abs(leftSlope[row] - leftSlope[row - 1]) >= 3)) { ++row; }
                row += 4;
                leftCompensateEnd = Math.Min(row, data.Height - 1);
            }

            {
                int row = 6;
                while (row < data.Height && rightBorder[row] != data.Width - 1
                    && Math.Abs(rightSlope[row] - rightSlope[row - 1]) < 3) { ++row; }
                rightCompensateStart = row;
                row += 5;
                while (row < data.Height
                    && (rightBorder[row] == data.Width - 1 || Math.Abs(rightSlope[row] - rightSlope[row - 1]) >= 3)) { ++row; }
                row += 4;
                rightCompensateEnd = Math.Min(row, data.Height - 1);
            }

            for (int row = leftCompensateStart; row < leftCompensateEnd; ++row)
            {
                leftBorder[row] = row * leftSlope[leftCompensateStart - 5] + leftZero[leftCompensateStart - 5];
            }

            for (int row = rightCompensateStart; row < rightCompensateEnd; ++row)
            {
                rightBorder[row] = row * rightSlope[rightCompensateStart - 5] + rightZero[rightCompensateStart - 5];
            }

            for (int row = 0; row < data.Height; ++row)
            {
                middleLine[row] = (leftBorder[row] + rightBorder[row]) / 2;
            }
        }

        public void HighlightBorderAndMiddleline()
        {
            for (int row = 0; row < data.Height; ++row)
            {
                SetColor(row, middleLine[row], 0xfe, 0, 0);
                SetColor(row, leftBorder[row], 0xfe, 0, 0);
                SetColor(row, rightBorder[row], 0xfe, 0, 0);
            }
        }

        public bool IsRing
        {
            get
            {
                int i;
                for(i = data.Width / 2; i >= 0; --i)
                {
                    
                }
                return true;
            }
        }
    }
}
