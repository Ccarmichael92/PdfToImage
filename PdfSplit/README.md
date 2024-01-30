Simplify converting PDF pages into individual images with PdfToBitmapList.

Only one line is required:
```cs
var pageList = Pdf2Bmp.Split(PdfFilePath)
```
Where PdfFilePath is the path to the pdf in file system. Passing a stream or byte array is also supported.

Returns List<Bitmap>