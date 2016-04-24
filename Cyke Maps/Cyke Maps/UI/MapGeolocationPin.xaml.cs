using System.ComponentModel;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml.Input;
using Windows.Devices.Geolocation;

namespace CykeMaps.UI
{
    public sealed partial class MapGeolocationPin : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }


        public MapGeolocationPin()
        {
            this.InitializeComponent();
            RootGrid.DataContext = this;

            VisualStateManager.GoToState(this, "Undefined", false);
            VisualStateManager.GoToState(this, "Small", false);
        }

        #region Fields

        private IconElement icon;

        public IconElement Icon
        {
            get { return icon; }
            set
            {
                icon = value;

                if (value == null) PrimaryButton.Visibility = Visibility.Collapsed;
                else PrimaryButton.Visibility = Visibility.Visible;

                OnPropertyChanged();
            }
        }

        private string title;

        public string Title
        {
            get { return title; }
            set
            {
                title = value;
                OnPropertyChanged();
            }
        }

        private string text;

        public string Text
        {
            get { return text; }
            set
            {
                text = value;
                OnPropertyChanged();
            }
        }

        private int heading;

        public int Heading
        {
            get { return heading; }
            set
            {
                if (HeadingStates.CurrentState != KnownHeading)
                {
                    VisualStateManager.GoToState(this, "KnownHeading", true);
                }
                heading = value;
                OnPropertyChanged();
            }
        }
        
        private PositionStatus status;

        public PositionStatus Status
        {
            get { return status; }
            set
            {
                status = value;
                OnPropertyChanged();

                switch (status)
                {
                    case PositionStatus.Ready:
                        VisualStateManager.GoToState(this, "Ready", true);
                        break;
                    case PositionStatus.Initializing:
                        VisualStateManager.GoToState(this, "Initializing", true);
                        break;
                    case PositionStatus.NoData:
                        break;
                    case PositionStatus.Disabled:
                        VisualStateManager.GoToState(this, "Undefined", true);
                        break;
                    case PositionStatus.NotInitialized:
                        VisualStateManager.GoToState(this, "Undefined", true);
                        break;
                    case PositionStatus.NotAvailable:
                        break;
                    default:
                        break;
                }
            }
        }


        #endregion

        private void Small_Tapped(object sender, TappedRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "FullPin", true);
        }

        private void Small_GotFocus(object sender, RoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "FullPin", true);
        }

        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            //VisualStateManager.GoToState(this, "Small", true);
        }

        private void OnPointerEntered(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "PointerOver", true);
            //VisualStateManager.GoToState(this, "FullPin", true);
        }

        private void OnPointerExited(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "Normal", true);
            //VisualStateManager.GoToState(this, "Small", true);
        }

        private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "Pressed", true);
        }

        private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "Normal", true);
        }
    }
}
