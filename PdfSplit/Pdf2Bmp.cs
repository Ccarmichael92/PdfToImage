using PdfSharpCore.Pdf.IO;
using PdfSharpCore.Pdf.IO.enums;
using System.Drawing;
using System.Drawing.Imaging;
using PdfDocument = PdfSharpCore.Pdf.PdfDocument;

namespace PdfToBitmapList
{
    public static class Pdf2Bmp
    {
        private static int Dpi { get; set; }
        
        /// <summary>
        /// Split Pdf into a list of Bitmap images.
        /// </summary>
        /// <param name="document">Pdf file path</param>
        /// <param name="dpi">DPI of generated images</param>
        /// <returns></returns>
        public static List<Bitmap> Split(string document, int dpi = 300)
        {
            PdfDocument doc = PdfReader.Open(document, PdfDocumentOpenMode.Import, PdfReadAccuracy.Moderate);
            Dpi = dpi;
            if (doc != null)
            {
                //Get page count to account for bug in pdfsharpcore that shows null pdf when not previously accessed.
                var pageCount = doc.PageCount;
                //Get the list of images
                var result = Pdf2ImageList(doc);
                return result;
            }
            

            return null;
        }
        /// <summary>
        /// Split Pdf into a list of Bitmap images.
        /// </summary>
        /// <param name="document">PdfSharpCore Pdf document</param>
        /// <param name="dpi">DPI of generated images</param>
        /// <returns></returns>
        public static List<Bitmap> Split(PdfDocument document, int dpi = 300)
        {
            Dpi = dpi;
            if (document != null)
            {
                var result = Pdf2ImageList(document);
                return result;
            }


            return null;
        }

        /// <summary>
        /// Split Pdf into a list of Bitmap images.
        /// </summary>
        /// <param name="document">Byte array containing a pdf document</param>
        /// <param name="dpi">DPI of generated images</param>
        /// <returns></returns>
        public static List<Bitmap> Split(byte[] document, int dpi = 300)
        {
            Dpi = dpi;
            if (document != null)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    ms.Write(document);
                    PdfDocument doc = PdfReader.Open(ms, PdfDocumentOpenMode.Import, PdfReadAccuracy.Moderate);

                    if (doc != null)
                    {
                        var result = Pdf2ImageList(doc);
                        return result;
                    }
                }
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
                size.Width = (int)document.Pages[index].Width;
                size.Height = (int)document.Pages[index].Height;

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

        private static Image GetPageImage(int pageNumber, Size size, PdfDoc document, int dpi)
        {
            return document.Render(pageNumber - 1, size.Width, size.Height, dpi, dpi, PdfRenderFlags.Annotations|PdfRenderFlags.CorrectFromDpi);
        }

        private static Image RenderPage(Stream pdfPath, int pageNumber, Size size)
        {
            using (var document = PdfDoc.Load(pdfPath))
            {
                var image = GetPageImage(pageNumber, size, document, Dpi);
                {
                    return image;
                }
            }

        }




    }
}