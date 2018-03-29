using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace WindowsServer.Web.Filters
{
    public class AntiOutSiteLinkAttribute : ActionFilterAttribute
    {
        public enum FileType
        {
            File = 1,
            Image
        }

        public FileType FileType { get; set; }

        public AntiOutSiteLinkAttribute(FileType fileType) => this.FileType = fileType;

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            
        }
    }
}
