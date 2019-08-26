using System;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SharpMap.Editors;

namespace SharpMap.UI.Helpers
{
    public static class MapCursors
    {
        public static readonly Cursor AddPoint = CreateCursor(Properties.Resources.AddPoint, 0, 0);
        public static readonly Cursor RemovePoint = CreateCursor(Properties.Resources.RemovePoint, 0, 0);
        public static readonly Cursor MovePoint;

        static MapCursors()
        {
            var asm = Assembly.GetAssembly(typeof(FeatureInteractor));
            if (asm != null)
            {
                using (var strm = asm.GetManifestResourceStream("SharpMap.Editors.Cursors.Move.cur"))
                {
                    if (strm != null)
                        MovePoint = new Cursor(strm);
                }
            }
        }

        public static readonly Bitmap AddFeatureTemplateBitmap = Properties.Resources.AddFeatureTemplate;

        public static Cursor CreateCursor(Bitmap bmp, int xHotSpot, int yHotSpot)
        {
            var ptr = bmp.GetHicon();
            var tmp = new IconInfo();
            GetIconInfo(ptr, ref tmp);
            tmp.xHotspot = xHotSpot;
            tmp.yHotspot = yHotSpot;
            tmp.fIcon = false;
            ptr = CreateIconIndirect(ref tmp);
            return new Cursor(ptr);
        }

        #region Native

        private struct IconInfo
        {
            public bool fIcon;
            public int xHotspot;
            public int yHotspot;
            public IntPtr hbmMask;
            public IntPtr hbmColor;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetIconInfo(IntPtr hIcon, ref IconInfo pIconInfo);

        [DllImport("user32.dll")]
        private static extern IntPtr CreateIconIndirect(ref IconInfo icon);

        #endregion
    }
}
