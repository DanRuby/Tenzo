using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace tEngine.Markers {
    public class Drawer {
        public static void CopyPart( WriteableBitmap source, Rect sourceRect, WriteableBitmap dest, Rect destRect ) {
            var sourceRc_ = CorrectRect( source, sourceRect );
            var destRc_ = CorrectRect( dest, destRect );

            if( sourceRc_.HasValue == false || destRc_.HasValue == false )
                return;

            var sourceRc = sourceRc_.Value;
            var destRc = destRc_.Value;


            if( (sourceRc.Width < 1 || sourceRc.Height < 1) ||
                (destRect.Width < 1 || destRect.Height < 1) )
                return;
            // Calculate stride of source
            var stride = source.PixelWidth*(source.Format.BitsPerPixel/8);

            // Create data array to hold source pixel data
            var data = new byte[stride*source.PixelHeight];

            // Copy source image pixels to the data array
            source.CopyPixels(
                new Int32Rect( (int) sourceRc.Left, (int) sourceRc.Top, (int) sourceRc.Width, (int) sourceRc.Height ),
                data, stride, 0 );

            // Write the pixel data to the WriteableBitmap.
            dest.WritePixels(
                new Int32Rect( (int) destRc.Left, (int) destRc.Top, (int) destRc.Width, (int) destRc.Height ),
                data, stride, 0 );
        }

        public static Rect? CorrectRect( WriteableBitmap bitmap, Rect rect ) {
            var result = new Rect( rect.Location, rect.Size );
            if( result.X > bitmap.PixelWidth || result.Y > bitmap.PixelHeight )
                return null;
            if( result.X + result.Width < 0 || result.Y + result.Height < 0 )
                return null;

            if( result.X < 0 ) {
                result.Width += result.X;
                result.X = 0;
            }
            if( result.X + result.Width > bitmap.PixelWidth )
                result.Width = bitmap.PixelWidth - result.X;

            if( result.Y < 0 ) {
                result.Height += result.Y;
                result.Y = 0;
            }
            if( result.Y + result.Height > bitmap.PixelHeight )
                result.Height = bitmap.PixelHeight - result.Y;

            return result;
        }

        public static void DrawRectangle( WriteableBitmap writeableBitmap, Rect rect, Color color ) {
            try {
                var rc_ = CorrectRect( writeableBitmap, rect );
                if( rc_.HasValue == false )
                    return;
                var rc = rc_.Value;

                if( rc.Width < 1 || rc.Height < 1 )
                    return;
                writeableBitmap.Lock();
                // Compute the pixel's color
                var colorData = color.R << 16; // R
                colorData |= color.G << 8; // G
                colorData |= color.B << 0; // B
                var bpp = writeableBitmap.Format.BitsPerPixel/8;

                unsafe {
                    for( int y = 0; y < rc.Height; y++ ) {
                        // Get a pointer to the back buffer
                        var pBackBuffer = (int) writeableBitmap.BackBuffer;

                        // Find the address of the pixel to draw
                        pBackBuffer += ((int) rc.Top + y)*writeableBitmap.BackBufferStride;
                        pBackBuffer += (int) rc.Left*bpp;

                        for( int x = 0; x < rc.Width; x++ ) {
                            // Assign the color data to the pixel
                            *((int*) pBackBuffer) = colorData;

                            // Increment the address of the pixel to draw
                            pBackBuffer += bpp;
                        }
                    }
                }
                writeableBitmap.AddDirtyRect( new Int32Rect( (int) rc.Left, (int) rc.Top, (int) rc.Width,
                    (int) rc.Height ) );
                writeableBitmap.Unlock();
            } catch( Exception ex ) {
                Debug.Assert( false, ex.Message );
            }
        }
    }
}