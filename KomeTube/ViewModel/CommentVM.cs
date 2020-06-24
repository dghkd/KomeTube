using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KomeTube.Common;
using KomeTube.Kernel.YtLiveChatDataModel;

namespace KomeTube.ViewModel
{
    public class CommentVM : ViewModelBase
    {
        #region Private Member

        private CommentData _data;
        private DateTime _dateTime;

        #endregion Private Member

        #region Constructor

        public CommentVM(CommentData data)
        {
            _data = data;
        }

        #endregion Constructor

        #region Public Member

        /// <summary>
        /// 取得留言時間
        /// <para>若要使用格式化後的留言時間字串請使用DateTimeText</para>
        /// </summary>
        public DateTime Date
        {
            get
            {
                if (DateTime.MinValue != _dateTime)
                {
                    return _dateTime;
                }

                if (_data.addChatItemAction.item.IsPaidMessage)
                {
                    double timeStamp = (double)_data.addChatItemAction.item.liveChatPaidMessageRenderer.timestampUsec / 1000000.0;
                    _dateTime = new DateTime(1970, 1, 1).AddSeconds(timeStamp).ToLocalTime();
                }
                else
                {
                    double timeStamp = (double)_data.addChatItemAction.item.liveChatTextMessageRenderer.timestampUsec / 1000000.0;
                    _dateTime = new DateTime(1970, 1, 1).AddSeconds(timeStamp).ToLocalTime();
                }

                return _dateTime;
            }
        }

        /// <summary>
        /// 取得格式化後留言時間字串
        /// </summary>
        public String DateTimeText
        {
            get
            {
                return this.Date.ToString("HH:mm:ss");
            }
        }

        /// <summary>
        /// 取得留言者名稱
        /// </summary>
        public String AuthorName
        {
            get
            {
                if (_data.addChatItemAction.item.IsPaidMessage)
                {
                    return _data.addChatItemAction.item.liveChatPaidMessageRenderer.authorName.simpleText;
                }
                else
                {
                    return _data.addChatItemAction.item.liveChatTextMessageRenderer.authorName.simpleText;
                }
            }

            set
            {
            }
        }

        /// <summary>
        /// 取得留言者徽章稱號
        /// </summary>
        public String AuthorBadges
        {
            get
            {
                string ret = "";

                if (_data.addChatItemAction.item.IsPaidMessage)
                {
                    foreach (var badge in _data.addChatItemAction.item.liveChatPaidMessageRenderer.authorBadges)
                    {
                        ret += badge.tooltip + " ";
                    }
                }
                else
                {
                    foreach (var badge in _data.addChatItemAction.item.liveChatTextMessageRenderer.authorBadges)
                    {
                        ret += badge.tooltip + " ";
                    }
                }

                return ret;
            }
        }

        /// <summary>
        /// 取得留言內容
        /// <para>若是付費留言則會顯示為"¥{金額} {留言內容}</para>
        /// </summary>
        public String Message
        {
            get
            {
                if (_data.addChatItemAction.item.IsPaidMessage)
                {
                    return String.Format("{0} {1}",
                        _data.addChatItemAction.item.liveChatPaidMessageRenderer.purchaseAmountText.simpleText,
                        _data.addChatItemAction.item.liveChatPaidMessageRenderer.message.simpleText);
                }
                else
                {
                    return _data.addChatItemAction.item.liveChatTextMessageRenderer.message.simpleText;
                }
            }
        }

        /// <summary>
        /// 取得付費金額(包含貨幣符號)，若非付費留言則回傳null
        /// </summary>
        public String PaidMessage
        {
            get
            {
                if (_data.addChatItemAction.item.IsPaidMessage)
                {
                    return _data.addChatItemAction.item.liveChatPaidMessageRenderer.purchaseAmountText.simpleText;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// 取得留言者頭像網址
        /// </summary>
        public String AuthorPhotoUrl
        {
            get
            {
                //thumbnails[0]: 32*32 size
                //thumbnails[1]: 64*64 size

                if (_data.addChatItemAction.item.IsPaidMessage)
                {
                    return _data.addChatItemAction.item.liveChatPaidMessageRenderer.authorPhoto.thumbnails[0].url;
                }
                else
                {
                    return _data.addChatItemAction.item.liveChatTextMessageRenderer.authorPhoto.thumbnails[0].url;
                }
            }
        }

        /// <summary>
        /// 取得留言者ID
        /// </summary>
        public String AuthorID
        {
            get
            {
                if (_data.addChatItemAction.item.IsPaidMessage)
                {
                    return _data.addChatItemAction.item.liveChatPaidMessageRenderer.authorExternalChannelId;
                }
                else
                {
                    return _data.addChatItemAction.item.liveChatTextMessageRenderer.authorExternalChannelId;
                }
            }
        }

        #endregion Public Member
    }
}