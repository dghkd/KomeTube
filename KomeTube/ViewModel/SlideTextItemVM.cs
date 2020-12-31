using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KomeTube.Common;

namespace KomeTube.ViewModel
{
    public class SlideTextItemVM : ViewModelBase
    {
        #region Private Member

        private string _text;
        private int _showTime;
        private bool _isClose;
        private bool _isSlideFinished;

        #endregion Private Member

        #region Constructor

        public SlideTextItemVM()
        {
            this.ShowTime = 2000;
        }

        #endregion Constructor

        #region Public Member

        public string Text
        {
            get { return _text; }
            set { _text = value; OnPropertyChanged(nameof(this.Text)); }
        }

        public int ShowTime
        {
            get { return _showTime; }
            set { _showTime = value; OnPropertyChanged(nameof(this.ShowTime)); }
        }

        public bool IsClose
        {
            get { return _isClose; }
            set { _isClose = value; OnPropertyChanged(nameof(this.IsClose)); }
        }

        public bool IsSlideFinished
        {
            get { return _isSlideFinished; }
            set
            {
                _isSlideFinished = value;
                OnPropertyChanged(nameof(this.IsSlideFinished));
                if (value)
                {
                    RaiseSlideFinished();
                }
            }
        }

        #endregion Public Member

        #region Event

        public delegate void SlideTextFinishedMethod(SlideTextItemVM sender);

        public event SlideTextFinishedMethod SlideFinished;

        private void RaiseSlideFinished()
        {
            if (this.SlideFinished != null)
            {
                this.SlideFinished(this);
            }
        }

        #endregion Event
    }
}