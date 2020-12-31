using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.IO;
using System.Globalization;

using CsvHelper;

using KomeTube.Common;
using KomeTube.Kernel;
using KomeTube.Kernel.YtLiveChatDataModel;

namespace KomeTube.ViewModel
{
    public class MainWindowVM : ViewModelBase
    {
        #region Private Member

        private String _videoUrl;
        private bool _isStopped;
        private bool _isEnableStop;
        private int _totalCommentCount;
        private int _totalAuthorCount;

        /// <summary>
        /// Key:Author ID, Value:Author Name
        /// </summary>
        private Dictionary<String, String> _authorTable;

        private String _statusText;
        private String _errText;

        private CommentLoader _cmtLoader;
        private object _lockCommentColleObj = new object();
        private ObservableCollection<CommentVM> _commentColle;

        private VotingCenterVM _votingCenterVM;
        private PuzzleCenterVM _puzzleCenterVM;
        private AssessmentCenterVM _assessmentCenterVM;

        #endregion Private Member

        #region Constructor

        public MainWindowVM()
        {
            _cmtLoader = new CommentLoader();
            _commentColle = new ObservableCollection<CommentVM>();
            this.CommentColle = CollectionViewSource.GetDefaultView(_commentColle);
            BindingOperations.EnableCollectionSynchronization(_commentColle, _lockCommentColleObj);

            _votingCenterVM = null;
            _puzzleCenterVM = null;
            _assessmentCenterVM = null;
            _authorTable = new Dictionary<string, string>();
            _isStopped = true;

            _cmtLoader.OnStatusChanged += On_CommetLoader_StatusChanged;
            _cmtLoader.OnError += On_CommentLoader_Error;
            _cmtLoader.OnCommentsReceive += On_CommentLoader_CommentsReceive;

            this.StatusText = "已停止";
        }

        #endregion Constructor

        #region Public Member

        /// <summary>
        /// 取得或設定影片網址
        /// </summary>
        public String VideoUrl
        {
            get { return _videoUrl; }
            set { _videoUrl = value; OnPropertyChanged(nameof(VideoUrl)); }
        }

        /// <summary>
        /// 取得是否已停止擷取留言狀態
        /// </summary>
        public bool IsStopped
        {
            get { return _isStopped; }
            private set { _isStopped = value; OnPropertyChanged(nameof(IsStopped)); }
        }

        /// <summary>
        /// 取得或設定是否允許暫停
        /// </summary>
        public bool IsEnableStop
        {
            get { return _isEnableStop; }
            set { _isEnableStop = value; OnPropertyChanged(nameof(IsEnableStop)); }
        }

        /// <summary>
        /// 取得總留言數量
        /// </summary>
        public int TotalCommentCount
        {
            get { return _totalCommentCount; }
            private set { _totalCommentCount = value; OnPropertyChanged(nameof(TotalCommentCount)); }
        }

        /// <summary>
        /// 取得總留言人數
        /// </summary>
        public int TotalAuthorCount
        {
            get { return _totalAuthorCount; }
            private set { _totalAuthorCount = value; OnPropertyChanged(nameof(TotalAuthorCount)); }
        }

        /// <summary>
        /// 目前處理狀態
        /// </summary>
        public String StatusText
        {
            get { return _statusText; }
            set { _statusText = value; OnPropertyChanged(nameof(StatusText)); }
        }

        /// <summary>
        /// 發生錯誤時的錯誤訊息
        /// </summary>
        public String ErrorText
        {
            get { return _errText; }
            set { _errText = value; OnPropertyChanged(nameof(ErrorText)); }
        }

        /// <summary>
        /// 留言集合
        /// </summary>
        public ICollectionView CommentColle
        {
            get;
            private set;
        }

        #endregion Public Member

        #region Command

        public const string CmdKey_Start = "CmdKey_Start";
        private CommandBase _cmdStart;

        public CommandBase CmdStart
        {
            get
            {
                return _cmdStart ?? (_cmdStart = new CommandBase(x => ExecuteCommand(CmdKey_Start)));
            }
        }

        public const string CmdKey_Stop = "CmdKey_Stop";
        private CommandBase _cmdStop;

        public CommandBase CmdStop
        {
            get
            {
                return _cmdStop ?? (_cmdStop = new CommandBase(x => ExecuteCommand(CmdKey_Stop)));
            }
        }

        public const string CmdKey_Vote = "CmdKey_Vote";
        private CommandBase _cmdVote;

        public CommandBase CmdVote
        {
            get
            {
                return _cmdVote ?? (_cmdVote = new CommandBase(x => ExecuteCommand(CmdKey_Vote), CanExecuteVote));
            }
        }

