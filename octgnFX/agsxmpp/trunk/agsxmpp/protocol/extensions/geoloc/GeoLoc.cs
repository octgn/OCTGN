/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * Copyright (c) 2003-2012 by AG-Software 											 *
 * All Rights Reserved.																 *
 * Contact information for AG-Software is available at http://www.ag-software.de	 *
 *																					 *
 * Licence:																			 *
 * The agsXMPP SDK is released under a dual licence									 *
 * agsXMPP can be used under either of two licences									 *
 * 																					 *
 * A commercial licence which is probably the most appropriate for commercial 		 *
 * corporate use and closed source projects. 										 *
 *																					 *
 * The GNU Public License (GPL) is probably most appropriate for inclusion in		 *
 * other open source projects.														 *
 *																					 *
 * See README.html for details.														 *
 *																					 *
 * For general enquiries visit our website at:										 *
 * http://www.ag-software.de														 *
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */ 

using System;
using System.Text;

using agsXMPP.Xml.Dom;

namespace agsXMPP.protocol.extensions.geoloc
{
    /*
    
    Element Name  	Inclusion  	Datatype  	    Definition
    
    alt 	        MAY 	    xs:decimal 	    Altitude in meters above or below sea level
    bearing 	    MAY 	    xs:decimal 	    GPS bearing (direction in which the entity is heading to reach its next waypoint), measured in decimal degrees relative to true north [2]
    datum 	        MAY 	    xs:string 	    GPS datum [3]
    description 	MAY 	    xs:string 	    A natural-language description of the location
    error 	        MAY 	    xs:decimal 	    Horizontal GPS error in arc minutes
    lat 	        MUST 	    xs:decimal 	    Latitude in decimal degrees North
    lon 	        MUST 	    xs:decimal 	    Longitude in decimal degrees East
    timestamp 	    MAY 	    xs:datetime 	UTC timestamp specifying the moment when the reading was taken (MUST conform to the DateTime profile of Jabber Date and Time Profiles [4])
           
    */

    /// <summary>
    /// XEP-0080 Geographical Location (GeoLoc)
    /// This JEP defines a format for capturing data about an entity's geographical location (geoloc).
    /// The namespace defined herein is intended to provide a semi-structured format for 
    /// describing a geographical location that may change fairly frequently, 
    /// where the geoloc information is provided as Global Positioning System (GPS) coordinates.
    /// </summary>
    public class GeoLoc : Element
    {
        #region << Constructors >>
        /// <summary>
        /// 
        /// </summary>
        public GeoLoc()
        {
            this.TagName    = "geoloc";
            this.Namespace  = Uri.GEOLOC;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Latitude"></param>
        /// <param name="Longitude"></param>
        public GeoLoc(double latitude, double longitude) : this()
        {
            Latitude    = latitude;
            Longitude   = longitude;
        }
        #endregion

        /// <summary>
        /// A natural-language description of the location
        /// </summary>
        public string Description
        {
            get { return GetTag("description"); }
            set { SetTag("description", value); }
        }

        /// <summary>
        /// GPS datum
        /// </summary>
        public string Datum
        {
            get { return GetTag("datum"); }
            set { SetTag("datum", value); }
        }

        /// <summary>
        /// Latitude in decimal degrees North
        /// </summary>
        public double Latitude
        {
            get { return GetTagDouble("lat"); }
            set { SetTag("lat", value); }
        }

        /// <summary>
        /// Longitude in decimal degrees East
        /// </summary>
        public double Longitude
        {
            get { return GetTagDouble("lon"); }
            set { SetTag("lon", value); }
        }

        /// <summary>
        /// Altitude in meters above or below sea level
        /// </summary>
        public double Altitude
        {
            get { return GetTagDouble("alt"); }
            set { SetTag("alt", value); }
        }

        /// <summary>
        /// GPS bearing (direction in which the entity is heading to reach its next waypoint),
        /// measured in decimal degrees relative to true north
        /// </summary>
        public double Bearing
        {
            get { return GetTagDouble("bearing"); }
            set { SetTag("bearing", value); }
        }

        /// <summary>
        /// Horizontal GPS error in arc minutes
        /// </summary>
        public double Error
        {
            get { return GetTagDouble("error"); }
            set { SetTag("error", value); }
        }

        /// <summary>
        /// UTC timestamp specifying the moment when the reading was taken           
        /// </summary>
        public DateTime Timestamp
        {
            get { return Util.Time.ISO_8601Date(GetTag("timestamp")); }
            set { SetTag("timestamp", Util.Time.ISO_8601Date(value)); }
        }
    }
}