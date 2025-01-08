using System;
using System.Collections.Generic;
using System.Text;

#pragma warning disable 1591

namespace PdfToBitmapList
{
    public enum PdfError
    {
        Success = (int)PdfiumNative.FPDF_ERR.FPDF_ERR_SUCCESS,
        Unknown = (int)PdfiumNative.FPDF_ERR.FPDF_ERR_UNKNOWN,
        CannotOpenFile = (int)PdfiumNative.FPDF_ERR.FPDF_ERR_FILE,
        InvalidFormat = (int)PdfiumNative.FPDF_ERR.FPDF_ERR_FORMAT,
        PasswordProtected = (int)PdfiumNative.FPDF_ERR.FPDF_ERR_PASSWORD,
        UnsupportedSecurityScheme = (int)PdfiumNative.FPDF_ERR.FPDF_ERR_SECURITY,
        PageNotFound = (int)PdfiumNative.FPDF_ERR.FPDF_ERR_PAGE
    }
}