        public const string CmdKey_Puzzle = "CmdKey_Puzzle";
        private CommandBase _cmdPuzzle;

        public CommandBase CmdPuzzle
        {
            get
            {
                return _cmdPuzzle ?? (_cmdPuzzle = new CommandBase(x => ExecuteCommand(CmdKey_Puzzle), CanExecutePuzzle));
            }
        }

        private CommandBase _cmdAssessment;

        public CommandBase CmdAssessment
        {
            get
            {
                return _cmdAssessment ?? (_cmdAssessment = new CommandBase(x => ExecuteCommand(nameof(this.CmdAssessment)), CanExecuteAssessment));
            }
        }

        public const string CmdKey_ExportComment = "CmdKey_ExportComment";
        private CommandBase _cmdExportComment;

        public CommandBase CmdExportComment
        {
            get
            {
                return _cmdExportComment ?? (_cmdExportComment = new CommandBase(x => ExecuteCommand(CmdKey_ExportComment)));
            }
        }

        public Func<String, MainWindowVM, bool> CommandAction;

        private void ExecuteCommand(String cmd)
        {
            if (this.CommandAction != null)
            {
                this.CommandAction(cmd, this);
            }
        }

        #endregion Command

        #region Public Method

        /// <summary>
        /// 開始取得留言
        /// </summary>
        public void Start()
        {
            _commentColle.Clear();
            _authorTable.Clear();
            this.ErrorText = null;
            _cmtLoader.Start(this.VideoUrl);
        }

        /// <summary>
        /// 停止取得留言
        /// </summary>
        public void Stop()
        {
            _cmtLoader.Stop();
        }

        /// <summary>
        /// 以CSV格式匯出留言
        /// </summary>
        public void ExportComment()
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "CSV |*.csv",
                Title = "匯出留言",
                DefaultExt = ".csv"
            };

            Nullable<bool> result = dlg.ShowDialog();
            if (result != true)
            {
                return;
            }

