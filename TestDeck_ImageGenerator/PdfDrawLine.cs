using PdfSharp.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDeck_ImageGenerator
{
    public class PdfDrawLine : Base
    {
        public PdfDrawLine() : base()
        {

        }        


        public void DrawLine(XGraphics gfx, int x, int y)
        {
            // DRAW A LINE 
            // FIRST INSTATIATE AN XPEN WITH ARGUMENTS INDICATING COLOR AND LINE THICKNESS
            // SECOND DRAW A LINE USING DRAWLINE METHOD - ARGUMENTS ARE: 1 = PEN, 2 = X COORD., 3 = Y COORD., 4 = LENGTH OF LINE, 5 = Y COORD.
            // Y COORD. ARE ACTUALLY -2 FROM DATA
            // LENGTH SHOULD BE -32 FROM X COORD.
            int len = x + 35;

            var pen = new XPen(XColors.Black, 2);
            gfx.DrawLine(pen, x, y, len, y);

        }

        /// <summary>
        /// Draws a polyline.
        /// </summary>
        void DrawLines(XGraphics gfx, int number)
        {
            BeginBox(gfx, number, "DrawLines");

            var pen = new XPen(XColors.DarkSeaGreen, 6);
            pen.LineCap = XLineCap.Round;
            pen.LineJoin = XLineJoin.Bevel;
            var points = new[] { new XPoint(20, 30), new XPoint(60, 120), new XPoint(90, 20), new XPoint(170, 90), new XPoint(230, 40) };
            gfx.DrawLines(pen, points);

            EndBox(gfx);
        }
    }
}
