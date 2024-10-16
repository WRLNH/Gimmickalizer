using NLog;
using NLog.Targets.Helper;
using NLog.Targets.Wrappers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace Gimmickalizer.Views
{
    public partial class RootView : HandyControl.Controls.Window
    {
        private Logger logger = LogManager.GetCurrentClassLogger();

        public RootView()
        {
            InitializeComponent();

            Dispatcher.Invoke(() =>
            {
                var target = new WpfRichTextBoxTarget
                {
                    Name = "RichText",
                    Layout = "[${longdate:useUTC=false}] :: [${level:uppercase=true}] :: ${logger}:${callsite} :: ${message} ${exception:innerFormat=tostring:maxInnerExceptionLevel=10:seperator=,:format=tostring}",
                    ControlName = "LogRichTextBox",
                    FormName = GetType().Name,
                    AutoScroll = true,
                    MaxLines = 1000,
                    UseDefaultRowColoringRules = true
                };
                var asyncWrapper = new AsyncTargetWrapper { Name = "RichTextAsync", WrappedTarget = target };

                LogManager.Configuration.AddTarget(asyncWrapper.Name, asyncWrapper);
                LogManager.Configuration.LoggingRules.Insert(0, new NLog.Config.LoggingRule("*", LogLevel.Info, asyncWrapper));
                LogManager.ReconfigExistingLoggers();
            });
            
        }

        private void ReadPath_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 获取指定的.osu文件夹路径
                DirectoryInfo FolderInfo = new DirectoryInfo(FolderPath.Text);
                FileInfo[] Files = FolderInfo.GetFiles();
                List<FileInfo> lstFiles = Files.ToList();
                for (int i = 0; i < lstFiles.Count; i++)
                {
                    if (lstFiles[i].Extension == ".osu")
                    {
                        DifficultySelect.Items.Add(lstFiles[i].Name);
                    }
                }
                DifficultySelect.SelectedIndex = 0;
            }
            catch
            {
                if (FolderPath.Text == "" || FolderPath.Text == null)
                {
                    logger.Info("你不输入路径想干嘛!");
                }
                else
                {
                    logger.Info("路径错啦! 检查下吧");
                }
            }
        }

        private void Gimmickalize_Click(object sender, RoutedEventArgs e)
        {
            logger.Info("111");
        }
    }
}
