using CykeMaps.Core.Actions;
using Geo.Gps;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Devices.Geolocation;
using Windows.Storage;

namespace CykeMaps.Core.Location
{
    public class Favorite : BasicLocation
    {
        #region Constructors

        public Favorite()
        {
            Name = "";
            Address = "";
            Description = "";
            Symbol = "";
            Collection = "";

            Actions = new List<IAction>()
                {
                    new ShowOnMapAction(),
                    new RouteToAction()
                };

            SecondaryActions = new List<IAction>()
                {
                    new PinToStartAction(),
                    new ShareAction(),
                    new EditFavoriteAction()
                };
        }

        public Favorite(GpsData data)
        {
            Name = data.Metadata["name"];
            Description = data.Metadata["desc"];
            Symbol = data.Metadata["symbol"];
            Timestamp = DateTime.Parse(data.Metadata["time"]);
            Location = new Geopoint(new BasicGeoposition()
            {
                Latitude = data.Waypoints[0].Coordinate.Latitude,
                Longitude = data.Waypoints[0].Coordinate.Longitude
            });
        }

        #endregion

        #region File Management
        
        public async Task<Favorite> LoadFromFile(StorageFile file)
        {
            var serializer = new XmlSerializer(typeof(gpxType));
            Stream stream = await file.OpenStreamForReadAsync();
            gpxType objectFromXml = (gpxType)serializer.Deserialize(stream);
            stream.Dispose();

            Name = (objectFromXml.metadata.name == null) ? "" : objectFromXml.metadata.name;
            Description = (objectFromXml.metadata.desc == null) ? "" : objectFromXml.metadata.desc;
            Timestamp = (objectFromXml.metadata.time == null) ? DateTime.Now : objectFromXml.metadata.time;
            Symbol = (objectFromXml.metadata.extensions.symbol == null) ? "" : objectFromXml.metadata.extensions.symbol;
            Address = (objectFromXml.metadata.extensions.address == null) ? "" : objectFromXml.metadata.extensions.address;
            Location = new Geopoint(new BasicGeoposition()
            {
                Latitude = (double) objectFromXml.wpt[0].lat,
                Longitude = (double)objectFromXml.wpt[0].lon,
                Altitude = (double)objectFromXml.wpt[0].ele
            });

            return this;
        }

        public async Task SaveToFile()
        {
            await SaveToFile("");
        }

        public async Task SaveToFile(string collection)
        {
            var invalids = System.IO.Path.GetInvalidFileNameChars();
            var filename = string.Join("_", Name.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.') + ".gpx";

            var serializer = new XmlSerializer(typeof(gpxType));
            StorageFolder FavoritesFolder = await ApplicationData.Current.RoamingFolder.CreateFolderAsync("Favorites", CreationCollisionOption.OpenIfExists);
            if (collection != "") FavoritesFolder = await FavoritesFolder.CreateFolderAsync(collection, CreationCollisionOption.OpenIfExists);

            StorageFile file = await FavoritesFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
            Stream stream = await file.OpenStreamForWriteAsync();

            using (stream)
            {
                gpxType objectToSave = new gpxType()
                {
                    metadata = new metadataType()
                    {
                        name = Name,
                        desc = Description,
                        extensions = new extensionsType()
                        {
                            symbol = Symbol,
                            address = Address
                        },
                        author = new personType()
                        {
                            name = "Cyke Maps"
                        },
                        timeSpecified = true,
                        time = Timestamp
                    },
                    creator = "Cyke Maps",
                    wpt = new wptType[]
                    {
                        new wptType()
                        {
                            lat = (decimal)Location.Position.Latitude,
                            lon = (decimal)Location.Position.Longitude,
                            eleSpecified = true,
                            ele = (decimal)Location.Position.Altitude
                        }
                    }
                };
                serializer.Serialize(stream, objectToSave);
            }

            // Reload the Favorites
            LibraryManager.Current.Reload(ReloadParameter.Favorites);
        }

        public async Task Delete()
        {
            // What characters are allowed?
            var invalids = System.IO.Path.GetInvalidFileNameChars();

            // Sanitize filename
            var fileName = string.Join("_", Name.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.') + ".gpx";

            // The Favorite should be in the Roaming Folder (or a subdirectory)
            StorageFolder FavoritesFolder = await ApplicationData.Current.RoamingFolder.CreateFolderAsync("Favorites", CreationCollisionOption.OpenIfExists);

            // If the Favorite is in a collection go into that folder
            if (Collection != "") FavoritesFolder = await FavoritesFolder.CreateFolderAsync(Collection, CreationCollisionOption.OpenIfExists);

            // Get the file
            StorageFile FavoriteFile = await FavoritesFolder.GetFileAsync(fileName);

            // Delete the File
            if (FavoriteFile != null) await FavoriteFile.DeleteAsync();

            // If the Collection folder is empty now we can delete it
            if ((await FavoritesFolder.GetFilesAsync()).Count == 0) await FavoritesFolder.DeleteAsync();


            // Reload the Favorites
            LibraryManager.Current.Reload(ReloadParameter.Favorites);
        }

        //public static async Task<T> ReadObjectFromXmlFileAsync<T>(string filename)
        //{
        //    // this reads XML content from a file ("filename") and returns an object  from the XML
        //    T objectFromXml = default(T);
        //    var serializer = new XmlSerializer(typeof(T));
        //    StorageFolder folder = await ApplicationData.Current.RoamingFolder.CreateFolderAsync("Favorites", CreationCollisionOption.OpenIfExists);
        //    StorageFile file = await folder.GetFileAsync(filename);
        //    Stream stream = await file.OpenStreamForReadAsync();
        //    objectFromXml = (T)serializer.Deserialize(stream);
        //    stream.Dispose();
        //    return objectFromXml;
        //}

        //public static async Task SaveObjectToXml<T>(T objectToSave, string filename)
        //{
        //    // stores an object in XML format in file called 'filename'
        //    var serializer = new XmlSerializer(typeof(T));
        //    StorageFolder folder = ApplicationData.Current.LocalFolder;
        //    StorageFile file = await folder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
        //    Stream stream = await file.OpenStreamForWriteAsync();

        //    using (stream)
        //    {
        //        serializer.Serialize(stream, objectToSave);
        //    }
        //}

        #endregion

        private string collection;

        public string Collection
        {
            get { return collection; }
            set
            {
                collection = value;
                OnPropertyChanged();
            }
        }

        private DateTime timestamp;

        public DateTime Timestamp
        {
            get { return timestamp; }
            set
            {
                timestamp = value;
                OnPropertyChanged();
            }
        }

        public GpsData ToGpsData()
        {
            GpsData data = new GpsData();

            data.Metadata.Add("name", Name);
            data.Metadata.Add("desc", Description);
            data.Metadata.Add("time", Timestamp.ToString());
            data.Metadata.Add("symbol", Symbol);
            data.Metadata.Add("author", "Cyke Maps");

            Geo.Geometries.Point point = new Geo.Geometries.Point(Location.Position.Latitude,
                                                        Location.Position.Longitude,
                                                        Location.Position.Altitude);

            data.Waypoints.Add(point);

            return data;
        }
    }
}
