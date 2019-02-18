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
using System.Collections.ObjectModel;

using KomeTube.ViewModel;

namespace KomeTube.View
{
    /// <summary>
    /// VotingCenterWindow.xaml 的互動邏輯
    /// </summary>
    public partial class VotingCenterWindow : Window
    {
        #region Private Member
        private VotingCenterVM _vm;
        #endregion


        #region Constructor
        public VotingCenterWindow(VotingCenterVM vm)
        {
            InitializeComponent();

            _vm = vm;
            _vm.CommandAction = On_VotingCenter_Action;

            this.DataContext = _vm;
            LV_Candidates.ItemsSource = _vm.VoteCandidateColle;
        }
        #endregion


        #region Event Handle
        private void On_Closed(object sender, EventArgs e)
        {
            _vm.Close();
        }

        private bool On_VotingCenter_Action(string cmdKey, VotingCenterVM sender)
        {
            switch (cmdKey)
            {
                case VotingCenterVM.CmdKey_Start:
                    _vm.Start();
                    break;
                case VotingCenterVM.CmdKey_Stop:
                    _vm.Stop();
                    break;

                default:
                    break;
            }
            return true;
        }
        #endregion
    }
}
