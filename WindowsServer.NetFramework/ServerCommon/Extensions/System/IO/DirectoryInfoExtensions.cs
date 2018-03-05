using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.IO
{
    public static class DirectoryInfoExtensions
    {
        public static void CopyTo(this DirectoryInfo directory, string destination, bool copySubDirectories, bool overwrite)
        {
            // Get the subdirectories for the specified directory.
            if (!directory.Exists)
            {
                throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + directory.FullName);
            }

            // If the destination directory doesn't exist, create it. 
            if (!Directory.Exists(destination))
            {
                Directory.CreateDirectory(destination);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = directory.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(destination, file.Name);
                file.CopyTo(tempPath, overwrite);
            }

            // If copying subdirectories, copy them and their contents to new location. 
            if (copySubDirectories)
            {
                DirectoryInfo[] dirs = directory.GetDirectories();
                foreach (DirectoryInfo sub in dirs)
                {
                    string tempPath = Path.Combine(destination, sub.Name);
                    CopyTo(sub, tempPath, copySubDirectories, overwrite);
                }
            }
        }
    }
}
