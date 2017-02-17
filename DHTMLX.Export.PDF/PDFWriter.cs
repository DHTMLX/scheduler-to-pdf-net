using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PdfSharp;
using System.IO;
using PdfSharp.Drawing;
using System.Web;
using PdfSharp.Pdf;

using PdfSharp.Drawing.Layout;
using DHTMLX.Export.PDF.Formatter;
using DHTMLX.Export.PDF.Scheduler;

using HtmlAgilityPack;

namespace DHTMLX.Export.PDF
{
    public enum Orientation
    {
        Default,
        Landscape,
        Portrait
    }
    public class SchedulerPDFWriter
    {

        public Orientation Orientation = Orientation.Default;
        public string HeaderImgPath = "header.png";
        public string FooterImgPath = "footer.png";
        public double HeaderImgHeight = 100;
        public double FooterImgHeight = 100;
        public double BaseFontSize = 7.0;
        public double WeekAgendarDayFont = 9.4;

        public bool TrimContentStrings = true;
        public bool TrimLayoutStrings = true;
        public bool MultipageMonthLayout = false;
        public double HeaderFontSize = 7.0;
        public double TimelineRowMinHeight = 50;
        public double BorderWidth = 0.5;

        protected XMLParser Parser;
        protected PdfDocument Pdf;
        protected PdfPage Page;
        protected List<PdfPage> Pages;
        protected List<XGraphics> Graphics;
        protected XGraphics Gfx;
        protected XFont F1;
        protected PDFImages Images = new PDFImages();
        protected bool NeedPagesNums;

        private Resizer _resizer;
        private double _pageWidth;
        private double _pageHeight;

        private double _initialOffsetTop = 30;
        private double _initialOffsetBottom = 30;
        private double _initialOffsetLeft = 30;
        private double _initialOffsetRight = 30;

        private double _offsetTop = 30;
        private double _offsetBottom = 30;
        private double _offsetLeft = 30;
        private double _offsetRight = 30;
        private double _headHeight = 20;
        private double _monthDayHeaderHeight = 20;
        private double _monthEventHeight = 12;
        private double _leftScaleWidth = 80;
        private double _timelineLeft = 80;
        private double _monthEventOffsetLeft = 2;
        private double _monthEventOffsetTop = 2;
        private double _yearMonthOffsetLeft = 10;
        private double _yearMonthOffsetTop = 10;
        private double _yearMonthLabelHeight = 20;
        private double _weekEventHeaderHeight = 10;
        private double _agendaColOneWidth = 150;
        private double _multidayLineHeight = 14;
        private double _weekAgendaEventHeight = 20;

        private string _pageNumTemplate = "Page {pageNum}/{allNum}";

        private double _monthDayWidth;
        private double _monthDayHeight;
        private double _contHeight;
        private double _contWidth;

        private double _weekDayWidth;
        private double _weekDayHeight;
        private double _headerHeight;
        private double _multiHeight;

        private ColorProfile _profile;

        private double _cellOffset = 3;

        private string _bgColor = "C2D5FC";
        private string _lineColor = "586A7E";
        private string _headerLineColor = "FFFFFF";

        private string _dayHeaderColor = "EBEFF4";
        private string _watermarkTextColor = "8b8b8b";
        private string _dayBodyColor = "FFFFFF";
        private string _dayHeaderColorInactive = "E2E3E6";
        private string _dayBodyColorInactive = "ECECEC";
        private string _eventColor = "FFE763";
        private string _eventTextColor = "887A2E";
        private string _scaleOneColor = "FCFEFC";
        private string _scaleTwoColor = "DCE6F4";
        private string _timelineTreeColor = "969394";
        private string _eventBorderColor = "B7A543";
        private string _textColor = "000000";
        private string _yearDayActiveColor = "EBEFF4";
        private string _yearDayInactiveColor = "D6D6D6";
        private string _multidayColor = "E1E6FF";
        private string _matrixEventColor = "FFFFFF";
        private string _view;
        private string _watermark;

        public string ContentType { get { return "application/pdf"; } }
        protected PDFPage CurrentPage { get; set; }

        private double[] SelectColor(string fullColor, double[] def)
        {
            if ((fullColor != "transparent")
                        && ((_profile == ColorProfile.FullColor) || (_profile == ColorProfile.Color)))
            {
                return RGBColor.GetColor(fullColor);
            }
            return def;
        }


        private string StripHtml(string html)
        {
            return HtmlEntity.DeEntitize(html);
        }
        private string TextWrap(string text, double width, XFont f, bool actuallyDo)
        {
            text = StripHtml(text);

            if (!actuallyDo)
                return text;
            return TextWrap(text, width, f);
        }

        private string TextWrap(string text, double width, XFont f)
        {
            text = StripHtml(text);

            width = _resizer.ResizeX(width);
            if ((Gfx.MeasureString(text, f).Width <= width) || (text.Length == 0))
            {
                return text;
            }
            while ((Gfx.MeasureString(text + "...", f).Width > width) && (text.Length > 0))
            {
                text = text.Substring(0, text.Length - 1);
            }
            return text + "...";
        }
        public double[] GetSizes(PageOrientation orientation, PageSize size)
        {
            var sizes = new double[2];
            var siz = PageSizeConverter.ToSize(size);

            if (orientation == PageOrientation.Landscape)
            {

                sizes[0] = siz.Height;
                sizes[1] = siz.Width;
            }
            else
            {
                sizes[1] = siz.Height;
                sizes[0] = siz.Width;
            }

            return sizes;

        }
        private void SetColorProfile(ColorProfile profile)
        {
            if ((profile == ColorProfile.Color) || (profile == ColorProfile.FullColor))
            {
                _bgColor = "C2D5FC";
                _lineColor = "c1d4fc";
                _headerLineColor = "FFFFFF";
                _dayHeaderColor = "EBEFF4";
                _dayBodyColor = "FFFFFF";
                _dayHeaderColorInactive = "E2E3E6";
                _dayBodyColorInactive = "ECECEC";
                _eventColor = "FFE763";
                _eventTextColor = "887A2E";
                _scaleOneColor = "FCFEFC";
                _scaleTwoColor = "DCE6F4";

                _timelineTreeColor = "969394";

                _eventBorderColor = "B7A543";
                _textColor = "000000";
                _yearDayActiveColor = "EBEFF4";
                _yearDayInactiveColor = "D6D6D6";
                _multidayColor = "E1E6FF";
                _matrixEventColor = "FFFFFF";
                _watermarkTextColor = "8b8b8b";
            }
            else
            {
                if (profile == ColorProfile.Gray)
                {
                    _bgColor = "D3D3D3";
                    _lineColor = "666666";
                    _headerLineColor = "FFFFFF";
                    _dayHeaderColor = "EEEEEE";
                    _dayBodyColor = "FFFFFF";
                    _dayHeaderColorInactive = "E3E3E3";
                    _dayBodyColorInactive = "ECECEC";
                    _eventColor = "DFDFDF";
                    _eventTextColor = "000000";
                    _scaleOneColor = "FFFFFF";

                    _timelineTreeColor = "BBBBBB";

                    _scaleTwoColor = "E4E4E4";
                    _eventBorderColor = "9F9F9F";
                    _textColor = "000000";
                    _yearDayActiveColor = "EBEFF4";
                    _yearDayInactiveColor = "E2E3E6";
                    _multidayColor = "E7E7E7";
                    _matrixEventColor = "FFFFFF";
                    _watermarkTextColor = "8b8b8b";
                }
                else
                {
                    _bgColor = "FFFFFF";
                    _lineColor = "000000";
                    _headerLineColor = "000000";
                    _dayHeaderColor = "FFFFFF";
                    _dayBodyColor = "FFFFFF";
                    _dayHeaderColorInactive = "FFFFFF";
                    _dayBodyColorInactive = "FFFFFF";
                    _eventColor = "FFFFFF";
                    _eventTextColor = "000000";

                    _timelineTreeColor = "FFFFFF";

                    _scaleOneColor = "FFFFFF";
                    _scaleTwoColor = "FFFFFF";
                    _eventBorderColor = "000000";
                    _textColor = "000000";
                    _yearDayActiveColor = "FFFFFF";
                    _yearDayInactiveColor = "FFFFFF";
                    _multidayColor = "FFFFFF";
                    _matrixEventColor = "FFFFFF";
                    _watermarkTextColor = "000000";
                }
            }
        }
        private void CreatePdf(PageOrientation orientation, PageSize size)
        {
            _resizer = new Resizer(orientation, Orientation);

            if (Pdf == default(PdfDocument))
            {
                Pdf = new PdfDocument();
                F1 = CreateFont("Helvetica", 10);
                Pdf.Version = 14;
                Pages = new List<PdfPage>();
            }
            NewPage(_resizer.Orient, size);
        }

        protected PdfPage NewPage(PageOrientation orient, PageSize size)
        {
            Page = new PdfPage(Pdf);
            Pdf.AddPage(Page);

            Gfx = XGraphics.FromPdfPage(Page, XGraphicsPdfPageOptions.Replace);
            if (Graphics == null)
                Graphics = new List<XGraphics>();
            Graphics.Add(Gfx);
            Pages.Add(Page);

            Page.Size = size;
            Page.Orientation = orient;

            TodayLabelDraw(Gfx);

            _headerHeight = _headHeight;
            _offsetTop = _initialOffsetTop;
            _offsetBottom = _initialOffsetBottom;
            _offsetLeft = _initialOffsetLeft;
            _offsetRight = _initialOffsetRight;

            var sizes = GetSizes(_resizer.Orient, size);
            _pageWidth = sizes[0] - _offsetLeft - _offsetRight;
            _pageHeight = sizes[1] - _offsetTop - _offsetBottom;

            PrintHeader();
            PrintFooter();
            return Page;
        }
        protected XFont CreateFont(string name, double size)
        {
            var fontSettings = new XPdfFontOptions(PdfFontEncoding.Unicode);
            return new XFont(name, size, new XFontStyle(), fontSettings);
        }


