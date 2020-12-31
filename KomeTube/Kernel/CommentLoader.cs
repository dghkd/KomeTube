using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;

using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using KomeTube.Kernel.YtLiveChatDataModel;

namespace KomeTube.Kernel
{
    public class CommentLoader
    {
        #region Private Member

        private String _videoUrl;
        private String _currentContinuation;
        private readonly Object _lockContinuation;

        private Task _mainTask;
        private CancellationTokenSource _mainTaskCancelTS;

        #endregion Private Member

        #region Constructor

        public CommentLoader()
        {
            _videoUrl = "";
            _currentContinuation = "";
            _lockContinuation = new object();
        }

        #endregion Constructor

        #region Public Member

        /// <summary>
        /// 使用者輸入的直播影片網址
        /// </summary>
        public String VideoUrl
        {
            get
            {
                return _videoUrl;
            }

            set
            {
                _videoUrl = value;
            }
        }

        /// <summary>
        /// 當前的continuation資料，用來取得下次留言列表
        /// </summary>
        public String CurrentContinuation
        {
            get
            {
                lock (_lockContinuation)
                {
                    return _currentContinuation;
                }
            }

            set
            {
                lock (_lockContinuation)
                {
                    _currentContinuation = value;
                }
            }
        }

        /// <summary>
        /// CommentLoader當前執行狀態
        /// </summary>
        public CommentLoaderStatus Status { get; private set; }

        #endregion Public Member

        #region Public Method

        /// <summary>
        /// 開始讀取留言
        /// <para>請監聽OnCommentsReceive事件取得留言列表</para>
        /// </summary>
        /// <param name="url">Youtube直播影片位址</param>
        public void Start(String url)
        {
            if (_mainTask != null
                && !_mainTask.IsCompleted)
            {
                //若任務仍在執行則不再重複啟動
                return;
            }

            _mainTaskCancelTS = new CancellationTokenSource();
            _mainTask = Task.Factory.StartNew(() => StartGetComments(url), _mainTaskCancelTS.Token);
            _mainTask.ContinueWith(StartGetCommentsCompleted, _mainTaskCancelTS.Token);
        }

        /// <summary>
        /// 停止取得留言
        /// </summary>
        public void Stop()
        {
            if (_mainTaskCancelTS != null)
            {
                //停止取得留言
                _mainTaskCancelTS.Cancel();
                RaiseStatusChanged(CommentLoaderStatus.StopRequested);
            }
        }

        #endregion Public Method

        #region Private Method

        /// <summary>
        /// 從影片位址解析vid後取得聊天室位址
        /// </summary>
        /// <param name="videoUrl">影片位址</param>
        /// <returns>回傳聊天室位址，若失敗則發出CanNotGetLiveChatUrl Error事件，並回傳空字串</returns>
        private String GetLiveChatRoomUrl(String videoUrl)
        {
            const String baseUrl = "www.youtube.com/watch?";
            String ret = "";
            String urlParamStr = videoUrl.Substring(videoUrl.IndexOf(baseUrl) + baseUrl.Length);
            String[] urlParamArr = urlParamStr.Split('&');
            String vid = "";

            //取得vid
            foreach (String param in urlParamArr)
            {
                if (param.IndexOf("v=") == 0)
                {
                    vid = param.Substring(2);
                    break;
                }
            }

            if (vid == "")
            {
                Debug.WriteLine(String.Format("[GetLiveChatRoomUrl] 無法取得聊天室位址. URL={0}", videoUrl));
                RaiseError(CommentLoaderErrorCode.CanNotGetLiveChatUrl, videoUrl);
                return "";
            }
            else
            {
                ret = String.Format("https://www.youtube.com/live_chat?v={0}&is_popout=1", vid);
            }

            return ret;
        }

