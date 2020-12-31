using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;

using KomeTube.Common;
using KomeTube.View;

namespace KomeTube.ViewModel
{
    public class PuzzleCenterVM : ViewModelBase
    {
        #region Private Member

        private bool _isStarted;
        private DateTime _startTime;
        private TimeSpan _elapsedTime;
        private bool _isClosed;
        private Timer _timerElapseTime;

        private ObservableCollection<PuzzleAnswerVM> _ansColle;
        private object _lockAnsColleObj;

        #endregion Private Member

        #region Constructor

        public PuzzleCenterVM()
        {
            _lockAnsColleObj = new object();

            _ansColle = new ObservableCollection<PuzzleAnswerVM>();
            this.AnsColle = CollectionViewSource.GetDefaultView(_ansColle);
            BindingOperations.EnableCollectionSynchronization(_ansColle, _lockAnsColleObj);
        }

        #endregion Constructor

        #region Public Member

        /// <summary>
        /// 取得或設定開始時間
        /// </summary>
        public DateTime StartTime
        {
            get { return _startTime; }
            set { _startTime = value; OnPropertyChanged(nameof(StartTime)); OnPropertyChanged(nameof(StartTimeText)); }
        }

        /// <summary>
        /// 取得格式化的開始時間字串
        /// </summary>
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

        /// <summary>
        /// 取得或設定經過時間
        /// </summary>
        public TimeSpan ElapsedTime
        {
            get { return _elapsedTime; }
            set { _elapsedTime = value; OnPropertyChanged(nameof(ElapsedTime)); OnPropertyChanged(nameof(ElapsedTimeText)); }
        }

        /// <summary>
        /// 取得格式化的經過時間字串
        /// </summary>
        public String ElapsedTimeText
        {
            get
            {
                String ret = String.Format("{0:00}:{1:00}:{2:00}", this.ElapsedTime.TotalHours, this.ElapsedTime.Minutes, this.ElapsedTime.Seconds);
                return ret;
            }
        }

        /// <summary>
        /// 取得是否為已開始狀態
        /// </summary>
        public bool IsStarted
        {
            get { return _isStarted; }
            private set { _isStarted = value; OnPropertyChanged(nameof(IsStarted)); }
        }

        /// <summary>
        /// 取得是否為已關閉狀態
        /// </summary>
        public bool IsClosed
        {
            get { return _isClosed; }
            private set { _isClosed = value; OnPropertyChanged(nameof(IsClosed)); }
        }

        /// <summary>
        /// 答案集合
        /// </summary>
        public ICollectionView AnsColle
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

        public const string CmdKey_AddAnswer = "CmdKey_AddAnswer";
        private CommandBase _cmdAddAnswer;

        public CommandBase CmdAddAnswer
        {
            get
            {
                return _cmdAddAnswer ?? (_cmdAddAnswer = new CommandBase(x => ExecuteCommand(CmdKey_AddAnswer)));
            }
        }

        public Func<String, PuzzleCenterVM, bool> CommandAction;

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
        /// 開始接收猜謎訊息
        /// </summary>
        public void Start()
        {
            this.StartTime = DateTime.Now;
            _timerElapseTime = new Timer(On_ElapseTime, null, 0, Timeout.Infinite);

            foreach (PuzzleAnswerVM ans in _ansColle)
            {
                ans.Clear();
            }

            this.IsStarted = true;
        }

        /// <summary>
        /// 結束猜謎
        /// </summary>
        public void Stop()
        {
            if (_timerElapseTime != null)
            {
                _timerElapseTime.Change(Timeout.Infinite, Timeout.Infinite);
                _timerElapseTime.Dispose();
                _timerElapseTime = null;
            }

            this.IsStarted = false;
        }

        /// <summary>
        /// 關閉
        /// <para>當猜謎視窗關閉後應呼叫此方法停止猜謎</para>
        /// </summary>
        public void Close()
        {
            this.Stop();
            this.CommandAction = null;
            this.IsClosed = true;
        }

        /// <summary>
        /// 增加答案
        /// </summary>
        /// <param name="answer"></param>
        public void AddAnswer(PuzzleAnswerVM answer)
        {
            _ansColle.Add(answer);
        }

        /// <summary>
        /// 移除答案
        /// </summary>
        /// <param name="answer"></param>
        public void RemoveAnswer(PuzzleAnswerVM answer)
        {
            _ansColle.Remove(answer);
        }

        /// <summary>
        /// 解析留言是否猜中答案
        /// </summary>
        /// <param name="vm">留言</param>
        /// <returns>若留言符合答案集合中任一答案則回傳True</returns>
        public bool Guessing(CommentVM vm)
        {
            //判斷猜謎是否已開始
            if (!this.IsStarted)
            {
                return false;
            }

            //比對答案集合
            for (int i = 0; i < _ansColle.Count; i++)
            {
                PuzzleAnswerVM ans = _ansColle.ElementAt(i);
                if (String.Equals(ans.Answer, vm.Message))
                {
                    //判斷同一位留言者是否已經猜中，避免重複記名
                    if (!ans.IsNameExist(vm))
                    {
                        ans.AddName(vm);
                        return true;
                    }
                }
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
    }
}