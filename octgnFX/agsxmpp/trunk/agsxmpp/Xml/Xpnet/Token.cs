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
/*
 * xpnet is a deriviative of James Clark's XP parser.
 * See copying.txt for more info.
 */
namespace agsXMPP.Xml.Xpnet
{
	/// <summary>
	/// A token that was parsed.
	/// </summary>
    public class Token
    {
        private int tokenEnd = -1;
        private int nameEnd = -1;
        private char refChar1 = (char)0;
        private char refChar2 = (char)0;

		/// <summary>
		/// The end of the current token, in relation to the beginning of the buffer.
		/// </summary>
        public int TokenEnd {
            get {return tokenEnd;}
            set {tokenEnd = value; }
        }

		/// <summary>
		/// The end of the current token's name, in relation to the beginning of the buffer.
		/// </summary>
		public int NameEnd 
		{
            get {return nameEnd;}
            set {nameEnd = value;}
        }

        //public char RefChar
        //{
        //    get {return refChar1;}
        //}

		/// <summary>
		/// The parsed-out character. &amp; for &amp;amp;
		/// </summary>
        public char RefChar1 {
            get {return refChar1;}
            set {refChar1 = value; }
        }
		/// <summary>
		/// The second of two parsed-out characters.  TODO: find example.
		/// </summary>
        public char RefChar2 {
            get {return refChar2;}
            set {refChar2 = value; }
        }

		/*
        public void getRefCharPair(char[] ch, int off) {
            ch[off] = refChar1;
            ch[off + 1] = refChar2;
        }
        */
    }
}