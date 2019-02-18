using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Media.Imaging;

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
        private ObservableCollection<CommentVM> _voterColle;
        private object _lockVoterColleObj;
        #endregion


        #region Constroctor
        public VoteCandidateVM()
        {
            _voterColle = new ObservableCollection<CommentVM>();
            _lockVoterColleObj = new object();
            _name = "";
            _count = 0;
            this.VoterColle = CollectionViewSource.GetDefaultView(_voterColle);
            BindingOperations.EnableCollectionSynchronization(_voterColle, _lockVoterColleObj);
        }
        #endregion


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
        /// 取得或設定是否顯示即時統計
        /// </summary>
        public bool IsShowStatistic
        {
            get { return _isShowStatistic; }
            set { _isShowStatistic = value; OnPropertyChanged(nameof(IsShowStatistic)); }
        }

        #endregion


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
        #endregion


        #region Public Method
        /// <summary>
        /// 將投票者加入名單並列入計票
        /// </summary>
        /// <param name="voter"></param>
        public void AddVoter(CommentVM voter)
        {
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

            OnPropertyChanged("");
        }
        #endregion
    }
}
