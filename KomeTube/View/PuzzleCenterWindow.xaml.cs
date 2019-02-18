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
    /// PuzzleCenterWindow.xaml 的互動邏輯
    /// </summary>
    public partial class PuzzleCenterWindow : Window
    {
        #region Private Member
        private PuzzleCenterVM _vm;
        #endregion


        #region Constructor
        public PuzzleCenterWindow(PuzzleCenterVM vm)
        {
            InitializeComponent();

            _vm = vm;
            _vm.CommandAction = On_PuzzleCenter_CommandAction;
            this.DataContext = _vm;
            LV_Answer.ItemsSource = _vm.AnsColle;
        }
        #endregion


        #region Event Handle

        private void On_Closed(object sender, EventArgs e)
        {
            _vm.Close();
        }

        private bool On_PuzzleCenter_CommandAction(string cmdKey, PuzzleCenterVM sender)
        {
            switch (cmdKey)
            {
                case PuzzleCenterVM.CmdKey_AddAnswer:

                    InputAnswerWindow dlg = new InputAnswerWindow();
                    dlg.Owner = this;
                    dlg.ShowDialog();
                    if (dlg.DialogResult == true)
                    {
                        PuzzleAnswerVM ans = new PuzzleAnswerVM();
                        ans.Answer = dlg.Result;
                        ans.CommandAction = On_PuzzleAnswer_CommandAction;
                        _vm.AddAnswer(ans);
                    }
                    break;

                case PuzzleCenterVM.CmdKey_Start:
                    _vm.Start();
                    break;

                case PuzzleCenterVM.CmdKey_Stop:
                    _vm.Stop();
                    break;

                default:
                    break;
            }

            return true;
        }

        private bool On_PuzzleAnswer_CommandAction(string cmdKey, PuzzleAnswerVM sender)
        {
            switch (cmdKey)
            {
                case PuzzleAnswerVM.CmdKey_Remove:
                    sender.CommandAction = null;
                    _vm.RemoveAnswer(sender);
                    break;

                case PuzzleAnswerVM.CmdKey_OpenNameColle:
                    NameListWindow dlg = new NameListWindow(sender.NameColle);
                    dlg.Owner = this;
                    dlg.ShowDialog();

                    break;
                default:
                    break;
            }
            return true;
        }
        #endregion
    }
}
