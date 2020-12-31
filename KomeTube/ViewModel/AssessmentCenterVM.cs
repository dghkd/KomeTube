using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using KomeTube.Common;

namespace KomeTube.ViewModel
{
    public class AssessmentCenterVM : ViewModelBase
    {
        #region Private Member

        private bool _isStarted;
        private bool _isClosed;
        private DateTime _startTime;
        private TimeSpan _elapsedTime;
        private Timer _timerElapseTime;
        private bool _canChangeRate;
        private bool _canChangeRateEnable;
        private Dictionary<string, CommentVM> _raterTable;
        private Dictionary<string, int> _raterScoreTable;

        private BitmapImage _img;
        private string _contestantName;
        private string _introduction;
        private int _totalScore;
        private double _averageScore;
        private int _raterCount;
        private ObservableCollection<CommentVM> _raterColle;
        private object _lockRaterColleObj = new object();
        private ObservableCollection<SlideTextItemVM> _showRaterColle;
        private object _lockShowRaterColleObj = new object();
        private ConcurrentQueue<SlideTextItemVM> _showRaterQueue;
        private int _showRaterRowCount;
        private System.Timers.Timer _showRaterQueueTimer;

        private int _minScore;
        private int _maxScore;
        private bool _isShowIntroduction;
        private bool _isShowTotalScore;
        private bool _isShowAverageScore;
        private bool _isShowRaterCount;
        private bool _isShowRaterName;
        private bool _isShowSpecialRateButton;
        private bool _isShowRaterListButton;

        //private Random _random;

        #endregion Private Member

        #region Constructor

        public AssessmentCenterVM()
        {
            _raterTable = new Dictionary<string, CommentVM>();
            _raterScoreTable = new Dictionary<string, int>();
            //_random = new Random();

            _raterColle = new ObservableCollection<CommentVM>();
            this.RaterColle = CollectionViewSource.GetDefaultView(_raterColle);
            BindingOperations.EnableCollectionSynchronization(_raterColle, _lockRaterColleObj);

            _showRaterColle = new ObservableCollection<SlideTextItemVM>();
            this.ShowRaterColle = CollectionViewSource.GetDefaultView(_showRaterColle);
            BindingOperations.EnableCollectionSynchronization(_showRaterColle, _lockShowRaterColleObj);

            _showRaterQueue = new ConcurrentQueue<SlideTextItemVM>();
            _showRaterQueueTimer = new System.Timers.Timer();
            _showRaterQueueTimer.Elapsed += On_ShowRaterQueueTimer_Elapsed;
            _showRaterQueueTimer.Interval = 100;

            this.MinScore = 0;
            this.MaxScore = 100;
            this.TotalScore = 0;
            this.AverageScore = 0;
            this.RaterCount = 0;

            this.IsShowTotalScore = true;
            this.IsShowAverageScore = true;
            this.IsShowRaterCount = true;

            this.IsShowRaterListButton = true;
            this.IsShowSpecialRateButton = true;

            this.IsShowRaterName = true;
            this.CanChangeRate = false;
            this.CanChangeRateEnable = true;
        }

        #endregion Constructor

        #region Public Member

        public bool IsStarted
        {
            get { return _isStarted; }
            set { _isStarted = value; OnPropertyChanged(nameof(this.IsStarted)); }
        }

        public bool IsClosed
        {
            get { return _isClosed; }
            set { _isClosed = value; OnPropertyChanged(nameof(this.IsClosed)); }
        }

        public DateTime StartTime
        {
            get { return _startTime; }
            set { _startTime = value; OnPropertyChanged(nameof(StartTime)); OnPropertyChanged(nameof(StartTimeText)); }
        }

        public String StartTimeText
        {
            get
            {
                if (this.StartTime == DateTime.MinValue)
                {
                    return "";
                }

                return this.StartTime.ToString("yyyy/MM/dd HH:mm:ss");
            }
        }

        public TimeSpan ElapsedTime
        {
            get { return _elapsedTime; }
            set { _elapsedTime = value; OnPropertyChanged(nameof(ElapsedTime)); OnPropertyChanged(nameof(ElapsedTimeText)); }
        }

        public String ElapsedTimeText
        {
            get
            {
                String ret = String.Format("{0:00}:{1:00}:{2:00}", this.ElapsedTime.TotalHours, this.ElapsedTime.Minutes, this.ElapsedTime.Seconds);
                return ret;
            }
        }

        public bool CanChangeRate
        {
            get { return _canChangeRate; }
            set { _canChangeRate = value; OnPropertyChanged(nameof(this.CanChangeRate)); }
        }

