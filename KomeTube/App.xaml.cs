using KomeTube.Kernel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace KomeTube
{
    /// <summary>
    /// App.xaml 的互動邏輯
    /// </summary>
    public partial class App : Application
    {
        public static StartupParameter AppStartupParameter = new StartupParameter();

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            //解析命令列參數
            ParseArgs(e);
        }

        /// <summary>
        /// 解析命令列參數
        /// </summary>
        /// <param name="e">程式啟動時帶入的命令列參數</param>
        private void ParseArgs(StartupEventArgs e)
        {
            for (int i = 0; i < e.Args.Length;)
            {
                string arg = e.Args[i];
                switch (arg)
                {
                    case "-url":
                        AppStartupParameter.Url = e.Args[i + 1];
                        i += 2;
                        break;

                    case "-o":
                        AppStartupParameter.OutputFilePath = e.Args[i + 1];
                        i += 2;
                        break;

                    case "-hide":
                        AppStartupParameter.IsHide = true;
                        i++;
                        break;

                    case "-close":
                        AppStartupParameter.IsClose = true;
                        i++;
                        break;

                    default:
                        break;
                }
            }
        }
    }
}