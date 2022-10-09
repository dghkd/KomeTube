using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using KomeTube.Common;
using KomeTube.Kernel.YtLiveChatDataModel;

namespace KomeTube.ViewModel
{
    public class CommentVM : ViewModelBase
    {
        #region Private Member

        private CommentData _data;
        private DateTime _dateTime;
        private bool _isEnableCopyMessage;

        #endregion Private Member

        #region Constructor

        public CommentVM(CommentData data)
        {
            _data = data;

            this.IsEnableCopyMessage = true;
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
                string msgText = "";
                if (_data.addChatItemAction.item.IsPaidMessage)
                {
                    msgText = FormatMessageText(_data.addChatItemAction.item.liveChatPaidMessageRenderer.message);
                    _data.addChatItemAction.item.liveChatPaidMessageRenderer.message.simpleText = msgText;

                    return String.Format("{0} {1}",
                        _data.addChatItemAction.item.liveChatPaidMessageRenderer.purchaseAmountText.simpleText,
                        _data.addChatItemAction.item.liveChatPaidMessageRenderer.message.simpleText);
                }
                else
                {
                    msgText = FormatMessageText(_data.addChatItemAction.item.liveChatTextMessageRenderer.message);
                    _data.addChatItemAction.item.liveChatTextMessageRenderer.message.simpleText = msgText;

                    return _data.addChatItemAction.item.liveChatTextMessageRenderer.message.simpleText;
                }
            }
        }

        /// <summary>
        /// 留言內容全文字版，表情符號改由shortcut表示，而非圖片網址
        /// </summary>
        public string ContentMessage
        {
            get
            {
                string content = "";
                if (_data.addChatItemAction.item.IsPaidMessage)
                {
                    content = FormatContentMessage(_data.addChatItemAction.item.liveChatPaidMessageRenderer.message);

                    return String.Format("{0} {1}",
                        _data.addChatItemAction.item.liveChatPaidMessageRenderer.purchaseAmountText.simpleText,
                        content);
                }
                else
                {
                    content = FormatContentMessage(_data.addChatItemAction.item.liveChatTextMessageRenderer.message);

                    return content;
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

        public String AuthorChannelUrl
        {
            get
            {
                string url = @"https://www.youtube.com/channel/" + this.AuthorID;
                return url;
            }
        }

        public bool IsEnableCopyMessage
        {
            get
            {
                return _isEnableCopyMessage;
            }
            set
            {
                if (_isEnableCopyMessage != value)
                {
                    _isEnableCopyMessage = value;
                    OnPropertyChanged(nameof(this.IsEnableCopyMessage));
                }
            }
        }

        #endregion Public Member

        #region Command

        private CommandBase _cmdOpenAuthorChannelUrl;

        public CommandBase CmdOpenAuthorChannelUrl
        {
            get
            {
                return _cmdOpenAuthorChannelUrl ?? (_cmdOpenAuthorChannelUrl = new CommandBase(x => OpenAuthorChannelUrl()));
            }
        }

        private CommandBase _cmdCopyContentMessage;

        public CommandBase CmdCopyContentMessage
        {
            get
            {
                return _cmdCopyContentMessage ?? (_cmdCopyContentMessage = new CommandBase(x => CopyContentMessage()));
            }
        }

        public Func<String, CommentVM, bool> CommandAction;

        private void ExecuteCommand(String cmd)
        {
            if (this.CommandAction != null)
            {
                this.CommandAction(cmd, this);
            }
        }

        #endregion Command

        #region Private Method

        private string FormatMessageText(Message msg)
        {
            string ret = "";
            for (int i = 0; i < msg.runs.Count; i++)
            {
                Runs r = msg.runs[i];
                ret += r.text;
                ret += FormatEmojiImage(r.emoji);
            }

            return ret;
        }

        private string FormatContentMessage(Message msg)
        {
            string ret = "";
            for (int i = 0; i < msg.runs.Count; i++)
            {
                Runs r = msg.runs[i];
                ret += r.text;
                if (r.emoji.shortcuts.Count > 0)
                {
                    //判斷表情符號類型
                    if (r.emoji.isCustomEmoji
                        && r.emoji.shortcuts.Count >= 2)
                    {
                        //頻道自訂表符
                        ret += r.emoji.shortcuts[1];
                    }
                    else if (r.emoji.isCustomEmoji
                        && r.emoji.shortcuts.Count < 2)
                    {
                        //YT通用自訂表符
                        ret += r.emoji.shortcuts[0];
                    }
                    else
                    {
                        //文字符號表符
                        ret += r.emoji.emojiId;
                    }
                }
            }

            return ret;
        }

        private string FormatEmojiImage(Emoji emoji)
        {
            if (emoji == null)
            {
                return "";
            }

            string ret = "";
            if (emoji.isCustomEmoji)
            {
                Thumbnails thumb = emoji.image.thumbnails.ElementAtOrDefault(0);
                if (thumb != null)
                {
                    string url = thumb.url;
                    int w = thumb.width;
                    int h = thumb.height;

                    ret = $"[img source='{url}' width={w} height={h}]";
                }
            }
            else
            {
                ret = emoji.emojiId;
            }

            return ret;
        }

        private void OpenAuthorChannelUrl()
        {
            System.Diagnostics.Process.Start(this.AuthorChannelUrl);
        }

        private void CopyContentMessage()
        {
            try
            {
                Clipboard.SetText(this.ContentMessage);
                this.IsEnableCopyMessage = false;
                Task.Run(() =>
                {
                    SpinWait.SpinUntil(() => false, 500);
                    this.IsEnableCopyMessage = true;
                });
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        #endregion Private Method
    }
}