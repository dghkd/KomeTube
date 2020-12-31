using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// AssessmentCenterWindow.xaml 的互動邏輯
    /// </summary>
    public partial class AssessmentCenterWindow : Window
    {
        #region Private Member

        private AssessmentCenterVM _vm;
        private ContestantListWindow _contestantListWnd;

        #endregion Private Member

        #region Constructor

        public AssessmentCenterWindow(AssessmentCenterVM vm)
        {
            InitializeComponent();
            _vm = vm;
            this.DataContext = _vm;
            _vm.CommandAction = On_CommandAction;

            _contestantListWnd = new ContestantListWindow(_vm.ContestantListViewModel);
            _contestantListWnd.Show();
        }

        private bool On_CommandAction(string cmdKey, AssessmentCenterVM sender)
        {
            switch (cmdKey)
            {
                case nameof(sender.CmdStart):
                    sender.Start();
                    break;

                case nameof(sender.CmdStop):
                    sender.Stop();
                    break;

                case nameof(sender.CmdOpenRaterColle):
                    NameListWindow wnd = new NameListWindow(sender.RaterColle);
                    wnd.Owner = this;
                    wnd.ShowDialog();
                    break;

                default:
                    break;
            }
            return true;
        }

        #endregion Constructor

        #region Event Handle

        private void On_Closed(object sender, EventArgs e)
        {
            _vm.Close();
            _contestantListWnd.Close();
        }

        private void On_TXT_ImageMask_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }

        private void On_TXT_ImageMask_Drop(object sender, DragEventArgs e)
        {
            _vm = this.DataContext as AssessmentCenterVM;
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

        private void On_TXT_ImageMask_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_vm == null)
            {
                return;
            }

            _vm.ImageObject = null;
        }

        private void On_GD_Score_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            int rowCount = (int)GD_Score.ActualHeight / 20;
            if (_vm != null
                && rowCount > 8)
            {
                _vm.ShowRaterRowCount = rowCount - 5;
            }
            else
                _vm.ShowRaterRowCount = 3;
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