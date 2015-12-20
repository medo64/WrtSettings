using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace WrtSettings {
    internal static class Helper {

        #region Toolstrip DPI scaling

        internal static void ScaleToolstrip(params ToolStrip[] toolstrips) {
            var sizeAndSet = GetSizeAndSet(toolstrips);
            var size = sizeAndSet.Key;
            var set = sizeAndSet.Value;

            var resources = WrtSettings.Properties.Resources.ResourceManager;
            foreach (var toolstrip in toolstrips) {
                toolstrip.ImageScalingSize = new Size(size, size);
                foreach (ToolStripItem item in toolstrip.Items) {
                    item.ImageScaling = ToolStripItemImageScaling.None;
                    if (item.Image != null) { //update only those already having image
                        Bitmap bitmap = null;
                        if (!string.IsNullOrEmpty(item.Name)) {
                            bitmap = resources.GetObject(item.Name + set) as Bitmap;
                        }
                        if ((bitmap == null) && !string.IsNullOrEmpty(item.Tag as string)) {
                            bitmap = resources.GetObject(item.Tag + set) as Bitmap;
                        }

                        item.ImageScaling = ToolStripItemImageScaling.None;
#if DEBUG
                        item.Image = (bitmap != null) ? new Bitmap(bitmap, size, size) : new Bitmap(size, size, PixelFormat.Format8bppIndexed);
#else
                        if (bitmap != null) { item.Image = new Bitmap(bitmap, size, size); }
#endif
                    }

                    var toolstripSplitButton = item as ToolStripSplitButton;
                    if (toolstripSplitButton != null) { ScaleToolstrip(toolstripSplitButton.DropDown); }
                }
            }
        }

        internal static void ScaleToolstripItem(ToolStripItem item, string name) {
            var sizeAndSet = GetSizeAndSet(item.GetCurrentParent());
            var size = sizeAndSet.Key;
            var set = sizeAndSet.Value;

            var resources = WrtSettings.Properties.Resources.ResourceManager;
            var bitmap = resources.GetObject(name + set) as Bitmap;
            item.ImageScaling = ToolStripItemImageScaling.None;
#if DEBUG
            item.Image = (bitmap != null) ? new Bitmap(bitmap, size, size) : new Bitmap(size, size, PixelFormat.Format8bppIndexed);
#else
            if (bitmap != null) { item.Image = new Bitmap(bitmap, size, size); }
#endif
        }

        private static KeyValuePair<int, string> GetSizeAndSet(params Control[] controls) {
            using (var g = controls[0].CreateGraphics()) {
                var scale = Math.Max(Math.Max(g.DpiX, g.DpiY), 96.0) / 96.0 + 0.25;
                scale += Settings.ScaleBoost;

                if (scale < 1.5) {
                    return new KeyValuePair<int, string>(16, "_16");
                } else if (scale < 2) {
                    return new KeyValuePair<int, string>(24, "_24");
                } else if (scale < 3) {
                    return new KeyValuePair<int, string>(32, "_32");
                } else {
                    var base32 = 16 * scale / 32;
                    var base48 = 16 * scale / 48;
                    if ((base48 - (int)base48) < (base32 - (int)base32)) {
                        return new KeyValuePair<int, string>(48 * (int)base48, "_48");
                    } else {
                        return new KeyValuePair<int, string>(32 * (int)base32, "_32");
                    }
                }
            }
        }

        #endregion

    }
}
