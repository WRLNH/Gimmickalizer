using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Gimmickalizer.ViewModels;

namespace Gimmickalizer.Views
{
    public partial class RootView : HandyControl.Controls.Window
    {
        //public string 

        public RootView()
        {
            InitializeComponent();
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
                    MessageBox.Show("你不输入路径想干嘛!");
                }
                else
                {
                    MessageBox.Show("路径错啦! 检查下吧");
                }
            }
        }

        private void Gimmickalize_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