        public MemoryStream Generate(string xml)
        {
            var data = new MemoryStream();

            Generate(xml, data);
            return data;
        }
        public void Generate(string xml, HttpResponse resp)
        {
            if (resp == default(HttpResponse))
            {
                throw new ArgumentNullException("resp", "HttpResponse is null");
            }
            var data = new MemoryStream();

            resp.ContentType = ContentType;
            resp.HeaderEncoding = Encoding.UTF8;

            resp.AppendHeader("Cache-Control", "max-age=0");
            Generate(xml, data);

            try
            {
                data.WriteTo(resp.OutputStream);
            }
            catch (Exception e)
            {
                throw new Exception("Can't write to the output stream:" + e.Message);
            }

        }
        public void PrintPage(PDFPage page)
        {
            _view = page.Mode;
            _profile = page.Profile;
            SetColorProfile(_profile);


            CurrentPage = page;
            switch (_view)
            {
                case "month":
                    PrintMonth(page);
                    break;
                case "year":
                    PrintYear(page);
                    break;
                case "agenda":
                case "map":
                    PrintAgenda(page);
                    break;
                case "treetimeline":
                case "timeline":
                    PrintTimeline(page);
                    break;
                case "matrix":
                    PrintMatrix(page);
                    break;
                case "week_agenda":
                    PrintWeekAgenda(page);
                    break;
                default:
                    PrintWeek(page);
                    break;
            }

            PrintWatermark();
        }

        public void PrintPages()
        {
            foreach (var i in Parser.Pages)
            {
                PrintPage(i);
            }
        }

        public void Generate(string xml, Stream resp)
        {
            if (string.IsNullOrEmpty(xml))
            {
                throw new ArgumentException("Input string is null or empty!", "xml");
            }
            Parser = new XMLParser();

            try
            {

                var pages = Parser.GetPages(xml);
                foreach (var page in pages)
                {
                    Parser.SetXML(page);
                    PrintPages();
                }
                if (NeedPagesNums)
                {
                    AgendaPagesDraw();
                }
                OutputPdf(resp);
            }
            catch (Exception e)
            {
                throw;
            }
        }

        private void OutputPdf(Stream resp)
        {
            Pdf.Save(resp, false);
        }

        private void PrintMonth(PDFPage page)
        {
            CreatePdf(PageOrientation.Landscape, PageSize.A4);
            MonthHeaderDraw();
            MonthContainerDraw();
            MonthEventsDraw(page);

        }

        private void PrintYear(PDFPage page)
        {
            CreatePdf(PageOrientation.Landscape, PageSize.A4);
            YearDraw(page);
        }

        private void PrintAgenda(PDFPage page)
        {
            CreatePdf(PageOrientation.Portrait, PageSize.A4);
            AgendaHeaderDraw();
            AgendaEventDraw(page);
            NeedPagesNums = true;
        }

        private void PrintTimeline(PDFPage page)
        {
            _leftScaleWidth = _timelineLeft;
            var size = PageSize.A4;
            CreatePdf(PageOrientation.Landscape, size);

            var rows = Parser.WeekRowsParsing(CurrentPage);
            var blocksNumber = (int)((_pageHeight - _headerHeight) / TimelineRowMinHeight);

            var pages = (int)Math.Ceiling((double)rows.Length / (double)blocksNumber);
            NeedPagesNums = true;
            for (var i = 0; i < pages; i++)
            {
                var currentPageRows = rows.Skip(i * blocksNumber).Take(blocksNumber).ToArray();
                WeekHeaderDraw();
                TimelineContainerDraw(currentPageRows);
                TimelineEventsDraw(page, i + 1, blocksNumber);
                if (i != pages - 1)
                {
                    NewPage(_resizer.Orient, size);
                }
            }
        }

        private void PrintMatrix(PDFPage page)
        {
            _leftScaleWidth = _timelineLeft;
            CreatePdf(PageOrientation.Landscape, PageSize.A4);
            WeekHeaderDraw();
            MatrixContainerDraw(page);
        }

        private void PrintWeek(PDFPage page)
        {
            CreatePdf(PageOrientation.Portrait, PageSize.A4);
            WeekHeaderDraw();
            WeekMultidayDraw(page);
            WeekContainerDraw();
            WeekEventsDraw(page);
            if (_profile == ColorProfile.BW)
                WeekBwBordersDraw();
        }

        private void PrintWeekAgenda(PDFPage page)
        {

            CreatePdf(PageOrientation.Portrait, PageSize.A4);
            var events = page.Events.ToArray();
            WeekAgendaContainerDraw();
            while (events.Length > 0)
            {

                events = WeekAgendaEventsDraw(events);
                if (events.Length > 0)
                {
                    Page = NewPage(PageOrientation.Portrait, PageSize.A4);
                    WeekAgendaContainerDraw();
                }
            }
        }

        private void PrintWatermark()
        {
            if (_watermark == null) return;

            F1 = CreateFont(F1.FontFamily.Name, 10);

            for (var i = 1; i <= Pages.Count; i++)
            {
                var graph = Graphics[i - 1];

                var text = new XTextFormatter(graph);
                var x = _offsetLeft;
                var y = _pageHeight + _offsetTop + F1.Size;
                text.DrawString(_watermark, F1, new XSolidBrush(RGBColor.GetXColor(_watermarkTextColor)), _resizer.Rect(x, y, graph.MeasureString(_watermark, F1).Width, F1.Size));
            }
        }

        private void MonthHeaderDraw()
        {
            var bgColor = RGBColor.GetColor(_bgColor);
            var borderColor = RGBColor.GetColor(_headerLineColor);
            var cols = Parser.MonthColsParsing(CurrentPage);
            var width = _pageWidth / cols[0].Length;
            var height = _headerHeight;
            var x = _offsetLeft;
            var y = _offsetTop;

            var font = new XFont(F1.FontFamily.Name, F1.Size);

            for (var i = 0; i < cols[0].Length; i++)
            {
                XRect cell = _resizer.Rect(x, y, width, height);

                Gfx.DrawRectangle(new XSolidBrush(RGBColor.GetXColor(bgColor)), cell);

                if (i > 0)
                {
                    var points = new XPoint[4];
                    points[0] = _resizer.Point(x, y);
                    points[1] = _resizer.Point(x, y + height);
                    Gfx.DrawLine(new XPen(RGBColor.GetXColor(borderColor), 1), points[0], points[1]);
                }

                var text = new XTextFormatter(Gfx);
                var content = TextWrap(cols[0][i], width - 2 * _cellOffset, font, TrimLayoutStrings);
                var textX = x + (width - Gfx.MeasureString(content, F1).Width) / 2;
                var textY = y + (height - F1.Size) / 2;

                text.DrawString(content, font, new XSolidBrush(RGBColor.GetXColor(RGBColor.GetColor(_textColor))), _resizer.Rect(textX, textY, Gfx.MeasureString(content, F1).Width, font.Size));

                x += width;
            }
        }

        private void MonthContainerDraw()
        {
            var borderColor = RGBColor.GetColor(_lineColor);
            var dayHeaderColor = RGBColor.GetColor(_dayHeaderColor);
            var dayBodyColor = RGBColor.GetColor(_dayBodyColor);
            var dayHeaderColorInactive = RGBColor
                    .GetColor(_dayHeaderColorInactive);
            var dayBodyColorInactive = RGBColor
                    .GetColor(_dayBodyColorInactive);
            var rows = Parser.MonthRowsParsing(CurrentPage);
            var width = _pageWidth / 7;
            var height = (_pageHeight - _headerHeight) / rows.GetLength(0);
            _monthDayWidth = width;
            _monthDayHeight = height;
            var x = _offsetLeft;
            var y = _offsetTop + _headerHeight;
            for (var i = 0; i < rows.GetLength(0); i++)
            {

                for (var j = 0; j < rows.GetLength(1); j++)
                {
                    var activeDay = GetActiveDay(rows, i, j);

                    var cell = _resizer.Rect(x, y, width, height);
                    var cellIn = _resizer.Rect(x, y, width, _monthDayHeaderHeight);
                    XBrush cellInBrush, cellBrush;
                    if (activeDay)
                    {
                        cellBrush = new XSolidBrush(RGBColor.GetXColor(dayBodyColor));
                        cellInBrush = new XSolidBrush(RGBColor.GetXColor(dayHeaderColor));
                    }
                    else
                    {
                        cellBrush = new XSolidBrush(RGBColor.GetXColor(dayBodyColorInactive));
                        cellInBrush = new XSolidBrush(RGBColor.GetXColor(dayHeaderColorInactive));
                    }


                    Gfx.DrawRectangle(cellBrush, cell);
                    Gfx.DrawRectangle(cellInBrush, cellIn);

                    var points = new XPoint[3];
                    points[0] = _resizer.Point(x, y);
                    points[1] = _resizer.Point(x + width, y);
                    points[2] = _resizer.Point(x, y + height);
                    Gfx.DrawLine(new XPen(RGBColor.GetXColor(borderColor), BorderWidth), points[0], points[2]);
                    Gfx.DrawLine(new XPen(RGBColor.GetXColor(borderColor), BorderWidth), points[0], points[1]);



                    var label = rows[i, j];
                    var textX = x + width - Gfx.MeasureString(label, F1).Width - _cellOffset;

                    var textY = y + (_monthDayHeaderHeight + F1.Size) / 2;

                    Gfx.DrawString(label, F1, new XSolidBrush(RGBColor.GetXColor(_textColor)), _resizer.Point(textX, textY));
                    x += width;
                }
                x = _offsetLeft;

                y += height;
            }


            var points2 = new XPoint[3];
            points2[0] = _resizer.Point(_offsetLeft + _pageWidth, _offsetTop);
            points2[1] = _resizer.Point(_offsetLeft + _pageWidth, _offsetTop + _pageHeight);
            points2[2] = _resizer.Point(_offsetLeft, _offsetTop + _pageHeight);

            Gfx.DrawLine(new XPen(RGBColor.GetXColor(borderColor), BorderWidth), points2[0], points2[1]);
            Gfx.DrawLine(new XPen(RGBColor.GetXColor(borderColor), BorderWidth), points2[1], points2[2]);


        }

