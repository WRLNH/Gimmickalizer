using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Gimmickalizer.Views;

public partial class MainView : UserControl
{
    public string InputFilePath;
    public MainView()
    {
        InitializeComponent();
        gimmickTypes.ItemsSource = new string[]
            {"Barlines", "Yellow Notes"}
        .OrderBy(x => x);

        
    }

    public void ErrorWindow(string message)
    {
        //Bitmap myBitmap = new("C:\\Users\\17894\\Desktop\\Gimmickalizer\\Gimmickalizer\\Assets\\avalonia-logo.ico");
        //myBitmap.Save("D:\\Android\\myBitmap.bmp");
        Window dialogWindow =
        new()
        {
            Position = new Avalonia.PixelPoint(1000, 500),
            Width = 200,
            Height = 100,
            Title = "Error",
            //Icon = WindowIcon(myBitmap),
            Content = new TextBlock
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Text = message
            }
        };
        dialogWindow.Show();
    }

    public void ButtonReadFilesClicked(object source, RoutedEventArgs args)
    {
        if (filePath.Text != null && filePath.Text != "")
        {
            InputFilePath = filePath.Text;
            try
            {
                DirectoryInfo root = new DirectoryInfo(InputFilePath);
                FileInfo[] files = root.GetFiles();
                List<FileInfo> lstFiles = files.ToList();
                for (int i = 0; i < lstFiles.Count; i++)
                {
                    if (lstFiles[i].Extension == ".osu")
                    {
                        diffComboBox.Items.Add(lstFiles[i].Name);
                    }
                }
                diffComboBox.SelectedIndex = 0;
            }
            catch
            {
                ErrorWindow("文件路径错啦！检查下吧");
            }
        }
        else
        {
            ErrorWindow("你不输入文件路径想干嘛？！");
        }
    }

    public void ButtonGenerateClicked(object source, RoutedEventArgs args)
    {
        //string filePath = "";
        //if (diffComboBox.SelectedIndex.ToString() != null && diffComboBox.SelectedIndex.ToString() != "")
        //{
        //    filePath = InputFilePath + "\\" + diffComboBox.SelectedItem.ToString();
        //}
        //else
        //{
        //    ErrorWindow("你真牛逼!");
        //}

        //try
        //{
        //    if (gimmickTypes.SelectedItem.ToString() == "Barlines")
        //    {
        //        Generator.Barlines(filePath);
        //    }
        //}
        //catch
        //{
        //    ErrorWindow("你不选Gimmick类型我怎么生成文件？");
        //}
    }
}

public class Generator
{
    public static void Barlines(string filePath)
    {
        string line;
        StreamReader sr = new StreamReader(filePath);

        line = sr.ReadLine();
        Debug.WriteLine(line);

        sr.Close();
    }
}