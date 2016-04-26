using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Collections.Generic;
using CykeMaps.Core;

namespace CykeMaps.UI.Sheets
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class SettingsSheet : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        
        public SettingsSheet()
        {
            this.InitializeComponent();
            (this.Content as FrameworkElement).DataContext = SettingsManager.Current; // For Binding
            
            ThemeComboBox.ItemsSource = ThemeEnum;
        }

        private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SettingsManager.Current.AppTheme = (ElementTheme)ThemeComboBox.SelectedValue;
        }
        
        private static readonly Dictionary<ElementTheme, string> ThemeEnumMapping = new Dictionary<ElementTheme, string>()
        {
            {ElementTheme.Default, "Default"},
            {ElementTheme.Light, "Light"},
            {ElementTheme.Dark, "Dark"}
        };

        public Dictionary<ElementTheme, string> ThemeEnum
        {
            get
            {
                return ThemeEnumMapping;
            }
        }
    }
}
