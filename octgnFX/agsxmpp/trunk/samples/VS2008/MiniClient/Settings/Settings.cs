using System;

using agsXMPP;
using agsXMPP.Xml.Dom;

namespace MiniClient.Settings
{
    /*   
    This class shows how agsXMPP could also be used read and write custom xml files.
    Here we use it for the application settings which are stored in xml files    
    */
    public class Settings : Element
    {
        public Settings()
        {
            this.TagName = "Settings";
        }        
        
        public Login Login
        {
            get { return (Login)SelectSingleElement(typeof(Login)); }
            set
            {
                RemoveTag(typeof(Login));
                if (value != null)
                    AddChild(value);
            }
        }
    }
}