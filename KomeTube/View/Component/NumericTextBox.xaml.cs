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
using System.Text.RegularExpressions;

namespace KomeTube.View.Component
{
    /// <summary>
    /// NumericTextBox.xaml 的互動邏輯
    /// </summary>
    public partial class NumericTextBox : TextBox
    {
        private String _textInput;

        #region Dependency Property

        public static readonly DependencyProperty MaxValueProperty = DependencyProperty.Register("MaxValue", typeof(double), typeof(NumericTextBox), new PropertyMetadata((double)0));
        public static readonly DependencyProperty MinValueProperty = DependencyProperty.Register("MinValue", typeof(double), typeof(NumericTextBox), new PropertyMetadata((double)0));

        #endregion Dependency Property

        #region Constructor

        public NumericTextBox()
        {
            InitializeComponent();
        }

        #endregion Constructor

        #region Property

        public double MaxValue
        {
            get { return (double)this.GetValue(MaxValueProperty); }
            set { this.SetValue(MaxValueProperty, value); }
        }

        public double MinValue
        {
            get { return (double)this.GetValue(MinValueProperty); }
            set { this.SetValue(MinValueProperty, value); }
        }

        #endregion Property

        #region Event

        private void On_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            this.SelectAll();
        }

        private void On_GotMouseCapture(object sender, MouseEventArgs e)
        {
            this.SelectAll();
        }

        private void On_LostFocus(object sender, RoutedEventArgs e)
        {
            double value = 0;

            try
            {
                value = Convert.ToDouble(this.Text);
            }
            catch (Exception)
            {
                this.Text = String.Format("{0}", this.MinValue);
                return;
            }

            if (value < this.MinValue)
            {
                value = this.MinValue;
            }
            this.Text = String.Format("{0}", value);
        }

        private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
            _textInput = e.Text;
        }

        private void PastingHandler(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String text = (String)e.DataObject.GetData(typeof(String));
                if (!IsTextAllowed(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if ("." == _textInput || "-" == _textInput)
            {
                return;
            }

            double value = this.MinValue;

            try
            {
                value = Convert.ToDouble(this.Text);
            }
            catch (Exception)
            {
                this.Text = String.Format("{0}", value);
                return;
            }
            if (value > this.MaxValue)
            {
                value = MaxValue;
            }

            this.Text = String.Format("{0}", value);

            if (MaxValue == value)
            {
                this.SelectAll();
            }
        }

        #endregion Event

        #region Private Function

        private bool IsTextAllowed(String text)
        {
            Regex regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text
            return !regex.IsMatch(text);
        }

        #endregion Private Function
    }
}