        private void MonthEventsDraw(PDFPage page)
        {
            var rowObjects = Parser.GetRowsObjects(page);

            var eventColor = RGBColor.GetColor(_eventColor);
            var eventBorderColor = RGBColor.GetColor(_eventBorderColor);
            var textColor = RGBColor.GetColor(_eventTextColor);
            var events = page.Events;
            var minHeight = events.Count != 0 ? events.Min(e => e.Y) : 0;
            var footenote = 1;
            var eventsGrid = new int[6][];

            for (var i = 0; i < 6; i++)
            {
                eventsGrid[i] = new int[7];
                for (var j = 0; j < 7; j++)
                {
                    eventsGrid[i][j] = 0;
                }
            }

            var evHeight = events.Sum(e => e.Height) / (double)events.Count + 3;

            for (var i = 0; i < events.Count; i++)
            {
                var eventText = events[i].Text;
                var day = events[i].Day;
                var week = events[i].Week - 1;

                eventsGrid[week][day] = (int)Math.Floor((events[i].Y - rowObjects.Where((r, ind) => ind < week).Sum(r => r.Height)) / (evHeight));//todo: remove magic with event indexes

                var width = (events[i].Width * _monthDayWidth) / 100;
                var type = events[i].Type;
                var bgColor = events[i].BackgroundColor;
                var color = events[i].Color;
                if (type == "event_line")
                {

                    var cellX = _offsetLeft + day * _monthDayWidth
                            + _monthEventOffsetLeft;
                    var cellY = _offsetTop;
                    cellY += _headerHeight;
                    cellY += week * _monthDayHeight;
                    cellY += _monthDayHeaderHeight;
                    cellY += eventsGrid[week][day]
                            * (_monthEventHeight + _monthEventOffsetTop);
                    cellY += _monthEventOffsetTop;


                    var height = _monthEventHeight;
                    var cellPen = new XPen(RGBColor.GetXColor(eventBorderColor), BorderWidth);

                    var borders = new XPoint[4];
                    borders[0] = _resizer.Point(cellX, cellY);
                    borders[1] = _resizer.Point(cellX + width, cellY);
                    borders[2] = _resizer.Point(cellX + width, cellY + height);
                    borders[3] = _resizer.Point(cellX, cellY + height);


                    var cellIn = _resizer.Rect(cellX, cellY, width, height);
                    var cellInBrush = new XSolidBrush(RGBColor.GetXColor(SelectColor(bgColor, eventColor)));

                    F1 = CreateFont(F1.FontFamily.Name, 8);

                    var label = TextWrap(eventText, width - 2 * _cellOffset, F1, TrimContentStrings);
                    var textX = cellX + _cellOffset;

                    var textY = cellY + (height + F1.Size) / 2;
                    var textBrush = new XSolidBrush(RGBColor.GetXColor(SelectColor(color, textColor)));

                    if ((cellY + height) >= (_offsetTop + _headerHeight + (week + 1)
                            * _monthDayHeight))
                    {
                        var eventFootenote = GetFootenoteExists(events, week, day);
                        if (eventFootenote > 0)
                        {
                            events[i].Footenote = eventFootenote;
                        }
                        else
                        {
                            events[i].Footenote = footenote;
                            footenote++;
                        }
                    }
                    else
                    {

                        Gfx.DrawRectangle(cellInBrush, cellIn);
                        Gfx.DrawLine(cellPen, borders[0], borders[1]);
                        Gfx.DrawLine(cellPen, borders[1], borders[2]);
                        Gfx.DrawLine(cellPen, borders[2], borders[3]);
                        Gfx.DrawLine(cellPen, borders[3], borders[0]);

                        Gfx.DrawString(label, F1, textBrush, _resizer.Point(textX, textY));
                    }
                }
                else
                {
                    var cellX = _offsetLeft + day * _monthDayWidth
                            + _monthEventOffsetLeft;
                    var cellY = _offsetTop;
                    cellY += _headerHeight;
                    cellY += week * _monthDayHeight;
                    cellY += _monthDayHeaderHeight;
                    cellY += eventsGrid[week][day]
                            * (_monthEventHeight + _monthEventOffsetTop);
                    cellY += _monthEventOffsetTop;
                    var height = _monthEventHeight;
                    F1 = CreateFont(F1.FontFamily.Name, 8);

                    var text = TextWrap(eventText, width - 2 * _cellOffset, F1, TrimContentStrings);

                    XSolidBrush textBrush = new XSolidBrush(RGBColor.GetXColor(SelectColor(color, textColor)));

                    var textX = cellX + _cellOffset;
                    var textY = cellY + (height + F1.Size) / 2;

                    if (cellY + height >= _offsetTop + _headerHeight + (week + 1)
                        * _monthDayHeight)
                    {
                        var eventFootenote = GetFootenoteExists(events, week, day);
                        if (eventFootenote > 0)
                        {
                            events[i].Footenote = eventFootenote;
                        }
                        else
                        {
                            events[i].Footenote = footenote;
                            footenote++;
                        }
                    }
                    else
                    {
                        Gfx.DrawString(text, F1, textBrush, _resizer.Point(textX, textY));

                    }
                }
            }
            if (_profile == ColorProfile.BW)
                MonthBwBordersDraw();
            MonthFootenotesDraw(events, footenote, page, minHeight);
        }

        private int GetFootenoteExists(IList<SchedulerEvent> events, int week, int day)
        {
            var result = -1;
            for (var i = 0; i < events.Count; i++)
            {
                var ev = events[i];
                var eventWeek = ev.Week - 1;
                var eventDay = ev.Day;
                if (eventWeek == week && eventDay == day)
                {
                    var foote = ev.Footenote;
                    if (foote > 0)
                    {
                        result = ev.Footenote;
                    }
                }
            }
            return result;
        }

        private void MonthFootenotesDraw(IList<SchedulerEvent> events, int footenotes, PDFPage exportPage, double minHeight)
        {
            if (footenotes > 1)
            {
                NeedPagesNums = true;
                Page = NewPage(PageOrientation.Landscape, PageSize.A4);
            }
            else
            {
                return;
            }

            if (MultipageMonthLayout)
            {

                var evs = new List<SchedulerEvent>();
                for (var i = 1; i < footenotes; i++)
                {
                    var block = getFootenoteEvents(events, i).ToList();
                    var offset = block.Min(e => e.Y) - minHeight;
                    foreach (var ev in block)
                    {
                        ev.Y -= offset;
                    }
                    evs.AddRange(block);
                }
                evs.ForEach(e => e.Footenote = 0);

                CurrentPage.Events = evs;

                MonthHeaderDraw();
                MonthContainerDraw();
                MonthEventsDraw(exportPage);
            }
            else
            {

                var rows = Parser.MonthRowsParsing(CurrentPage);
                var borderColor = RGBColor.GetColor(_lineColor);
                var eventColor = RGBColor.GetColor(_eventColor);
                var eventTextColor = RGBColor.GetColor(_eventTextColor);

                var labelHeight = 20;
                var height = 20;
                var width = 160;
                var offsetLeft = 20;
                var x = _offsetLeft;
                var y = _offsetTop;
                for (var i = 1; i < footenotes; i++)
                {
                    var footEvents = getFootenoteEvents(events, i);


                    var day = footEvents[0].Day;
                    var week = footEvents[0].Week - 1;
                    if (y + labelHeight + height > _offsetTop + _pageHeight)
                    {
                        x += width + offsetLeft;
                        if (x + width > _offsetLeft + _pageWidth)
                        {
                            Page = NewPage(PageOrientation.Landscape, PageSize.A4);

                            x = _offsetLeft;
                        }
                        y = _offsetTop;
                    }
                    var label = rows[week, day] + "[" + i.ToString() + "]";


                    var labelX = x + (width - Gfx.MeasureString(label, F1).Width) / 2;
                    var labelY = y + labelHeight - _cellOffset;
                    Gfx.DrawString(label, F1, new XSolidBrush(RGBColor.GetXColor(_textColor)), _resizer.Point(labelX, labelY));

                    y += labelHeight;
                    for (var j = 0; j < footEvents.Count; j++)
                    {
                        if (y + height > _offsetTop + _pageHeight)
                        {
                            x += width + offsetLeft;
                            if (x + width > _offsetLeft + _pageWidth)
                            {
                                Page = NewPage(PageOrientation.Landscape, PageSize.A4);
                                x = _offsetLeft;
                            }
                            y = _offsetTop;
                            labelX = x + (width - Gfx.MeasureString(label, F1).Width) / 2;
                            labelY = y + labelHeight - _cellOffset;

                            Gfx.DrawString(label, F1, new XSolidBrush(RGBColor.GetXColor(_textColor)), _resizer.Point(labelX, labelY));

                            y += labelHeight;
                        }

                        var borders = new XPoint[4];
                        borders[0] = _resizer.Point(x, y);
                        borders[1] = _resizer.Point(x + width, y);
                        borders[2] = _resizer.Point(x + width, y + height);
                        borders[3] = _resizer.Point(x, y + height);
                        var contBg = _resizer.Rect(x, y, width, height);

                        var evBrush = new XSolidBrush(RGBColor.GetXColor(SelectColor(footEvents[j].BackgroundColor, eventColor)));

                        Gfx.DrawRectangle(evBrush, contBg);
                        Gfx.DrawLine(new XPen(RGBColor.GetXColor(borderColor), BorderWidth), borders[0], borders[1]);
                        Gfx.DrawLine(new XPen(RGBColor.GetXColor(borderColor), BorderWidth), borders[1], borders[2]);
                        Gfx.DrawLine(new XPen(RGBColor.GetXColor(borderColor), BorderWidth), borders[2], borders[3]);
                        Gfx.DrawLine(new XPen(RGBColor.GetXColor(borderColor), BorderWidth), borders[3], borders[0]);

                        var text = TextWrap(footEvents[j].Text, width - 2 * _cellOffset, F1, TrimContentStrings);

                        var textX = x + _cellOffset;
                        var textY = y + (height + F1.Size) / 2;

                        Gfx.DrawString(text, F1, new XSolidBrush(RGBColor.GetXColor(SelectColor(footEvents[j].Color, eventTextColor))), _resizer.Point(textX, textY));

                        y += height;
                    }
                }
            }
        }

