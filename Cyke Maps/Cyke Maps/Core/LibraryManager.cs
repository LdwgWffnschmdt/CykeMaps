﻿using CykeMaps.Core.Location;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Storage;
using System.Linq;
using Windows.UI.Xaml.Data;
using System.Collections.ObjectModel;
using System.Globalization;
using Windows.System.Threading;
using Windows.UI.Core;
using CykeMaps.UI.Navigation;

namespace CykeMaps.Core
{
    public enum ReloadParameter
    {
        All,
        Favorites,
        Tracks,
        RecentSearches,
        RecentLocations
    }

    public sealed partial class LibraryManager : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private StorageFolder FavoritesFolder;

        private int FavoriteIndex = 0;

        #region Unfiltered Library

        private Collection<Collection<Collection<object>>> library;

        public Collection<Collection<Collection<object>>> Library
        {
            get { return library; }
            set
            {
                library = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Filtered Library

        private Collection<Collection<Collection<object>>> filteredLibrary;

        public Collection<Collection<Collection<object>>> FilteredLibrary
        {
            get { return filteredLibrary; }
            set
            {
                filteredLibrary = value;
                OnPropertyChanged();
            }
        }


        private CollectionViewSource filteredLibraryViewSource;

        public CollectionViewSource FilteredLibraryViewSource
        {
            get { return filteredLibraryViewSource; }
            set
            {
                filteredLibraryViewSource = value;
                OnPropertyChanged();
            }
        }


        #endregion

        private bool searchQueryBackStateSet = false;

        private string searchQuery = "";

        public string SearchQuery
        {
            get { return searchQuery; }
            set
            {
                if (searchQuery != value)
                {
                    if (!searchQueryBackStateSet && value != "")
                    {
                        State EmptySearchQueryState = new State()
                        {
                            OnBackAction = new Action(EmptySearchQuery)
                        };
                        searchQueryBackStateSet = true;
                        MainPage.MainNavigationManager.NavigateTo(EmptySearchQueryState, null);
                    }

                    string delayedText;
                    searchQuery = delayedText = value;
                    OnPropertyChanged();

                    TimeSpan delay = TimeSpan.FromMilliseconds(200);

                    ThreadPoolTimer DelayTimer = ThreadPoolTimer.CreateTimer(
                        (source) =>
                        {
                            if (delayedText == searchQuery)
                            {
                                RefreshFilteredLibrary();
                            }
                        }, delay);
                }
            }
        }

        private void EmptySearchQuery()
        {
            searchQueryBackStateSet = false;

            // If the Search Query is already empty
            if (searchQuery == "")
            {
                MainPage.MainNavigationManager.NavigateBack();
                return;
            }

            SearchQuery = "";
        }

        /// <summary>
        /// This holds the instance to the Only LibraryManager in this app.
        /// </summary>
        public static LibraryManager Instance { get; protected set; }

        public LibraryManager()
        {
            // Check is the instance doesnt already exist.
            if (Instance != null)
            {
                //if there is an instance in the app already present then simply throw an error.
                throw new Exception("Only one library manager can exist in a App.");
            }

            // Setting the instance to the static instance field.
            Instance = this;
            


            library = new Collection<Collection<Collection<object>>>("Library");
            library.Items.Add(new Collection<Collection<object>>("Favoriten", "Library")); // Index 0
            library.Items.Add(new Collection<Collection<object>>("Tracks", "Directions"));
            library.Items.Add(new Collection<Collection<object>>("Recent Searches", "Find"));
            library.Items.Add(new Collection<Collection<object>>("Recent Locations", "MapPin"));



            // FILTERED LIBRARY
            filteredLibrary = new Collection<Collection<Collection<object>>>("Library");
            filteredLibrary.Items.Add(new Collection<Collection<object>>("Favoriten", "Library")); // Index 0
            filteredLibrary.Items.Add(new Collection<Collection<object>>("Tracks", "Directions"));
            filteredLibrary.Items.Add(new Collection<Collection<object>>("Recent Searches", "Find"));
            filteredLibrary.Items.Add(new Collection<Collection<object>>("Recent Locations", "MapPin"));

            filteredLibraryViewSource = new CollectionViewSource();
            filteredLibraryViewSource.Source = filteredLibrary.Items;
            filteredLibraryViewSource.IsSourceGrouped = true;
            filteredLibraryViewSource.ItemsPath = new Windows.UI.Xaml.PropertyPath("Items");

            Reload(ReloadParameter.All);
        }

        #region Favorites

        private async Task LoadFavorites()
        {
            FavoritesFolder = await ApplicationData.Current.RoamingFolder.CreateFolderAsync("Favorites", CreationCollisionOption.OpenIfExists);

            IReadOnlyList<StorageFolder> collections = await FavoritesFolder.GetFoldersAsync();

            // Empty the existing list
            library.Items[FavoriteIndex].Items.Clear();
            filteredLibrary.Items[FavoriteIndex].Items.Clear();

            foreach (StorageFolder collection in collections)
            {
                try
                {
                    Collection<object> col = new Collection<object>(collection.DisplayName, "SolidStar");

                    var FavoritesFiles = await collection.GetFilesAsync();

                    foreach (StorageFile file in FavoritesFiles)
                    {
                        Favorite fav = await new Favorite().LoadFromFile(file);
                        fav.Collection = collection.DisplayName;
                        col.Items.Add(fav);
                    }

                    library.Items[FavoriteIndex].Items.Add(col);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }


            // And the unsorted ones
            try
            {
                Collection<object> col = new Collection<object>("Unsortiert", "OutlineStar");

                var FavoritesFiles = await FavoritesFolder.GetFilesAsync();

                foreach (StorageFile file in FavoritesFiles)
                {
                    col.Items.Add(await new Favorite().LoadFromFile(file));
                }

                library.Items[FavoriteIndex].Items.Add(col);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        #endregion

        /// <summary>
        /// Reloads the Library from the Files
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public async Task Reload(ReloadParameter parameter)
        {
            if (parameter == ReloadParameter.All || parameter == ReloadParameter.Favorites) await LoadFavorites();
            await RefreshFilteredLibrary();
            OnPropertyChanged("Library");
        }

        /// <summary>
        /// Refreshes the Filtered Library
        /// </summary>
        private async Task RefreshFilteredLibrary()
        {
            bool changed = false;

            int i = 0;
            // Loop through the different Favorite Collections
            foreach (Collection<object> favCollection in library.Items[FavoriteIndex].Items)
            {
                //if (favCollection == null ||favCollection.Items == null) continue; // Maybe an empty folder could result in this...

                // Does this Favorite Collection contain any Favorite that matches the Query?
                var fr = from fFavs in favCollection.Items
                         where (searchQuery == "" || // If there is no query everything is matched
                               CultureInfo.CurrentCulture.CompareInfo.IndexOf((fFavs as Favorite).Name, SearchQuery, CompareOptions.IgnoreCase) >= 0 ||
                               CultureInfo.CurrentCulture.CompareInfo.IndexOf((fFavs as Favorite).Address, SearchQuery, CompareOptions.IgnoreCase) >= 0 ||
                               CultureInfo.CurrentCulture.CompareInfo.IndexOf((fFavs as Favorite).Description, SearchQuery, CompareOptions.IgnoreCase) >= 0)
                         select fFavs;


                // Maybe this Collection didn't exist in the last run
                if (filteredLibrary.Items[FavoriteIndex].Items.Count < i + 1)
                {
                    filteredLibrary.Items[FavoriteIndex].Items.Add(new Collection<object>(favCollection.Name, favCollection.Symbol));
                }

                // There is a change
                if (fr.Count() != filteredLibrary.Items[FavoriteIndex].Items[i].Items.Count)
                {
                    changed = true;
                    filteredLibrary.Items[FavoriteIndex].Items[i].Items = new ObservableCollection<object>(fr);
                }

                i++;
            }


            //  This will limit the amount of view refreshes 
            if (!changed) return;

            // Send the notification
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    // The LinQ stuff is to hide all empty collections
                    FilteredLibraryViewSource.Source = filteredLibrary.Items.Where(header => header.Items.Count > 0)
                                                                            .Select(header => new Collection<object>(header.Name, header.Symbol)
                                                                            {
                                                                                Items = new ObservableCollection<object>(header.Items.Where(collection => collection.Items.Count > 0))
                                                                            }).ToList();
                });

        }
    }
}
