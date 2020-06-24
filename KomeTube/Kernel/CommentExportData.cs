using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomeTube.Kernel
{
    internal class CommentExportData
    {
        public string Date { get; set; }
        public string AuthorName { get; set; }
        public string AuthorBadges { get; set; }
        public string Message { get; set; }
        public string PaidMsg { get; set; }
        public string AuthorID { get; set; }
    }
}