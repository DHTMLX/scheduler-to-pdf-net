dhtmlxScheduler v4.x to PDF print tool for .NET
----------------------------------------------------------

This project can be used to print dhtmlxScheduler to PDF using legacy export extension 

Docs :

  - http://docs.dhtmlx.com/scheduler/pdf_v4.html

Demo:

  - http://docs.dhtmlx.com/scheduler/samples/04_export/05_standalone_export.html

### Disclaimer
This version of the export tool won't be actively developed. We encourage you to use the new version of the export tool instead: 

 - https://dhtmlx.com/docs/products/dhtmlxGantt/export.shtml
 - http://docs.dhtmlx.com/scheduler/pdf.html


### Usage

- Add DHTMLX.Export.PDF project to the solution. 
- Add reference to it from your web project and create a backend handler.
- Call the handler using client side extension of dhtmlxScheduler http://docs.dhtmlx.com/scheduler/pdf_v4.html#triggeringtheexport

### Sample backend implementation 

#### ASP.NET WebForms

~~~cs
//Generate.ashx

using System.Web;
using DHTMLX.Export.PDF;
using System.Web.Configuration;

public class Generate : IHttpHandler
{

    public void ProcessRequest(HttpContext context)
    {           
        PagesSection pageSection = new PagesSection();
        pageSection.ValidateRequest = false;

        var generator = new SchedulerPDFWriter();
        var xml = context.Server.UrlDecode(context.Request.Form["mycoolxmlbody"]);
        generator.Generate(xml, context.Response);
    }

    public bool IsReusable { get { return false; } }
}
~~~

#### ASP.NET MVC

~~~cs
using System.IO;
using System.Web;
using System.Web.Mvc;
using DHTMLX.Export.PDF;

namespace scheduler2pdf.Controllers
{
    [HandleError]
    public class GeneratorController : Controller
    {
        [ValidateInput(false)]
        public ActionResult Export()
        {
            var generator = new SchedulerPDFWriter();
            var xml = this.Server.UrlDecode(this.Request.Form["mycoolxmlbody"]);           
            MemoryStream pdf = generator.Generate(xml);         
            return File(pdf.ToArray(), generator.ContentType);            
        }
    }
}

~~~

### License

Distributed under the MIT software license  
Copyright (c) 2017 Dinamenta UAB