        private IList<SchedulerEvent> getFootenoteEvents(IList<SchedulerEvent> events,
                int footenote)
        {
            var nums = 0;
            for (var i = 0; i < events.Count; i++)
            {
                if (events[i].Footenote == footenote)
                {
                    nums++;
                }
            }
            var footEvents = new List<SchedulerEvent>(nums);
            for (var i = 0; i < events.Count; i++)
            {
                if (events[i].Footenote == footenote)
                {
                    footEvents.Add(events[i]);
                }
            }
            return footEvents;
        }

        private void MonthBwBordersDraw()
        {
            var borderColor = RGBColor.GetColor(_lineColor);
            var x = _offsetLeft;
            var y = _offsetTop;
            Line(borderColor, x, y, x + _pageWidth, y);
            Line(borderColor, x, y, x, y + _headerHeight);

            x = _offsetLeft + _pageWidth;
            Line(borderColor, x, y, x, y + _headerHeight);
        }

        private void WeekHeaderDraw()
        {
            var bgColor = RGBColor.GetColor(_bgColor);
            var borderColor = RGBColor.GetColor(_headerLineColor);
            var cols = Parser.WeekColsParsing(CurrentPage);

            _weekDayWidth = (_pageWidth - _leftScaleWidth) / cols[0].Length;
            var defHeight = _headerHeight;
            _headerHeight = defHeight * cols.Length;

            var ratios = Parser.ScaleRatios(CurrentPage).Select(r => (double)r).ToList();


            for (var scale = 0; scale < cols.Length && scale < 2; scale++)
            {
                var x = _offsetLeft + _leftScaleWidth;
                var height = defHeight;

                var y = _offsetTop + defHeight * scale;


                var width = (_pageWidth - _leftScaleWidth) / cols[scale].Length;


                if (_weekDayWidth > width)
                    _weekDayWidth = width;
                for (var col = 0; col < cols[scale].Length; col++)
                {
                    if (scale == 0 && ratios.Count > 0 && cols[1].Length > 0 && col < ratios.Count)
                    {
                        width = (_pageWidth - _leftScaleWidth) / cols[1].Length * ratios[col];
                    }
                    else
                    {
                        width = (_pageWidth - _leftScaleWidth) / cols[scale].Length;
                    }

                    var cell = _resizer.Rect(x, y, width, height);

                    var cellBrush = new XSolidBrush(RGBColor.GetXColor(bgColor));
                    Gfx.DrawRectangle(cellBrush, cell);

                    var points = new XPoint[3];
                    points[0] = _resizer.Point(x, y);
                    points[1] = _resizer.Point(x + width, y);
                    points[2] = _resizer.Point(x, y + height);

                    Gfx.DrawLine(new XPen(RGBColor.GetXColor(borderColor), BorderWidth), points[0], points[2]);
                    Gfx.DrawLine(new XPen(RGBColor.GetXColor(borderColor), BorderWidth), points[0], points[1]);


                    F1 = new XFont(F1.FontFamily.Name, HeaderFontSize);

                    var text = TextWrap(cols[scale][col], width
                            - 2 * _cellOffset, F1, TrimLayoutStrings);

                    var textX = x + (width - Gfx.MeasureString(text, F1).Width) / 2;
                    var textY = y + (height + F1.Size) / 2;
                    Gfx.DrawString(text, F1, new XSolidBrush(RGBColor.GetXColor(_textColor)), _resizer.Point(textX, textY));

                    x += width;
                }

            }
        }

        private void WeekMultidayDraw(PDFPage page)
        {
            var bgColor = RGBColor.GetColor(_multidayColor);
            var borderColor = RGBColor.GetColor(_headerLineColor);

            _multiHeight = GetMultidayHeight(page);
            var x = _offsetLeft;
            var y = _offsetTop + _headerHeight;
            var cell = _resizer.Rect(x, y, _pageWidth, _multiHeight);

            var cellBrush = new XSolidBrush(RGBColor.GetXColor(bgColor));
            Gfx.DrawRectangle(cellBrush, cell);

            var points = new XPoint[4];
            points[0] = _resizer.Point(x + _leftScaleWidth, y + 0);
            points[1] = _resizer.Point(x + _leftScaleWidth, y + _multiHeight);
            points[2] = _resizer.Point(x + 0, y + 0);
            points[3] = _resizer.Point(x + _pageWidth, y + 0);

            Gfx.DrawLine(new XPen(RGBColor.GetXColor(borderColor), BorderWidth), points[0], points[1]);
            Gfx.DrawLine(new XPen(RGBColor.GetXColor(borderColor), BorderWidth), points[2], points[3]);

            _headerHeight += _multiHeight;
        }

        private double GetMultidayHeight(PDFPage page)
        {
            var cols = Parser.WeekColsParsing(page);
            var events = page.Multiday;
            var scheme = new int[cols[0].Length];
            for (var i = 0; i < scheme.Length; i++)
                scheme[i] = 0;
            for (var i = 0; i < events.Count; i++)
            {
                var day = events[i].Day;
                var len = events[i].Len;
                for (var j = day; j < day + len; j++)
                    scheme[j]++;
            }
            var max = scheme[0];
            for (var i = 1; i < scheme.Length; i++)
            {
                if (scheme[i] > max)
                    max = scheme[i];
            }
            return max * _multidayLineHeight;
        }

        private void WeekContainerDraw()
        {
            var bgColor = RGBColor.GetColor(_bgColor);
            var borderColor = RGBColor.GetColor(_lineColor);
            var headerLineColor = RGBColor.GetColor(_headerLineColor);
            var scaleOneColor = RGBColor.GetColor(_scaleOneColor);
            var scaleTwoColor = RGBColor.GetColor(_scaleTwoColor);
            var rows = Parser.WeekRowsParsing(CurrentPage);
            var width = _leftScaleWidth;
            var height = (_pageHeight - _headerHeight) / rows.Length;
            _weekDayHeight = height;
            var x = _offsetLeft;
            var y = _offsetTop + _headerHeight;
            for (var i = 0; i < rows.Length; i++)
            {

                var cell = _resizer.Rect(x, y, width, height);
                var cellBrush = new XSolidBrush(RGBColor.GetXColor(bgColor));

                var scaleOne = _resizer.Rect(x + width, y, _pageWidth - width, height / 2);
                var scaleOneBrush = new XSolidBrush(RGBColor.GetXColor(scaleOneColor));

                var scaleTwo = _resizer.Rect(x + width, y + height / 2, _pageWidth - width, height / 2);
                var scaleTwoBrush = new XSolidBrush(RGBColor.GetXColor(scaleTwoColor));

                Gfx.DrawRectangle(cellBrush, cell);
                Gfx.DrawRectangle(scaleOneBrush, scaleOne);
                Gfx.DrawRectangle(scaleTwoBrush, scaleTwo);

                Gfx.DrawLine(new XPen(RGBColor.GetXColor(headerLineColor), BorderWidth), _resizer.Point(x, y), _resizer.Point(x + width, y));

                var text = TextWrap(rows[i], width - 2 * _cellOffset, F1, TrimLayoutStrings);
                var textX = x + (width - Gfx.MeasureString(text, F1).Width) / 2;
                var textY = y + (height + F1.Size) / 2;

                Gfx.DrawString(text, F1, new XSolidBrush(RGBColor.GetXColor(_textColor)), _resizer.Point(textX, textY));

                y += height;
            }

            var cols = Parser.WeekColsParsing(CurrentPage);
            width = (_pageWidth - _leftScaleWidth) / cols[0].Length;
            x = _offsetLeft + _leftScaleWidth;
            y = _offsetTop + _headerHeight;
            for (var scale = 0; scale < cols.Length; scale++)
            {
                for (var col = 0; col < cols[scale].Length; col++)
                {
                    var points = new XPoint[2];
                    points[0] = _resizer.Point(x, y);
                    points[1] = _resizer.Point(x, _pageHeight + _offsetTop);

                    Gfx.DrawLine(new XPen(RGBColor.GetXColor(borderColor), BorderWidth), points[0], points[1]);

                    x += width;
                }
                y += _headerHeight;
            }

            var points2 = new XPoint[2];
            points2[0] = _resizer.Point(x, y - _multiHeight);
            points2[1] = _resizer.Point(x, _offsetTop + _pageHeight);

            Gfx.DrawLine(new XPen(RGBColor.GetXColor(borderColor), BorderWidth), points2[0], points2[1]);
        }

