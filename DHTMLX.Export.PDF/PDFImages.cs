using System.Collections.Generic;
using System.IO;
using PdfSharp.Drawing;
using System.Drawing;

using DHTMLX.Export.PDF.Scheduler;
namespace DHTMLX.Export.PDF.Scheduler
{
    public class PDFImages
    {

        private Dictionary<string, XImage> _cache = new Dictionary<string, XImage>();
        public XImage Get(string path)
        {
            if (_cache.ContainsKey(path))
            {
                return _cache[path];
            }

            if (File.Exists(path))
            {
                FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
                var img = new Bitmap(fileStream, false);
                _cache.Add(path, (XImage)img);

                return _cache[path];
            }

            return null;
        }

    }
}
