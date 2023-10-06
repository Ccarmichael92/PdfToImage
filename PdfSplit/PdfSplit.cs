using PdfiumViewer;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using PdfDocument = PdfSharp.Pdf.PdfDocument;

namespace DmsApp.Helper
{
    public static class PdfSplit
    {

        private static List<PdfDocument> SplitPdf(PdfDocument document)
        {
            List<PdfDocument> returnList = new List<PdfDocument>();

            var pageCount = document.PageCount;

            var index = 0;
            while (index < pageCount)
            {
                PdfDocument temp = new PdfDocument();

                temp.AddPage(document.Pages[index]);

                returnList.Add(temp);

                index++;
            }

            return returnList;
        }

        private static List<Bitmap> ImageSplitPdf(PdfDocument document)
        {
            List<Bitmap> returnList = new List<Bitmap>();

            var pageCount = document.PageCount;

            var index = 0;
            while (index < pageCount)
            {
                PdfDocument temp = new PdfDocument();

                temp.AddPage(document.Pages[index]);

                Size size = new Size();
                size.Width = (int)temp.Pages[0].Width;
                size.Height = (int)temp.Pages[0].Height;

                using (MemoryStream ms = new MemoryStream())
                {
                    temp.Save(ms);
                    Image image = RenderPage(ms, 1, size);
                    Bitmap bitmapImage = new Bitmap(image);
                    returnList.Add(bitmapImage);
                }

                //var image = RenderPage(temp, 1, size);

                index++;
            }

            return returnList;
        }

        private static List<string> SplitString(PdfDocument document)
        {
            List<string> returnList = new List<string>();

            var pageCount = document.PageCount;

            var index = 0;
            while (index < pageCount)
            {
                PdfDocument temp = new PdfDocument();

                temp.AddPage(document.Pages[index]);

                var newPath = Path.GetTempFileName();
                newPath = Path.ChangeExtension(newPath, "pdf");

                temp.Save(newPath);

                returnList.Add(newPath);

                index++;
            }

            return returnList;

        }

        public static List<Bitmap> Split(string document, SplitOption option)
        {
            PdfDocument doc = PdfReader.Open(document, PdfDocumentOpenMode.Import);

            if (option == SplitOption.PdfList)
            {
                //return SplitPdf(doc);
            }
            else if (option == SplitOption.PdfStringList)
            {
                //return SplitString(doc);
            }
            else if (option == SplitOption.ImageList)
            {
                return ImageSplitPdf(doc);
            }

            return null;
        }

        private static Image GetPageImage(int pageNumber, Size size, PdfiumViewer.PdfDocument document, int dpi)
        {
            return document.Render(pageNumber - 1, size.Width, size.Height, dpi, dpi, PdfRenderFlags.Annotations);
        }

        private static Image RenderPage(Stream pdfPath, int pageNumber, Size size)
        {
            using (var document = PdfiumViewer.PdfDocument.Load(pdfPath))
            {


                var image = GetPageImage(pageNumber, size, document, 150);
                {
                    return image;
                }
            }

        }




    }

    public enum SplitOption
    {
        PdfList,
        PdfStringList,
        ImageList,
        ImageStringList
    }
}