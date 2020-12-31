using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using KomeTube.Common;

namespace KomeTube.ViewModel
{
    public class ContestantVM : ViewModelBase
    {
        #region Private Member

        private BitmapImage _img;
        private string _name;
        private string _introduction;
        private int _totalScore;
        private double _averageScore;
        private int _raterCount;
        private ObservableCollection<CommentVM> _raterColle;
        private object _lockRaterColleObj = new object();

        private bool _isShowIntroduction;
        private bool _isShowTotalScore;
        private bool _isShowAverageScore;
        private bool _isShowRaterCount;
        private bool _isShowRaterListButton;

        #endregion Private Member

        #region Constructor

        public ContestantVM()
        {
            _raterColle = new ObservableCollection<CommentVM>();
            this.RaterColle = CollectionViewSource.GetDefaultView(_raterColle);
            BindingOperations.EnableCollectionSynchronization(_raterColle, _lockRaterColleObj);
        }

        #endregion Constructor

        #region Public Member

        public string Name
        {
            get { return _name; }
            set { _name = value; OnPropertyChanged(nameof(this.Name)); }
        }

        public BitmapImage ImageObject
        {
            get { return _img; }
            set
            {
                _img = value;
                OnPropertyChanged(nameof(this.ImageObject));
            }
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

        public bool IsShowRaterListButton
        {
            get { return _isShowRaterListButton; }
            set { _isShowRaterListButton = value; OnPropertyChanged(nameof(this.IsShowRaterListButton)); }
        }

        public ICollectionView RaterColle
        {
            get;
            private set;
        }

        #endregion Public Member

        #region Command

        private CommandBase _cmdRemove;

        public CommandBase CmdRemove
        {
            get
            {
                return _cmdRemove ?? (_cmdRemove = new CommandBase(x => ExecuteCommand(nameof(this.CmdRemove))));
            }
        }

        public Func<String, ContestantVM, bool> CommandAction { get; set; }

        private void ExecuteCommand(String cmd)
        {
            if (this.CommandAction != null)
            {
                this.CommandAction(cmd, this);
            }
        }

        #endregion Command

        #region Public Method

        public void AddRater(ObservableCollection<CommentVM> raterColle)
        {
            foreach (CommentVM cmt in raterColle)
            {
                _raterColle.Add(cmt);
            }
        }

        #endregion Public Method
    }
}