using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace DHTMLX.Export.PDF.Scheduler
{
    public enum ColorProfile
    {
        Color,
        FullColor,
        Gray,
        BW,
        Custom
    }

    public class XMLParser
    {
        public List<PDFPage> Pages { get; set; }

        public ColorProfile StringToColorProfile(string profile)
        {
            if (profile == null)
                return ColorProfile.Color;
            switch (profile.ToLower())
            {
                case "gray":
                    return ColorProfile.Gray;
                case "full_color":
                    return ColorProfile.FullColor;
                case "color":
                    return ColorProfile.Color;
                case "bw":
                    return ColorProfile.BW;
                case "custom":
                    return ColorProfile.Custom;
                default:
                    return ColorProfile.Color;
            }
        }

        public static List<XmlNode> GetElementsByTagName(XmlNode parent, string name)
        {
            var list = new List<XmlNode>();
            foreach (XmlNode node in parent.ChildNodes)
            {
                if (node.LocalName == name)
                {
                    list.Add(node);
                }
                else
                {
                    list.AddRange(GetElementsByTagName(node, name));
                }
            }
            return list;
        }

        public void ParsePages(XmlNode root)
        {

            if (root.Name == "pages")
            {
                var n1 = GetElementsByTagName(root, "page");
                if (n1 != null && n1.Count > 0)
                {

                    for (var i = 0; i < n1.Count; i++)
                    {
                        var page = new PDFPage { Node = n1[i] };
                        Pages.Add(page);
                        ParseConfig(page);
                        EventsParsing(page);
                    }
                }
            }
            else
            {
                var page = new PDFPage { Node = root };
                Pages.Add(page);
                ParseConfig(page);
                EventsParsing(page);
            }

        }
        public void ParseConfig(PDFPage page)
        {
            XmlNode scale = null;
            var root = page.Node;
            foreach (XmlNode nod in root.ChildNodes)
            {
                if (nod.LocalName == "scale")
                {
                    scale = nod;
                    break;
                }
            }

            var toCheck = new List<XmlNode> { root };
            if (scale != null)
                toCheck.Add(scale);
            if (root.ParentNode != null && root.ParentNode.NodeType != XmlNodeType.Document && root.ParentNode.Name != "data")
            {
                toCheck.Add(root.ParentNode);
            }

            for (var i = 0; i < toCheck.Count; i++)
            {
                if (toCheck[i].Attributes["profile"] != null && page.Profile == default(ColorProfile))
                {
                    page.Profile = StringToColorProfile(toCheck[i].Attributes["profile"].Value);
                }
                if (toCheck[i].Attributes["header"] != null && page.Header == default(bool))
                {
                    page.Header = (toCheck[i].Attributes["header"].Value == "true");
                }
                if (toCheck[i].Attributes["footer"] != null && page.Footer == default(bool))
                {
                    page.Footer = (toCheck[i].Attributes["footer"].Value == "true");
                }

                if (toCheck[i].Attributes["mode"] != null && page.Mode == default(string))
                    page.Mode = toCheck[i].Attributes["mode"] != null ? toCheck[i].Attributes["mode"].Value : "";

                if (toCheck[i].Attributes["today"] != null && page.TodayLabel == default(string))
                    page.TodayLabel = toCheck[i].Attributes["today"] != null ? toCheck[i].Attributes["today"].Value : "";

            }


        }

        public IEnumerable<XmlNode> GetPages(string xml)
        {
            var dom = new XmlDocument();
            dom.LoadXml(xml);
            var root = dom.DocumentElement;
            var res = new List<XmlNode>();

            if (root.FirstChild.Name != "data")
            {
                res.Add(root);
            }
            else
            {
                foreach (var nod in root.ChildNodes)
                    res.Add(nod as XmlNode);
            }
            return res;
        }


        public void SetXML(XmlNode root)
        {
            Pages = new List<PDFPage>();
            ParsePages(root);
        }

        public string[][] MonthColsParsing(PDFPage page)
        {

            var n1 = GetElementsByTagName(page.Node, "column");
            if (n1 != null && n1.Count > 0)
            {
                page.Cols = new[] { new string[n1.Count] };

                for (var i = 0; i < n1.Count; i++)
                {
                    var col = n1[i];
                    page.Cols[0][i] = col.FirstChild.Value;
                }
            }
            return page.Cols;
        }

        public IList<MonthRow> GetRowsObjects(PDFPage page)
        {
            if (page.RowObs == null)
            {
                var n1 = GetElementsByTagName(page.Node, "row");
                if (n1 != null && n1.Count > 0)
                {
                    page.RowObs = new List<MonthRow>(7);
                    for (var i = 0; i < n1.Count; i++)
                    {
                        var r = new MonthRow();
                        page.RowObs.Add(r);

                        var node = n1[i];

                        double height;
                        var hght = node.Attributes["height"].Value;

                        if (double.TryParse(hght, out height))
                            r.Height = height;
                        else
                            r.Height = default(double);

                        var week = node.FirstChild.Value;
                        r.Cells = week.Split(new[] { "|" }, StringSplitOptions.None);

                    }
                }
            }
            return page.RowObs;
        }

        public string[,] MonthRowsParsing(PDFPage page)
        {
            var n1 = GetElementsByTagName(page.Node, "row");
            if (n1 != null && n1.Count > 0)
            {
                page.Rows = new string[n1.Count, 7];
                for (var i = 0; i < n1.Count; i++)
                {
                    var row = n1[i];
                    var week = row.FirstChild.Value;
                    var days = week.Split(new[] { "|" }, StringSplitOptions.None);
                    for (var j = 0; j < days.Length; j++)
                        page.Rows[i, j] = days[j];
                }
            }
            return page.Rows;
        }

        public void EventsParsing(PDFPage page)
        {
            var n1 = GetElementsByTagName(page.Node, "event");
            if (n1 != null && n1.Count > 0)
            {
                for (var i = 0; i < n1.Count; i++)
                {
                    var ev = n1[i];
                    SchedulerEvent oEv = new SchedulerEvent();
                    oEv.Parse(ev);
                    if ((oEv.Type == "event_line") && (page.Mode != "month") && (page.Mode != "timeline") && (page.Mode != "treetimeline"))
                    {
                        page.Multiday.Add(oEv);
                    }
                    else
                    {
                        page.Events.Add(oEv);
                    }
                }
            }
        }

        public int[] ScaleRatios(PDFPage page)
        {
            var n1 = GetElementsByTagName(page.Node, "column");
            return n1.Where(n => n.Attributes["second_scale"] != null).GroupBy(n => n.Attributes["second_scale"].Value).Select(g => g.Count()).ToArray();
        }

        public string[][] WeekColsParsing(PDFPage page)
        {
            if (page.Cols != null)
                return page.Cols;
            var n1 = GetElementsByTagName(page.Node, "column");

            if (n1 != null && n1.Count > 0)
            {

                var scale1 = n1.Where(n => n.Attributes["second_scale"] == null).ToList();
                var scale2 = n1.Where(n => n.Attributes["second_scale"] != null).ToList();

                string[][] scales;
                if (scale2.Count > 0)
                {
                    scales = new string[2][];
                    scales[0] = new string[scale1.Count];
                    scales[1] = new string[scale2.Count];
                }
                else
                {
                    scales = new string[1][];
                    scales[0] = new string[scale1.Count];
                }

                for (var i = 0; i < scale1.Count; i++)
                {
                    scales[0][i] = scale1[i].FirstChild.Value;
                }

                for (var i = 0; i < scale2.Count; i++)
                {
                    scales[1][i] = scale2[i].FirstChild.Value;
                }
                page.Cols = scales;

            }
            return page.Cols;
        }

        public string[] WeekRowsParsing(PDFPage page)
        {
            string[] rows = null;
            var n1 = GetElementsByTagName(page.Node, "row");
            if (n1 != null && n1.Count > 0)
            {
                rows = new string[n1.Count];
                for (var i = 0; i < n1.Count; i++)
                {
                    var row = n1[i];
                    rows[i] = row.FirstChild.Value;
                }
            }
            return rows;
        }

        public SchedulerMonth[] YearParsing(PDFPage page)
        {
            SchedulerMonth[] monthes = null;
            var n1 = GetElementsByTagName(page.Node, "month");
            if (n1 != null && n1.Count > 0)
            {
                monthes = new SchedulerMonth[n1.Count];
                for (var i = 0; i < n1.Count; i++)
                {
                    monthes[i] = new SchedulerMonth();
                    var mon = n1[i];
                    monthes[i].Parse(mon);
                }
            }
            return monthes;
        }

        public string[] AgendaColsParsing(PDFPage page)
        {
            string[] cols = null;
            var n1 = GetElementsByTagName(page.Node, "column");
            if (n1 != null && n1.Count > 0)
            {
                cols = new string[n1.Count];
                for (var i = 0; i < n1.Count; i++)
                {
                    var col = n1[i];
                    cols[i] = col.FirstChild.Value;
                }
            }
            return cols;
        }

    }
}