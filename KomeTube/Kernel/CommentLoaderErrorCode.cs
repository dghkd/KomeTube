using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomeTube.Kernel
{
    public enum CommentLoaderErrorCode
    {
        /// <summary>
        /// 無法取得聊天室位址
        /// <para>發生此錯誤時，附帶的錯誤資訊為使用者輸入的影片位址(URL)</para>
        /// </summary>
        CanNotGetLiveChatUrl,

        /// <summary>
        /// 無法取得聊天室HTML內容
        /// <para>發生此錯誤時，附帶的錯誤資訊為Exception message.</para>
        /// </summary>
        CanNotGetLiveChatHtml,

        /// <summary>
        /// 無法解析HTML
        /// <para>發生此錯誤時，附帶的錯誤資訊為聊天室HTML內容.</para>
        /// </summary>
        CanNotParseLiveChatHtml,

        /// <summary>
        /// 取得留言時發生錯誤
        /// <para>發生此錯誤時，附帶的錯誤資訊為Exception message.</para>
        /// </summary>
        GetCommentsError,
    }
}