using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using PdfSharp;
using TheArtOfDev;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.PdfSharp;

namespace GenderPayGap.WebUI.Classes.API
{
    public static class PDF
    {
        public static byte[] HtmlToPDF(string html)
        {
            using (var pdfDocument = PdfGenerator.GeneratePdf(html, PageSize.A4))
            {
                using (var stream = new MemoryStream())
                {
                    pdfDocument.Save(stream, true);
                    return stream.ToArray();
                }
            }
        }
    }
}