        /// <summary>
        /// 取得Youtube API 'get_live_chat'的位址
        /// </summary>
        /// <param name="continuation">continuation參數。此參數應從ParseLiveChatHtml或GetComment方法取得</param>
        /// <returns>回傳Youtube API 'get_live_chat'的位址</returns>
        private String GetLiveChatUrl(String continuation)
        {
            String ret = String.Format(@"https://www.youtube.com/live_chat/get_live_chat?continuation={0}&pbj=1", continuation);

            return ret;
        }

        /// <summary>
        /// 開始取得留言，此方法將會進入長時間迴圈，若要停止請使用_mainTaskCancelTS發出cancel請求
        /// </summary>
        /// <param name="url">直播影片位址</param>
        private void StartGetComments(String url)
        {
            RaiseStatusChanged(CommentLoaderStatus.Started);

            String continuation = "";

            //取得聊天室位址
            String liveChatRoomUrl = GetLiveChatRoomUrl(url);
            if (liveChatRoomUrl == "")
            {
                Debug.WriteLine(String.Format("[StartGetComments] GetLiveChatRoomUrl無法取得html內容"));
                return;
            }

            //取得continuation和第一次訪問的留言列表
            List<CommentData> firstCommentList = ParseLiveChatHtml(liveChatRoomUrl, ref continuation);
            if (continuation == "")
            {
                Debug.WriteLine(String.Format("[StartGetComments] ParseLiveChatHtml無法取得continuation參數"));
                return;
            }

            this.VideoUrl = url;
            this.CurrentContinuation = continuation;
            RaiseCommentsReceive(firstCommentList);

            //持續取得留言
            while (!_mainTaskCancelTS.IsCancellationRequested)
            {
                List<CommentData> comments = GetComments(ref continuation);
                this.CurrentContinuation = continuation;

                if (comments != null
                    && comments.Count > 0)
                {
                    RaiseCommentsReceive(comments);
                }

                if (continuation == "")
                {
                    Debug.WriteLine(String.Format("[StartGetComments] GetComments無法取得continuation參數"));
                    return;
                }

                SpinWait.SpinUntil(() => false, 1000);
            }
        }

        /// <summary>
        /// 取得留言的Task結束(StartGetComments方法結束)
        /// </summary>
        /// <param name="sender">已完成的Task</param>
        /// <param name="obj"></param>
        private void StartGetCommentsCompleted(Task sender, object obj)
        {
            if (sender.IsFaulted)
            {
                //取得留言時發生其他exception造成Task結束
                RaiseError(CommentLoaderErrorCode.GetCommentsError, sender.Exception.Message);
            }

            RaiseStatusChanged(CommentLoaderStatus.Completed);
        }

        /// <summary>
        /// 在html中取出window["ytInitialData"] 後方的json code，並解析出continuation
        /// </summary>
        /// <param name="liveChatUrl">聊天室位址</param>
        /// <returns>回傳continuation參數值</returns>
        private List<CommentData> ParseLiveChatHtml(String liveChatUrl, ref String continuation)
        {
            String htmlContent = "";
            List<CommentData> initComments = new List<CommentData>();

            RaiseStatusChanged(CommentLoaderStatus.GetLiveChatHtml);
            //取得HTML內容
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    client.DefaultRequestHeaders.Add("User-Agent", @"Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:59.0) Gecko/20100101 Firefox/59.0");
                    htmlContent = client.GetStringAsync(liveChatUrl).Result;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(String.Format("[ParseLiveChatHtml] 無法取得聊天室HTML內容. Excetion:{0}", e.Message));
                    RaiseError(CommentLoaderErrorCode.CanNotGetLiveChatHtml, e.Message);
                    return null;
                }
            }
            RaiseStatusChanged(CommentLoaderStatus.ParseLiveChatHtml);

            //解析HTML
            Match match = Regex.Match(htmlContent, "window\\[\"ytInitialData\"\\] = ({.+});\\s*</script>", RegexOptions.Singleline);
            if (!match.Success)
            {
                Debug.WriteLine(String.Format("[ParseLiveChatHtml] 無法解析HTML. HTML content:{0}", htmlContent));
                RaiseError(CommentLoaderErrorCode.CanNotParseLiveChatHtml, htmlContent);
                return null;
            }

