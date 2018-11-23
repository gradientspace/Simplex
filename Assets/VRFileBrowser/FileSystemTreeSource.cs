using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using g3;

namespace f3
{
    interface ITreeSource
    {
        string[] GetFolders(string path);
        string[] GetFiles(string path);
    }



    public class FileSystemTreeSource : ITreeSource
    {
        public List<string> ValidExtensions { get; set; }
        public bool FilterInaccessibleFolders { get; set; }

        public FileSystemTreeSource()
        {
            ValidExtensions = new List<string>();
            FilterInaccessibleFolders = true;
        }

        public string[] filter_folders(string[] list, string sAdd = null)
        {
            List<string> filtered = new List<string>();
            if (sAdd != null)
                filtered.Add(sAdd);

            foreach (string s in list) {
                string name = Path.GetFileName(s);
                if (name.StartsWith("$"))
                    continue;
                if (FilterInaccessibleFolders && FileSystemUtils.CanAccessFolder(s) == false)
                    continue;
                filtered.Add(name);
            }
            return filtered.ToArray();
        }

        public string[] GetFolders(string path)
        {
            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException(path);
            string[] directories = Directory.GetDirectories(path);
            if (Path.GetPathRoot(path).Equals(path, StringComparison.CurrentCultureIgnoreCase))
                return filter_folders(directories);
            else
                return filter_folders(directories, "..");
        }



        public string[] filter_files(string[] list)
        {
            List<string> filtered = new List<string>();
            foreach (string s in list) {
                string name = Path.GetFileName(s);
                if (name.StartsWith("$"))
                    continue;

                bool bValidExtension = (ValidExtensions.Count == 0) ? true : false;
                foreach (string ext in ValidExtensions) {
                    if (name.EndsWith(ext, StringComparison.CurrentCultureIgnoreCase)) {
                        bValidExtension = true;
                        break;
                    }
                }
                if (bValidExtension)
                    filtered.Add(name);
            }
            return filtered.ToArray();
        }


        public string[] GetFiles(string path)
        {
            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException(path);
            string[] files = Directory.GetFiles(path);
            return filter_files(files);
        }
    }
}
