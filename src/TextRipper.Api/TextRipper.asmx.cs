using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Text;

namespace TextRipper.Api
{
    /// <summary>
    /// Summary description for TextRipper
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class TextRipper : System.Web.Services.WebService
    {

        [WebMethod]
        public string GetText(byte[] buffer, string contentType)
        {
            var fileName = WriteFile(buffer);
            var text = ConvertFile(fileName, contentType);
            text = SanitizeXmlString(text);
            return text;
        }

        /// <summary>
        /// Remove illegal XML characters from a string.
        /// </summary>
        public string SanitizeXmlString(string xml)
        {
            if (xml == null)
            {
                throw new ArgumentNullException("xml");
            }

            StringBuilder buffer = new StringBuilder(xml.Length);

            foreach (char c in xml)
            {
                if (IsLegalXmlChar(c))
                {
                    buffer.Append(c);
                }
            }

            return buffer.ToString();
        }

        /// <summary>
        /// Whether a given character is allowed by XML 1.0.
        /// </summary>
        public bool IsLegalXmlChar(int character)
        {
            return
            (
                 character == 0x9 /* == '\t' == 9   */          ||
                 character == 0xA /* == '\n' == 10  */          ||
                 character == 0xD /* == '\r' == 13  */          ||
                (character >= 0x20 && character <= 0xD7FF) ||
                (character >= 0xE000 && character <= 0xFFFD) ||
                (character >= 0x10000 && character <= 0x10FFFF)
            );
        }


        private string WriteFile(byte[] buffer)
        {
            var uploadPath = Server.MapPath(@"~/upload/");
            var newFileName = string.Format(@"{0}/{1}", uploadPath, Guid.NewGuid());
            System.IO.File.WriteAllBytes(newFileName, buffer);
            return newFileName;
        }

        private string ConvertFile(string fileName, string contentType)
        {
            string value = string.Empty;
            switch (contentType)
            {
                case @"application/pdf":
                    value = convertPdf(fileName);
                    break;
			case @"application/msword":
				value=convertDoc(fileName);
				break;
			default:
                    value = "Unable to convert file: " + contentType;
                    break;
			}
			System.IO.File.Delete(fileName);
            return value;
        }
		
		private string convertDoc(string fileName)
		{
			string outputPath = Server.MapPath(@"~/upload/" + System.IO.Path.GetFileNameWithoutExtension(fileName) + ".txt");
            string binPath = Server.MapPath(@"~/binaries/abiword/abiword");
            
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = binPath;
            proc.StartInfo.Arguments = string.Format("--to=txt {0} --to-name={1}", fileName, outputPath);
            proc.StartInfo.UseShellExecute=false;
            proc.Start();
            proc.WaitForExit();

            return System.IO.File.ReadAllText(outputPath);
		}
		
        private string convertPdf(string fileName)
        {
            string outputPath = Server.MapPath(@"~/upload/" + System.IO.Path.GetFileNameWithoutExtension(fileName) + ".txt");
            string binPath = Server.MapPath(@"~/binaries/xpdf-3.02pl6-linux/pdftotext");
            
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = binPath;
            proc.StartInfo.Arguments = string.Format("{0} {1} -q", fileName, outputPath);
            proc.StartInfo.UseShellExecute=false;
            proc.Start();
            proc.WaitForExit();

            return System.IO.File.ReadAllText(outputPath);
        }
    }
}
