using System;
using System.Collections.Generic;
using System.Text;

namespace PdfToBitmapList
{
    /// <summary>
    /// Flags that influence the page rendering process.
    /// </summary>
    [Flags]
    public enum PdfRenderFlags
    {
        /// <summary>
        /// No flags.
        /// </summary>
        None = 0,
        /// <summary>
        /// Render for printing.
        /// </summary>
        ForPrinting = PdfiumNative.FPDF.PRINTING,
        /// <summary>
        /// Set if annotations are to be rendered.
        /// </summary>
        Annotations = PdfiumNative.FPDF.ANNOT,
        /// <summary>
        /// Set if using text rendering optimized for LCD display.
        /// </summary>
        LcdText = PdfiumNative.FPDF.LCD_TEXT,
        /// <summary>
        /// Don't use the native text output available on some platforms.
        /// </summary>
        NoNativeText = PdfiumNative.FPDF.NO_NATIVETEXT,
        /// <summary>
        /// Grayscale output.
        /// </summary>
        Grayscale = PdfiumNative.FPDF.GRAYSCALE,
        /// <summary>
        /// Limit image cache size.
        /// </summary>
        LimitImageCacheSize = PdfiumNative.FPDF.RENDER_LIMITEDIMAGECACHE,
        /// <summary>
        /// Always use halftone for image stretching.
        /// </summary>
        ForceHalftone = PdfiumNative.FPDF.RENDER_FORCEHALFTONE,
        /// <summary>
        /// Render with a transparent background.
        /// </summary>
        Transparent = 0x1000,
        /// <summary>
        /// Correct height/width for DPI.
        /// </summary>
        CorrectFromDpi = 0x2000
    }
}
