namespace CykeMaps.Core.Actions
{
    public class AddFavoriteAction : BasicAction
    {
        public AddFavoriteAction() : base("Favorite", "Favorit hinzufügen", new Commands.AddFavorite()) { }
    }

    public class EditFavoriteAction : BasicAction
    {
        public EditFavoriteAction() : base("Favorite", "Favorit bearbeiten", new Commands.EditFavorite()) { }
    }

    public class RemoveFavoriteAction : BasicAction
    {
        public RemoveFavoriteAction() : base("UnFavorite", "Favorit entfernen", new Commands.RemoveFavorite()) { }
    }

    public class SaveRouteAction : BasicAction
    {
        public SaveRouteAction() : base("Save", "Route speichern", new Commands.SaveRoute()) { }
    }

    public class EditSavedRouteAction : BasicAction
    {
        public EditSavedRouteAction() : base("Edit", "Route bearbeiten", new Commands.EditSavedRoute()) { }
    }

    public class PinToStartAction : BasicAction
    {
        public PinToStartAction() : base("Pin", "An Start anheften", new Commands.PinToStart()) { }
    }

    public class UnPinFromStartAction : BasicAction
    {
        public UnPinFromStartAction() : base("UnPin", "Vom Start entfernen", new Commands.UnPinFromStart()) { }
    }

    public class ShareAction : BasicAction
    {
        public ShareAction() : base("Send", "Teilen", new Commands.Share()) { }
    }

    public class ShowOnMapAction : BasicAction
    {
        public ShowOnMapAction() : base("Map", "Auf Karte", new Commands.ShowOnMap()) { }
    }

    public class RouteToAction : BasicAction
    {
        public RouteToAction() : base("Forward", "Wegbeschreibung", new Commands.RouteTo()) { }
    }

    public class RouteFromAction : BasicAction
    {
        public RouteFromAction() : base("Back", "Wegbeschreibung", new Commands.RouteFrom()) { }
    }

    public class CallAction : BasicAction
    {
        public CallAction() : base("Phone", "Anrufen", new Commands.Call()) { }
    }

    public class VisitWebsiteAction : BasicAction
    {
        public VisitWebsiteAction() : base("Globe", "Website besuchen", new Commands.VisitWebsite()) { }
    }

    public class SendEmailAction : BasicAction
    {
        public SendEmailAction() : base("Mail", "Email schreiben", new Commands.SendEmail()) { }
    }

    public class ShowCategoryAction : BasicAction
    {
        public ShowCategoryAction() : base("PhoneBook", "Kategorie", new Commands.ShowCategory()) { }
    }

    public class ShowOpeningHoursAction : BasicAction
    {
        public ShowOpeningHoursAction() : base("Calendar", "Öffnungszeiten", new Commands.ShowOpeningHours()) { }
    }

    public class RateAction : BasicAction
    {
        public RateAction() : base("LikeDislike", "Bewerten", new Commands.Rate()) { }
    }
}
