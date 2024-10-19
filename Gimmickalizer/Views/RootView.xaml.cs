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
        public Logger logger = LogManager.GetCurrentClassLogger();
        public List<FileInfo> lstFiles;
        public string inputFilePath;

        public RootView()
        {
            InitializeComponent();

            // logger相关
            Dispatcher.Invoke(() =>
            {
                var target = new WpfRichTextBoxTarget
                {
                    Name = "RichText",
                    Layout = "[${time}] [${level:uppercase=true}] :: ${message} ${exception:innerFormat=tostring:maxInnerExceptionLevel=10:seperator=,:format=tostring}",
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
                lstFiles = FolderInfo.GetFiles().ToList();
                foreach (FileInfo file in lstFiles)
                {
                    if (file.Extension == ".osu")
                    {
                        DifficultySelect.Items.Add(file.Name);
                    }
                }
                DifficultySelect.SelectedIndex = 0;
            }
            catch
            {
                if (FolderPath.Text == "" || FolderPath.Text == null)
                {
                    logger.Error("你不输入路径想干嘛!");
                }
                else
                {
                    logger.Error("路径错啦! 检查下吧");
                }
            }
        }

        private void Gimmickalize_Click(object sender, RoutedEventArgs e)
        {
            logger.Info("开始Gimmickalize...");
            try
            {
                try
                {
                    string fileFullName = FolderPath.Text + '\\' + DifficultySelect.SelectedItem.ToString();
                    StreamReader openFile = new(fileFullName);
                    logger.Info("File opened");

                    switch (GimmickTypeSelect.SelectedIndex)
                    {
                        case 0:
                            {
                                Barlines.MakeBarlines(in openFile, in fileFullName, in logger);
                                break;
                            }
                        case 1:
                            {
                                YellowAlternate.MakeYellowAlternate(in openFile, in fileFullName, in logger);
                                break;
                            }
                    }

                    openFile.Close();
                }
                catch
                {
                    logger.Error("Gimmickalize失败...");
                }
            }
            catch
            {
                logger.Error("打开文件失败...");
            }
        }
    }
}