        public bool CanChangeRateEnable
        {
            get { return _canChangeRateEnable; }
            set { _canChangeRateEnable = value; OnPropertyChanged(nameof(this.CanChangeRateEnable)); }
        }

        public BitmapImage ImageObject
        {
            get { return _img; }
            set
            {
                _img = value;
                OnPropertyChanged(nameof(this.ImageObject));
                ClearData();
            }
        }

        public string ContestantName
        {
            get { return _contestantName; }
            set { _contestantName = value; OnPropertyChanged(nameof(this.ContestantName)); }
        }

        public string Introduction
        {
            get { return _introduction; }
            set { _introduction = value; OnPropertyChanged(nameof(this.Introduction)); }
        }

        public int TotalScore
        {
            get { return _totalScore; }
            set { _totalScore = value; OnPropertyChanged(nameof(this.TotalScore)); }
        }

        public double AverageScore
        {
            get { return _averageScore; }
            set { _averageScore = value; OnPropertyChanged(nameof(this.AverageScore)); }
        }

        public int RaterCount
        {
            get { return _raterCount; }
            set { _raterCount = value; OnPropertyChanged(nameof(this.RaterCount)); }
        }

        public int MinScore
        {
            get { return _minScore; }
            set { _minScore = value; OnPropertyChanged(nameof(this.MinScore)); }
        }

        public int MaxScore
        {
            get { return _maxScore; }
            set { _maxScore = value; OnPropertyChanged(nameof(this.MaxScore)); }
        }

        public bool IsShowIntroduction
        {
            get { return _isShowIntroduction; }
            set { _isShowIntroduction = value; OnPropertyChanged(nameof(this.IsShowIntroduction)); }
        }

        public bool IsShowTotalScore
        {
            get { return _isShowTotalScore; }
            set { _isShowTotalScore = value; OnPropertyChanged(nameof(this.IsShowTotalScore)); }
        }

        public bool IsShowAverageScore
        {
            get { return _isShowAverageScore; }
            set { _isShowAverageScore = value; OnPropertyChanged(nameof(this.IsShowAverageScore)); }
        }

        public bool IsShowRaterCount
        {
            get { return _isShowRaterCount; }
            set { _isShowRaterCount = value; OnPropertyChanged(nameof(this.IsShowRaterCount)); }
        }

        public bool IsShowRaterName
        {
            get { return _isShowRaterName; }
            set { _isShowRaterName = value; OnPropertyChanged(nameof(this.IsShowRaterName)); }
        }

        public bool IsShowSpecialRateButton
        {
            get { return _isShowSpecialRateButton; }
            set { _isShowSpecialRateButton = value; OnPropertyChanged(nameof(this.IsShowSpecialRateButton)); }
        }

        public bool IsShowRaterListButton
        {
            get { return _isShowRaterListButton; }
            set { _isShowRaterListButton = value; OnPropertyChanged(nameof(this.IsShowRaterListButton)); }
        }

        public int ShowRaterRowCount
        {
            get { return _showRaterRowCount; }
            set { _showRaterRowCount = value; OnPropertyChanged(nameof(this.ShowRaterRowCount)); }
        }

        public ICollectionView RaterColle
        {
            get;
            private set;
        }

        public ICollectionView ShowRaterColle
        {
            get;
            private set;
        }

        #endregion Public Member

        #region Command

        private CommandBase _cmdStart;

        public CommandBase CmdStart
        {
            get
            {
                return _cmdStart ?? (_cmdStart = new CommandBase(x => ExecuteCommand(nameof(this.CmdStart))));
            }
        }

        private CommandBase _cmdStop;

        public CommandBase CmdStop
        {
            get
            {
                return _cmdStop ?? (_cmdStop = new CommandBase(x => ExecuteCommand(nameof(this.CmdStop))));
            }
        }

        private CommandBase _cmdOpenRaterColle;

        public CommandBase CmdOpenRaterColle
        {
            get
            {
                return _cmdOpenRaterColle ?? (_cmdOpenRaterColle = new CommandBase(x => ExecuteCommand(nameof(this.CmdOpenRaterColle))));
            }
        }

        public Func<String, AssessmentCenterVM, bool> CommandAction { get; set; }

        private void ExecuteCommand(String cmd)
        {
            if (this.CommandAction != null)
            {
                this.CommandAction(cmd, this);
            }
        }

        #endregion Command

        #region Public Method

        public void Start()
        {
            this.StartTime = DateTime.Now;
            _timerElapseTime = new Timer(On_ElapseTime, null, 0, Timeout.Infinite);

            ClearData();

            this.CanChangeRateEnable = false;
            this.IsStarted = true;
        }

