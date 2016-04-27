using CykeMaps.Core;
using CykeMaps.Core.Location;
using CykeMaps.Core.Route;
using CykeMaps.UI.Navigation;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Windows.Data.Text;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;

namespace CykeMaps.UI.Sheets
{
    /// <summary>
    /// The most important Sheet. Shows the Library (Favorites, Tracks, Recent Searches)
    /// </summary>
    public sealed partial class LibrarySheet : Page
    {
        private CollectionViewSource ViewSource = LibraryManager.Current.FilteredLibraryViewSource;

        public LibrarySheet()
        {
            this.InitializeComponent();
        }
        
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationManager.Current.NavigateTo(NavigationManager.Current.Settings, null);
        }

        private void EnableSelect(object sender, RoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "SelectEnabled", true);
        }

        private void CancelSelect(object sender, RoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "SelectDisabled", true);
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is Favorite) NavigationManager.Current.ShowLocation((e.ClickedItem as Favorite));
            else if (e.ClickedItem is FavoriteRoute) NavigationManager.Current.ShowRoute((e.ClickedItem as FavoriteRoute));
        }

        private void ListViewItemInnerGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (LibraryManager.Current.SearchQuery == "") return;
            foreach (TextBlock tb in FindVisualChildren<TextBlock>(sender as Grid))
            {
                var mySemanticTextQuery = new SemanticTextQuery(LibraryManager.Current.SearchQuery);
                IReadOnlyList<TextSegment> ranges = mySemanticTextQuery.Find(tb.Text);
                HighlightRanges(tb, tb.Text, ranges);
            }
        }
        
        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        public void HighlightRanges(TextBlock tb, string TextContent, IReadOnlyList<TextSegment> ranges)
        {
            tb.Text = "";

            int currentPosition = 0;
            foreach (var range in ranges)
            {
                // Add the next chunk of non-range text
                if (range.StartPosition > currentPosition)
                {
                    int length = (int)range.StartPosition - currentPosition;
                    var subString = TextContent.Substring(currentPosition, length);
                    tb.Inlines.Add(new Run() { Text = subString });
                    currentPosition += length;
                }
                // Add the next range
                var boldString = TextContent.Substring((int)range.StartPosition, (int)range.Length);
                tb.Inlines.Add(new Run() { Text = boldString, FontWeight = FontWeights.Bold });
                currentPosition += (int)range.Length;
            }
            // Add the text after the last matching segment
            if (currentPosition < TextContent.Length)
            {
                var subString = TextContent.Substring(currentPosition);
                tb.Inlines.Add(new Run() { Text = subString });
            }
        }

    }
}
