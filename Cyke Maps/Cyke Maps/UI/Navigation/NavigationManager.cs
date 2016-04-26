using System;
using System.Collections.Generic;
using CykeMaps.Core.Location;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Core;
using Windows.Foundation.Metadata;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using CykeMaps.UI.Sheets;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using CykeMaps.Core.Route;
using CykeMaps.Core.Route.RouteRequest;
using CykeMaps.Core;

namespace CykeMaps.UI.Navigation
{
    public class NavigationManager : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// The shared State is here to give Information about the State back to the MainPage so it can set parameters like MapCenter
        /// </summary>
        public State SharedState;

        /// <summary>
		/// This holds the instance to the Only NavigationManager in this app.
		/// </summary>
		public static NavigationManager Current { get; protected set; }

        /// <summary>
        /// This will hold the reference to the frame that is to be manipulated.
        /// </summary>
        public Frame SheetFrame;

        /// <summary>
        /// The reference to the ScrollViewer on the MainPage
        /// </summary>
        private ScrollViewer scrollViewer;
        
        /// <summary>
        /// The StatusBar on Mobile devices
        /// </summary>
        private StatusBar statusBar;

        /// <summary>
        /// The Stack of States to enable Stack based Navigation.
        /// </summary>
        public Stack<State> StateStack { get; protected set; }


        protected double ScrollViewerPreviousOffset = 0;

        /// <summary>
        /// The default constructor to instantiate this class with reference to a Frame, a StateWrapper and the ScrollViewer on the MainPage
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="sharedState"></param>
        /// <param name="scrollViewer"></param>
        public NavigationManager(ref Frame frame, ref ScrollViewer scrollViewer)
        {
            // Check is the instance doesnt already exist.
            if (Current != null)
            {
                //if there is an instance in the app already present then simply throw an error.
                throw new Exception("Only one navigation manager can exist in a App.");
            }

            // Setting the instance to the static instance field.
            Current = this;

            // Setting the frame / scrollViewer reference.
            this.SheetFrame = frame;
            this.scrollViewer = scrollViewer;

            // Handle the StatusBar on Mobile
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                statusBar = StatusBar.GetForCurrentView();
            }

            //initializing the stack.
            this.StateStack = new Stack<State>();

            // Create the initial State
            this.SharedState = new State();
            this.SharedState.Sheet = typeof(LibrarySheet);
            this.SharedState.SheetVisibility = Visibility.Hidden;

            // Add the initial State to the Stack
            NavigateTo(this.SharedState, null);

            // Hooking up the events for BackRequest.
            SystemNavigationManager.GetForCurrentView().BackRequested += NavigationManager_BackRequested;

            // Also listen to the InputPane (SIP aka Onscreen Keyboard)
            InputPane.GetForCurrentView().Hiding += InputPane_Hiding;
            InputPane.GetForCurrentView().Showing += InputPane_Showing;

            // Listen to the ScrollViewer
            scrollViewer.ViewChanging += ScrollViewer_ViewChanging;
            scrollViewer.ViewChanged += ScrollViewer_ViewChanged;

