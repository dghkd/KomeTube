using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomeTube.Kernel
{
    public enum CommentLoaderStatus
    {
        Null,
        Started,
        GetLiveChatHtml,
        ParseLiveChatHtml,
        GetComments,

        StopRequested,
        Completed,
    }
}