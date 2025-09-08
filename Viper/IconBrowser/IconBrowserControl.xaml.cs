namespace Viper.IconBrowser
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Xml.Linq;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using Microsoft.VisualStudio.Shell;
    using System.Windows.Media;

    /// <summary>
    /// Interaction logic for IconBrowserControl.xaml
    /// </summary>
    public partial class IconBrowserControl : UserControl
    {
        private readonly FileSystemWatcher _watcher;
        private readonly string _iconsXamlPath;
        private List<IconItem> _allIcons = new List<IconItem>();

        private DateTime _lastRead = DateTime.MinValue;

        public IconBrowserControl()
        {
            InitializeComponent();

#if DEBUG
            var root = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\source\repos\precisionvalve\PathMasterX\";
#else
            var root = Directory.GetCurrentDirectory();
#endif
            var iconsFile = Directory.GetFiles(root, "?cons.xaml", SearchOption.AllDirectories)
                                     .FirstOrDefault(f => f.EndsWith("icons.xaml", StringComparison.OrdinalIgnoreCase));

            if (iconsFile == null || !File.Exists(iconsFile))
            {
                MessageBox.Show("icons.xaml not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _iconsXamlPath = iconsFile;

            _watcher = new FileSystemWatcher
            {
                Path = Path.GetDirectoryName(_iconsXamlPath),
                Filter = Path.GetFileName(_iconsXamlPath),
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.Attributes
            };

            _watcher.Changed += IconsFile_Changed;
            _watcher.EnableRaisingEvents = true;

            // Initial load
            LoadIcons();

            IconList.ItemsSource = _allIcons;
        }

        private void IconsFile_Changed(object sender, FileSystemEventArgs e)
        {
            var now = DateTime.Now;
            if ((now - _lastRead).TotalMilliseconds < 500)
            {
                return;
            }

            _lastRead = now;

            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                try
                {
                    LoadIcons();
                    SearchBox.Text = string.Empty;
                    IconList.ItemsSource = _allIcons;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error reloading icons: {ex.Message}");
                }
            });
        }

        private void LoadIcons()
        {

            var doc = XDocument.Load(_iconsXamlPath);
            XNamespace xns = "http://schemas.microsoft.com/winfx/2006/xaml"; // for x:Key

            _allIcons = new List<IconItem>(doc.Descendants()
                .Where(e => e.Attribute(xns + "Key") != null && !string.IsNullOrWhiteSpace(e.Value))
                .Select(e => new IconItem(e.Attribute(xns + "Key")?.Value ?? "", e.Value.Trim())));
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string filter = SearchBox.Text.Trim();
            IconList.ItemsSource = string.IsNullOrEmpty(filter) ? _allIcons : new List<IconItem>(_allIcons.Where(i => i.Key.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0));
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            PlaceHolderText.Visibility = Visibility.Hidden;
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            PlaceHolderText.Visibility = SearchBox.Text.Length == 0 ? Visibility.Visible : Visibility.Hidden;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = new Uri(@"https://yqnn.github.io/svg-path-editor/").AbsoluteUri,
                UseShellExecute = true
            });
            e.Handled = true;
        }

        private void Copy_Icon_Name(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is IconItem item)
            {
                string textToCopy = item.Key;
                if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                {
                    textToCopy = item.PathData;
                }

                Clipboard.SetText(textToCopy);
            }
        }

        private void Open_Material_Icon_Website(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = new Uri(@"https://pictogrammers.com/library/mdi/").AbsoluteUri,
                UseShellExecute = true
            });
            e.Handled = true;
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DependencyObject ancestor = (sender as FrameworkElement)?.Parent;
            while (ancestor != null)
            {
                if (ancestor is UIElement element && element.Focusable)
                {
                    element.Focus();
                    break;
                }

                ancestor = VisualTreeHelper.GetParent(ancestor);
            }
        }

        private void Open_RandomOrg_Website(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = new Uri(@"https://www.random.org/strings/?num=1&len=32&digits=on&upperalpha=on&loweralpha=on&unique=on&format=html&rnd=new").AbsoluteUri,
                UseShellExecute = true
            });

            e.Handled = true;
        }
    }

    public class IconItem
    {
        public IconItem(string key, string pathData)
        {
            Key = key;
            PathData = pathData;
        }

        public string Key { get; }
        public string PathData { get; }
    }
}
