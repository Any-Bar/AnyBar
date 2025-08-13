using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.Shell;

namespace Flow.Bar.Helpers.Image;

/// <summary>
/// Subclass of <see cref="SIIGBF"/>
/// </summary>
[Flags]
public enum ThumbnailOptions
{
    None = 0x00,
    BiggerSizeOk = 0x01,
    InMemoryOnly = 0x02,
    IconOnly = 0x04,
    ThumbnailOnly = 0x08,
    InCacheOnly = 0x10,
}

public class WindowsThumbnailProvider
{
    // Based on https://stackoverflow.com/questions/21751747/extract-thumbnail-for-any-file-in-windows

    private static readonly Guid GUID_IShellItem = typeof(IShellItem).GUID;

    private static readonly HRESULT S_EXTRACTIONFAILED = (HRESULT)0x8004B200;

    private static readonly HRESULT S_PATHNOTFOUND = (HRESULT)0x8004B205;

    public static BitmapSource GetThumbnail(string fileName, int width, int height, ThumbnailOptions options)
    {
        var hBitmap = GetHBitmap(Path.GetFullPath(fileName), width, height, options);

        try
        {
            return Imaging.CreateBitmapSourceFromHBitmap(hBitmap, nint.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }
        finally
        {
            // delete HBitmap to avoid memory leaks
            PInvoke.DeleteObject(hBitmap);
        }
    }

    private static unsafe HBITMAP GetHBitmap(string fileName, int width, int height, ThumbnailOptions options)
    {
        var retCode = PInvoke.SHCreateItemFromParsingName(
            fileName,
            null,
            GUID_IShellItem,
            out var nativeShellItem);

        if (retCode != HRESULT.S_OK)
        {
            var exception = Marshal.GetExceptionForHR(retCode);
            if (exception is not null)
            {
                throw exception;
            }
            else
            {
                throw new InvalidOperationException($"Failed to get {nameof(IShellItem)}. HRESULT: {retCode}");
            }
        }

        if (nativeShellItem is not IShellItemImageFactory imageFactory)
        {
            Marshal.ReleaseComObject(nativeShellItem);
            throw new InvalidOperationException($"Failed to get {nameof(IShellItemImageFactory)}");
        }

        var size = new SIZE
        {
            cx = width,
            cy = height
        };

        HBITMAP hBitmap = default;
        try
        {
            try
            {
                imageFactory.GetImage(size, (SIIGBF)options, &hBitmap);
            }
            catch (COMException ex) when (options == ThumbnailOptions.ThumbnailOnly &&
                (ex.HResult == S_PATHNOTFOUND || ex.HResult == S_EXTRACTIONFAILED))
            {
                // Fallback to IconOnly if extraction fails or files cannot be found
                imageFactory.GetImage(size, (SIIGBF)ThumbnailOptions.IconOnly, &hBitmap);
            }
            catch (FileNotFoundException) when (options == ThumbnailOptions.ThumbnailOnly)
            {
                // Fallback to IconOnly if files cannot be found
                imageFactory.GetImage(size, (SIIGBF)ThumbnailOptions.IconOnly, &hBitmap);
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                throw new InvalidOperationException("Failed to get thumbnail", ex);
            }
        }
        finally
        {
            if (nativeShellItem != null)
            {
                Marshal.ReleaseComObject(nativeShellItem);
            }
        }

        return hBitmap;
    }
}
