using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using System.IO;
using Ionic.Zlib;



namespace SteamWorldQuestFileUnpacker
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<string> fileList = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
            this.MinHeight = 110;
            this.MinWidth = 400;
        }



        private void Button_Click(object sender, RoutedEventArgs e)
        {
            fileList.Clear();
            StringBuilder fileNames = new StringBuilder();
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == true)
                foreach (string filename in openFileDialog.FileNames)
                {
                    fileNames.Append(filename);
                    fileNames.Append("\n");
                    fileList.Add(filename);
                }

            sourcePathText.Text = fileNames.ToString();
        }


        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            fileList.RemoveAll(filePath => filePath.Substring(filePath.Length - 2) == ".z");

            var progress = new Progress<Tuple<int, string>>(value => currentOperationText.Text = "Processing " + value.Item1 + "/" + fileList.Count.ToString() + " file: " + value.Item2);
            await Task.Run(() =>
            {
                int k = 1;
                foreach (string filePath in fileList)
                {
                    ((IProgress<Tuple<int, string>>)progress).Report(Tuple.Create<int, string>(k, System.IO.Path.GetFileName(filePath)));
                    using (FileStream fs = File.OpenRead(filePath))
                    using (FileStream streamWriter = File.Open(filePath + ".z", FileMode.Create))
                    using (Stream compressor = new ZlibStream(fs, CompressionMode.Compress, CompressionLevel.Default))
                    {
                        byte[] fileSize = BitConverter.GetBytes((int)new FileInfo(filePath).Length);
                        if (!BitConverter.IsLittleEndian)
                            Array.Reverse(fileSize);
                        streamWriter.Write(fileSize, 0, 4);
                        compressor.CopyTo(streamWriter, 4096);
                    }
                    k++;
                    
                }
            });
            fileList.Clear();
            currentOperationText.Text = "Finished";

        }
    
        private async void Button_Click_3(object sender, RoutedEventArgs e)
        {
            fileList.RemoveAll(filePath => filePath.Substring(filePath.Length - 2) != ".z");

            var progress = new Progress<Tuple<int,string>>(value => currentOperationText.Text = "Processing "+value.Item1 + "/" + fileList.Count.ToString() +" file: " + value.Item2);
            await Task.Run(() =>
            {
                int k = 1;
                foreach (string filePath in fileList)
                {                 
                    ((IProgress<Tuple<int, string>>)progress).Report(Tuple.Create<int, string>(k,System.IO.Path.GetFileName(filePath)));
                    using(FileStream fs = File.OpenRead(filePath))
                    {
                        for (int i = 1; i <= 4; i++)
                            fs.ReadByte();
                        using(FileStream streamWriter = File.Open(filePath.Remove(filePath.Length - 2), FileMode.Create))
                        using(Stream decompressor = new ZlibStream(fs, CompressionMode.Decompress, CompressionLevel.Default))
                        {
                            decompressor.CopyTo(streamWriter,4096);
                        }
                    }
                    k++;                 
                }    
            });
            fileList.Clear();
            currentOperationText.Text = "Finished";
        }
    }
}
