using PdfSharp.Drawing;
using PdfSharp;


namespace DHTMLX.Export.PDF
{
    internal class Resizer
    {
        private double _xRatio = 1;
        private double _yRatio = 1;
        public PageOrientation Orient { get; set; }
        private bool _keep = true;
        private void _CalcRatio(Orientation actual, Orientation desired)
        {
            if (desired == Orientation.Default || actual == desired)
                return;

            _keep = false;

            _yRatio = 1.0 / _xRatio;
        }

        public Resizer(PageOrientation from, Orientation to)
        {
            _CalcRatio(ToDHXOrient(from), to);
            if (_keep)
                Orient = from;
            else
                Orient = ToPDFOrient(to);
        }

        public PageOrientation ToPDFOrient(Orientation orient)
        {
            return orient == Orientation.Landscape ? PageOrientation.Landscape : PageOrientation.Portrait;
        }

        public Orientation ToDHXOrient(PageOrientation orient)
        {
            return orient == PageOrientation.Landscape ? Orientation.Landscape : Orientation.Portrait;
        }

        public XPoint Point(double x, double y)
        {
            return new XPoint(ResizeX(x), ResizeY(y));
        }

        public double ResizeX(double x)
        {
            if (_keep)
                return x;

            return x * _xRatio;
        }

        public double ResizeY(double y)
        {
            if (_keep)
                return y;

            return y * _yRatio;
        }

        public XRect Rect(double x, double y, double width, double height)
        {
            return new XRect(ResizeX(x), ResizeY(y), ResizeX(width), ResizeY(height));
        }
    }
}