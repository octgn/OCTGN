using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;

namespace Octgn.Tools.SetBuilder
{
    public class setConfig
    {
        public List<gameSettings> gameList;
        public setConfig()
        {
            gameList = new List<gameSettings> { };
        }
        public void Load(string filename)
        {
            gameList.Clear();
            if (File.Exists(filename))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(filename);
                XmlNodeList list = doc.GetElementsByTagName("game");
                foreach (XmlNode game in list)
                {
                    gameSettings gS = new gameSettings(Guid.Parse(game.Attributes["id"].Value));
                    gS.saveAllProperties = Convert.ToBoolean(game.Attributes["saveAll"].Value);
                    foreach (XmlNode propNode in game.ChildNodes)
                    {
                        gameProperty gP = new gameProperty(propNode.Attributes["name"].Value);
                        gP.alwaysSave = Convert.ToBoolean(propNode.Attributes["alwaysSave"].Value);
                        gS.propertyList.Add(gP);
                    }
                    gameList.Add(gS);
                }
            }
        }
        public Boolean HasSet(Guid id)
        {
            Boolean has = false;
            foreach (gameSettings gS in gameList)
            {
                if (gS.id == id) has = true;
            }
            return has;
        }
        public gameSettings GetSet(Guid id)
        {
            foreach (gameSettings gS in gameList)
            {
                if (gS.id == id) return gS;
            }
            return null;
        }
        public gameSettings AddSet(Guid guid, List<String> properties)
        {
            gameSettings setting = new gameSettings(guid, properties);
            gameList.Add(setting);
            return setting;
        }
    
        public void Save(string filename)
        {
            if (File.Exists(filename)) File.Delete(filename);
            XmlDocument doc = new XmlDocument();
            XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", "utf-8", "yes");
            doc.AppendChild(dec);
            XmlNode gamesNode = doc.CreateNode(XmlNodeType.Element, "games", null);
            foreach (gameSettings setting in gameList)
            {
                XmlNode gameNode = doc.CreateNode(XmlNodeType.Element, "game", null);
                XmlNode gameId = doc.CreateNode(XmlNodeType.Attribute, "id", null);
                gameId.Value = setting.id.ToString();
                gameNode.Attributes.SetNamedItem(gameId);
                XmlNode gameSave = doc.CreateNode(XmlNodeType.Attribute, "saveAll", null);
                gameSave.Value = setting.saveAllProperties.ToString();
                gameNode.Attributes.SetNamedItem(gameSave);
                foreach (gameProperty prop in setting.propertyList)
                {
                    XmlNode propNode = doc.CreateNode(XmlNodeType.Element, "property", null);
                    XmlNode propName = doc.CreateNode(XmlNodeType.Attribute, "name", null);
                    XmlNode propSave = doc.CreateNode(XmlNodeType.Attribute, "alwaysSave", null);
                    propName.Value = prop.property;
                    propSave.Value = prop.alwaysSave.ToString();
                    propNode.Attributes.SetNamedItem(propName);
                    propNode.Attributes.SetNamedItem(propSave);
                    gameNode.AppendChild(propNode);
                }
                gamesNode.AppendChild(gameNode);
            }
            doc.AppendChild(gamesNode);
            doc.Save(filename);
        }
    }

    public class gameSettings
    {
        public Guid id;
        public Boolean saveAllProperties;
        public List<gameProperty> propertyList;
        public gameSettings(Guid gameId)
        {
            id = gameId;
            saveAllProperties = false;
            propertyList = new List<gameProperty> { };
        }
        public gameSettings(Guid gameId, List<string> properties)
        {
            id = gameId;
            propertyList = new List<gameProperty> { };
            foreach (string propName in properties)
            {
                gameProperty prop = new gameProperty(propName);
                propertyList.Add(prop);
            }
        }
        public void UpdateSet(List<string> currentProperties)
        {
            List<string> oldProperties = new List<String> { };
            List<gameProperty> dumpProperties = new List<gameProperty> { };
            foreach (gameProperty prop in propertyList) oldProperties.Add(prop.property);
            foreach (gameProperty prop in propertyList)
            {
                if (!currentProperties.Contains(prop.property)) dumpProperties.Add(prop);
            }
            foreach (gameProperty prop in dumpProperties) propertyList.Remove(prop);
            foreach (string propName in currentProperties)
            {
                if (!oldProperties.Contains(propName)) propertyList.Add(new gameProperty(propName));
            }
        }
        public gameProperty GetProperty(string propName)
        {
            foreach (gameProperty prop in propertyList)
            {
                if (prop.property == propName) return prop;
            }
            return null;
        }
        public void SetProperty(string propName, Boolean value)
        {
            foreach (gameProperty prop in propertyList)
            {
                if (prop.property == propName) prop.alwaysSave = value;
            }
        }
    }
    public class gameProperty
    {
        public string property;
        public Boolean alwaysSave;
        public gameProperty(string propertyName)
        {
            property = propertyName;
            alwaysSave = false;
        }
    }
}
