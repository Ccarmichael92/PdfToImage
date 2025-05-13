using System.Drawing;
using System.Drawing.Imaging;
using System.Net.NetworkInformation;
using Windows.Storage;

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
            Dpi = dpi;
            using (var fs = new FileStream(document, FileMode.Open, FileAccess.Read))
            {
                if (fs.Length > 0)
                {
                    using (var ms = new MemoryStream())
                    {
                        fs.CopyTo(ms);
                        ms.Position = 0;
                        return (List<Bitmap>)Pdf2ImageList(ms);
                    }
                } 
            }
            return null;
        }

        /// <summary>
        /// Split Pdf into a list of Bitmap images.
        /// </summary>
        /// <param name="document">Pdf file path</param>
        /// <param name="saveLocation">Location to save images</param>
        /// <param name="dpi">DPI of generated images</param>
        /// <returns></returns>
        public static List<string> Split(string document, string saveLocation, int dpi = 300)
        {
            Dpi = dpi;
            using (var fs = new FileStream(document, FileMode.Open, FileAccess.Read))
            {
                if (fs.Length > 0)
                {
                    using (var ms = new MemoryStream())
                    {
                        fs.CopyTo(ms);
                        ms.Position = 0;
                        return (List<string>)Pdf2ImageList(ms, saveLocation, true);
                    }
                }
            }
            return null;
        }


        /// <summary>
        /// Split Pdf into a list of Bitmap images.
        /// </summary>
        /// <param name="document">MemoryStream of Pdf Document</param>
        /// <param name="dpi">DPI of generated images</param>
        /// <returns></returns>
        public static List<Bitmap> Split(MemoryStream document, int dpi = 300)
        {
            Dpi = dpi;
            if (document.Length > 0)
            {
                var result = (List<Bitmap>)Pdf2ImageList(document);
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

                    if (ms.Length > 0)
                    {
                        var result = (List<Bitmap>)Pdf2ImageList(ms);
                        return result;
                    }
                }
            }

            return null;
        }

        private static object Pdf2ImageList(MemoryStream ms, string? saveLocation = null, bool saveToDisk = false)
        {
            List<Bitmap> returnList = new List<Bitmap>();
            List<string> imageString = new List<string>();

            
                PdfDoc pdf = PdfDoc.Load(ms);

                var index = 0;
                while (index < pdf.PageCount())
                {
                    var size = pdf.PageSizes[index];

                    Image image = GetPageImage(index, Size.Round(size), pdf, Dpi);
                    //save to disk if requested
                    if (saveToDisk)
                    {
                        //set save location to temp files if not passed in
                        if (saveLocation == null)
                        {
                            saveLocation = Path.GetTempPath();
                        }

                        var fileName = $"{Guid.NewGuid()}.png";
                        var localSaveLocation = Path.Combine(saveLocation!, fileName);
                        image.Save(localSaveLocation, ImageFormat.Png);
                        imageString.Add(localSaveLocation);
                    }
                    else
                    {
                        Bitmap bitmapImage = new Bitmap(image);
                        returnList.Add(bitmapImage);
                    }
                    image.Dispose();

                    index++;
                }

                if (saveToDisk)
                {
                    return imageString;
                }
                else
                {
                    return returnList;
                }
            
        }

        private static Image GetPageImage(int pageNumber, Size size, PdfDoc document, int dpi)
        {
            return document.Render(pageNumber, size.Width, size.Height, dpi, dpi, PdfRotation.Rotate0, PdfRenderFlags.Annotations | PdfRenderFlags.CorrectFromDpi | PdfRenderFlags.ForPrinting, true);
        }

    }
}