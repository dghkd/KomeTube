using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using KomeTube.Common;

namespace KomeTube.ViewModel
{
    public class ContestantListVM : ViewModelBase
    {
        #region Private Member

        private bool _isShowIntroduction;
        private bool _isShowTotalScore;
        private bool _isShowAverageScore;
        private bool _isShowRaterCount;
        private bool _isShowRaterListButton;

        private ObservableCollection<ContestantVM> _contestantColle;
        private object _lockContestantColleObj = new object();

        #endregion Private Member

        #region Constructor

        public ContestantListVM()
        {
            _contestantColle = new ObservableCollection<ContestantVM>();
            this.ContestantColle = CollectionViewSource.GetDefaultView(_contestantColle);
            BindingOperations.EnableCollectionSynchronization(_contestantColle, _lockContestantColleObj);

            this.IsShowAverageScore = true;
            this.IsShowTotalScore = true;
            this.IsShowRaterCount = true;
            this.IsShowIntroduction = false;
            this.IsShowRaterListButton = false;
        }

        #endregion Constructor

        #region Public Member

        public bool IsShowIntroduction
        {
            get { return _isShowIntroduction; }
            set
            {
                _isShowIntroduction = value;
                OnPropertyChanged(nameof(this.IsShowIntroduction));

                foreach (ContestantVM item in _contestantColle)
                {
                    item.IsShowIntroduction = value;
                }
            }
        }

        public bool IsShowTotalScore
        {
            get { return _isShowTotalScore; }
            set
            {
                _isShowTotalScore = value; OnPropertyChanged(nameof(this.IsShowTotalScore));

                foreach (ContestantVM item in _contestantColle)
                {
                    item.IsShowTotalScore = value;
                }
            }
        }

        public bool IsShowAverageScore
        {
            get { return _isShowAverageScore; }
            set
            {
                _isShowAverageScore = value; OnPropertyChanged(nameof(this.IsShowAverageScore));
                foreach (ContestantVM item in _contestantColle)
                {
                    item.IsShowAverageScore = value;
                }
            }
        }

        public bool IsShowRaterCount
        {
            get { return _isShowRaterCount; }
            set
            {
                _isShowRaterCount = value; OnPropertyChanged(nameof(this.IsShowRaterCount));
                foreach (ContestantVM item in _contestantColle)
                {
                    item.IsShowRaterCount = value;
                }
            }
        }

        public bool IsShowRaterListButton
        {
            get { return _isShowRaterListButton; }
            set
            {
                _isShowRaterListButton = value; OnPropertyChanged(nameof(this.IsShowRaterListButton));

                foreach (ContestantVM item in _contestantColle)
                {
                    item.IsShowRaterListButton = value;
                }
            }
        }

        public ICollectionView ContestantColle
        {
            get;
            private set;
        }

        #endregion Public Member

        #region Public Method

        public void AddContestant(ContestantVM vm)
        {
            vm.IsShowAverageScore = this.IsShowAverageScore;
            vm.IsShowTotalScore = this.IsShowTotalScore;
            vm.IsShowRaterCount = this.IsShowRaterCount;
            vm.IsShowIntroduction = this.IsShowIntroduction;
            vm.IsShowRaterListButton = this.IsShowRaterListButton;
            vm.CommandAction += On_Contestant_CommandAction;
            _contestantColle.Add(vm);
        }

        public void RemoveContestant(ContestantVM vm)
        {
            vm.CommandAction -= On_Contestant_CommandAction;
            _contestantColle.Remove(vm);
        }

        #endregion Public Method

        #region Event Handle

        private bool On_Contestant_CommandAction(string cmdKey, ContestantVM sender)
        {
            switch (cmdKey)
            {
                case nameof(sender.CmdRemove):
                    RemoveContestant(sender);
                    break;

                default:
                    break;
            }
            return true;
        }

        #endregion Event Handle
    }
}