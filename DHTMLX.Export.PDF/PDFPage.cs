using System.Collections.Generic;
using DHTMLX.Export.PDF.Scheduler;
using System.Xml;

namespace DHTMLX.Export.PDF
{
    public class PDFPage
    {
        public List<SchedulerEvent> Multiday { get; set; }
        public List<SchedulerEvent> Events { get; set; }
        public PDFPage()
        {
            Multiday = new List<SchedulerEvent>();
            Events = new List<SchedulerEvent>();
        }
        public ColorProfile Profile { get; set; }
        public bool Header { get; set; }
        public bool Footer { get; set; }
        public string Mode { get; set; }
        public string TodayLabel { get; set; }
        public XmlNode Node { get; set; }
        public string[][] Cols { get; set; }
        public string[,] Rows { get; set; }
        public List<MonthRow> RowObs { get; set; }

    }
}
