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
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Collections.Concurrent;

    /// <summary>
    /// Interaction logic for IconBrowserControl.xaml
    /// </summary>
    public partial class IconBrowserControl : UserControl
    {
        private readonly FileSystemWatcher _watcher;
        private readonly string _iconsXamlPath;
        private ConcurrentBag<IconItem> _allIcons = new ConcurrentBag<IconItem>();

        private DateTime _lastRead = DateTime.MinValue;

        public IconBrowserControl()
        {
            InitializeComponent();
            IconIndexBorder.Visibility = Visibility.Collapsed;
            SubIconBorder.Visibility = Visibility.Collapsed;

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

            _ = Task.Run(() =>
            {
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

            _allIcons = new ConcurrentBag<IconItem>(doc.Descendants()
                .Where(e => e.Attribute(xns + "Key") != null && !string.IsNullOrWhiteSpace(e.Value))
                .Select(e => new IconItem(e.Attribute(xns + "Key")?.Value ?? "", e.Value.Trim())));
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string filter = SearchBox.Text.Trim();
            foreach (var item in _allIcons)
            {
                item.IsCloseMatch = false;
            }

            if (!string.IsNullOrEmpty(filter))
            {
                var matchedIcons = _allIcons.Where(i => i.Key.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) >= 0);
                var filteredList = filter.FindMatchesLimitDistance(_allIcons.Except(matchedIcons).Select(i => i.Key), 6);
                var closeMatchIcons = _allIcons.Where(i => filteredList.Contains(i.Key));
                var filteredCloseMatches = closeMatchIcons.Except(matchedIcons);
                filteredCloseMatches.ToList().ForEach(i => i.IsCloseMatch = true);
                var combinedList = matchedIcons.Concat(filteredCloseMatches);
                IconList.ItemsSource = combinedList;
            }
            else
            {
                IconList.ItemsSource = _allIcons;
            }
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            PlaceHolderText.Visibility = Visibility.Hidden;
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            PlaceHolderText.Visibility = SearchBox.Text.Length == 0 ? Visibility.Visible : Visibility.Hidden;
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

        private void SetIcon(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fwe && fwe.DataContext is IconItem iconItem)
            {
                IconPath.Data = Geometry.Parse(iconItem.PathData);
                HandleMask();
            }
        }

        private void ClearIcon(object sender, RoutedEventArgs e)
        {
            IconPath.Data = Geometry.Empty;
            HandleMask();
        }

        private void SetSubIcon(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fwe && fwe.DataContext is IconItem iconItem)
            {
                SubIconBorder.Visibility = Visibility.Visible;
                SubIconPath.Data = Geometry.Parse(iconItem.PathData);
                HandleMask();
            }
        }

        private void ClearSubIcon(object sender, RoutedEventArgs e)
        {
            SubIconBorder.Visibility = Visibility.Collapsed;
            SubIconPath.Data = Geometry.Empty;
            HandleMask();
        }

        private void FlopIconIndexVisibility(object sender, RoutedEventArgs e)
        {
            if (IconIndexBorder.Visibility == Visibility.Visible)
            {
                IconIndexBorder.Visibility = Visibility.Collapsed;
                IconIndexText.Text = string.Empty;
                IconIndexMenuItem.Header = "Set Icon Index";
            }
            else
            {
                IconIndexBorder.Visibility = Visibility.Visible;
                IconIndexText.Text = "1";
                IconIndexMenuItem.Header = "Clear Icon Index";
            }

            HandleMask();
        }

        private void ClearIconIndex(object sender, RoutedEventArgs e)
        {
            HandleMask();
        }

        private void HandleMask()
        {
            if (SubIconBorder.Visibility == Visibility.Visible)
            {
                if (IconIndexBorder.Visibility == Visibility.Visible)
                {
                    MainIconBorderMask.Data = Geometry.Parse("M 0 15 L 3 15 A 12 12 0 0 0 15 3 L 15 0 L 36 0 L 36 19 L 33 19 A 14 14 180 0 0 19 33 L 19 36 L 0 36 Z");
                    MainIconBorderMask.Fill = Brushes.Black;
                }
                else
                {
                    MainIconBorderMask.Data = Geometry.Parse("M 36 19 L 33 19 A 14 14 180 0 0 19 33 L 19 36 L 0 36 L 0 0 L 36 0 Z");
                    MainIconBorderMask.Fill = Brushes.Black;
                }
            }
            else
            {
                if (IconIndexBorder.Visibility == Visibility.Visible)
                {
                    MainIconBorderMask.Data = Geometry.Parse("M 0 15 L 3 15 A 12 12 0 0 0 15 3 L 15 0 L 36 0 L 36 36 L 0 36 Z");
                    MainIconBorderMask.Fill = Brushes.Black;
                }
                else
                {
                    MainIconBorderMask.Data = Geometry.Parse("M0 0H100V100H0V0z");
                    MainIconBorderMask.Fill = Brushes.White;
                }
            }
        }

        private void ClearSearchBox(object sender, RoutedEventArgs e)
        {
            SearchBox.Clear();
        }

        private void OpenLink(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                string targetUri = string.Empty;
                switch (comboBox.SelectedIndex)
                {
                    case -1:
                        break;
                    case 0:
                        targetUri = @"https://yqnn.github.io/svg-path-editor/";
                        break;
                    case 1:
                        targetUri = @"https://pictogrammers.com/library/mdi/";
                        break;
                    case 2:
                        targetUri = @"https://www.random.org/strings/?num=1&len=32&digits=on&upperalpha=on&loweralpha=on&unique=on&format=html&rnd=new";
                        break;
                }

                if (!string.IsNullOrEmpty(targetUri))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = new Uri(targetUri).AbsoluteUri,
                        UseShellExecute = true
                    });

                    comboBox.SelectedIndex = -1;
                }
            }
        }
    }

    /// <summary>
    /// Extension methods for <see langword="string"/> types.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Uses the Levenshtein Distance Algorithm to fuzzy match a source string against a collection of potential matches.
        /// </summary>
        /// <param name="source">The source string.</param>
        /// <param name="options">The list of potential matches.</param>
        /// <returns>The single closest match.</returns>
        public static string FindClosestMatch(this string source, IEnumerable<string> options)
        {
            if (string.IsNullOrEmpty(source) || options == null || !options.Any())
            {
                return string.Empty;
            }

            string closestMatch = string.Empty;
            int smallestDistance = int.MaxValue;
            foreach (string option in options)
            {
                int distance = ComputeLevenshteinDistance(source, option);
                if (distance < smallestDistance)
                {
                    smallestDistance = distance;
                    closestMatch = option;

                    if (smallestDistance == 0)
                    {
                        return closestMatch;
                    }
                }
            }

            Debug.WriteLine($"{source} : Closest Match: {closestMatch} at distance: {smallestDistance}");
            return closestMatch;
        }

        /// <summary>
        /// Finds multiple closest matches.
        /// </summary>
        /// <param name="source">The source string to match.</param>
        /// <param name="options">The list of strings to match against.</param>
        /// <param name="maximumNumberOfMatches">The maximum number of close matches to find.</param>
        /// <returns>A list of the closest n# of matches.</returns>
        public static List<string> FindMatches(this string source, IEnumerable<string> options, int maximumNumberOfMatches)
        {
            var dictionary = new Dictionary<string, int>();
            var list = new List<string>();
            if (string.IsNullOrEmpty(source) || options == null || !options.Any() || maximumNumberOfMatches == 0)
            {
                return list;
            }

            foreach (string option in options)
            {
                dictionary.Add(option, ComputeLevenshteinDistance(source, option));
            }

            list.AddRange(from KeyValuePair<string, int> item in dictionary.OrderBy(x => x.Value).Take(maximumNumberOfMatches)
                          select item.Key);

            return list;
        }

        /// <summary>
        /// Finds multiple closest matches.
        /// </summary>
        /// <param name="source">The source string to match.</param>
        /// <param name="options">The list of strings to match against.</param>
        /// <param name="maxDistance">The maximum distance from the input string to allow.</param>
        /// <returns>A list of the closest n# of matches.</returns>
        public static List<string> FindMatchesLimitDistance(this string source, IEnumerable<string> options, int maxDistance)
        {
            var dictionary = new Dictionary<string, int>();
            var list = new List<string>();
            if (string.IsNullOrEmpty(source) || options == null || !options.Any())
            {
                return list;
            }

            foreach (string option in options)
            {
                string iconTrimmed = option.Substring(0, option.Length - 4);
                string materialTrimmed = iconTrimmed.EndsWith("ial", StringComparison.OrdinalIgnoreCase) ? iconTrimmed.Substring(0, iconTrimmed.Length - 8) : iconTrimmed;
                string allTrimmed = materialTrimmed.EndsWith("Custom", StringComparison.OrdinalIgnoreCase) ? materialTrimmed.Substring(0, materialTrimmed.Length - 6) : materialTrimmed;
                if (ComputeLevenshteinDistance(source, allTrimmed) is int distance && distance <= maxDistance)
                {
                    dictionary.Add(option, distance);
                }
            }

            var sorted = dictionary.OrderBy(x => x.Value).Take(50);

            list.AddRange(from KeyValuePair<string, int> item in sorted
                          select item.Key);

            return list;
        }

        // taken from https://github.com/DanHarltey/Fastenshtein/blob/master/src/Fastenshtein/StaticLevenshtein.cs MIT license
        private static int ComputeLevenshteinDistance(string source, string option)
        {
            if (option.Length == 0)
            {
                return source.Length;
            }

            int[] costs = new int[option.Length];

            for (int i = 0; i < costs.Length;)
            {
                costs[i] = ++i;
            }

            for (int i = 0; i < source.Length; i++)
            {
                int cost = i;
                int previousCost = i;

                char value1Char = source[i];

                for (int j = 0; j < option.Length; j++)
                {
                    int currentCost = cost;

                    cost = costs[j];

                    if (value1Char != option[j])
                    {
                        if (previousCost < currentCost)
                        {
                            currentCost = previousCost;
                        }

                        if (cost < currentCost)
                        {
                            currentCost = cost;
                        }

                        ++currentCost;
                    }

                    costs[j] = currentCost;
                    previousCost = currentCost;
                }
            }

            return costs[costs.Length - 1];
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
        public bool IsCloseMatch { get; set; }
    }
}