            //解析json data
            String ytInitialData = match.Groups[1].Value;
            dynamic jsonData;
            try
            {
                jsonData = JsonConvert.DeserializeObject<Dictionary<String, dynamic>>(ytInitialData);

                var data = jsonData["contents"]["liveChatRenderer"]["continuations"][0]["invalidationContinuationData"];
                if (data == null)
                {
                    data = jsonData["contents"]["liveChatRenderer"]["continuations"][0]["timedContinuationData"];
                }
                continuation = Convert.ToString(JsonHelper.TryGetValue(data, "continuation", ""));

                var actions = jsonData["contents"]["liveChatRenderer"]["actions"];
                initComments = ParseComment(actions);
            }
            catch (Exception e)
            {
                Debug.WriteLine(String.Format("[ParseLiveChatHtml] 無法解析json data:{0}", e.Message));
                RaiseError(CommentLoaderErrorCode.CanNotParseLiveChatHtml, ytInitialData);
                return null;
            }

            return initComments;
        }

        /// <summary>
        /// 利用Youtube API 'get_live_chat'取得聊天室留言，並解析continuation參數供下次取得留言使用
        /// </summary>
        /// <param name="continuation">Continuation參數</param>
        /// <returns>成功時回傳留言資料，失敗則回傳null。</returns>
        private List<CommentData> GetComments(ref String continuation)
        {
            if (continuation == null || continuation == "")
            {
                RaiseError(CommentLoaderErrorCode.GetCommentsError, new Exception("continuation參數錯誤"));
                return null;
            }

            RaiseStatusChanged(CommentLoaderStatus.GetComments);

            String chatUrl = GetLiveChatUrl(continuation);
            List<CommentData> ret = null;
            String resp = "";

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    //取得聊天室留言
                    client.DefaultRequestHeaders.Add("User-Agent", @"Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:59.0) Gecko/20100101 Firefox/59.0");
                    resp = client.GetStringAsync(chatUrl).Result;

                    //解析continuation供下次取得留言使用
                    dynamic jsonData = JsonConvert.DeserializeObject<Dictionary<String, dynamic>>(resp);
                    var data = jsonData["response"]["continuationContents"]["liveChatContinuation"]["continuations"][0]["invalidationContinuationData"];
                    if (data == null)
                    {
                        data = jsonData["response"]["continuationContents"]["liveChatContinuation"]["continuations"][0]["timedContinuationData"];
                    }
                    continuation = Convert.ToString(JsonHelper.TryGetValue(data, "continuation", ""));

                    //解析留言資料
                    var commentActions = jsonData["response"]["continuationContents"]["liveChatContinuation"]["actions"];
                    ret = ParseComment(commentActions);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(String.Format("[GetComments] 無法取得聊天室HTML內容. Excetion:{0}", e.Message));
                    RaiseError(CommentLoaderErrorCode.GetCommentsError, e.Message);
                    return null;
                }
            }

            return ret;
        }

        /// <summary>
        /// 解析留言資訊
        /// </summary>
        /// <param name="commentActions">json data.</param>
        /// <returns>成功則回傳留言列表，失敗則回傳null</returns>
        private List<CommentData> ParseComment(dynamic commentActions)
        {
            if (commentActions == null)
            {
                return null;
            }

            List<CommentData> ret = new List<CommentData>();
            for (int i = 0; i < commentActions.Count; i++)
            {
                CommentData cmt = new CommentData();
                cmt.addChatItemAction.clientId = Convert.ToString(JsonHelper.TryGetValueByXPath(commentActions[i], "addChatItemAction.clientId", ""));

                var txtMsgRd = JsonHelper.TryGetValueByXPath(commentActions[i], "addChatItemAction.item.liveChatTextMessageRenderer", null);
                if (txtMsgRd != null)
                {
                    ParseTextMessage(cmt.addChatItemAction.item.liveChatTextMessageRenderer, txtMsgRd);
                }
                else
                {
                    dynamic paidMsgRd = JsonHelper.TryGetValueByXPath(commentActions[i], "addChatItemAction.item.liveChatPaidMessageRenderer", null);
                    if (paidMsgRd == null)
                    {
                        continue;
                    }
                    ParsePaidMessage(cmt.addChatItemAction.item.liveChatPaidMessageRenderer, paidMsgRd);
                }

                ret.Add(cmt);
            }
            return ret;
        }

        /// <summary>
        /// 解析付費留言資訊
        /// </summary>
        /// <param name="liveChatPaidMessageRenderer">付費留言</param>
        /// <param name="paidMsgRd">json data.</param>
        private void ParsePaidMessage(LiveChatPaidMessageRenderer liveChatPaidMessageRenderer, dynamic paidMsgRd)
        {
            //解析留言內容
            ParseTextMessage(liveChatPaidMessageRenderer, paidMsgRd);

            //解析付費留言內容
            liveChatPaidMessageRenderer.purchaseAmountText.simpleText = Convert.ToString(JsonHelper.TryGetValueByXPath(paidMsgRd, "purchaseAmountText.simpleText", ""));
            liveChatPaidMessageRenderer.headerBackgroundColor = Convert.ToInt64(JsonHelper.TryGetValueByXPath(paidMsgRd, "headerBackgroundColor", 0));
            liveChatPaidMessageRenderer.headerTextColor = Convert.ToInt64(JsonHelper.TryGetValueByXPath(paidMsgRd, "headerTextColor", 0));
            liveChatPaidMessageRenderer.bodyBackgroundColor = Convert.ToInt64(JsonHelper.TryGetValueByXPath(paidMsgRd, "bodyBackgroundColor", 0));
            liveChatPaidMessageRenderer.bodyTextColor = Convert.ToInt64(JsonHelper.TryGetValueByXPath(paidMsgRd, "bodyTextColor", 0));
            liveChatPaidMessageRenderer.authorNameTextColor = Convert.ToInt64(JsonHelper.TryGetValueByXPath(paidMsgRd, "authorNameTextColor", 0));
            liveChatPaidMessageRenderer.timestampColor = Convert.ToInt64(JsonHelper.TryGetValueByXPath(paidMsgRd, "timestampColor", 0));
        }

        /// <summary>
        /// 解析留言內容
        /// </summary>
        /// <param name="liveChatTextMessageRenderer"></param>
        /// <param name="txtMsgRd">json data.</param>
        private void ParseTextMessage(LiveChatTextMessageRenderer liveChatTextMessageRenderer, dynamic txtMsgRd)
        {
            liveChatTextMessageRenderer.authorExternalChannelId = Convert.ToString(JsonHelper.TryGetValueByXPath(txtMsgRd, "authorExternalChannelId", ""));
            liveChatTextMessageRenderer.authorName.simpleText = Convert.ToString(JsonHelper.TryGetValueByXPath(txtMsgRd, "authorName.simpleText", ""));
            liveChatTextMessageRenderer.authorPhoto.thumbnails = ParseAuthorPhotoThumb(JsonHelper.TryGetValueByXPath(txtMsgRd, "authorPhoto.thumbnails", null));
            liveChatTextMessageRenderer.contextMenuAccessibility.accessibilityData.label = Convert.ToString(JsonHelper.TryGetValueByXPath(txtMsgRd, "contextMenuAccessibility.accessibilityData.label", ""));
            liveChatTextMessageRenderer.id = Convert.ToString(JsonHelper.TryGetValueByXPath(txtMsgRd, "id", ""));
            liveChatTextMessageRenderer.timestampUsec = Convert.ToInt64(JsonHelper.TryGetValueByXPath(txtMsgRd, "timestampUsec", 0));

            //留言包含自訂表情符號或空格時runs陣列會分割成多元素
            dynamic runs = JsonHelper.TryGetValueByXPath(txtMsgRd, "message.runs");
            if (runs != null)
            {
                for (int i = 0; i < runs.Count; i++)
                {
                    string xPath = String.Format($"message.runs.{i.ToString()}.text");
                    liveChatTextMessageRenderer.message.simpleText += Convert.ToString(JsonHelper.TryGetValueByXPath(txtMsgRd, xPath, ""));
                }
            }
            else
                liveChatTextMessageRenderer.message.simpleText = "";

            var authorBadges = JsonHelper.TryGetValueByXPath(txtMsgRd, "authorBadges", null);
            if (authorBadges != null)
            {
                //留言者可能擁有多個徽章 (EX:管理員、會員)
                for (int i = 0; i < authorBadges.Count; i++)
                {
                    AuthorBadge badge = new AuthorBadge();
                    badge.tooltip = Convert.ToString(JsonHelper.TryGetValueByXPath(authorBadges[i], "liveChatAuthorBadgeRenderer.tooltip"));
                    liveChatTextMessageRenderer.authorBadges.Add(badge);
                }
            }
        }

        /// <summary>
        /// 解析留言者縮圖
        /// </summary>
        /// <param name="authorPhotoData">json data.</param>
        private List<Thumbnail> ParseAuthorPhotoThumb(dynamic authorPhotoData)
        {
            if (authorPhotoData == null)
            {
                return null;
            }

            List<Thumbnail> ret = new List<Thumbnail>();

            for (int i = 0; i < authorPhotoData.Count; i++)
            {
                Thumbnail thumb = new Thumbnail();
                thumb.url = JsonHelper.TryGetValue(authorPhotoData[i], "url", "");
                thumb.width = JsonHelper.TryGetValue(authorPhotoData[i], "width", "");
                thumb.height = JsonHelper.TryGetValue(authorPhotoData[i], "height", "");
                ret.Add(thumb);
            }

            return ret;
        }

        #endregion Private Method

        #region Event

        public delegate void ErrorHandleMethod(CommentLoader sender, CommentLoaderErrorCode errCode, object obj);

        /// <summary>
        /// CommentLoader發生錯誤事件
        /// </summary>
        public event ErrorHandleMethod OnError;

        /// <summary>
        /// 發出錯誤事件
        /// </summary>
        /// <param name="errCode">錯誤碼</param>
        /// <param name="obj">附帶的錯誤資訊</param>
        private void RaiseError(CommentLoaderErrorCode errCode, object obj)
        {
            Debug.WriteLine(String.Format("[RaiseError] errCode:{0}, {1}", errCode.ToString(), obj));
            if (OnError != null)
            {
                OnError(this, errCode, obj);
            }
        }

        public delegate void CommentsReceiveMethod(CommentLoader sender, List<CommentData> lsComments);

        /// <summary>
        /// CommentLoader取得新留言事件
        /// </summary>
        public event CommentsReceiveMethod OnCommentsReceive;

        /// <summary>
        /// 發出收到留言事件
        /// </summary>
        /// <param name="lsComments">收到的留言資料列表</param>
        private void RaiseCommentsReceive(List<CommentData> lsComments)
        {
            if (OnCommentsReceive != null)
            {
                OnCommentsReceive(this, lsComments);
            }
        }

        public delegate void StatusChangedMethod(CommentLoader sender, CommentLoaderStatus status);

        /// <summary>
        /// CommentLoader執行時發生的各階段事件
        /// <para>GetComments狀態會持續發生</para>
        /// </summary>
        public event StatusChangedMethod OnStatusChanged;

        /// <summary>
        /// 發出執行階段狀態事件
        /// </summary>
        /// <param name="status">正在執行的狀態</param>
        private void RaiseStatusChanged(CommentLoaderStatus status)
        {
            this.Status = status;
            //Debug.WriteLine(String.Format("[OnStatusChanged] {0}", this.Status.ToString()));
            if (OnStatusChanged != null)
            {
                OnStatusChanged(this, status);
            }
        }

        #endregion Event
    }
}