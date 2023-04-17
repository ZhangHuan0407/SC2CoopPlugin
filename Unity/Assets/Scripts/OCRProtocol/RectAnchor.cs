using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Game.OCR
{
    // 为了和 System.Drawing.Rectangle 坐标系一致，左上是min点，右下是max点，左上(0,0)
    [Serializable]
    public struct RectAnchor
    {
        public static readonly Regex StrRegex = new Regex("L:(?<Left>\\-?[0-9]+),T:(?<Top>\\-?[0-9]+),W:(?<Width>[0-9]+),H:(?<Height>[0-9]+)");

        public int Left;
        public int Top;
        public int Width;
        public int Height;

        public int Right
        {
            get => Left + Width;
            set => Width = value - Left;
        }
        public int Bottom
        {
            get => Top + Height;
            set => Height = value - Top;
        }

        public RectAnchor(int left, int top, int width, int height)
        {
            Left = left;
            Top = top;
            Width = width;
            Height = height;
        }

        //private PointF Lerp(PointF from, PointF to, PointF value)
        //{
        //    float x = (to.X - from.X) * value.X + from.X;
        //    float y = (to.Y - from.Y) * value.Y + from.Y;
        //    return new PointF(x, y);
        //}
        //private Point ClampToInt(Point min, Point max, PointF value)
        //{
        //    int x = (int)Math.Round(value.X);
        //    if (x > max.X)
        //        x = max.X;
        //    if (x < min.X)
        //        x = min.X;
        //    int y = (int)Math.Round(value.Y);
        //    if (y > max.Y)
        //        y = max.Y;
        //    if (y < min.Y)
        //        y = min.Y;
        //    return new Point(x, y);
        //}

        public static bool operator ==(RectAnchor l, RectAnchor r)
        {
            return l.Left == r.Left &&
                   l.Top == r.Top &&
                   l.Width == r.Width &&
                   l.Height == r.Height;
        }
        public static bool operator !=(RectAnchor l, RectAnchor r)
        {
            return l.Left != r.Left &&
                   l.Top != r.Top &&
                   l.Width != r.Width &&
                   l.Height != r.Height;
        }

        public static bool TryParse(string content, out RectAnchor rectAnchor)
        {
            if (StrRegex.Match(content) is Match match &&
                match.Success)
            {
                rectAnchor.Left = int.Parse(match.Groups["Left"].Value);
                rectAnchor.Top = int.Parse(match.Groups["Top"].Value);
                rectAnchor.Width = int.Parse(match.Groups["Width"].Value);
                rectAnchor.Height = int.Parse(match.Groups["Height"].Value);
                return true;
            }
            else
            {
                rectAnchor = default;
                return false;
            }
        }
        public override string ToString() => $"L:{Left},T:{Top},W:{Width},H:{Height}";
    }
}