        public void Stop()
        {
            //停止紀錄時間
            if (_timerElapseTime != null)
            {
                _timerElapseTime.Change(Timeout.Infinite, Timeout.Infinite);
                _timerElapseTime.Dispose();
                _timerElapseTime = null;
            }

            this.IsStarted = false;
            this.CanChangeRateEnable = true;
        }

        public void ClearData()
        {
            _raterColle.Clear();
            _raterTable.Clear();
            _raterScoreTable.Clear();

            this.TotalScore = 0;
            this.AverageScore = 0;
            this.RaterCount = 0;
        }

        public void Close()
        {
            this.Stop();
            this.CommandAction = null;
            this.IsClosed = true;
        }

        public void RemoveRate(CommentVM vm)
        {
            if (_raterColle.Remove(vm))
            {
                //int rateScore = 0;
                int rateScore = _raterScoreTable[vm.AuthorID];
                //if (Int32.TryParse(vm.Message, out rateScore))
                //{
                this.TotalScore -= rateScore;
                this.RaterCount = _raterColle.Count;
                this.AverageScore = Math.Round((double)this.TotalScore / this.RaterCount, 4);
                //}
            }
        }

        /// <summary>
        /// 解析留言是否為有效分數並進行評分
        /// </summary>
        /// <param name="vm">留言</param>
        /// <returns>若評分成功則回傳TRUE</returns>
        public bool Rate(CommentVM vm)
        {
            //判斷評分是否已開始
            if (!this.IsStarted)
            {
                return false;
            }

            int rateScore = 0;
            if (Int32.TryParse(vm.Message, out rateScore))
            {
                if (rateScore >= this.MinScore
                    && rateScore <= this.MaxScore)
                {
                    //判斷是否已評過分
                    if (_raterTable.ContainsKey(vm.AuthorID))
                    {
                        if (!this.CanChangeRate)
                        {
                            //不允許重新評分
                            return false;
                        }
                        else
                        {
                            //允許重新評分
                            //移除舊分數
                            CommentVM oldRate = _raterTable[vm.AuthorID];
                            RemoveRate(oldRate);
                            _raterTable.Remove(vm.AuthorID);
                            _raterScoreTable.Remove(vm.AuthorID);
                        }
                    }

                    _raterScoreTable.Add(vm.AuthorID, rateScore);
                    _raterTable.Add(vm.AuthorID, vm);
                    _raterColle.Add(vm);
                    this.RaterCount = _raterColle.Count;
                    this.TotalScore += rateScore;
                    this.AverageScore = Math.Round((double)this.TotalScore / this.RaterCount, 4);
                    AddShowRaterColle(vm, rateScore);
                }
            }

            return false;
        }

        #endregion Public Method

        #region Private Method

        /// <summary>
        /// 新增至評分者滑動動畫佇列中等待開始滑動動畫
        /// </summary>
        /// <param name="rater">要顯示滑動動畫的評分</param>
        /// <param name="score">分數</param>
        private void AddShowRaterColle(CommentVM rater, int score)
        {
            SlideTextItemVM showRater = new SlideTextItemVM();
            showRater.Text = rater.AuthorName + ": " + score.ToString();
            showRater.SlideFinished += On_ShowRater_SlideFinished;
            showRater.ShowTime = 3 * 1000;
            _showRaterQueue.Enqueue(showRater);
            _showRaterQueueTimer.Start();
        }

        private void RemoveShowRater(SlideTextItemVM vm)
        {
            _showRaterColle.Remove(vm);
        }

        #endregion Private Method

        #region Event Handle

        /// <summary>
        /// 處理評分者名稱滑動動畫佇列
        /// </summary>
        private void On_ShowRaterQueueTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            SlideTextItemVM showRater;
            if (_showRaterQueue.TryDequeue(out showRater))
            {
                //插入列表最上層於UI上顯示
                _showRaterColle.Insert(0, showRater);
                if (_showRaterColle.Count > this.ShowRaterRowCount)
                {
                    SlideTextItemVM t = _showRaterColle.ElementAtOrDefault(this.ShowRaterRowCount);
                    if (t != null)
                    {
                        t.IsClose = true;
                    }
                }
            }
            else
            {
                //佇列清空，關閉計時器
                _showRaterQueueTimer.Stop();
            }
        }

        private void On_ShowRater_SlideFinished(SlideTextItemVM sender)
        {
            RemoveShowRater(sender);
        }

        private void On_ElapseTime(object state)
        {
            TimeSpan elapse = DateTime.Now - this.StartTime;
            this.ElapsedTime = elapse;
            _timerElapseTime.Change(1000, Timeout.Infinite);
        }

        #endregion Event Handle
    }
}