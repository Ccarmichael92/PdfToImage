using PdfiumViewer;
using PdfSharp.Pdf.IO;
using System.Drawing;
using System.Drawing.Imaging;
using PdfDocument = PdfSharp.Pdf.PdfDocument;

namespace DmsApp.Helper
{
    public static class PdfSplit
    {
        public static List<Bitmap> Split(string document, ImageFormat format)
        {
            PdfDocument doc = PdfReader.Open(document, PdfDocumentOpenMode.Import);
            if(doc != null)
            {
                var result = Pdf2ImageList(doc);
                return result;
            }
            

            return null;
        }

        private static List<Bitmap> Pdf2ImageList(PdfDocument document)
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

                index++;
            }

            return returnList;
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
}