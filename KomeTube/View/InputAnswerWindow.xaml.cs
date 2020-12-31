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
using System.Windows.Shapes;

namespace KomeTube.View
{
    /// <summary>
    /// InputAnswerWindow.xaml 的互動邏輯
    /// </summary>
    public partial class InputAnswerWindow : Window
    {
        public InputAnswerWindow()
        {
            InitializeComponent();
        }

        public String Result { get; set; }

        private void On_OK_Click(object sender, RoutedEventArgs e)
        {
            this.Result = TXTBOX_Answer.Text;
            this.DialogResult = true;
        }

        private void On_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}