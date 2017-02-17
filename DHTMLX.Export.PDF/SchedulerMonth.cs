using System;
using System.Xml;

namespace DHTMLX.Export.PDF.Scheduler
{
    public class SchedulerMonth
    {
        private string _monthName;
        private string[,] _rows;

        public void Parse(XmlNode parent)
        {
            _monthName = parent.Attributes["label"].Value;
            var parsRows = XMLParser.GetElementsByTagName(parent, "row");
            var cols = XMLParser.GetElementsByTagName(parent, "column");

            if ((parsRows.Count != 0) && (cols.Count != 0))
            {
                _rows = new string[parsRows.Count + 1, 7];
                for (var i = 0; i < cols.Count; i++)
                {
                    _rows[0, i] = cols[i].FirstChild.Value;
                }
                for (var i = 1; i <= parsRows.Count; i++)
                {
                    var values = parsRows[i - 1].FirstChild.Value.Split(new[] { "|" }, StringSplitOptions.None);
                    for (var j = 0; j < values.Length; j++)
                    {
                        _rows[i, j] = values[j];
                    }
                }
            }
        }

        public string GetLabel()
        {
            return _monthName;
        }

        public string[,] GetRows()
        {
            return _rows;
        }

        public string[,] GetOnlyDays()
        {
            var days = new string[_rows.GetLength(0) - 1, 7];


            for (var i = 1; i < _rows.GetLength(0); i++)
            {
                for (var j = 0; j < 7; j++)
                {
                    days[i - 1, j] = _rows[i, j];
                }
            }
            return days;
        }
    }
}