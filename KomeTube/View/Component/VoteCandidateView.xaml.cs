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
using System.Diagnostics;

using KomeTube.ViewModel;

namespace KomeTube.View.Component
{
    /// <summary>
    /// VoteCandidateView.xaml 的互動邏輯
    /// </summary>
    public partial class VoteCandidateView : UserControl
    {
        #region Private Member

        private VoteCandidateVM _vm;

        #endregion Private Member

        #region Constructor

        public VoteCandidateView()
        {
            InitializeComponent();
        }

        #endregion Constructor

        #region Event Handle

        private void On_Loaded(object sender, RoutedEventArgs e)
        {
            _vm = this.DataContext as VoteCandidateVM;
            _vm.CommandAction = On_Candidate_CommandAction;
        }

        private void On_TXT_Name_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }

        private void On_TXT_Name_Drop(object sender, DragEventArgs e)
        {
            _vm = this.DataContext as VoteCandidateVM;
            if (_vm == null)
            {
                return;
            }

            String[] dropFiles = (String[])e.Data.GetData(DataFormats.FileDrop);
            String filePath = dropFiles[0];
            try
            {
                _vm.ImageObject = LoadImage(filePath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(String.Format("Set image source fail:{0}", ex.Message));
                _vm.ImageObject = null;
            }
        }

        private void On_TXT_Name_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_vm == null)
            {
                return;
            }

            _vm.ImageObject = null;
        }

        private bool On_Candidate_CommandAction(string cmdKey, VoteCandidateVM sender)
        {
            switch (cmdKey)
            {
                case VoteCandidateVM.CmdKey_OpenVoterColle:
                    NameListWindow wnd = new NameListWindow(sender.VoterColle);
                    wnd.ShowDialog();
                    break;

                default:
                    break;
            }
            return true;
        }

        #endregion Event Handle

        #region Private Method

        private BitmapImage LoadImage(String path)
        {
            if (!System.IO.File.Exists(path))
            {
                return null;
            }

            byte[] imgFileByte = System.IO.File.ReadAllBytes(path);
            if (imgFileByte == null
                || imgFileByte.Length == 0)
                return null;

            BitmapImage image = new BitmapImage();
            using (var mem = new System.IO.MemoryStream(imgFileByte))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }

        #endregion Private Method
    }
}