using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Threading;

using KomeTube.Common;

namespace KomeTube.ViewModel
{
    public class VotingCenterVM : ViewModelBase
    {
        #region Private Member

        private bool _isStarted;
        private DateTime _startTime;
        private TimeSpan _elapsedTime;
        private int _column;
        private int _row;
        private bool _isShowStatistic;
        private String _voteTitle;
        private bool _isClosed;
        private ObservableCollection<CommentVM> _allVoteColle;
        private ObservableCollection<VoteCandidateVM> _voteCandidateColle;
        private object _lockAllVoteColleObj = new object();
        private object _lockVoteCandidateColleObj = new object();
        private Timer _timerElapseTime;
        private Dictionary<String, CommentVM> _voterTable;

        #endregion Private Member

        #region Constructor

        public VotingCenterVM()
        {
            _allVoteColle = new ObservableCollection<CommentVM>();
            _voteCandidateColle = new ObservableCollection<VoteCandidateVM>();
            this.AllVoteColle = CollectionViewSource.GetDefaultView(_allVoteColle);
            this.VoteCandidateColle = CollectionViewSource.GetDefaultView(_voteCandidateColle);
            BindingOperations.EnableCollectionSynchronization(_allVoteColle, _lockAllVoteColleObj);
            BindingOperations.EnableCollectionSynchronization(_voteCandidateColle, _lockVoteCandidateColleObj);

            this.Column = 2;
            this.Row = 1;

            this.IsClosed = false;
            this.IsShowStatistic = true;
            _voterTable = new Dictionary<string, CommentVM>();
        }

        #endregion Constructor

        #region Publice Member

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

        /// <summary>
        /// 總投票數
        /// </summary>
        public int VoteCount
        {
            get
            {
                return _allVoteColle.Count;
            }
        }

        public int Column
        {
            get { return _column; }
            set
            {
                _column = value;
                ChangeVoteCandidate();
                OnPropertyChanged(nameof(Column));
            }
        }

        public int Row
        {
            get { return _row; }
            set
            {
                _row = value;
                ChangeVoteCandidate();
                OnPropertyChanged(nameof(Row));
            }
        }

        public bool IsStarted
        {
            get { return _isStarted; }
            set { _isStarted = value; OnPropertyChanged(nameof(IsStarted)); }
        }

        public bool IsShowStatistic
        {
            get { return _isShowStatistic; }
            set
            {
                _isShowStatistic = value;
                OnPropertyChanged(nameof(IsShowStatistic));

                foreach (VoteCandidateVM vm in _voteCandidateColle)
                {
                    vm.IsShowStatistic = _isShowStatistic;
                }
            }
        }

        public String VoteTitle
        {
            get { return _voteTitle; }
            set { _voteTitle = value; OnPropertyChanged(nameof(VoteTitle)); }
        }

        public bool IsClosed
        {
            get { return _isClosed; }
            private set { _isClosed = value; OnPropertyChanged(nameof(IsClosed)); }
        }

        /// <summary>
        /// 投票者名單
        /// </summary>
        public ICollectionView AllVoteColle
        {
            get;
            private set;
        }

        /// <summary>
        /// 投票選項集合
        /// </summary>
        public ICollectionView VoteCandidateColle
        {
            get;
            private set;
        }

        #endregion Publice Member

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

        public Func<String, VotingCenterVM, bool> CommandAction;

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
        /// 開始接收投票
        /// </summary>
        public void Start()
        {
            this.StartTime = DateTime.Now;
            _timerElapseTime = new Timer(On_ElapseTime, null, 0, Timeout.Infinite);

            _allVoteColle.Clear();
            _voterTable.Clear();

            List<VoteCandidateVM> invalidCandidates = new List<VoteCandidateVM>();
            foreach (VoteCandidateVM candidate in _voteCandidateColle)
            {
                candidate.Clear();
                candidate.IsReadOnly = true;
                if (candidate.Name == "")
                {
                    invalidCandidates.Add(candidate);
                }
            }

            //移除無效選項
            foreach (VoteCandidateVM candidate in invalidCandidates)
            {
                _voteCandidateColle.Remove(candidate);
            }

            this.IsStarted = true;
        }

        /// <summary>
        /// 結束投票
        /// </summary>
        public void Stop()
        {
            //停止紀錄時間
            if (_timerElapseTime != null)
            {
                _timerElapseTime.Change(Timeout.Infinite, Timeout.Infinite);
                _timerElapseTime.Dispose();
                _timerElapseTime = null;
            }

            foreach (VoteCandidateVM candidate in _voteCandidateColle)
            {
                candidate.IsReadOnly = false;
            }

            this.IsStarted = false;
        }

        /// <summary>
        /// 關閉投票
        /// <para>當投票視窗關閉後應呼叫此方法停止投票</para>
        /// </summary>
        public void Close()
        {
            this.Stop();
            this.CommandAction = null;
            this.IsClosed = true;
        }

        /// <summary>
        /// 解析留言是否為有效票並進行投票
        /// </summary>
        /// <param name="vm">留言</param>
        /// <returns>若投票成功則回傳True</returns>
        public bool Vote(CommentVM vm)
        {
            //判斷投票是否已開始
            if (!this.IsStarted)
            {
                return false;
            }

            int voteNum = 0;
            if (Int32.TryParse(vm.Message, out voteNum))
            {
                //判斷是否已投過票
                if (_voterTable.ContainsKey(vm.AuthorID))
                    return false;

                //計票
                bool isValidVote = false;
                foreach (VoteCandidateVM candidate in _voteCandidateColle)
                {
                    if (candidate.Num == voteNum)
                    {
                        _voterTable.Add(vm.AuthorID, vm);
                        _allVoteColle.Add(vm);
                        OnPropertyChanged(nameof(this.VoteCount));

                        candidate.AddVoter(vm);
                        isValidVote = true;
                        break;
                    }
                }

                //計所有得票率
                if (isValidVote)
                {
                    foreach (VoteCandidateVM candidate in _voteCandidateColle)
                    {
                        candidate.Rate = Math.Round((double)candidate.Count / this.VoteCount, 4) * 100;
                    }
                }

                return isValidVote;
            }

            return false;
        }

        #endregion Public Method

        #region Event Handle

        private void On_ElapseTime(object state)
        {
            TimeSpan elapse = DateTime.Now - this.StartTime;
            this.ElapsedTime = elapse;
            _timerElapseTime.Change(1000, Timeout.Infinite);
        }

        #endregion Event Handle

        #region Private Method

        /// <summary>
        /// 依據Column * Row調整投票選項數量
        /// </summary>
        private void ChangeVoteCandidate()
        {
            int count = this.Column * this.Row;

            //刪除多餘選項
            while (_voteCandidateColle.Count > count)
            {
                VoteCandidateVM del = _voteCandidateColle.ElementAt(count);
                del.Clear();
                del.CommandAction = null;

                _voteCandidateColle.RemoveAt(count);
            }

            //新增不足選項
            while (_voteCandidateColle.Count < count)
            {
                VoteCandidateVM candidate = new VoteCandidateVM();
                _voteCandidateColle.Add(candidate);
            }

            //初始化所有選項編號與狀態
            for (int i = 0; i < _voteCandidateColle.Count; i++)
            {
                _voteCandidateColle.ElementAt(i).Num = i + 1;
                _voteCandidateColle.ElementAt(i).IsShowStatistic = this.IsShowStatistic;
            }
        }

        #endregion Private Method
    }
}