        private void WeekEventsDraw(PDFPage page)
        {
            var eventBorderColor = RGBColor.GetColor(_eventBorderColor);
            var eventColor = RGBColor.GetColor(_eventColor);

            var textColor = RGBColor.GetColor(_eventTextColor);
            var textFormatter = new DHXTextFormatter(Gfx);
            var events = page.Events;
            var defAlign = textFormatter.Alignment;

            for (var i = 0; i < events.Count; i++)
            {
                var x = _offsetLeft + _leftScaleWidth + events[i].X
                        * _weekDayWidth / 100;
                var y = _offsetTop + _headerHeight + events[i].Y
                        * _weekDayHeight / 100;
                var width = events[i].Width * _weekDayWidth / 100;
                var height = events[i].Height * _weekDayHeight / 100;
                var text = events[i].Text;
                var headerText = events[i].HeaderText;
                var bgColor = events[i].BackgroundColor;
                var color = events[i].Color;


                var borders = new XPoint[4];
                borders[0] = _resizer.Point(x, y);
                borders[1] = _resizer.Point(x + width, y);
                borders[2] = _resizer.Point(x + width, y + height);
                borders[3] = _resizer.Point(x, y + height);


                var eventBg = _resizer.Rect(x, y, width, height);
                var evBrush = new XSolidBrush(RGBColor.GetXColor(SelectColor(bgColor, eventColor)));

                F1 = new XFont(F1.FontFamily.Name, BaseFontSize);

                var textY = y + (_weekEventHeaderHeight - F1.Size) / 2;
                var headTxtBrush = new XSolidBrush(RGBColor.GetXColor(SelectColor(color, textColor)));

                var bodyTxt = text;
                var bodyTextX = x;
                var bodyTextWidth = width;
                if (width > 3 * _cellOffset)
                {
                    bodyTextX = x + _cellOffset;
                    bodyTextWidth = width - 2 * _cellOffset;
                }
                var bodyTextY = y;
                var bodyTextHeight = height;
                if (height > 3 * _cellOffset)
                {
                    bodyTextY = y + _cellOffset + _weekEventHeaderHeight;
                    bodyTextHeight = height - 2 * _cellOffset;
                }



                var bodyTxtBrush = new XSolidBrush(RGBColor.GetXColor(SelectColor(color, textColor)));

                Gfx.DrawRectangle(evBrush, eventBg);
                var borderPen = new XPen(RGBColor.GetXColor(eventBorderColor), BorderWidth);
                Gfx.DrawLine(borderPen, borders[0], borders[1]);
                Gfx.DrawLine(borderPen, borders[1], borders[2]);
                Gfx.DrawLine(borderPen, borders[2], borders[3]);
                Gfx.DrawLine(borderPen, borders[3], borders[0]);

                Gfx.DrawLine(borderPen, _resizer.Point(x, y + _weekEventHeaderHeight), _resizer.Point(x + width, y + _weekEventHeaderHeight));

                textFormatter.Alignment = XParagraphAlignment.Center;
                textFormatter.DrawString(headerText, F1, headTxtBrush, _resizer.Rect(x, textY, width, _weekEventHeaderHeight));

                textFormatter.Alignment = defAlign;

                textFormatter.DrawString(bodyTxt, F1, bodyTxtBrush, _resizer.Rect(bodyTextX, bodyTextY, bodyTextWidth, bodyTextHeight));

            }

            // preparing scheme to calculate multiday position
            var cols = Parser.WeekColsParsing(CurrentPage);
            var scheme = new int[cols[0].Length];
            for (var i = 0; i < scheme.Length; i++)
                scheme[i] = 0;

            events = page.Multiday;
            var offset = 1;
            for (var i = 0; i < events.Count; i++)
            {
                var ev = events[i];
                var day = ev.Day;
                var len = ev.Len;
                var text = ev.Text;
                var bgColor = ev.BackgroundColor;
                var color = ev.Color;

                var width = len * _weekDayWidth - 2 * offset;
                var height = _monthEventHeight;
                var x = _offsetLeft + _leftScaleWidth + day
                        * _weekDayWidth + offset;
                var y = _offsetTop + _headHeight + scheme[day]
                        * _multidayLineHeight;

                var cont = _resizer.Rect(x, y, width, height);
                var evBrush = new XSolidBrush(RGBColor.GetXColor(SelectColor(bgColor, eventColor)));

                F1 = new XFont(F1.FontFamily.Name, BaseFontSize);
                var txtX = x + _cellOffset;
                var txtY = y + (_weekEventHeaderHeight - F1.Size) / 2;
                var textBrush = new XSolidBrush(RGBColor.GetXColor(SelectColor(color, textColor)));

                Gfx.DrawRectangle(evBrush, cont);
                var borders = new XPoint[4];
                borders[0] = _resizer.Point(x, y);
                borders[1] = _resizer.Point(x + width, y);
                borders[2] = _resizer.Point(x + width, y + height);
                borders[3] = _resizer.Point(x, y + height);
                Gfx.DrawLine(new XPen(RGBColor.GetXColor(eventBorderColor), BorderWidth), borders[0], borders[1]);
                Gfx.DrawLine(new XPen(RGBColor.GetXColor(eventBorderColor), BorderWidth), borders[1], borders[2]);
                Gfx.DrawLine(new XPen(RGBColor.GetXColor(eventBorderColor), BorderWidth), borders[2], borders[3]);
                Gfx.DrawLine(new XPen(RGBColor.GetXColor(eventBorderColor), BorderWidth), borders[3], borders[0]);
                textFormatter.DrawString(text, F1, textBrush, _resizer.Rect(txtX, txtY, width, height));
                for (var j = day; j < day + len; j++)
                    scheme[j]++;
            }
        }

        private void WeekBwBordersDraw()
        {
            var borderColor = RGBColor.GetColor(_lineColor);

            var x = _offsetLeft;
            var y = _offsetTop;
            Line(borderColor, x + _leftScaleWidth, y, x + _pageWidth,
                    y);

            if (_multiHeight > 0)
            {
                y = _offsetTop + _headerHeight - _multiHeight;
                Line(borderColor, x + _leftScaleWidth, y, x
                        + _pageWidth, y);
                Line(borderColor, x, y, x, y + _pageHeight
                        - (_headerHeight - _multiHeight));
            }

            y = _offsetTop + _headerHeight;
            Line(borderColor, x, y, x + _pageWidth, y);

            y = _offsetTop + _pageHeight;
            Line(borderColor, x, y, x + _pageWidth, y);

            x = _offsetLeft + _leftScaleWidth;
            Line(borderColor, x, y, x, y + _headerHeight);

            x = _offsetLeft + _pageWidth;
            Line(borderColor, x, y, x, y + _headerHeight);

        }

        private void Line(double[] color, double x1, double y1, double x2, double y2)
        {
            Gfx.DrawLine(new XPen(RGBColor.GetXColor(color), BorderWidth), _resizer.Point(x1, y1), _resizer.Point(x2, y2));
        }
        private void Line(double[] color, double x1, double y1, double x2, double y2, XRect cell)
        {
            Gfx.DrawLine(new XPen(RGBColor.GetXColor(color), BorderWidth), _resizer.Point(cell.Location.X + x1, cell.Location.Y + y1), _resizer.Point(cell.Location.X + x2, cell.Location.Y + y2));
        }

        private void _drawTimelineContainerPart(string[] rows)
        {
            var headerLineColor = RGBColor.GetColor(_headerLineColor);
            var x = _offsetLeft;
            var y = _offsetTop + _headerHeight;
            var width = _leftScaleWidth;
            var height = TimelineRowMinHeight;
            for (var i = 0; i < rows.Length; i++)
            {
                var cell = _resizer.Rect(x, y, width, height);
                var cellBrush = new XSolidBrush(RGBColor.GetXColor(_bgColor));

                var scaleOne = _resizer.Rect(x + width, y, _pageWidth - width, height);
                var scaleOneBrush = new XSolidBrush(RGBColor.GetXColor(_scaleOneColor));

                Gfx.DrawRectangle(cellBrush, cell);
                Gfx.DrawRectangle(scaleOneBrush, scaleOne);

                Line(headerLineColor, x, y, x + width, y);

                var text = TextWrap(rows[i], width - 2 * _cellOffset, F1, TrimLayoutStrings);
                var textX = x + (width - Gfx.MeasureString(text, F1).Width) / 2;
                var textY = y + (height + F1.Size) / 2;
                Gfx.DrawString(text, F1, new XSolidBrush(RGBColor.GetXColor(_textColor)), _resizer.Point(textX, textY));

                y += height;
            }
        }


        private double _getPadding(TreeTimelineCategory row)
        {
            double levelPadding = 0.0;
            for (var lev = 0; lev < row.Level; lev++)
            {
                levelPadding += Gfx.MeasureString("ww", F1).Width;
            }
            if (row.IsExpandable)
                levelPadding -= Gfx.MeasureString("+ ", F1).Width;
            if (levelPadding < 0)
                levelPadding = 0.0;
            return levelPadding;
        }
        private void _drawTreeLevelScales(string[] rows)
        {
            var x = _offsetLeft;
            var y = _offsetTop + _headerHeight;
            var width = _leftScaleWidth;
            var height = (_pageHeight - _headerHeight) / rows.Length;
            var pars = new TreeTimelineParser();
            F1 = new XFont(F1.FontFamily.Name, F1.Size, XFontStyle.Bold);
            for (var i = 0; i < rows.Length; i++)
            {

                var row = pars.Parse(rows[i]);
                if (row.IsExpandable)
                {
                    var levelPadding = _getPadding(row);
                    var scaleOne = _resizer.Rect(x + width, y, _pageWidth - width, height);
                    var scaleOneBrush = new XSolidBrush(RGBColor.GetXColor(_timelineTreeColor));


                    Gfx.DrawRectangle(scaleOneBrush, scaleOne);


                    var textX = levelPadding + x;
                    var textY = y + (height + F1.Size) / 2;
                    var text = (row.Expanded ? " - " : " + ") + row.Text;
                    text = TextWrap(text, _pageWidth, F1, TrimLayoutStrings);
                    Gfx.DrawString(text, F1, new XSolidBrush(RGBColor.GetXColor(_textColor)), _resizer.Point(textX, textY));
                }
                y += height;
            }
            F1 = new XFont(F1.FontFamily.Name, F1.Size, XFontStyle.Regular);
        }
        private void _drawTreeTimelineContainerPart(string[] rows)
        {
            var headerLineColor = RGBColor.GetColor(_headerLineColor);
            var y = _offsetTop + _headerHeight;
            var width = _leftScaleWidth;
            var height = (_pageHeight - _headerHeight) / rows.Length;

            var pars = new TreeTimelineParser();

            for (var i = 0; i < rows.Length; i++)
            {
                var x = _offsetLeft;
                var row = pars.Parse(rows[i]);

                var levelPadding = _getPadding(row);

                XRect cell;
                XBrush cellBrush;

                if (row.IsExpandable)
                {
                    F1 = new XFont(F1.FontFamily.Name, F1.Size, XFontStyle.Bold);
                    cell = _resizer.Rect(x, y, width, height);
                    cellBrush = new XSolidBrush(RGBColor.GetXColor(_timelineTreeColor));
                    Gfx.DrawRectangle(cellBrush, cell);
                }
                else
                {
                    F1 = new XFont(F1.FontFamily.Name, F1.Size, XFontStyle.Regular);

                    cell = _resizer.Rect(x, y, width, height);
                    cellBrush = new XSolidBrush(RGBColor.GetXColor(_bgColor));

                    var scaleOne = _resizer.Rect(x + width, y, _pageWidth - width, height);
                    var scaleOneBrush = new XSolidBrush(RGBColor.GetXColor(_scaleOneColor));

                    var text = TextWrap(row.Text, width - 2 * _cellOffset - levelPadding, F1, TrimLayoutStrings);
                    var textX = levelPadding + x;
                    var textY = y + (height + F1.Size) / 2;
                    Gfx.DrawRectangle(cellBrush, cell);
                    Gfx.DrawRectangle(scaleOneBrush, scaleOne);
                    Gfx.DrawString(text, F1, new XSolidBrush(RGBColor.GetXColor(_textColor)), _resizer.Point(textX, textY));
                }

                Line(headerLineColor, x, y, x + width, y);

                y += height;
            }

        }

