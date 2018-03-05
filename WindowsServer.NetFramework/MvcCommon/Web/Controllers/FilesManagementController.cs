using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using WindowsServer.Encryption;
using WindowsServer.Zip.Zip;

namespace WindowsServer.Web.Controllers
{
    public class FilesManagementController : Controller
    {
        public ActionResult BrowseDirectory(string pp)
        {
            ManagementConsoleAuthentication.VerifyTokenCookie(this);

            pp = pp ?? string.Empty;
            var physicalPath = Encoding.UTF8.GetString(EncryptedBase64.FromEncryptedBase64String(pp));
            if (string.IsNullOrEmpty(physicalPath))
            {
                physicalPath = Server.MapPath("~");
            }

            StringBuilder sb = new StringBuilder(1024);
            sb.Append("<html><body>");
            sb.Append(ManagementUtility.BuildHeaderHtml());
            sb.Append("<h2>" + HttpUtility.HtmlEncode(physicalPath) + "</h2>");
            sb.Append("<a href=\"" + Url.Action("ZipDirectory", new { pp = EncryptedBase64.ToEncryptedBase64String(Encoding.UTF8.GetBytes(physicalPath)) }) + "\">zip</a>");
            sb.Append("<table>");
            // Directories
            var directories = from d in Directory.GetDirectories(physicalPath)
                              orderby d ascending
                              select d;
            foreach (var directory in directories)
            {
                sb.Append("<tr><td>D</td><td><a href=\"");
                sb.Append(Url.Action("BrowseDirectory", new { pp = EncryptedBase64.ToEncryptedBase64String(Encoding.UTF8.GetBytes(directory)) }));
                sb.Append("\">");
                sb.Append(HttpUtility.HtmlEncode(Path.GetFileName(directory)));
                sb.Append("</a></td></tr>");
            }
            sb.Append("</table>");

            // Files
            sb.Append("<table>");
            // Directories
            var files = from d in Directory.GetFiles(physicalPath)
                        orderby d ascending
                        select d;
            foreach (var file in files)
            {
                sb.Append("<tr><td>F</td><td><a href=\"");
                sb.Append(Url.Action("DownloadFile", new { pp = EncryptedBase64.ToEncryptedBase64String(Encoding.UTF8.GetBytes(file)) }));
                sb.Append("\">");
                sb.Append(HttpUtility.HtmlEncode(Path.GetFileName(file)));
                sb.Append("</a></td></tr>");
            }
            sb.Append("</table>");
            sb.Append("</body></html>");

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/html");
        }

        public ActionResult DownloadFile(string pp)
        {
            ManagementConsoleAuthentication.VerifyTokenCookie(this);

            pp = pp ?? string.Empty;
            var physicalPath = Encoding.UTF8.GetString(EncryptedBase64.FromEncryptedBase64String(pp));
            return File(physicalPath, "application/octet-stream", Path.GetFileName(physicalPath));
        }

        public ActionResult ZipDirectory(string pp, string exclude)
        {
            ManagementConsoleAuthentication.VerifyTokenCookie(this);

            pp = pp ?? string.Empty;
            var physicalPath = Encoding.UTF8.GetString(EncryptedBase64.FromEncryptedBase64String(pp));

            List<string> excludedItems = null;
            if (!string.IsNullOrWhiteSpace(exclude))
            {
                excludedItems = new List<string>(exclude.Trim().Split(','));
            }

            using (var zipFileStream = new MemoryStream())
            {
                using (var logWriter = new StreamWriter(new MemoryStream(), Encoding.UTF8))
                {
                    using (ZipOutputStream s = new ZipOutputStream(zipFileStream))
                    {
                        s.SetLevel(9);
                        ZipDirectoryInternal(physicalPath, string.Empty, s, logWriter, excludedItems);

                        // Add log
                        ZipEntry entry = new ZipEntry("__log.txt");
                        entry.DateTime = DateTime.Now;
                        s.PutNextEntry(entry);
                        logWriter.Flush();
                        logWriter.BaseStream.Seek(0, SeekOrigin.Begin);
                        logWriter.BaseStream.CopyTo(s);

                        s.Finish();
                        s.Close();
                    }
                }

                return File(zipFileStream.ToArray(), "application/zip");
            }

        }

        private void ZipDirectoryInternal(string root, string relative, ZipOutputStream zip, StreamWriter logWriter, List<string> excludedItems)
        {
            string path = Path.Combine(root, relative);

            try
            {
                string[] filenames = Directory.GetFiles(path);
                foreach (string file in filenames)
                {
                    if ((excludedItems != null) && excludedItems.Contains(Path.GetFileName(file), StringComparer.InvariantCultureIgnoreCase))
                    {
                        logWriter.WriteLine("Skipped file: " + file);
                        continue;
                    }

                    try
                    {
                        using (FileStream fs = System.IO.File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            ZipEntry entry = new ZipEntry(string.IsNullOrEmpty(relative) ? Path.GetFileName(file) : Path.Combine(relative, Path.GetFileName(file)));
                            entry.DateTime = DateTime.Now;
                            zip.PutNextEntry(entry);
                            fs.CopyTo(zip);
                        }
                        logWriter.WriteLine("Added file: " + file);
                    }
                    catch (Exception ex)
                    {
                        logWriter.WriteLine("Failed to add file: " + file + " " + ex.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                logWriter.WriteLine("Failed to enumerate files in " + path + " " + ex.ToString());
            }

            try
            {
                string[] directories = Directory.GetDirectories(path);
                foreach (string directory in directories)
                {
                    string d = Path.GetFileName(directory);
                    if ((excludedItems != null) && excludedItems.Contains(d, StringComparer.InvariantCultureIgnoreCase))
                    {
                        logWriter.WriteLine("Skipped directory: " + directory);
                        continue;
                    }

                    // Excluded items impact only the first directory level. Do not pass it down.
                    ZipDirectoryInternal(root, Path.Combine(relative, d), zip, logWriter, null);
                }
            }
            catch (Exception ex)
            {
                logWriter.WriteLine("Failed to enumerate sub directories in " + path + " " + ex.ToString());
            }

        }

    }
}
