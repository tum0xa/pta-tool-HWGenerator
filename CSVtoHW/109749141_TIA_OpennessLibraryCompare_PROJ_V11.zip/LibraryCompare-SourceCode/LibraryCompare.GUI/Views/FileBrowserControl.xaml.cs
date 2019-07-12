using System.Collections.Generic;
using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace LibraryCompare.GUI.Views
{
    /// <summary>
    ///     Interaction logic for FolderBrowserControl.xaml
    /// </summary>
    public partial class FileBrowserControl
    {
        public FileBrowserControl()
        {
            InitializeComponent();
            LayoutRoot.DataContext = this;
        }

        private void BrowseFolder(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = string.IsNullOrEmpty(Filter) == false ? Filter : "All files (*.*)|*.*",
                InitialDirectory = string.IsNullOrEmpty(Path) == false && Directory.Exists(Path) ? Path : @"C:\"
            };
            if (dlg.ShowDialog() == true)
                Path = dlg.FileName;
        }

        private void TextBox_DragOver(object sender, DragEventArgs e)
        {
            var data = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (data != null && data.Length == 1)
            {
                var extensions = GetExtensions();
                if (extensions.Contains(".*"))
                {
                    e.Effects = DragDropEffects.Copy;
                }
                else
                {
                    var ext = System.IO.Path.GetExtension(data[0]);
                    if (extensions.Contains(ext))
                        e.Effects = DragDropEffects.Copy;
                    else
                        e.Effects = DragDropEffects.None;
                }
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void TextBox_Drop(object sender, DragEventArgs e)
        {
            var data = e.Data.GetData(DataFormats.FileDrop) as string[];
            Path = data?[0];
        }

        private HashSet<string> GetExtensions()
        {
            var ret = new HashSet<string>();

            if (string.IsNullOrEmpty(Filter))
            {
                ret.Add(".*");
                return ret;
            }

            var filter = Filter.Split('|');
            for (var i = 1; i < filter.Length; i += 2)
                foreach (var extension in filter[i].Split(';'))
                {
                    var ext = extension.Trim().TrimStart('*');
                    ret.Add(ext);
                }

            return ret;
        }

        #region TextBox DP

        public string Path
        {
            get { return (string) GetValue(PathProperty); }
            set { SetValue(PathProperty, value); }
        }

        public static readonly DependencyProperty PathProperty =
            DependencyProperty.Register("Path", typeof(string),
                typeof(FileBrowserControl), new PropertyMetadata(""));

        #endregion

        #region Filter DP

        public string Filter
        {
            get { return (string) GetValue(FilterProperty); }
            set { SetValue(FilterProperty, value); }
        }

        public static readonly DependencyProperty FilterProperty =
            DependencyProperty.Register("Filter", typeof(string),
                typeof(FileBrowserControl), new PropertyMetadata(""));

        #endregion
    }
}