        private void TimelineContainerDraw(string[] rows)
        {
            var borderColor = RGBColor.GetColor(_lineColor);

            var isTree = false;
            if (rows.Length > 0)
            {
                isTree = TreeTimelineParser.IsTreeRow(rows[0]);
            }

            if (isTree)
            {
                _drawTreeTimelineContainerPart(rows);
            }
            else
            {
                _drawTimelineContainerPart(rows);
            }

            var cols = Parser.WeekColsParsing(CurrentPage);
            var width = double.MaxValue;
            var height = TimelineRowMinHeight;
            _weekDayHeight = height;
            var colCount = -1;
            for (var i = 0; i < cols.Length; i++)
            {
                var scale = (_pageWidth - _leftScaleWidth) / cols[i].Length;
                if (width > scale)
                    width = scale;
                if (colCount < cols[i].Length)
                    colCount = cols[i].Length;
            }

            var x = _offsetLeft + _leftScaleWidth;
            var y = _offsetTop + _headerHeight;

            var colHeight = height * rows.Length + _offsetTop + _headerHeight;
            for (var i = 0; i < colCount; i++)
            {
                Gfx.DrawLine(new XPen(RGBColor.GetXColor(borderColor), BorderWidth),
                    _resizer.Point(x, y), _resizer.Point(x, colHeight));

                x += width;
            }

            if (isTree)
                _drawTreeLevelScales(rows);

            Gfx.DrawLine(new XPen(RGBColor.GetXColor(borderColor), BorderWidth),
                _resizer.Point(_offsetLeft + _leftScaleWidth, y), _resizer.Point(_offsetLeft + _leftScaleWidth + width, y));

            y = _offsetTop + _headerHeight + height;
            for (var i = 0; i < rows.Length; i++)
            {
                Gfx.DrawLine(new XPen(RGBColor.GetXColor(borderColor), BorderWidth),
                    _resizer.Point(_offsetLeft + _leftScaleWidth, y), _resizer.Point(_offsetLeft + _pageWidth, y));
                y += height;
            }

            Gfx.DrawLine(new XPen(RGBColor.GetXColor(borderColor), BorderWidth),
                _resizer.Point(_offsetLeft + _leftScaleWidth, _offsetTop + _headerHeight),
                _resizer.Point(_offsetLeft + _pageWidth, _offsetTop + _headerHeight));

            Gfx.DrawLine(new XPen(RGBColor.GetXColor(borderColor), BorderWidth),
                _resizer.Point(x, _offsetTop),
                _resizer.Point(x, colHeight));
        }

        private void MatrixContainerDraw(PDFPage page)
        {
            var bgColor = RGBColor.GetColor(_bgColor);
            var borderColor = RGBColor.GetColor(_lineColor);
            var headerLineColor = RGBColor.GetColor(_headerLineColor);
            var matrixEventColor = RGBColor.GetColor(_matrixEventColor);
            var textColor = RGBColor.GetColor(_textColor);
            var rows = Parser.WeekRowsParsing(CurrentPage);
            var height = (_pageHeight - _headerHeight) / rows.Length;
            var cols = Parser.WeekColsParsing(CurrentPage);
            var events = page.Events;
            var columns = cols[0];
            for (var i = 0; i < rows.Length; i++)
            {
                for (var j = 0; j <= columns.Length; j++)
                {
                    var ev = events[i * (columns.Length + 1) + j];
                    var evBgColor = ev.BackgroundColor;
                    var evTextColor = ev.Color;
                    var text = ev.Text;
                    var x = _offsetLeft + Math.Max(j - 1, 0)
                            * _weekDayWidth + (j != 0 ? 1 : 0)
                            * _leftScaleWidth;
                    var y = _offsetTop + _headerHeight + i * height;

                    var width = (j == 0) ? _leftScaleWidth : _weekDayWidth;


                    var cell = _resizer.Rect(x, y, width, height);
                    var cellBrush = new XSolidBrush(RGBColor.GetXColor(SelectColor(evBgColor, matrixEventColor)));



                    if (j == 0)
                        cellBrush = new XSolidBrush(RGBColor.GetXColor(bgColor));
                    Gfx.DrawRectangle(cellBrush, cell);


                    // draw cell text
                    F1 = new XFont(F1.FontFamily.Name, (j == 0) ? BaseFontSize : 8.4);
                    var txt = TextWrap(text, width - 2 * _cellOffset, F1, TrimContentStrings);
                    var textX = x + (width - Gfx.MeasureString(txt, F1).Width) / 2;
                    var textY = y + (height - F1.Size) / 2;
                    var txtBrush = new XSolidBrush(RGBColor.GetXColor(SelectColor(evTextColor, textColor)));

                    Gfx.DrawString(txt, F1, txtBrush, _resizer.Point(textX, textY));

                    // draw borders
                    if (j == 0)
                    {
                        Line(headerLineColor, x, y, x + width, y);
                    }
                    else
                    {
                        Line(borderColor, x, y, x + width, y);
                        Line(borderColor, x, y, x, y + height);
                    }
                }
            }

            // border right
            Line(borderColor, _offsetLeft + _pageWidth, _offsetTop, _offsetLeft + _pageWidth, _offsetTop
                    + _pageHeight);

            Line(borderColor, _offsetLeft, _offsetTop + _pageHeight, _offsetLeft + _pageWidth, _offsetTop
                    + _pageHeight);
        }

        private void TimelineEventsDraw(PDFPage page, int pageNumber, int blocksNumber)
        {
            var eventBorderColor = RGBColor.GetColor(_eventBorderColor);
            var eventColor = RGBColor.GetColor(_eventColor);
            var textColor = RGBColor.GetColor(_eventTextColor);
            var events = page.Events;

            var offset = 1;
            for (var i = 0; i < events.Count; i++)
            {
                var ev = events[i];
                var text = ev.Text;
                var bgColor = ev.BackgroundColor;
                var color = ev.Color;

                if (ev.Week >= pageNumber * blocksNumber) continue;
                if (ev.Week < (pageNumber - 1) * blocksNumber) continue;

                var x = _offsetLeft + _leftScaleWidth + ev.X
                        * _weekDayWidth / 100 + offset;
                var y = _offsetTop + _headerHeight + ev.Y
                        * _weekDayHeight / 100 + ((ev.Week - (pageNumber - 1) * blocksNumber)
                        * _weekDayHeight);

                var width = ev.Width * _weekDayWidth / 100;
                var height = _monthEventHeight;

                var cell = _resizer.Rect(x, y, width, height);

                var cellBrush = new XSolidBrush(RGBColor.GetXColor(SelectColor(bgColor, eventColor)));

                F1 = new XFont(F1.FontFamily.Name, BaseFontSize);
                var txt = TextWrap(text, width, F1, TrimContentStrings);
                var txtX = x + _cellOffset;
                var txtY = y + (_weekEventHeaderHeight + F1.Size) / 2;
                var txtBrush = new XSolidBrush(RGBColor.GetXColor(SelectColor(color, textColor)));


                Gfx.DrawRectangle(cellBrush, cell);

                var borders = new XPoint[4];
                borders[0] = _resizer.Point(x, y);
                borders[1] = _resizer.Point(x + width, y);
                borders[2] = _resizer.Point(x + width, y + height);
                borders[3] = _resizer.Point(x, y + height);
                Gfx.DrawLine(new XPen(RGBColor.GetXColor(eventBorderColor), BorderWidth), borders[0], borders[1]);
                Gfx.DrawLine(new XPen(RGBColor.GetXColor(eventBorderColor), BorderWidth), borders[1], borders[2]);
                Gfx.DrawLine(new XPen(RGBColor.GetXColor(eventBorderColor), BorderWidth), borders[2], borders[3]);
                Gfx.DrawLine(new XPen(RGBColor.GetXColor(eventBorderColor), BorderWidth), borders[3], borders[0]);

                Gfx.DrawString(txt, F1, txtBrush, _resizer.Point(txtX, txtY));
            }
        }

