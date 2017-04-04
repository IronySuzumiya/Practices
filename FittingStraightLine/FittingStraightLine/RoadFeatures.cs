using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace FittingStraightLine
{
    public enum RoadTypeEnum
    {
        Unknown,
        Curve,
        CrossRoad,
        Ring
    };

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

        public ImageFrame image;

        public RoadFeatures(BitmapData data)
        {
            image = new ImageFrame(data);

            rightBorder = new int[image.height];
            leftBorder = new int[image.height];
            middleLine = new int[image.height];
            leftSlope = new int[image.height];
            rightSlope = new int[image.height];
            leftZero = new int[image.height];
            rightZero = new int[image.height];
            middleSlope = new int[image.height];
            middleZero = new int[image.height];
            rightBorder = new int[image.height];

            for (int i = 0; i < image.height; ++i)
            {
                rightBorder[i] = image.width - 1;
                rightZero[i] = image.width - 1;
                middleZero[i] = image.width / 2;
            }
        }

        public void SearchForBorders()
        {
            var borderSearchStart = image.width / 2;

            for (int row = 0; row < image.height; ++row)
            {
                for (int col = borderSearchStart - 1; col >= 0; --col)
                {
                    if (image.IsBlack(row, col) && image.IsWhite(row, col + 1))
                    {
                        leftBorder[row] = col + 1;
                        break;
                    }
                }

                for (int col = borderSearchStart; col < image.width - 1; ++col)
                {
                    if (image.IsWhite(row, col) && image.IsBlack(row, col + 1))
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
            for (int row = 4; row < image.height; ++row)
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

        public bool IsCrossRoad
        {
            get
            {
                int notFoundLeftBorderCnt = 0;
                int notFoundRightBorderCnt = 0;
                for (int row = 0; row < image.height; ++row)
                {
                    if(leftBorder[row] == 0)
                    {
                        ++notFoundLeftBorderCnt;
                    }
                    if(rightBorder[row] == image.width - 1)
                    {
                        ++notFoundRightBorderCnt;
                    }
                }
                if(notFoundLeftBorderCnt > 3 && notFoundRightBorderCnt > 3
                    && notFoundLeftBorderCnt + notFoundRightBorderCnt > 15)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool IsCurve
        {
            get
            {
                int blackCnt = 0;
                for (int row = image.height - 1; row >= 40; --row)
                {
                    if (image.IsBlack(row, middleLine[row]))
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

        public bool IsRing
        {
            get
            {
                int i;
                for (i = image.width / 2; i >= 0; --i)
                {

                }
                return false;
            }
        }

        public RoadTypeEnum RoadType
        {
            get
            {
                return IsRing ? RoadTypeEnum.Ring :
                    IsCurve ? RoadTypeEnum.Curve :
                    IsCrossRoad ? RoadTypeEnum.CrossRoad :
                    RoadTypeEnum.Unknown;
            }
        }

        public void CompensateCurve()
        {
            int row;
            for (row = image.height - 1; row > 8; --row)
            {
                if (image.IsWhite(row, middleLine[row]))
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
                if (rightBorder[row_] == image.width - 1)
                {
                    ++rightBorderNotFoundCnt;
                }
            }
            if (leftBorderNotFoundCnt > rightBorderNotFoundCnt && leftBorderNotFoundCnt > 6)
            {
                for (int row_ = image.height - 1; row_ > row; --row_)
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
                for (int row_ = image.height - 1; row_ > row; --row_)
                {
                    middleLine[row_] = image.width - 1;
                }
                for (int cnt = 0; cnt < 12; ++cnt)
                {
                    middleLine[row - cnt] = image.width - 1 - cnt * (image.width - 1 - middleLine[row - 12]) / 12;
                }
            }
        }

        public void CompensateCrossRoad()
        {
            var leftCompensateStart = image.height - 1;
            var rightCompensateStart = image.height - 1;
            var leftCompensateEnd = image.height - 1;
            var rightCompensateEnd = image.height - 1;

            {
                int row = 6;
                while (row < image.height && leftBorder[row] != 0
                    && Math.Abs(leftSlope[row] - leftSlope[row - 1]) < 3) { ++row; }
                leftCompensateStart = row;
                row += 5;
                while (row < image.height
                    && (leftBorder[row] == 0 || Math.Abs(leftSlope[row] - leftSlope[row - 1]) >= 3)) { ++row; }
                row += 4;
                leftCompensateEnd = Math.Min(row, image.height - 1);
            }

            {
                int row = 6;
                while (row < image.height && rightBorder[row] != image.width - 1
                    && Math.Abs(rightSlope[row] - rightSlope[row - 1]) < 3) { ++row; }
                rightCompensateStart = row;
                row += 5;
                while (row < image.height
                    && (rightBorder[row] == image.width - 1 || Math.Abs(rightSlope[row] - rightSlope[row - 1]) >= 3)) { ++row; }
                row += 4;
                rightCompensateEnd = Math.Min(row, image.height - 1);
            }

            for (int row = leftCompensateStart; row < leftCompensateEnd; ++row)
            {
                leftBorder[row] = row * leftSlope[leftCompensateStart - 5] + leftZero[leftCompensateStart - 5];
            }

            for (int row = rightCompensateStart; row < rightCompensateEnd; ++row)
            {
                rightBorder[row] = row * rightSlope[rightCompensateStart - 5] + rightZero[rightCompensateStart - 5];
            }

            for (int row = 0; row < image.height; ++row)
            {
                middleLine[row] = (leftBorder[row] + rightBorder[row]) / 2;
            }
        }

        public void CompensateRing()
        {

        }

        public void HighlightBorderAndMiddleline()
        {
            for (int row = 0; row < image.height; ++row)
            {
                image.SetColor(row, middleLine[row], 0xfe, 0, 0);
                image.SetColor(row, leftBorder[row], 0xfe, 0, 0);
                image.SetColor(row, rightBorder[row], 0xfe, 0, 0);
            }
        }
    }
}
