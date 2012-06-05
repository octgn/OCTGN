using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Octgn.Data.Models
{
    public class GameObject
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public Guid Guid { get; set; }
        public Guid GameGuid { get; set; }

        private List<GameObjectProperty> properties;

        public List<GameObjectProperty> Properties
        {
            get
            {
                if (properties == null)
                {
                    properties = new List<GameObjectProperty>();
                }
                return (properties);
            }
        }

        private List<GameObject> childObjects;

        public List<GameObject> ChildObjects
        {
            get
            {
                if (childObjects == null)
                {
                    childObjects = new List<GameObject>();
                }
                return (childObjects);
            }
        }
    }
}