        private void YearDraw(PDFPage page)
        {
            var bgColor = RGBColor.GetColor(_bgColor);
            var borderColor = RGBColor.GetColor(_lineColor);
            var headerLineColor = RGBColor.GetColor(_headerLineColor);
            var textColor = RGBColor.GetColor(_textColor);
            var eventColor = RGBColor.GetColor(_eventColor);
            var yearDayActiveColor = RGBColor.GetColor(_yearDayActiveColor);
            var yearDayInactiveColor = RGBColor.GetColor(_yearDayInactiveColor);
            var monthes = Parser.YearParsing(CurrentPage);
            var events = page.Events;
            var width = (_pageWidth - _yearMonthOffsetLeft * 3) / 4;
            var height = (_pageHeight - _yearMonthOffsetTop * 2) / 3;
            var monthX = _offsetLeft;
            var monthY = _offsetTop;
            for (var i = 0; i < 3; i++)
            {
                for (var j = 0; j < 4; j++)
                {
                    var mon = monthes[i * 4 + j];
                    var label = mon.GetLabel();
                    var rows = mon.GetRows();
                    var onlyDays = mon.GetOnlyDays();

                    var monthCont = _resizer.Rect(monthX, monthY, width, height);
                    var monthContBrush = new XSolidBrush(RGBColor.GetXColor(bgColor));


                    var labelText = TextWrap(label, width, F1, TrimLayoutStrings);
                    var labelTextX = monthX + (width - Gfx.MeasureString(label, F1).Width) / 2;
                    var labelTextY = monthY + (_yearMonthLabelHeight + F1.Size) / 2;
                    var labelTextBrush = new XSolidBrush(RGBColor.GetXColor(textColor));

                    Gfx.DrawRectangle(monthContBrush, monthCont);
                    Gfx.DrawString(labelText, F1, labelTextBrush, _resizer.Point(labelTextX, labelTextY));


                    var cellWidth = width / 7;
                    var cellHeight = (height - _yearMonthLabelHeight)
                            / rows.GetLength(0);
                    var cellX = monthX;
                    var cellY = monthY + _yearMonthLabelHeight;
                    for (var k = 0; k < rows.GetLength(0); k++)
                    {
                        for (var l = 0; l < 7; l++)
                        {

                            var cell = _resizer.Rect(cellX, cellY, cellWidth, cellHeight);
                            XBrush cellBrush;

                            var ind = GetEventIndex(events, l, k - 1, i * 4 + j);
                            if (k == 0)
                            {
                                cellBrush = new XSolidBrush(RGBColor.GetXColor(bgColor));
                            }
                            else
                            {
                                if (GetActiveDay(onlyDays, k - 1, l))
                                {
                                    if (ind == -1)
                                    {
                                        cellBrush = new XSolidBrush(RGBColor.GetXColor(yearDayActiveColor));
                                    }
                                    else
                                    {
                                        var ev = events[ind];
                                        var color = ev.BackgroundColor;
                                        cellBrush = new XSolidBrush(RGBColor.GetXColor(SelectColor(color, eventColor)));

                                    }
                                }
                                else
                                {
                                    cellBrush = new XSolidBrush(RGBColor.GetXColor(yearDayInactiveColor));

                                }
                            }
                            Gfx.DrawRectangle(cellBrush, cell);

                            if (k == 0)
                                Line(headerLineColor, 0, 0, cellWidth, 0,
                                        cell);
                            else
                                Line(borderColor, 0, 0, cellWidth, 0, cell);
                            if (l > 0)
                                if (k == 0)
                                    Line(headerLineColor, 0, 0, 0,
                                            cellHeight, cell);
                                else
                                    Line(borderColor, 0, 0, 0, cellHeight,
                                            cell);

                            var cellText = rows[k, l];
                            var cellTextX = cellX + (cellWidth - Gfx.MeasureString(cellText, F1).Width) / 2;
                            var cellTextY = cellY + (cellHeight + F1.Size) / 2;
                            XSolidBrush evBrush;

                            if (ind > -1)
                            {
                                var ev = events[ind];
                                var color = ev.Color;
                                evBrush = new XSolidBrush(RGBColor.GetXColor(SelectColor(color, textColor)));

                            }
                            else
                                evBrush = new XSolidBrush(RGBColor.GetXColor(textColor));
                            Gfx.DrawString(cellText, F1, evBrush, _resizer.Point(cellTextX, cellTextY));
                            cellX += cellWidth;
                        }
                        cellX = monthX;
                        cellY += cellHeight;
                    }
                    if (_profile == ColorProfile.BW)
                    {
                        Line(headerLineColor, 0, 0, 0, height, monthCont);
                        Line(headerLineColor, 0, 0, width, 0, monthCont);
                        Line(headerLineColor, 0, height, width, height,
                                monthCont);
                        Line(headerLineColor, width, 0, width, height,
                                monthCont);
                    }
                    monthX += _yearMonthOffsetLeft + width;
                }
                monthX = _offsetLeft;
                monthY += _yearMonthOffsetTop + height;
            }
        }

        private void AgendaHeaderDraw()
        {
            var bgColor = RGBColor.GetColor(_bgColor);
            var borderColor = RGBColor.GetColor(_headerLineColor);
            var textColor = RGBColor.GetColor(_textColor);
            var cols = Parser.AgendaColsParsing(CurrentPage);

            var width = _pageWidth;
            var height = _headerHeight;
            var x = _offsetLeft;
            var y = _offsetTop;

            var headerBg = _resizer.Rect(x, y, width, height);

            var monthContBrush = new XSolidBrush(RGBColor.GetXColor(bgColor));
            Gfx.DrawRectangle(monthContBrush, headerBg);

            var dateWidth = _agendaColOneWidth;
            var nameWidth = width - _agendaColOneWidth;

            var sep = new[] { _resizer.Point(x + dateWidth, y), _resizer.Point(x + dateWidth, y + _headerHeight) };
            Gfx.DrawLine(new XPen(RGBColor.GetXColor(borderColor), 0.3), sep[0], sep[1]);


            F1 = new XFont(F1.FontFamily.Name, BaseFontSize);
            var dateText = cols[0];

            var textBrush = new XSolidBrush(RGBColor.GetXColor(textColor));
            var dateTextX = x + (dateWidth - Gfx.MeasureString(dateText, F1).Width) / 2;
            var dateTextY = y + (height + F1.Size) / 2;
            Gfx.DrawString(dateText, F1, textBrush, _resizer.Point(dateTextX, dateTextY));



            var nameText = cols[1];
            var nameTextX = x + dateWidth + (nameWidth - Gfx.MeasureString(nameText, F1).Width) / 2;
            var nameTextY = y + (height + F1.Size) / 2;
            Gfx.DrawString(nameText, F1, textBrush, _resizer.Point(nameTextX, nameTextY));


            var borders = new[]{
                _resizer.Point(x, y),
                _resizer.Point(x + width, y),
                _resizer.Point(x + width, y + height),
                _resizer.Point(x, y + height)};
            Gfx.DrawLine(new XPen(RGBColor.GetXColor(_lineColor), 0.3), borders[0], borders[1]);
            Gfx.DrawLine(new XPen(RGBColor.GetXColor(_lineColor), 0.3), borders[1], borders[2]);
            Gfx.DrawLine(new XPen(RGBColor.GetXColor(_lineColor), 0.3), borders[2], borders[3]);
            Gfx.DrawLine(new XPen(RGBColor.GetXColor(_lineColor), 0.3), borders[3], borders[0]);

        }

        private void AgendaEventDraw(PDFPage page)
        {
            var scaleOneColor = RGBColor.GetColor(_scaleOneColor);
            var scaleTwoColor = RGBColor.GetColor(_scaleTwoColor);
            var borderColor = RGBColor.GetColor(_lineColor);
            var textColor = RGBColor.GetColor(_textColor);
            var events = page.Events;
            var width = _pageWidth;
            var height = _headerHeight;
            var x = _offsetLeft;
            var y = _offsetTop + _headerHeight;

            var linePen = new XPen(RGBColor.GetXColor(_eventBorderColor), BorderWidth);

            F1 = new XFont(F1.FontFamily.Name, BaseFontSize);
            for (var i = 0; i < events.Count; i++)
            {
                if (y + height > _offsetTop + _pageHeight)
                {

                    Gfx.DrawLine(linePen, _resizer.Point(x, y), _resizer.Point(x + width, y));


                    Page = NewPage(PageOrientation.Portrait, PageSize.A4);

                    AgendaHeaderDraw();
                    y = _offsetTop + _headerHeight;
                }



                var headerCont = _resizer.Rect(x, y, width, height);
                XBrush headerBrush;
                if (i % 2 == 0)
                {
                    headerBrush = new XSolidBrush(RGBColor.GetXColor(scaleOneColor));
                }
                else
                {
                    headerBrush = new XSolidBrush(RGBColor.GetXColor(scaleTwoColor));
                }

                var dateWidth = _agendaColOneWidth;

                Gfx.DrawRectangle(headerBrush, headerCont);

                var borders = new[]{
                    _resizer.Point(x, y),
                    _resizer.Point(x + dateWidth, y),
                    _resizer.Point(x + dateWidth, y + height),
                    _resizer.Point(x, y + height)};
                Gfx.DrawLine(new XPen(RGBColor.GetXColor(borderColor), BorderWidth), borders[0], borders[1]);
                Gfx.DrawLine(new XPen(RGBColor.GetXColor(borderColor), BorderWidth), borders[1], borders[2]);
                Gfx.DrawLine(new XPen(RGBColor.GetXColor(borderColor), BorderWidth), borders[2], borders[3]);
                Gfx.DrawLine(new XPen(RGBColor.GetXColor(borderColor), BorderWidth), borders[3], borders[0]);

                var headerText = events[i].HeaderAgendaText;
                var text = events[i].Text;

                var textBrush = new XSolidBrush(RGBColor.GetXColor(textColor));

                var dateTextX = x + (dateWidth - Gfx.MeasureString(headerText, F1).Width) / 2;
                var dateTextY = y + (height + F1.Size) / 2;
                Gfx.DrawString(headerText, F1, textBrush, _resizer.Point(dateTextX, dateTextY));



                var nameTextX = x + dateWidth + _cellOffset;
                var nameTextY = y + (height + F1.Size) / 2;
                Gfx.DrawString(text, F1, textBrush, _resizer.Point(nameTextX, nameTextY));

                Gfx.DrawLine(new XPen(RGBColor.GetXColor(borderColor), BorderWidth), _resizer.Point(x, y), _resizer.Point(x, y + height));
                Gfx.DrawLine(new XPen(RGBColor.GetXColor(borderColor), BorderWidth), _resizer.Point(x, y), _resizer.Point(x + width, y));
                Gfx.DrawLine(new XPen(RGBColor.GetXColor(borderColor), BorderWidth), _resizer.Point(x + width, y), _resizer.Point(x + width, y + height));
                Gfx.DrawLine(new XPen(RGBColor.GetXColor(borderColor), BorderWidth), _resizer.Point(x + dateWidth, y), _resizer.Point(x + dateWidth, y + height));


                y += height;
            }

            Gfx.DrawLine(new XPen(RGBColor.GetXColor(borderColor), BorderWidth), _resizer.Point(x, y), _resizer.Point(x + width, y));


        }