            ScrollViewerPreviousOffset = scrollViewer.VerticalOffset;
        }

        #region Navigation Methods

        /// <summary>
        /// Navigate to a State
        /// </summary>
        /// <param name="nextState"></param>
        /// <param name="parameter"></param>
        public void NavigateTo(State nextState, object parameter)
        {
            State previousState = null; // We don't know yet if there is a State
            if (StateStack.Count > 0) // Actually there should always be a State except for the very first call
            {
                previousState = StateStack.Peek(); // The currently visible State will be the previous State
                if (previousState == nextState) // It's the same State --> senseless
                    return;

                // If the current State is a Modal, delete it from the Stack so you can't go back here
                if (previousState.IsModal && nextState.HasSheet)
                {
                    StateStack.Pop();
                }
            }

            // Add it to the Stack
            StateStack.Push(nextState);

            // Change the Sheet if there is one (otherwise it just stays the same)
            Type sheet = nextState.Sheet;
            if (nextState.Sheet != null) SheetFrame.Navigate(nextState.Sheet, parameter);

            // If the current State is modal and the next one has a sheet, remove the modal one from the frames backstack
            if (previousState != null && previousState.IsModal && nextState.HasSheet)
            {
                if (previousState.HasSheet) SheetFrame.BackStack.RemoveAt(SheetFrame.BackStack.Count - 1);
            }

            // Change the shared State and notify about it
            SharedState = nextState;
            OnPropertyChanged("SharedState");

            if (SharedState.HasSheetVisibility) SetVisibility(SharedState.SheetVisibility);
            
            // Check if we still can go back
            UpdateBackButtonVisibility();
        }

        /// <summary>
        /// Go back one State
        /// </summary>
        public void NavigateBack()
        {
            // If we can't go back return immediately
            if (!CanGoBack) return;

            // Manipulate Stack
            State previousState = StateStack.Pop(); // The currently shown State
            State nextState = StateStack.Peek();    // Older State

            // Check if we can go back to this State or if it was a modal one /////// THIS SHOULD NORMALLY NOT HAPPEN, IT'S JUST TO BE SURE
            while (nextState.IsModal)
            {
                StateStack.Pop(); // Take the modal away
                nextState = StateStack.Peek(); // Take a look at the next one
            }

            // Check if the next State (in History the older one) had a Sheet --> Navigate there
            if (SheetFrame.CanGoBack && previousState.Sheet != null)
                SheetFrame.GoBack();

            SharedState = nextState;
            OnPropertyChanged("SharedState");

            if (SharedState.HasSheetVisibility) SetVisibility(SharedState.SheetVisibility);

            // Invoke the GoBackAction!
            if (previousState.HasOnBackAction) previousState.OnBackAction.Invoke();

            // Check if we still can go back
            UpdateBackButtonVisibility();
        }

        #endregion

        #region BackButtonVisibilty Region

        void UpdateBackButtonVisibility()
        {
            SystemNavigationManager.GetForCurrentView().
                AppViewBackButtonVisibility = CanGoBack ?
                 AppViewBackButtonVisibility.Visible :
                     AppViewBackButtonVisibility.Collapsed;
        }

        /// <summary>
        /// Checks if we can go back
        /// </summary>
        public bool CanGoBack
        {
            get
            {
                // There must always remain one State in the Stack!
                return StateStack.Count > 1;
            }
        }

        #endregion

        #region Event Methods for back button

        private void NavigationManager_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (CanGoBack)
            {
                this.NavigateBack();
                e.Handled = true;
            }
        }

        #endregion

        #region ScrollViewer Manipulation

        /// <summary>
        /// Scrolls the ScrollViewer up and down
        /// </summary>
        /// <param name="visibility"></param>
        internal void SetVisibility(Visibility visibility)
        {
            if (scrollViewer.VerticalScrollMode == ScrollMode.Disabled) return;

            switch (visibility)
            {
                case Visibility.Full:
                    scrollViewer.ChangeView(0, scrollViewer.ScrollableHeight, 1, false);
                    break;
                case Visibility.Half:
                    scrollViewer.ChangeView(0, scrollViewer.ScrollableHeight / MainPage.SheetHalfFactor, 1, false);
                    break;
                case Visibility.Hidden:
                    scrollViewer.ChangeView(0, 0, 1, false);
                    break;
            }
        }

        private void ScrollViewer_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            // Give the Statusbar a white Background if the SheetVisibility is Full
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                if (e.NextView.VerticalOffset == scrollViewer.ScrollableHeight && SettingsManager.Current.AppTheme != ElementTheme.Light)
                {
                    statusBar.ForegroundColor = Colors.LightGray;
                }
                else if (e.FinalView.VerticalOffset < scrollViewer.ScrollableHeight)
                {
                    statusBar.ForegroundColor = Colors.Black;
                }
            }
            
            //ScrollViewerPreviousOffset = scrollViewer.VerticalOffset;
            
            // If the Search input is scrolled out of view the focus is set to the scrollViewer, thus removing the SIP
            if (InputPane.GetForCurrentView().OccludedRect.Height > e.NextView.VerticalOffset)
            {
                SheetFrame.Focus(FocusState.Programmatic);
            }
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            
            // Kill the rubberband effect (bounce back)
            if ((scrollViewer.VerticalOffset == 0 && scrollViewer.VerticalOffset < ScrollViewerPreviousOffset) ||
                (scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight && scrollViewer.VerticalOffset > ScrollViewerPreviousOffset))
            {
                scrollViewer.CancelDirectManipulations();
            }
            
            ScrollViewerPreviousOffset = scrollViewer.VerticalOffset;

            // Make it large again
            SheetFrame.Height = scrollViewer.VerticalOffset;
            //if (SheetFrame.Height != scrollViewer.ScrollableHeight) SheetFrame.Height = scrollViewer.ScrollableHeight; SLOWER, RESOURCE ARM VARIANT

            if (e.IsIntermediate) return;
            
            // Swipe up is s significant State change, so it should be Backbuttonable :D
            if (SharedState.SheetVisibility != Visibility.Full && SharedState.SheetVisibility != Visibility.Half && Math.Abs(scrollViewer.VerticalOffset - scrollViewer.ScrollableHeight) < 0.001)
            {
                // Add this to the Stack
                NavigateTo(VisibilityFull, null);
            }

            Debug.WriteLine("SCROLL:" + scrollViewer.VerticalOffset);

            Debug.WriteLine(scrollViewer.ScrollableHeight + " / " + MainPage.SheetHalfFactor + " = " + scrollViewer.ScrollableHeight / MainPage.SheetHalfFactor);

            // Swipe up is s significant State change, so it should be Backbuttonable :D
            if (Math.Abs(scrollViewer.VerticalOffset - scrollViewer.ScrollableHeight / MainPage.SheetHalfFactor) < 10)
            {
                // Add this to the Stack
                if (SharedState.SheetVisibility != Visibility.Half && SharedState.SheetVisibility != Visibility.Full) NavigateTo(VisibilityHalf, null);

                //SheetFrame.Height = scrollViewer.ScrollableHeight / MainPage.SheetHalfFactor; SLOWER, RESOURCE ARM VARIANT
            }

            // Swipe down, directly after a Swipe up should remove the Swipe up from the Stack (UP->DOWN, now Back is the same State)
            if ((SharedState == VisibilityFull || SharedState == VisibilityHalf) && scrollViewer.VerticalOffset <= MainPage.HiddenSheetPeekSize)
            {
                // Manipulate Stack
                State previousState = StateStack.Pop(); // The currently shown State
                State nextState = StateStack.Peek();    // Older State

                // Pop through dem States til we get the one we're searching for
                while (nextState == VisibilityFull || nextState == VisibilityHalf)
                {
                    previousState = StateStack.Pop(); // Take the modal away
                    nextState = StateStack.Peek(); // Take a look at the next one
                }
                
                SharedState = nextState;
                OnPropertyChanged("SharedState");

                if (SharedState.HasSheetVisibility) SetVisibility(SharedState.SheetVisibility);
                
                // Check if we still can go back
                UpdateBackButtonVisibility();
            }
        }

        private void ScrollViewer_ManipulationDelta(object sender, Windows.UI.Xaml.Input.ManipulationDeltaRoutedEventArgs e)
        {
            Debug.WriteLine("------------------Manipulation Delta NavMan--------------------------------");
            Debug.WriteLine(e.IsInertial);
            Debug.WriteLine(e.Velocities.Linear.Y);

            Debug.WriteLine(e.Cumulative.Translation.Y);
            Debug.WriteLine(e.Delta.Translation.Y);
            Debug.WriteLine("--------------------------------------------------");

            //if (scrollViewer.VerticalOffset <= 0 || scrollViewer.VerticalOffset >= scrollViewer.ActualHeight - 48 - 48)
                //e.Handled = true;
        }

        #endregion

        #region InputPane aka SIP aka Keyboard

        // Handle the InputPane ourself. Just act as if it we did the necessary things to ensure the Control is in the View
        private void InputPane_Showing(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            if (scrollViewer.VerticalOffset <= sender.OccludedRect.Height)
            {
                args.EnsuredFocusedElementInView = true;
            }
        }

        private void InputPane_Hiding(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            args.EnsuredFocusedElementInView = false;
        }

        #endregion

        #region Often used States

        public State VisibilityHidden = new State()
        {
            SheetVisibility = Visibility.Hidden
        };

        public State VisibilityHalf = new State()
        {
            SheetVisibility = Visibility.Half
        };

        public State VisibilityFull = new State()
        {
            SheetVisibility = Visibility.Full
        };

        public State Settings = new State()
        {
            SheetVisibility = Visibility.Full,
            Sheet = typeof(SettingsSheet)
        };

        public State Library = new State()
        {
            Sheet = typeof(LibrarySheet)
        };

        #endregion

        public void ShowLocation(ILocation location)
        {
            State locationState = new State()
            {
                //MapCenter = location.Location,
                Sheet = typeof(LocationSheet),
                SheetVisibility = Visibility.Half
            };

            NavigateTo(locationState, location);
        }

        public void ShowOnMap(ILocation location)
        {
            State showOnMapState = new State()
            {
                MapCenter = location.Location,
                MapZoomLevel = 16,
                MapHeading = 90,
                SheetVisibility = Visibility.Hidden
            };

            NavigateTo(showOnMapState, null);
        }

        public void ShowOnMap(IRoute route)
        {
            State showOnMapState = new State()
            {
                MapHeading = 90,
                //MapElements = new List<MapElement>() { route }; // TODO: BLALBLAB
                SheetVisibility = Visibility.Hidden
            };

            NavigateTo(showOnMapState, null);
        }

        public void RouteTo(ILocation destination)
        {
            State routeToState = new State()
            {
                SheetVisibility = Visibility.Full,
                Sheet = typeof(RouteRequestSheet)
            };

            KomootRouteRequest request = new KomootRouteRequest(destination);

            NavigateTo(routeToState, request);
        }

        public void RouteFrom(ILocation start)
        {
            State routeToState = new State()
            {
                SheetVisibility = Visibility.Full,
                Sheet = typeof(RouteRequestSheet)
            };

            KomootRouteRequest request = new KomootRouteRequest(new BasicLocation(), start);

            NavigateTo(routeToState, request);
        }

        public void ShowRoute(IRoute route)
        {
            State routeToState = new State()
            {
                SheetVisibility = Visibility.Hidden
            };

            Windows.UI.Xaml.Controls.Maps.MapPolyline line = new Windows.UI.Xaml.Controls.Maps.MapPolyline();
            line.Path = new Windows.Devices.Geolocation.Geopath(route.Track);


            MainPage.Current.MapMain.MapElements.Add(line);

            NavigateTo(routeToState, null);
        }

        public void AddFavorite(ILocation location)
        {
            if (SheetFrame.CurrentSourcePageType == typeof(LocationSheet))
            {
                (SheetFrame.Content as LocationSheet).AddEditFavorite(this, new RoutedEventArgs());
                State cancelAddEditFavoriteState = new State()
                {
                    OnBackAction = new Action((SheetFrame.Content as LocationSheet).CancelAddEditFavorite)
                };

                NavigateTo(cancelAddEditFavoriteState, null);
            }
            else
            {
                State addFavoriteState = new State()
                {
                    Sheet = typeof(AddFavoriteSheet),
                    SheetVisibility = Visibility.Full,
                    IsModal = true
                };

                NavigateTo(addFavoriteState, location);
            }
        }

        public void EditFavorite(ILocation location)
        {
            if (SheetFrame.CurrentSourcePageType == typeof(LocationSheet))
            {
                (SheetFrame.Content as LocationSheet).AddEditFavorite(this, new RoutedEventArgs());

                State cancelAddEditFavoriteState = new State()
                {
                    OnBackAction = new Action((SheetFrame.Content as LocationSheet).CancelAddEditFavorite)
                };

                NavigateTo(cancelAddEditFavoriteState, null);
            }
            else
            {
                throw new NotImplementedException();
                /* editFavoriteState = new State()
                {
                    Sheet = typeof(EditFavoriteSheet),
                    SheetVisibility = Visibility.Full,
                    IsModal = true
                };

                NavigateTo(editFavoriteState, location);*/
            }
        }
    }
}
