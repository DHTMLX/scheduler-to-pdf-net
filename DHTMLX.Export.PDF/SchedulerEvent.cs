using System;
using System.Xml;
using System.Text.RegularExpressions;

namespace DHTMLX.Export.PDF.Scheduler
{
    public class SchedulerEvent
    {
        private string _backgroundColor;
        private string _text = "";
        private string _color;

        public SchedulerEvent(){
            Footenote = -1;
        }

        public string Text
        {
            get { return _text ?? (_text = ""); }
            protected set { _text = value; }
        }
        public int Day { get; protected set; }
        public int Week { get; protected set; }
        public int Month { get; protected set; }
        public int Len { get; protected set; }
        public double X { get; protected set; }
        public double Y { get; set; }
        public string Type { get; protected set; }
        public double Width { get; protected set; }
        public double Height { get; protected set; }
        public string HeaderText { get; protected set; }
        public string HeaderAgendaText { get; protected set; }

        public string BackgroundColor
        {
            get
            {
                _backgroundColor = ProcessColor(_backgroundColor);
                return _backgroundColor;
            }
            protected set { _backgroundColor = value; }
        }

        public string Color
        {
            get
            {
                _color = ProcessColor(_color);
                return _color;
            }
            protected set { _color = value; }
        }

        public int Footenote { get; set; }

        private string GetNodeValue(XmlNode node)
        {
            var txt = node.FirstChild != null ? node.FirstChild.Value : "";
            if (string.IsNullOrEmpty(txt))
                txt = node.InnerText;

            if (string.IsNullOrEmpty(txt))
                txt = "";
            return txt;
        }
        private int GetNumAttributeValue(XmlNode node, string attr)
        {
            string val = node.Attributes[attr] != null ? node.Attributes[attr].Value : "0";
            int result = 0;
            if (!string.IsNullOrEmpty(val) && val != "undefined")
                int.TryParse(val, out result);
            return result;
        }
        private string GetAttributeValue(XmlNode node, string attr)
        {
            return node.Attributes[attr] != null ? node.Attributes[attr].Value : "0";

        }
        private double GetDoublAttributeValue(XmlNode node, string attr)
        {
            string val = node.Attributes[attr] != null ? node.Attributes[attr].Value : "0";
            double result = 0;

            if (!string.IsNullOrEmpty(val) && val != "undefined")
                double.TryParse(val, System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out result);
            return result;
        }
        public void Parse(XmlNode parent)
        {
            var children = parent.ChildNodes;
            var bgProcessed = false;
            for (var i = 0; i < children.Count; i++)
            {
                switch (children[i].LocalName)
                {
                    case "body":
                        Text = GetNodeValue(children[i]);
                        BackgroundColor = GetAttributeValue(children[i], "backgroundColor");
                        Color = GetAttributeValue(children[i], "color");
                        bgProcessed = true;
                        break;
                    case "header":
                        HeaderText = GetNodeValue(children[i]);
                        break;
                    case "head":
                        HeaderAgendaText = GetNodeValue(children[i]);
                        break;
                }
            }
            if (!bgProcessed)
            {
                BackgroundColor = GetAttributeValue(parent, "backgroundColor");
                Color = GetAttributeValue(parent, "color");
            }

            Day = GetNumAttributeValue(parent, "day");
            Week = GetNumAttributeValue(parent, "week");
            Len = GetNumAttributeValue(parent, "len");
            Month = GetNumAttributeValue(parent, "month");

            X = GetDoublAttributeValue(parent, "x");
            Y = GetDoublAttributeValue(parent, "y");
            Width = GetDoublAttributeValue(parent, "width");
            Height = GetDoublAttributeValue(parent, "height");

            Type = GetAttributeValue(parent, "type");
        }

        private string ProcessColor(string color)
        {

            if (Regex.IsMatch(color, "#[0-9A-Fa-f]{6}"))
            {
                return color.Substring(1);
            }

            if (Regex.IsMatch(color, "[0-9A-Fa-f]{6}"))
            {
                return color;
            }

            var m3 = Regex.Match(color, "rgb\\s?\\(\\s?(\\d{1,3})\\s?,\\s?(\\d{1,3})\\s?,\\s?(\\d{1,3})\\s?\\)");

            if (m3.Length > 0)
            {
                var r = m3.Groups[1].Value;
                var g = m3.Groups[2].Value;
                var b = m3.Groups[3].Value;
                r = int.Parse(r).ToString("x");
                r = (r.Length == 1) ? "0" + r : r;
                g = int.Parse(g).ToString("x");
                g = (g.Length == 1) ? "0" + g : g;
                b = int.Parse(b).ToString("x");
                b = (b.Length == 1) ? "0" + b : b;
                color = r + g + b;
                return color;
            }
            return "transparent";
        }
    }
}