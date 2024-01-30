using PdfToBitmapList;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace PdfToBitmapList
{
    internal class PdfLibrary : IDisposable
    {
        private static readonly object _syncRoot = new object();
        private static PdfLibrary _library;

        public static void EnsureLoaded()
        {
            lock (_syncRoot)
            {
                if (_library == null)
                    _library = new PdfLibrary();
            }
        }

        private bool _disposed;

        private PdfLibrary()
        {
            try
            {
                PdfiumNative.FPDF_InitEmbeddedLibraries();
            }
            catch { }
            PdfiumNative.FPDF_InitLibrary();
        }

        ~PdfLibrary()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                PdfiumNative.FPDF_DestroyLibrary();

                _disposed = true;
            }
        }
    }
}
