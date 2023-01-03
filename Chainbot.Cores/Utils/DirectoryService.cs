using Chainbot.Contracts.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using Chainbot.Contracts.Classes;
using System.Windows.Media;
using System.Windows.Interop;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Chainbot.Cores.Utils
{
    public class DirectoryService : IDirectoryService
    {
        public List<DirOrFileItem> Query(string path, Predicate<DirOrFileItem> match = null)
        {
            var di = new DirectoryInfo(path);

            var items = new List<DirOrFileItem>();

            InitDirectory(di,items, match);

            return items;
        }


        private void InitDirectory(DirectoryInfo di, List<DirOrFileItem> items, Predicate<DirOrFileItem> match)
        {
            DirectoryInfo[] dis = di.GetDirectories();
            for (int j = 0; j < dis.Length; j++)
            {
                var item = new DirItem();
                item.Path = dis[j].FullName;
                item.ParentPath = Path.GetDirectoryName(item.Path);
                item.Name = dis[j].Name;
                item.FileSystemInfo = dis[j];

                if (match == null)
                {
                    items.Add(item);
                    item.Children = Query(item.Path, match);
                }
                else
                {
                    if (match(item))
                    {
                        items.Add(item);
                        item.Children = Query(item.Path, match);
                    }
                }
            }

            FileInfo[] fis = di.GetFiles();
            for (int i = 0; i < fis.Length; i++)
            {
                var item = new FileItem();
                item.Path = fis[i].FullName;
                item.ParentPath = Path.GetDirectoryName(item.Path);
                item.Name = fis[i].Name;
                item.Extension = fis[i].Extension;
                item.FileSystemInfo = fis[i];

                var icon = System.Drawing.Icon.ExtractAssociatedIcon(item.Path);

                ImageSource imageSource = Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

                item.AssociatedIcon = imageSource;

                if (match == null)
                {
                    items.Add(item);
                }
                else
                {
                    if (match(item))
                    {
                        items.Add(item);
                    }
                }       
            }
        }
    }
}
