using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;

namespace TextRipper.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public FileContentResult Index(FormCollection collection)
        {
            var ripper = new TextRipper.Api.TextRipper(Server);

            string fileName = string.Empty;
            string contentType = string.Empty;
            byte[] buffer = { };
            string sansExtension = string.Empty;

            foreach (string inputTagName in Request.Files)
            {
                HttpPostedFileBase file = Request.Files[inputTagName];
                if (file.ContentLength > 0)
                {
                    fileName = System.IO.Path.GetFileNameWithoutExtension(file.FileName);
                    BinaryReader reader = new BinaryReader(file.InputStream);
                    buffer = reader.ReadBytes(file.ContentLength);
                    contentType = file.ContentType;
                    sansExtension = System.IO.Path.GetFileNameWithoutExtension(file.FileName);
                }
                break;
            }

            var text = ripper.GetText(buffer, contentType);
            
            return File(System.Text.Encoding.UTF8.GetBytes(text),
                 "text/plain",
                  string.Format("{0}.txt", sansExtension));
        }

    }
}
