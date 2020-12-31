using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace KomeTube.View.Component
{
    /// <summary>
    /// SlideTextItem.xaml 的互動邏輯
    /// </summary>
    public partial class SlideTextItem : UserControl
    {
        #region Event

        public static readonly RoutedEvent BeginShowEvent =
            EventManager.RegisterRoutedEvent("BeginShow", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SlideTextItem));

        public static readonly RoutedEvent BeginSlideEvent =
            EventManager.RegisterRoutedEvent("BeginSlide", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SlideTextItem));

        public static readonly RoutedEvent FinishedSlideEvent =
            EventManager.RegisterRoutedEvent("FinishedSlide", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SlideTextItem));

        public event RoutedEventHandler BeginShow
        {
            add { AddHandler(BeginShowEvent, value); }
            remove { RemoveHandler(BeginShowEvent, value); }
        }

        public event RoutedEventHandler BeginSlide
        {
            add { AddHandler(BeginSlideEvent, value); }
            remove { RemoveHandler(BeginSlideEvent, value); }
        }

        public event RoutedEventHandler FinishedSlide
        {
            add { AddHandler(FinishedSlideEvent, value); }
            remove { RemoveHandler(FinishedSlideEvent, value); }
        }

        #endregion Event

        #region Dependency Property

        public static readonly DependencyProperty ShowTimeProperty =
            DependencyProperty.Register("ShowTime", typeof(int), typeof(SlideTextItem), new PropertyMetadata(1000));

        public static readonly DependencyProperty IsCloseProperty =
                    DependencyProperty.Register("IsClose", typeof(bool), typeof(SlideTextItem), new PropertyMetadata(true, OnIsClosePropertyChanged));

        public static readonly DependencyProperty IsSlideFinishedProperty =
            DependencyProperty.Register("IsSlideFinished", typeof(bool), typeof(SlideTextItem), new PropertyMetadata(false));

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(SlideTextItem), new PropertyMetadata(""));

        private static void OnIsClosePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SlideTextItem item = d as SlideTextItem;
            bool newValue = (bool)e.NewValue;
            bool oldValue = (bool)e.OldValue;

            if (item != null
                && newValue != oldValue)
            {
                if (newValue)
                {
                    item.Close();
                }
                else
                {
                    item.Show();
                }
            }
        }

        #endregion Dependency Property

        #region Private Member

        private DispatcherTimer _closeTimer;

        #endregion Private Member

        #region Constructor

        public SlideTextItem()
        {
            InitializeComponent();
            _closeTimer = new DispatcherTimer();
            _closeTimer.Tick += On_CloseTimer_Tick;
        }

        #endregion Constructor

        #region Public Member

        public int ShowTime
        {
            get { return (int)GetValue(ShowTimeProperty); }
            set { SetValue(ShowTimeProperty, value); }
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public bool IsClose
        {
            get { return (bool)GetValue(IsCloseProperty); }
            set { SetValue(IsCloseProperty, value); }
        }

        public bool IsSlideFinished
        {
            get { return (bool)GetValue(IsSlideFinishedProperty); }
            set { SetValue(IsSlideFinishedProperty, value); }
        }

        #endregion Public Member

        #region Public Method

        public void Show()
        {
            this.IsClose = false;
            this.IsSlideFinished = false;
            RaiseEvent(new RoutedEventArgs(BeginShowEvent));
            _closeTimer.Interval = TimeSpan.FromMilliseconds(this.ShowTime);
            _closeTimer.Start();
        }

        public void Close()
        {
            this.IsClose = true;
            _closeTimer.Stop();
            RaiseEvent(new RoutedEventArgs(BeginSlideEvent));
        }

        #endregion Public Method

        #region Event Handler

        private void On_Loaded(object sender, RoutedEventArgs e)
        {
            Show();
        }

        private void On_CloseTimer_Tick(object sender, EventArgs e)
        {
            Close();
        }

        private void On_SlideStoryboard_Completed(object sender, EventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(FinishedSlideEvent));
            this.IsSlideFinished = true;
        }

        #endregion Event Handler
    }
}