using PdfToBitmapList.PdfiumModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PdfToBitmapList
{
    internal class PdfFile
    {
        private static readonly Encoding FPDFEncoding = new UnicodeEncoding(false, false, false);

        private IntPtr _document;
        private IntPtr _form;
        private bool _disposed;
        private PdfiumNative.FPDF_FORMFILLINFO _formCallbacks;
        private GCHandle _formCallbacksHandle;
        private readonly int _id;
        private Stream? _stream;

        private PageData _currentPageData = null;
        private int _currentPageDataPageNumber = -1;
        public PdfBookmarkCollection Bookmarks { get; private set; }

        public PdfFile(Stream stream, string password)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            PdfLibrary.EnsureLoaded();

            _stream = stream;
            _id = StreamManager.Register(stream);

            var document = PdfiumNative.FPDF_LoadCustomDocument(stream, password, _id);
            if (document == IntPtr.Zero)
            {
                try
                {
                    throw new PdfException((PdfError)PdfiumNative.FPDF_GetLastError());
                }
                finally
                {
                    Dispose();
                }
            }

            LoadDocument(document);
        }

        protected void LoadDocument(IntPtr document)
        {
            _document = document;

            PdfiumNative.FPDF_GetDocPermissions(_document);

            _formCallbacks = new PdfiumNative.FPDF_FORMFILLINFO();
            _formCallbacksHandle = GCHandle.Alloc(_formCallbacks, GCHandleType.Pinned);

            // Depending on whether XFA support is built into the PDFium library, the version
            // needs to be 1 or 2. We don't really care, so we just try one or the other.

            for (int i = 1; i <= 2; i++)
            {
                _formCallbacks.version = i;

                _form = PdfiumNative.FPDFDOC_InitFormFillEnvironment(_document, _formCallbacks);
                if (_form != IntPtr.Zero)
                    break;
            }

            PdfiumNative.FPDF_SetFormFieldHighlightColor(_form, 0, 0xFFE4DD);
            PdfiumNative.FPDF_SetFormFieldHighlightAlpha(_form, 100);

            PdfiumNative.FORM_DoDocumentJSAction(_form);
            PdfiumNative.FORM_DoDocumentOpenAction(_form);

            Bookmarks = new PdfBookmarkCollection();

            LoadBookmarks(Bookmarks, PdfiumNative.FPDF_BookmarkGetFirstChild(document, IntPtr.Zero));
        }

        private void LoadBookmarks(PdfBookmarkCollection bookmarks, IntPtr bookmark)
        {
            if (bookmark == IntPtr.Zero)
                return;

            bookmarks.Add(LoadBookmark(bookmark));
            while ((bookmark = PdfiumNative.FPDF_BookmarkGetNextSibling(_document, bookmark)) != IntPtr.Zero)
                bookmarks.Add(LoadBookmark(bookmark));
        }

        private PdfBookmark LoadBookmark(IntPtr bookmark)
        {
            var result = new PdfBookmark
            {
                Title = GetBookmarkTitle(bookmark),
                PageIndex = (int)GetBookmarkPageIndex(bookmark)
            };

            //Action = NativeMethods.FPDF_BookmarkGetAction(_bookmark);
            //if (Action != IntPtr.Zero)
            //    ActionType = NativeMethods.FPDF_ActionGetType(Action);

            var child = PdfiumNative.FPDF_BookmarkGetFirstChild(_document, bookmark);
            if (child != IntPtr.Zero)
                LoadBookmarks(result.Children, child);

            return result;
        }

        private uint GetBookmarkPageIndex(IntPtr bookmark)
        {
            IntPtr dest = PdfiumNative.FPDF_BookmarkGetDest(_document, bookmark);
            if (dest != IntPtr.Zero)
                return PdfiumNative.FPDFDest_GetDestPageIndex(_document, dest);

            return 0;
        }

        private string GetBookmarkTitle(IntPtr bookmark)
        {
            uint length = PdfiumNative.FPDF_BookmarkGetTitle(bookmark, null, 0);
            byte[] buffer = new byte[length];
            PdfiumNative.FPDF_BookmarkGetTitle(bookmark, buffer, length);

            string result = Encoding.Unicode.GetString(buffer);
            if (result.Length > 0 && result[result.Length - 1] == 0)
                result = result.Substring(0, result.Length - 1);

            return result;
        }

        public List<SizeF> GetPDFDocInfo()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);

            int pageCount = PdfiumNative.FPDF_GetPageCount(_document);
            var result = new List<SizeF>(pageCount);

            for (int i = 0; i < pageCount; i++)
            {
                result.Add(GetPDFDocInfo(i));
            }

            return result;
        }

        public SizeF GetPDFDocInfo(int pageNumber)
        {
            double height;
            double width;
            PdfiumNative.FPDF_GetPageSizeByIndex(_document, pageNumber, out width, out height);

            return new SizeF((float)width, (float)height);
        }

        public bool RenderPDFPageToDC(int pageNumber, IntPtr dc, int dpiX, int dpiY, int boundsOriginX, int boundsOriginY, int boundsWidth, int boundsHeight, PdfiumNative.FPDF flags)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);

            PdfiumNative.FPDF_RenderPage(dc, GetPageData(pageNumber).Page, boundsOriginX, boundsOriginY, boundsWidth, boundsHeight, 0, flags);

            return true;
        }

        public int GetPageCount()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);
            return PdfiumNative.FPDF_GetPageCount(_document);
        }

        private PageData GetPageData(int pageNumber)
        {
            if (_currentPageDataPageNumber != pageNumber)
            {
                _currentPageData?.Dispose();
                _currentPageData = new PageData(_document, _form, pageNumber);
                _currentPageDataPageNumber = pageNumber;
            }

            return _currentPageData;
        }

        public bool RenderPDFPageToBitmap(int pageNumber, IntPtr bitmapHandle, int dpiX, int dpiY, int boundsOriginX, int boundsOriginY, int boundsWidth, int boundsHeight, int rotate, PdfiumNative.FPDF flags, bool renderFormFill)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);

            var pageData = GetPageData(pageNumber);

            PdfiumNative.FPDF_RenderPageBitmap(bitmapHandle, pageData.Page, boundsOriginX, boundsOriginY, boundsWidth, boundsHeight, rotate, flags);

            if (renderFormFill)
            {
                PdfiumNative.FPDF_FFLDraw(_form, bitmapHandle, pageData.Page, boundsOriginX, boundsOriginY, boundsWidth, boundsHeight, rotate, flags);
            }

            return true;
        }


        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                StreamManager.Unregister(_id);

                _currentPageData?.Dispose();
                _currentPageData = null;

                if (_form != IntPtr.Zero)
                {
                    PdfiumNative.FORM_DoDocumentAAction(_form, PdfiumNative.FPDFDOC_AACTION.WC);
                    PdfiumNative.FPDFDOC_ExitFormFillEnvironment(_form);
                    _form = IntPtr.Zero;
                }

                if (_document != IntPtr.Zero)
                {
                    PdfiumNative.FPDF_CloseDocument(_document);
                    _document = IntPtr.Zero;
                }

                if (_formCallbacksHandle.IsAllocated)
                    _formCallbacksHandle.Free();

                if (_stream != null)
                {
                    _stream.Dispose();
                    _stream = null;
                }

                _disposed = true;
            }
        }


        private class PageData : IDisposable
        {
            private readonly IntPtr _form;
            private bool _disposed;

            public IntPtr Page { get; private set; }

            public IntPtr TextPage { get; private set; }

            public double Width { get; private set; }

            public double Height { get; private set; }

            public PageData(IntPtr document, IntPtr form, int pageNumber)
            {
                _form = form;

                Page = PdfiumNative.FPDF_LoadPage(document, pageNumber);
                TextPage = PdfiumNative.FPDFText_LoadPage(Page);
                PdfiumNative.FORM_OnAfterLoadPage(Page, form);
                PdfiumNative.FORM_DoPageAAction(Page, form, PdfiumNative.FPDFPAGE_AACTION.OPEN);

                Width = PdfiumNative.FPDF_GetPageWidth(Page);
                Height = PdfiumNative.FPDF_GetPageHeight(Page);
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    PdfiumNative.FORM_DoPageAAction(Page, _form, PdfiumNative.FPDFPAGE_AACTION.CLOSE);
                    PdfiumNative.FORM_OnBeforeClosePage(Page, _form);
                    PdfiumNative.FPDFText_ClosePage(TextPage);
                    PdfiumNative.FPDF_ClosePage(Page);

                    _disposed = true;
                }
            }
        }

    }
}
