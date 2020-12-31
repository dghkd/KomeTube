using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Collections.Concurrent;
using System.Timers;

using KomeTube.Common;

namespace KomeTube.ViewModel
{
    public class VoteCandidateVM : ViewModelBase
    {
        #region Private Member

        private int _num;
        private String _name;
        private int _count;
        private double _rate;
        private bool _isReadOnly;
        private BitmapImage _img;
        private bool _isShowStatistic;
        private bool _isShowVoterSlide;
        private bool _isShowVoteListButton;
        private ObservableCollection<CommentVM> _voterColle;
        private object _lockVoterColleObj;
        private ObservableCollection<SlideTextItemVM> _showVoterColle;
        private object _lockShowVoterColleObj;
        private ConcurrentQueue<SlideTextItemVM> _showVoterQueue;
        private Timer _showVoterQueueTimer;

        #endregion Private Member

        #region Constroctor

        public VoteCandidateVM()
        {
            _voterColle = new ObservableCollection<CommentVM>();
            _showVoterColle = new ObservableCollection<SlideTextItemVM>();
            _showVoterQueue = new ConcurrentQueue<SlideTextItemVM>();
            _lockVoterColleObj = new object();
            _lockShowVoterColleObj = new object();
            _showVoterQueueTimer = new Timer();
            _showVoterQueueTimer.Elapsed += On_ShowVoterQueue_Elapsed;
            _showVoterQueueTimer.Interval = 100;

            _name = "";
            _count = 0;

            this.VoterColle = CollectionViewSource.GetDefaultView(_voterColle);
            BindingOperations.EnableCollectionSynchronization(_voterColle, _lockVoterColleObj);

            this.ShowVoterColle = CollectionViewSource.GetDefaultView(_showVoterColle);
            BindingOperations.EnableCollectionSynchronization(_showVoterColle, _lockShowVoterColleObj);
        }

        #endregion Constroctor

        #region Public Member

        /// <summary>
        /// 取得或設定編號
        /// </summary>
        public int Num
        {
            get { return _num; }
            set { _num = value; OnPropertyChanged(nameof(Num)); }
        }

