using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using WindowsServer.Configuration;
using WindowsServer.DataBase;
using WindowsServer.Encryption;

namespace WindowsServer.Web.Controllers
{
    public class DBManagementController : Controller
    {
        public ActionResult BrowseConnections()
        {
            ManagementConsoleAuthentication.VerifyTokenCookie(this);

            StringBuilder sb = new StringBuilder(1024);
            sb.Append("<html><body>");
            sb.Append(ManagementUtility.BuildHeaderHtml());
            sb.Append("<h3>AppSettings</h3><br/>");
            sb.Append("<table>");
            foreach (var key in ConfigurationCenter.Global.DiscoverAllKeys())
            {
                sb.Append("<tr><td><a href=\"");
                sb.Append(Url.Action("GetTables", new { ask = key })); // ask: app setting key
                sb.Append("\">");
                sb.Append(HttpUtility.HtmlEncode(key));
                sb.Append("</a></td></tr>");
            }
            sb.Append("</table><br/>");

            // Files
            sb.Append("<h3>Conn</h3><br/>");
            sb.Append("<table>");
            for (int i = 0; i < WebConfigurationManager.ConnectionStrings.Count; i++)
            {
                var v = WebConfigurationManager.ConnectionStrings[i].Name;
                sb.Append("<tr><td><a href=\"");
                sb.Append(Url.Action("GetTables", new { csk = v })); // csk: connection string key
                sb.Append("\">");
                sb.Append(HttpUtility.HtmlEncode(v));
                sb.Append("</a></td></tr>");
            }
            sb.Append("</table><br/>");
            sb.Append("</body></html>");

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/html");
        }

        public ActionResult GetTables(string ask, string csk, string eb64cs)
        {
            ManagementConsoleAuthentication.VerifyTokenCookie(this);

            string connectionString;
            if (!string.IsNullOrEmpty(ask))
            {
                connectionString = ConfigurationCenter.Global[ask];
            }
            else if (!string.IsNullOrEmpty(csk))
            {
                connectionString = WebConfigurationManager.ConnectionStrings[csk].ConnectionString;
            }
            else
            {
                connectionString = Encoding.UTF8.GetString(EncryptedBase64.FromEncryptedBase64String(eb64cs));
            }

            var tables = new List<string>();
            DbHelper.ExecuteReader(
                connectionString,
                "SELECT [name] FROM sys.tables",
                reader =>
                {
                    tables.Add((string)reader[0]);
                }
            );

            StringBuilder sb = new StringBuilder(1024);
            sb.Append("<html><body>");
            sb.Append(ManagementUtility.BuildHeaderHtml());
            sb.Append("<h3>Tables</h3><br/>");
            sb.Append("<table>");
            foreach (var table in tables)
            {
                sb.Append("<tr><td><a href=\"");
                sb.Append(Url.Action("GetRows", new { ask = ask, csk = csk, eb64cs = eb64cs, t = table }));
                sb.Append("\">");
                sb.Append(HttpUtility.HtmlEncode(table));
                sb.Append("</a></td></tr>");
            }
            sb.Append("</table><br/>");
            sb.Append("</body></html>");

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/html");
        }

        public ActionResult GetRows(string ask, string csk, string eb64cs, string t)
        {
            ManagementConsoleAuthentication.VerifyTokenCookie(this);

            string connectionString;
            if (!string.IsNullOrEmpty(ask))
            {
                connectionString = ConfigurationCenter.Global[ask];
            }
            else if (!string.IsNullOrEmpty(csk))
            {
                connectionString = WebConfigurationManager.ConnectionStrings[csk].ConnectionString;
            }
            else
            {
                connectionString = Encoding.UTF8.GetString(EncryptedBase64.FromEncryptedBase64String(eb64cs));
            }

            // To avoid sql injection
            t = t.Replace("\'", string.Empty);
            t = t.Replace("\"", string.Empty);
            t = t.Replace(" ", string.Empty);

            StringBuilder sb = new StringBuilder(1024);
            sb.Append("<html><body>");
            sb.Append(ManagementUtility.BuildHeaderHtml());
            sb.Append("<h3>" + HttpUtility.HtmlEncode(t) + "</h3><br/>");
            sb.Append("<table>");

            DataTable schema = null;
            DbHelper.ExecuteReader(
                connectionString,
                "SELECT * FROM " + t,
                reader =>
                {
                    if (schema == null)
                    {
                        schema = reader.GetSchemaTable();
                        sb.Append("<tr>");
                        foreach (DataRow row in schema.Rows)
                        {
                            sb.Append("<td>");
                            sb.Append(row["ColumnName"].ToString());
                            sb.Append("</td>");
                        }
                        sb.Append("</tr>");
                    }

                    sb.Append("<tr>");
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        sb.Append("<td>");
                        sb.Append(reader[i].ToString());
                        sb.Append("</td>");
                    }
                    sb.Append("</tr>");
                }
            );

            sb.Append("</table><br/>");
            sb.Append("</body></html>");

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/html");
        }

    }
}