            ExportComment(dlg.FileName);
        }

        /// <summary>
        /// 以CSV格式匯出留言
        /// </summary>
        public void ExportComment(string filename)
        {
            using (StreamWriter sw = new StreamWriter(filename, false, Encoding.UTF8))
            {
                using (CsvWriter cw = new CsvWriter(sw, CultureInfo.InvariantCulture))
                {
                    cw.Configuration.HasHeaderRecord = false;
                    cw.Configuration.SanitizeForInjection = true;

                    //寫入內容
                    List<CommentVM> commentLs = _commentColle.ToList();
                    foreach (CommentVM vm in commentLs)
                    {
                        CommentExportData data = new CommentExportData()
                        {
                            Date = string.Format("{0:yyyy/MM/dd HH:mm:ss.fff}", vm.Date),
                            AuthorName = vm.AuthorName,
                            AuthorBadges = vm.AuthorBadges,
                            Message = vm.Message,
                            PaidMsg = vm.PaidMessage,
                            AuthorID = vm.AuthorID
                        };
                        cw.WriteRecord<CommentExportData>(data);
                        cw.NextRecord();
                    }
                }
            }
        }

        /// <summary>
        /// 建立投票所View Model.
        /// <para>當投票所使用完畢後需要呼叫VotingCenterVM.Close成員方法進行關閉，才能建立新的投票所</para>
        /// </summary>
        /// <returns>回傳新的投票所，若投票所尚未關閉則回傳NULL</returns>
        public VotingCenterVM CreateVotingCenter()
        {
            if (_votingCenterVM == null
                || _votingCenterVM.IsClosed)
            {
                _votingCenterVM = new VotingCenterVM();
                return _votingCenterVM;
            }

            return null;
        }

        /// <summary>
        /// 建立猜謎View Model.
        /// <para>當猜謎使用完畢後需要呼叫PuzzleCenterVM.Close成員方法進行關閉，才能建立新的猜謎</para>
        /// </summary>
        /// <returns>回傳新的猜謎，若猜謎尚未關閉則回傳NULL</returns>
        public PuzzleCenterVM CreatePuzzleCenter()
        {
            if (_puzzleCenterVM == null
                || _puzzleCenterVM.IsClosed)
            {
                _puzzleCenterVM = new PuzzleCenterVM();
                return _puzzleCenterVM;
            }

            return null;
        }

        /// <summary>
        /// 建立評分View Model
        /// <para>當評分使用完畢後須呼叫AssessmentCenterVM.Close成員方法進行關閉，才能建立新的評分</para>
        /// </summary>
        /// <returns>回傳新的評分，若評分尚未關閉則回傳NULL</returns>
        public AssessmentCenterVM CreateAssessmentCenter()
        {
            if (_assessmentCenterVM == null
                || _assessmentCenterVM.IsClosed == true)
            {
                _assessmentCenterVM = new AssessmentCenterVM();
                return _assessmentCenterVM;
            }

            return null;
        }

        #endregion Public Method

        #region Private Method

        private bool CanExecuteVote(object arg)
        {
            if (_votingCenterVM == null
                || _votingCenterVM.IsClosed)
            {
                return true;
            }
            return false;
        }

        private bool CanExecutePuzzle(object arg)
        {
            if (_puzzleCenterVM == null
                || _puzzleCenterVM.IsClosed)
            {
                return true;
            }
            return false;
        }

        private bool CanExecuteAssessment(object arg)
        {
            if (_assessmentCenterVM == null
                || _assessmentCenterVM.IsClosed)
            {
                return true;
            }
            return false;
        }

        #endregion Private Method

        #region Event Handle

        /// <summary>
        /// CommentLoader收到新留言處理函式
        /// </summary>
        /// <param name="sender">CommentLoader</param>
        /// <param name="lsComments">新留言</param>
        private void On_CommentLoader_CommentsReceive(CommentLoader sender, List<CommentData> lsComments)
        {
            if (lsComments != null)
            {
                foreach (CommentData cmt in lsComments)
                {
                    //將留言加入留言集合中
                    CommentVM vm = new CommentVM(cmt);
                    _commentColle.Add(vm);
                    this.TotalCommentCount = _commentColle.Count;

                    //紀錄留言者ID供人數統計
                    if (!_authorTable.ContainsKey(vm.AuthorID))
                    {
                        _authorTable.Add(vm.AuthorID, vm.AuthorName);
                        this.TotalAuthorCount = _authorTable.Keys.Count;
                    }

                    //將留言送至投票中心
                    if (_votingCenterVM != null)
                    {
                        _votingCenterVM.Vote(vm);
                    }

                    //將留言送至猜謎中心
                    if (_puzzleCenterVM != null)
                    {
                        _puzzleCenterVM.Guessing(vm);
                    }

                    //將留言送至評分中心
                    if (_assessmentCenterVM != null)
                    {
                        _assessmentCenterVM.Rate(vm);
                    }
                }
            }
        }

        /// <summary>
        /// CommentLoader讀取留言發生錯誤處理函式
        /// </summary>
        /// <param name="sender">CommentLoader</param>
        /// <param name="errCode">錯誤碼</param>
        /// <param name="obj">附加錯誤資訊</param>
        private void On_CommentLoader_Error(CommentLoader sender, CommentLoaderErrorCode errCode, object obj)
        {
            String errStr = "";

            switch (errCode)
            {
                case CommentLoaderErrorCode.CanNotGetLiveChatUrl:
                    errStr = String.Format("無法取得聊天室位址。請檢查輸入的網址:{0}", Convert.ToString(obj));
                    break;

                case CommentLoaderErrorCode.CanNotGetLiveChatHtml:
                    errStr = String.Format("無法取得聊天室內容。 {0}", Convert.ToString(obj));
                    break;

                case CommentLoaderErrorCode.CanNotParseLiveChatHtml:
                    errStr = String.Format("無法解析聊天室HTML。 {0}", Convert.ToString(obj));
                    break;

                case CommentLoaderErrorCode.GetCommentsError:
                    errStr = String.Format("取得留言時發生錯誤。 {0}", Convert.ToString(obj));
                    break;

                default:
                    break;
            }

            this.ErrorText = errStr;
        }

        /// <summary>
        /// CommentLoader處理狀態改變
        /// </summary>
        /// <param name="sender">CommentLoader</param>
        /// <param name="status">改變後的狀態</param>
        private void On_CommetLoader_StatusChanged(CommentLoader sender, CommentLoaderStatus status)
        {
            switch (status)
            {
                case CommentLoaderStatus.Null:
                    this.StatusText = "已停止";
                    break;

                case CommentLoaderStatus.Started:
                    this.StatusText = "開始";
                    this.IsStopped = false;
                    this.IsEnableStop = false;
                    break;

                case CommentLoaderStatus.GetLiveChatHtml:
                    this.StatusText = "讀取聊天室";
                    break;

                case CommentLoaderStatus.ParseLiveChatHtml:
                    this.StatusText = "解析聊天室內容";
                    break;

                case CommentLoaderStatus.GetComments:
                    this.StatusText = "取得留言";
                    this.IsEnableStop = true;
                    break;

                case CommentLoaderStatus.StopRequested:
                    this.StatusText = "停止中";
                    break;

                case CommentLoaderStatus.Completed:
                    this.StatusText = "已停止";
                    this.IsStopped = true;
                    this.IsEnableStop = false;
                    break;

                default:
                    break;
            }
        }

        #endregion Event Handle
    }
}