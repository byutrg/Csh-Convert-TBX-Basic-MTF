using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

using TBXTools.ConversionAPI.MTF;

namespace TBX_to_MTF_Converter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string AssemblyVersion
        {
            get => "v" + Assembly.GetEntryAssembly().GetName().Version.ToString(2);
        }

        string MTFOutputPath { get; set; }
        string TBXFilePath { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            ValidateForm();
        }

        private void ValidateForm()
        {
            foreach (var fileTextBox in MainGrid.Children.OfType<TextBox>().Where(tb => tb.Tag.Equals("input")))
            {
                if (fileTextBox.Name.StartsWith("tbx") && !IsValidTBXFilePath(fileTextBox.Text) ||
                    fileTextBox.Name.StartsWith("mtf") && !IsValidMTFFilePath(fileTextBox.Text))
                {
                    fileTextBox.Background = Brushes.Red;
                    convertButton.IsEnabled = false;
                    statusLabel.Content = "One or more paths are invalid or empy.";
                }
                else
                {
                    fileTextBox.Background = Brushes.White;
                    convertButton.IsEnabled = true;
                    statusLabel.Content = "Ready to convert.";
                }
            }
        }

        private bool IsValidTBXFilePath(string path)
        {
            return File.Exists(path);
        }

        private bool IsValidMTFFilePath(string path)
        {
            return !string.IsNullOrWhiteSpace(path) &&
                !System.IO.Path.GetInvalidPathChars().Any(c => path.Contains(c)) &&
                Directory.Exists(System.IO.Path.GetDirectoryName(path));
        }

        private void TbxBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.FileName = "TBX Document";
            openFileDialog.DefaultExt = "tbx";
            openFileDialog.Filter = "TBX documents (.tbx)|*.tbx";

            bool? result = openFileDialog.ShowDialog();
            if (result == true)
            {
                tbxInput.Text = openFileDialog.FileName;
            }
        }

        private void TbxInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (File.Exists((sender as TextBox).Text))
            {
                TBXFilePath = (sender as TextBox).Text;
                ValidateForm();
            }
        }

        private void MtfOutput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidMTFFilePath((sender as TextBox).Text))
            {
                MTFOutputPath = (sender as TextBox).Text;
            }
        }

        private void MtfBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = "MTF Document";
            saveFileDialog.DefaultExt = "mtf.xml";
            saveFileDialog.Filter = "MTF XML Documents (.mtf.xml)|*.mtf.xml";
            bool? result = saveFileDialog.ShowDialog();
            if (result == true)
            {
                mtfOutput.Text = saveFileDialog.FileName;
                ValidateForm();
            }
        }

        private void ConvertButton_Click(object sender, RoutedEventArgs e)
        {
            TBXTools.LoggingManager.Output = LogOutput;

            using (FileStream tbxInput = new FileStream(TBXFilePath, FileMode.Open))
            using (FileStream mtfOutput = new FileStream(MTFOutputPath, FileMode.Create))
            {
                if (TBXTools.ConversionAPI.MTF.Convert.Convert_TBX_MTF(tbxInput, mtfOutput))
                {
                    statusLabel.Content = "Conversion was successful.";
                } else
                {
                    statusLabel.Content = "Conversion failed.";
                }
            }
        }

        private void LogOutput(string message, params object[] args)
        {
            using (var logFile = File.AppendText(TBXFilePath + ".log"))
            {
                logFile.WriteLine(message);
            }
        }

        private void TbxInput_LostFocus(object sender, RoutedEventArgs e)
        {
            ValidateForm();
        }

        private void MtfOutput_LostFocus(object sender, RoutedEventArgs e)
        {
            ValidateForm();
        }
    }
}