        /// <summary>
        /// 取得或設定名稱
        /// </summary>
        public String Name
        {
            get { return _name; }
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        /// <summary>
        /// 取得票數
        /// </summary>
        public int Count
        {
            get { return _count; }
            private set { _count = value; OnPropertyChanged(nameof(Count)); }
        }

        /// <summary>
        /// 取得或設定得票率
        /// </summary>
        public double Rate
        {
            get { return _rate; }
            set { _rate = value; OnPropertyChanged(nameof(Rate)); }
        }

        /// <summary>
        /// 取得或設定名稱是否可更改
        /// </summary>
        public bool IsReadOnly
        {
            get { return _isReadOnly; }
            set { _isReadOnly = value; OnPropertyChanged(nameof(IsReadOnly)); }
        }

        /// <summary>
        /// 取得或設定圖片內容
        /// </summary>
        public BitmapImage ImageObject
        {
            get { return _img; }
            set { _img = value; OnPropertyChanged(nameof(ImageObject)); }
        }

        /// <summary>
        /// 投票者集合
        /// </summary>
        public ICollectionView VoterColle
        {
            get;
            private set;
        }

        /// <summary>
        /// 顯示投票者動畫集合
        /// </summary>
        public ICollectionView ShowVoterColle
        {
            get;
            private set;
        }

        /// <summary>
        /// 取得或設定是否顯示即時統計
        /// </summary>
        public bool IsShowStatistic
        {
            get { return _isShowStatistic; }
            set { _isShowStatistic = value; OnPropertyChanged(nameof(IsShowStatistic)); }
        }

        /// <summary>
        /// 取得或設定是否顯示投票者滑動動畫
        /// </summary>
        public bool IsShowVoterSlide
        {
            get { return _isShowVoterSlide; }
            set { _isShowVoterSlide = value; OnPropertyChanged(nameof(this.IsShowVoterSlide)); }
        }

        /// <summary>
        /// 取得或設定是否顯示投票者名單按鈕
        /// </summary>
        public bool IsShowVoterListButton
        {
            get { return _isShowVoteListButton; }
            set
            {
                _isShowVoteListButton = value;
                OnPropertyChanged(nameof(this.IsShowVoterListButton));
            }
        }

        #endregion Public Member

        #region Command

        public const string CmdKey_OpenVoterColle = "CmdKey_OpenVoterColle";
        private CommandBase _cmdOpenVoterColle;

        public CommandBase CmdOpenVoterColle
        {
            get
            {
                return _cmdOpenVoterColle ?? (_cmdOpenVoterColle = new CommandBase(x => ExecuteCommand(CmdKey_OpenVoterColle)));
            }
        }

        public Func<String, VoteCandidateVM, bool> CommandAction;

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
        /// 將投票者加入名單並列入計票
        /// </summary>
        /// <param name="voter"></param>
        public void AddVoter(CommentVM voter)
        {
            AddShowVoterColle(voter);

            _voterColle.Add(voter);
            this.Count = _voterColle.Count;

            //得票率需由投票中心(VotingCenterVM)計算後設定至Rate成員
        }

        /// <summary>
        /// 清除統計資料
        /// </summary>
        public void Clear()
        {
            _voterColle.Clear();
            this.Count = 0;
            this.Rate = 0;

            _showVoterColle.Clear();
            SlideTextItemVM tmp;
            while (_showVoterQueue.TryDequeue(out tmp)) ;

            OnPropertyChanged("");
        }

        /// <summary>
        /// 從投票者滑動動畫列表中移除
        /// </summary>
        /// <param name="vm">要移除的滑動項目</param>
        public void RemoveShowVoter(SlideTextItemVM vm)
        {
            _showVoterColle.Remove(vm);
        }

        /// <summary>
        /// 從投票列表中移除投票
        /// </summary>
        /// <param name="vm">要移除的投票</param>
        public void RemoveVote(CommentVM vm)
        {
            _voterColle.Remove(vm);
            this.Count = _voterColle.Count;
        }

        #endregion Public Method

        #region Private Method

        /// <summary>
        /// 新增至投票者滑動動畫佇列中等待開始滑動動畫
        /// </summary>
        /// <param name="vote">要顯示滑動動畫的投票</param>
        private void AddShowVoterColle(CommentVM vote)
        {
            SlideTextItemVM showVoter = new SlideTextItemVM();
            showVoter.Text = vote.AuthorName;
            showVoter.SlideFinished += On_ShowVoter_SlideFinished;
            _showVoterQueue.Enqueue(showVoter);
            _showVoterQueueTimer.Start();
        }

        #endregion Private Method

        #region Event Handle

        /// <summary>
        /// 當滑動動畫項目完成滑動後
        /// </summary>
        /// <param name="sender">完成滑動動畫的項目</param>
        private void On_ShowVoter_SlideFinished(SlideTextItemVM sender)
        {
            //從滑動列表中移除
            RemoveShowVoter(sender);
        }

        /// <summary>
        /// 處理投票者名稱滑動動畫佇列
        /// </summary>
        private void On_ShowVoterQueue_Elapsed(object sender, ElapsedEventArgs e)
        {
            SlideTextItemVM showVoter;
            if (_showVoterQueue.TryDequeue(out showVoter))
            {
                //插入列表最上層於UI上顯示
                _showVoterColle.Insert(0, showVoter);
                if (_showVoterColle.Count > 2)
                {
                    //UI上顯示超過兩列，令第三列提早觸發滑動動畫
                    SlideTextItemVM t = _showVoterColle.ElementAtOrDefault(2);
                    if (t != null)
                    {
                        t.IsClose = true;
                    }
                }
            }
            else
            {
                //佇列清空，關閉計時器
                _showVoterQueueTimer.Stop();
            }
        }

        #endregion Event Handle
    }
}