        private void PrintFooter()
        {
            if (CurrentPage.Footer)
            {
                var im = Images.Get(FooterImgPath);
                if (im != null)
                {
                    Gfx.DrawImage(im, _resizer.Point(_offsetLeft, _pageHeight + _offsetTop - FooterImgHeight));
                    _pageHeight -= FooterImgHeight;
                    _offsetBottom += FooterImgHeight;
                }
            }
        }

        private void AgendaPagesDraw()
        {
            for (var i = 1; i <= Pages.Count; i++)
            {
                var graph = Graphics[i - 1];
                if (_profile == ColorProfile.BW
                        && CurrentPage.Mode != "month")
                    MonthBwBordersDraw();
                var str = _pageNumTemplate;
                str = str.Replace("{pageNum}", i.ToString());
                str = str.Replace("{allNum}", Pages.Count.ToString());

                var text = new XTextFormatter(graph);
                var x = _pageWidth + _offsetLeft - graph.MeasureString(str, F1).Width;

                var y = _pageHeight + _offsetTop + F1.Size;
                if (CurrentPage.Footer && Images.Get(FooterImgPath) != null)
                    y += FooterImgHeight;

                text.DrawString(str, F1, new XSolidBrush(RGBColor.GetXColor(_textColor)), _resizer.Rect(x, y, graph.MeasureString(str, F1).Width, F1.Size));
            }

        }


        private void WeekAgendaContainerDraw()
        {
            var cols = Parser.AgendaColsParsing(CurrentPage);
            var x = _offsetLeft;
            var contWidth = (_pageWidth) / 2;
            var contHeight = (_pageHeight) / 3;
            _contWidth = contWidth;
            _contHeight = contHeight;


            for (var i = 0; i < 3; i++)
            {
                var y = _offsetTop + contHeight * i;
                WeekAgendarDayDraw(cols[i], contWidth, contHeight, x, y);
            }
            x += contWidth;
            for (var i = 0; i < 2; i++)
            {
                var y = _offsetTop + contHeight * i;
                WeekAgendarDayDraw(cols[i + 3], contWidth, contHeight, x, y);
            }

            var tallContHeight = contHeight / 2;
            for (var i = 0; i < 2; i++)
            {
                var y = _offsetTop + contHeight * 2 + tallContHeight * i;
                WeekAgendarDayDraw(cols[i + 5], contWidth, tallContHeight, x, y);
            }
            var headerLineColor = RGBColor.GetColor(_headerLineColor);
            for (var i = 0; i < 3; i++)
            {
                var y = _offsetTop + contHeight * i;
                Line(headerLineColor, x, y, x, y + _monthDayHeaderHeight);
            }
        }

        private void WeekAgendarDayDraw(string name, double width, double height, double x, double y)
        {
            var bgColor = RGBColor.GetColor(_bgColor);
            var borderColor = RGBColor.GetColor(_lineColor);
            var textColor = RGBColor.GetColor(_textColor);
            F1 = new XFont(F1.FontFamily.Name, WeekAgendarDayFont);
            var dayCont = _resizer.Rect(x, y, width, height);

            var borders = new[]{
                _resizer.Point(x, y),
                _resizer.Point(x + width, y),
                _resizer.Point(x + width, y + height),
                _resizer.Point(x, y + height)};
            Gfx.DrawLine(new XPen(RGBColor.GetXColor(bgColor), BorderWidth), borders[0], borders[1]);
            Gfx.DrawLine(new XPen(RGBColor.GetXColor(bgColor), BorderWidth), borders[1], borders[2]);
            Gfx.DrawLine(new XPen(RGBColor.GetXColor(bgColor), BorderWidth), borders[2], borders[3]);
            Gfx.DrawLine(new XPen(RGBColor.GetXColor(bgColor), BorderWidth), borders[3], borders[0]);

            Line(borderColor, 0, 0, width, 0, dayCont);
            Line(borderColor, 0, 0, 0, height, dayCont);
            Line(borderColor, width, 0, width, height, dayCont);
            Line(borderColor, 0, height, width, height, dayCont);

            var labelCont = _resizer.Rect(x, y, width, _monthDayHeaderHeight);
            Gfx.DrawRectangle(new XSolidBrush(RGBColor.GetXColor(bgColor)), labelCont);

            var txt = name;
            x = x + _cellOffset + (width - _cellOffset - Gfx.MeasureString(name, F1).Width) / 2;
            y = y + (_monthDayHeaderHeight + F1.Size) / 2;
            Gfx.DrawString(txt, F1, new XSolidBrush(RGBColor.GetXColor(textColor)), _resizer.Point(x, y));
        }

        private SchedulerEvent[] WeekAgendaEventsDraw(SchedulerEvent[] events)
        {
            var borderColor = RGBColor.GetColor(_lineColor);
            var textColor = RGBColor.GetColor(_textColor);
            var offsets = new int[7];
            for (var i = 0; i < offsets.Length; i++)
                offsets[i] = 0;
            var rest = new List<SchedulerEvent>();

            for (var i = 0; i < events.Length; i++)
            {
                var ev = events[i];
                var day = ev.Day;
                var contHeight = (day < 5) ? _contHeight : _contHeight / 2;
                var contWidth = _contWidth;
                double x;
                switch (day)
                {
                    case 0:
                    case 2:
                    case 4:
                        x = _offsetLeft;
                        break;
                    default:
                        x = _offsetLeft + contWidth;
                        break;
                }
                var contStartY = _offsetTop + Math.Floor(day / 2.0) * _contHeight - (day > 5 ? contHeight : 0);
                var offset = offsets[day] * _weekAgendaEventHeight;
                var y = contStartY + _monthDayHeaderHeight + offset;

                if (contStartY + contHeight < y + _weekAgendaEventHeight)
                {
                    rest.Add(ev);
                    continue;
                }


                var borders = new[]{
                    _resizer.Point(x, y),
                    _resizer.Point(x + _contWidth, y),
                    _resizer.Point(x + _contWidth, y + _weekAgendaEventHeight),
                    _resizer.Point(x, y + _weekAgendaEventHeight)};
                Gfx.DrawLine(new XPen(RGBColor.GetXColor(borderColor), BorderWidth), borders[0], borders[1]);
                Gfx.DrawLine(new XPen(RGBColor.GetXColor(borderColor), BorderWidth), borders[1], borders[2]);
                Gfx.DrawLine(new XPen(RGBColor.GetXColor(borderColor), BorderWidth), borders[2], borders[3]);
                Gfx.DrawLine(new XPen(RGBColor.GetXColor(borderColor), BorderWidth), borders[3], borders[0]);

                F1 = new XFont(F1.FontFamily.Name, 9);

                x = x + _cellOffset;
                y = y + (_weekAgendaEventHeight + F1.Size) / 2;
                Gfx.DrawString(ev.Text, F1, new XSolidBrush(RGBColor.GetXColor(textColor)), _resizer.Point(x, y));

                offsets[day]++;
            }
            var eventsList = new SchedulerEvent[rest.Count];
            for (var i = 0; i < rest.Count; i++)
                eventsList[i] = rest[i];
            return eventsList;
        }

        private void TodayLabelDraw()
        {
            var g1 = Graphics[0];
            TodayLabelDraw(g1);
        }

        private void TodayLabelDraw(XGraphics p1)
        {
            F1 = new XFont(F1.FontFamily.Name, 10);

            var today = CurrentPage.TodayLabel;
            var todayText = today;
            var todayX = _offsetLeft;
            var todayY = _offsetTop - _cellOffset;
            p1.DrawString(todayText, F1, new XSolidBrush(RGBColor.GetXColor(_textColor)), _resizer.Point(todayX, todayY));
        }

        private int GetEventIndex(IList<SchedulerEvent> events, int day, int week, int month)
        {
            for (var i = 0; i < events.Count; i++)
            {
                var evDay = events[i].Day;
                var evWeek = events[i].Week;
                var evMonth = events[i].Month;
                if ((evDay == day) && (evWeek == week) && (evMonth == month))
                {
                    return i;
                }
            }
            return -1;
        }

        private bool GetActiveDay(string[,] rows, int row, int col)
        {
            bool flag;
            var flagCount = 0;
            if (int.Parse(rows[0, 0]) == 1)
            {
                flag = true;
                flagCount = 1;
            }
            else
            {
                flag = false;
            }

            var prevDay = int.Parse(rows[0, 0]);
            for (var i = 0; i < rows.Length; i++)
            {
                for (var j = 0; j < rows.GetLength(1); j++)
                {
                    if (int.Parse(rows[i, j]) < prevDay && flagCount < 2)
                    {
                        flag = !flag;
                        flagCount++;
                    }
                    if (i == row && j == col)
                    {
                        return flag;
                    }
                    prevDay = int.Parse(rows[i, j]);
                }
            }
            return flag;
        }

        public string GetView()
        {
            return _view;
        }



        private void PrintHeader()
        {
            if (CurrentPage.Header)
            {
                var im = Images.Get(HeaderImgPath);
                if (im != null)
                {
                    Gfx.DrawImage(im, _resizer.Point(_offsetLeft, _offsetTop));
                    _pageHeight -= HeaderImgHeight;
                    _offsetTop += HeaderImgHeight;
                }
            }
        }

        public void SetWatermark(string mark)
        {
            _watermark = mark;
        }
    }
}