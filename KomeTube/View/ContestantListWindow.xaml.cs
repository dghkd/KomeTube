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

using KomeTube.ViewModel;

namespace KomeTube.View
{
    /// <summary>
    /// ContestantListWindow.xaml 的互動邏輯
    /// </summary>
    public partial class ContestantListWindow : Window
    {
        private ContestantListVM _vm;

        public ContestantListWindow(ContestantListVM vm)
        {
            InitializeComponent();
            _vm = vm;
            this.DataContext = _vm;
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
    }
}