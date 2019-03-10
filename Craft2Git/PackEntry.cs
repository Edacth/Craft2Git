using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Windows.Media;

namespace Craft2Git
{

    public class PackEntry
    {
        public int format_version { get; set; }
        public Header header { get; set; }
        public Module[] modules { get; set; }
        public Dependency[] dependencies { get; set; }
        public string filePath { get; set; }
        public string iconPath { get; set;}
        public ImageSource iconSource { get; set; }

        public PackEntry()
        {
            header = new Header();
            
        }

        public void loadIcon()
        {
            try
            {
                
                BitmapImage icon = new BitmapImage();
                icon.BeginInit();
                icon.CacheOption = BitmapCacheOption.OnLoad;
                icon.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                icon.UriSource = new Uri(iconPath);
                icon.EndInit();

                iconSource = icon;
            }
            catch
            {

                BitmapSource icon = BitmapImage.Create(
                2,
                2,
                96,
                96,
                PixelFormats.Indexed1,
                new BitmapPalette(new List<System.Windows.Media.Color> { Colors.Transparent }),
                new byte[] { 0, 0, 0, 0 },
                1);
                iconSource = icon;
            }

        }
    }

    public class Header
    {
        public string description { get; set; }
        public string name { get; set; }
        public string uuid { get; set; }
        public int[] version { get; set; }
    }

    public class Module
    {
        public string description { get; set; }
        public string type { get; set; }
        public string uuid { get; set; }
        public int[] version { get; set; }
    }

    public class Dependency
    {
        public string uuid { get; set; }
        public int[] version { get; set; }
    }

}
