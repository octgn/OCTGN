using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Octgn.Tools.SetBuilder
{
    public class gameItem
    {
        public Boolean changed;
        public string name;
        public string objectType;

    }

    public class setObject : gameItem
    {
        public Guid id;
        public Guid gameId;
        public string gameVersion;
        public string version;
        public List<markerObject> setMarkers;
        public List<cardObject> setCards;
        public packagingObject setPackaging;
        public setObject(Guid gameGuid)
        {
            changed = false;
            name = "New Set";
            id = Guid.NewGuid();
            gameId = gameGuid;
            gameVersion = "0.0.0.0";
            version = "0.0.0.0";
            setMarkers = new List<markerObject> { };
            setCards = new List<cardObject> { };
            setPackaging = new packagingObject();
            objectType = "set";
        }
        public setObject(Guid gameGuid, string sName)
        {
            changed = false;
            name = sName;
            id = Guid.NewGuid();
            gameId = gameGuid;
            gameVersion = "0.0.0.0";
            version = "0.0.0.0";
            setMarkers = new List<markerObject> { };
            setCards = new List<cardObject> { };
            setPackaging = new packagingObject();
            objectType = "set";
        }
        public setObject(Guid gameGuid, string sName, Guid sId)
        {
            changed = false;
            name = sName;
            id = sId;
            gameId = gameGuid;
            gameVersion = "0.0.0.0";
            version = "0.0.0.0";
            setMarkers = new List<markerObject> { };
            setCards = new List<cardObject> { };
            setPackaging = new packagingObject();
            objectType = "set";
        }
        public void SetName(string sName)
        {
            name = sName;
        }
        public void SetId(Guid sId)
        {
            id = sId;
        }
        public void SetGameId(Guid sGameId)
        {
            gameId = sGameId;
        }
        public void SetGameVersion(string sGameVersion)
        {
            gameVersion = sGameVersion;
        }
        public void SetVersion(string sVersion)
        {
            version = sVersion;
        }
        public void AddCard()
        {
            setCards.Add(new cardObject());
        }
        public void AddCard(cardObject card)
        {
            setCards.Add(card);
        }
        public void AddMarker()
        {
            setMarkers.Add(new markerObject());
        }
        public void AddMarker(markerObject marker)
        {
            setMarkers.Add(marker);
        }
    }

    public class cardObject : gameItem
    {
        public Guid id;
        public List<cardProperty> cardProperties;
        public List<cardAlternate> cardAlternates;
        public cardObject()
        {
            changed = false;
            name = "New Card";
            id = Guid.NewGuid();
            cardProperties = new List<cardProperty> { };
            cardAlternates = new List<cardAlternate> { };
            objectType = "card";
        }
        public cardObject(string nm)
        {
            changed = false;
            name = nm;
            id = Guid.NewGuid();
            cardProperties = new List<cardProperty> { };
            cardAlternates = new List<cardAlternate> { };
            objectType = "card";
        }
        public cardObject(string nm, Guid guid)
        {
            changed = false;
            name = nm;
            id = guid;
            cardProperties = new List<cardProperty> { };
            cardAlternates = new List<cardAlternate> { };
            objectType = "card";
        }
        public cardObject(string nm, Guid guid, List<gameProperty> properties)
        {
            changed = false;
            name = nm;
            id = guid;
            cardProperties = new List<cardProperty> { };
            foreach (gameProperty prop in properties)
            {
                if (prop.alwaysSave) cardProperties.Add(new cardProperty(prop.property, ""));
            }
            cardAlternates = new List<cardAlternate> { };
            objectType = "card";

        }
        public void SetName(string newName)
        {
            name = newName;
        }
        public void SetId(Guid newId)
        {
            id = newId;
        }
        public cardProperty AddProperty(string pName, string pValue)
        {
            RemoveProperty(pName);
            cardProperty newProp = new cardProperty(pName, pValue);
            cardProperties.Add(newProp);
            return newProp;
        }
        public void RemoveProperty(string pName)
        {
            for (int index = 0; index < cardProperties.Count; index++)
                if (cardProperties[index].name == pName) cardProperties.Remove(cardProperties[index]);
        }
        public void AddAlternate(cardAlternate aCard)
        {
            RemoveAlternate(aCard.type);
            cardAlternates.Add(aCard);
        }
        public cardAlternate AddAlternate(string aName, string aType)
        {
            RemoveAlternate(aType);
            cardAlternate newAlt = new cardAlternate(aName, aType);
            cardAlternates.Add(newAlt);
            return newAlt;
        }
        public void RemoveAlternate(string aType)
        {
            for (int index = 0; index < cardAlternates.Count; index++)
                if (cardAlternates[index].type == aType) cardAlternates.Remove(cardAlternates[index]);
        }
    }

    public class cardAlternate : gameItem
    {
        public string type;
        public List<cardProperty> alternateProperties;
        public cardAlternate(string aName, string aType)
        {
            changed = false;
            name = aName;
            type = aType;
            alternateProperties = new List<cardProperty> { };
            objectType = "card alternate";
        }
        public cardAlternate(string aName, string aType, List<gameProperty> properties)
        {
            changed = false;
            name = aName;
            type = aType;
            alternateProperties = new List<cardProperty> { };
            objectType = "card alternate";
            foreach (gameProperty prop in properties)
            {
                if (prop.alwaysSave) alternateProperties.Add(new cardProperty(prop.property, ""));
            }
        }
        public void SetName(string aName)
        {
            name = aName;
        }
        public void setType(string aType)
        {
            type = aType;
        }
        public cardProperty AddProperty(string pName, string pValue)
        {
            RemoveProperty(pName);
            cardProperty newProp = new cardProperty(pName, pValue);
            alternateProperties.Add(newProp);
            return newProp; 
        }
        public void RemoveProperty(string pName)
        {
            for (int index = 0; index < alternateProperties.Count; index++)
                if (alternateProperties[index].name == pName) alternateProperties.Remove(alternateProperties[index]);
        }
    }

    public class markerObject : gameItem
    {
        public Guid id;

        public markerObject()
        {
            changed = false;
            name = "New Marker";
            id = Guid.NewGuid();
            objectType = "marker";
        }
        public markerObject(string nm)
        {
            changed = false;
            name = nm;
            id = Guid.NewGuid();
            objectType = "marker";
        }
        public markerObject(string nm, Guid newGuid)
        {
            changed = false;
            name = nm;
            id = newGuid;
        }
    }

    public class cardProperty : gameItem
    {
        public string value;
        public cardProperty(string pName, string pValue)
        {
            changed = false;
            name = pName;
            value = pValue;
            objectType = "property";
        }
        public void SetName(string pName)
        {
            name = pName;
        }
        public void SetValue(string pValue)
        {
            value = pValue;
        }
    }

    public class packagingObject : gameItem
    {
        public List<packObject> packList;
        public packagingObject()
        {
            packList = new List<packObject> { };
            objectType = "packaging";
        }
    }

    public class packObject : gameItem
    {
        public Guid id;
        public List<pickObject> pickList;
        public List<optionsObject> optionsList;
        public packObject(string pName)
        {
            name = pName;
            id = Guid.NewGuid();
            optionsList = new List<optionsObject> { };
            pickList = new List<pickObject> { };
            objectType = "pack";
        }
    }

    public class pickObject : gameItem
    {
        public string qty;
        public string key;
        public string value;

        public pickObject(string pQty, string pKey, string pValue)
        {
            qty = pQty;
            key = pKey;
            value = pValue;
            objectType = "pick";
            name = "pick";
        }
    }

    public class optionObject : gameItem
    {
        public string probability;
        public List<pickObject> pickList;
        public List<optionsObject> optionsList;
        public optionObject(string oProb)
        {
            probability = oProb;
            optionsList = new List<optionsObject> { };
            pickList = new List<pickObject> { };
            objectType = "option";
            name = "option";
        }
    }

    public class optionsObject : gameItem
    {
        public List<optionObject> optionList;
        public optionsObject()
        {
            optionList = new List<optionObject> { };
            objectType = "options list";
            name = "options";
        }
    }

}
