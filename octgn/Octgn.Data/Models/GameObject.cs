using System;
using System.Collections.Generic;

namespace Octgn.Data.Models
{
    public class GameObject
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public Guid Guid { get; set; }
        public Guid GameGuid { get; set; }

        private List<GameObjectProperty> _properties;

        public List<GameObjectProperty> Properties
        {
            get
            {
                if (_properties == null)
                {
                    _properties = new List<GameObjectProperty>();
                }
                return (_properties);
            }
        }

        private List<GameObject> _childObjects;

        public List<GameObject> ChildObjects
        {
            get
            {
                if (_childObjects == null)
                {
                    _childObjects = new List<GameObject>();
                }
                return (_childObjects);
            }
        }
    }
}
