namespace Octgn.Core.DataExtensionMethods
{
    using System;
    using System.Linq;

    using Octgn.Core.DataManagers;
    using Octgn.DataNew.Entities;

    public static class SetExtensionMethods
    {
        public static string GetPackUri(this Set set)
        {
            return "pack://file:,,," + set.PackageName.Replace('\\', ',');
        }

        public static Uri GetPictureUri(this Set set, string path)
        {
            return new Uri(set.GetPackUri() + path);
        }

        public static Game GetGame(this Set set)
        {
            return GameManager.Get().Games.FirstOrDefault(x=>x.Id == set.GameId);
        }
    }
}