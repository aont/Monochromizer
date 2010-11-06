/*
 *  Image Processing Utilities for C#2.0 (VC# 2005)
 *  
 *         Copyright junki, Jan, 2006 -
 * 
 *  http://junki.main.jp/
 *  http://code.junki.main.jp/
 * 
 *  how to use : see http://junki.main.jp/csgr/006Library1.htm
 * 
 * This library is free for any non commercial usage. In the case of
 * modifying and/or redistributing the code, it's obligate to retain
 * the original copyright notice.
 * 
 * Moified by Aont 08/14
 * http://d.hatena.ne.jp/aont/
 */

using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace Aont
{
    public class BmpProc : IDisposable
    {
        bool flagDispose = false;

        protected Bitmap bitmap;
        public readonly int Width, Height;
        protected BitmapData bmpData;
        protected IntPtr Scan0;
        protected int Stride;
        public readonly int DataLength;

        public readonly PixelFormat Format;
        protected BmpProc(Bitmap @Bitmap, PixelFormat Format)
        {
            bitmap = Bitmap;
            Width = Bitmap.Width;
            Height = Bitmap.Height;
            Rectangle rect = new Rectangle(0, 0, Width, Height);
            bmpData = Bitmap.LockBits(rect, ImageLockMode.ReadWrite, Format);
            Scan0 = bmpData.Scan0;
            Stride = Math.Abs(bmpData.Stride);

            DataLength = Stride * Height;
        }
        protected virtual void Dispose(bool flag)
        {
            if (!flagDispose)
            {
                if (flag)
                {
                    bitmap.UnlockBits(bmpData);
                }
                this.flagDispose = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~BmpProc()
        {
            Dispose(false);
        }
    }
    public class BmpProc32 : BmpProc
    {
        public BmpProc32(Bitmap @Bitmap) : base(Bitmap, PixelFormat.Format32bppArgb) { }
        public Color this[int x, int y]
        {
            get
            {
                int ofs = Stride * y + x * 4;
                return Color.FromArgb(Marshal.ReadByte(Scan0, ofs + 3), Marshal.ReadByte(Scan0, ofs + 2), Marshal.ReadByte(Scan0, ofs + 1), Marshal.ReadByte(Scan0));
            }
            set
            {
                int ofs = Stride * y + x * 4;
                Marshal.WriteInt32(Scan0, ofs, value.ToArgb());
            }
        }
        public Color GetPixel(int x, int y)
        { return this[x, y]; }
        public void SetPixel(int x, int y, Color color)
        { this[x, y] = color; }
    }
    public  class BmpProc24 : BmpProc
    {
        public BmpProc24(Bitmap @Bitmap) : base(Bitmap, PixelFormat.Format24bppRgb) { }
        public Color this[int x, int y]
        {
            get
            {
                int ofs = Stride * y + x * 3;
                return Color.FromArgb(Marshal.ReadByte(Scan0, ofs + 2), Marshal.ReadByte(Scan0, ofs + 1), Marshal.ReadByte(Scan0));
            }
            set
            {
                int ofs = Stride * y + x * 3;
                Marshal.WriteByte(Scan0, ofs, value.R);
                Marshal.WriteByte(Scan0, ofs + 1, value.R);
                Marshal.WriteByte(Scan0, ofs + 2, value.R);
            }
        }
        public Color GetPixel(int x, int y)
        { return this[x, y]; }
        public void SetPixel(int x, int y, Color color)
        { this[x, y] = color; }
    }
    public class BmpProc8 : BmpProc
    {
        public Color[] Palette { get { return bitmap.Palette.Entries; } }
        public BmpProc8(Bitmap @Bitmap) : base(Bitmap, PixelFormat.Format8bppIndexed) { }
        public byte this[int x, int y]
        {
            get
            {
                return Marshal.ReadByte(Scan0, Stride * y + x);
            }
            set
            {
                Marshal.WriteByte(Scan0, Stride * y + x, value);
            }
        }
        public byte this[int index]
        {
            get { return Marshal.ReadByte(Scan0, index); }
            set { Marshal.WriteByte(Scan0, index, value); }
        }
        public Color GetPixel(int x, int y)
        {
            return Palette[this[x, y]];
        }
        public void SetPixel(int x, int y, Color color)
        {
            int i;
            for (i = 0; i < 256; ++i)
            {
                if (Palette[i] == color)
                    break;
            }
            if (i < 256)
            {
                this[x, y] = (byte)i;
            }
            else
            {
                throw new Exception("Color not found in Palette");
            }
        }
    }
    public class BmpProc1 : BmpProc
    {
        public BmpProc1(Bitmap @Bitmap) : base(Bitmap, PixelFormat.Format1bppIndexed) { }
        public bool this[int x, int y]
        {
            get
            {
                int xr;
                int ofs = Stride * y + Math.DivRem(x, 8, out xr);
                return ((Marshal.ReadByte(Scan0, ofs) >> (7 - xr) & 1) == 1);
            }

            set
            {
                int xr;
                int ofs = Stride * y + Math.DivRem(x, 8, out xr);
                if (value)
                    Marshal.WriteByte(Scan0, ofs,
                        (byte)(Marshal.ReadByte(Scan0, ofs) | (1 << (7 - xr)))
                        );
                else
                    Marshal.WriteByte(Scan0, ofs,
                        (byte)(Marshal.ReadByte(Scan0, ofs) & (~(1 << (7 - xr))))
                        );
            }
        }
        public bool this[int index]
        {
            get
            {
                int xr;
                int ofs = Math.DivRem(index, 8, out xr);
                return ((Marshal.ReadByte(Scan0, ofs) >> (7 - xr) & 1) == 1);
            }
            set
            {
                int xr;
                int ofs = Math.DivRem(index, 8, out xr);
                if (value)
                    Marshal.WriteByte(Scan0, ofs,
                        (byte)(Marshal.ReadByte(Scan0, ofs) | (1 << (7 - xr)))
                        );
                else
                    Marshal.WriteByte(Scan0, ofs,
                        (byte)(Marshal.ReadByte(Scan0, ofs) & (~(1 << (7 - xr))))
                        );
            }
        }
    }
}