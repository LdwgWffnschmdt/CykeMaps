using CykeMaps.Core.Location;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml.Input;
using System.Diagnostics;

namespace CykeMaps.UI
{
    public sealed partial class MapPin : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public MapPin()
        {
            this.InitializeComponent();
            (this.Content as FrameworkElement).DataContext = this;
        }
        
        private void Small_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Debug.WriteLine("Small_Tapped");
            if (DataContext != null) MainPage.MainNavigationManager.ShowLocation(DataContext as ILocation);
            this.Focus(FocusState.Pointer);
            //VisualStateManager.GoToState(this, "FullPin", true);
        }

        private void Small_GotFocus(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Small_GotFocus" + e.OriginalSource + " : " + sender.ToString());
        }

        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("OnLostFocus");
            //VisualStateManager.GoToState(this, "Small", true);
        }

        private void OnPointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Debug.WriteLine("OnPointerEntered");
            VisualStateManager.GoToState(this, "PointerOver", true);
            //VisualStateManager.GoToState(this, "FullPin", true);
        }

        private void OnPointerExited(object sender, PointerRoutedEventArgs e)
        {
            Debug.WriteLine("OnPointerExited");
            VisualStateManager.GoToState(this, "Normal", true);
            //VisualStateManager.GoToState(this, "Small", true);
        }

        private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            Debug.WriteLine("OnPointerPressed");
            VisualStateManager.GoToState(this, "Pressed", true);
        }

        private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            Debug.WriteLine("OnPointerReleased");
            VisualStateManager.GoToState(this, "Normal", true);
        }
    }
}
