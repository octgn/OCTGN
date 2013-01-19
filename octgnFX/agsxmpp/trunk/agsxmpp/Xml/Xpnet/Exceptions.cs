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
    /// Base class for other exceptions
    /// </summary>
    public class TokenException : System.Exception
    {
    }

    /// <summary>
    /// An empty token was detected.  This only happens with a buffer of length 0 is passed in
    /// to the parser.
    /// </summary>
    public class EmptyTokenException : TokenException
    {
    }

    /// <summary>
    /// End of prolog.
    /// </summary>
    public class EndOfPrologException : TokenException
    {
    }
    /**
     * Thrown to indicate that the byte subarray being tokenized is a legal XML
     * token, but that subsequent bytes in the same entity could be part of
     * the token.  For example, <code>Encoding.tokenizeProlog</code>
     * would throw this if the byte subarray consists of a legal XML name.
     * @version $Revision: 1.3 $ $Date: 1998/02/17 04:24:06 $
     */
    public class ExtensibleTokenException : TokenException
    {
        private TOK tokType;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tokType"></param>
        public ExtensibleTokenException(TOK tokType)
        {
            this.tokType = tokType;
        }

        /**
         * Returns the type of token in the byte subarrary.
         */
        public TOK TokenType
        {
            get { return tokType; }
        }
    }

    /// <summary>
    /// Several kinds of token problems.
    /// </summary>
    public class InvalidTokenException : TokenException
    {
        private int offset;
        private byte type;

        /// <summary>
        /// An illegal character
        /// </summary>
        public const byte ILLEGAL_CHAR = 0;
        /// <summary>
        /// Doc prefix wasn't XML
        /// </summary>
        public const byte XML_TARGET = 1;
        /// <summary>
        /// More than one attribute with the same name on the same element
        /// </summary>
        public const byte DUPLICATE_ATTRIBUTE = 2;

        /// <summary>
        /// Some other type of bad token detected
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="type"></param>
        public InvalidTokenException(int offset, byte type)
        {
            this.offset = offset;
            this.type = type;
        }

        /// <summary>
        /// Illegal character detected
        /// </summary>
        /// <param name="offset"></param>
        public InvalidTokenException(int offset)
        {
            this.offset = offset;
            this.type = ILLEGAL_CHAR;
        }

        /// <summary>
        /// Offset into the buffer where the problem ocurred.
        /// </summary>
        public int Offset
        {
            get { return this.offset; }
        }
        
        /// <summary>
        /// Type of exception
        /// </summary>
        public int Type
        {
            get { return this.type; }
        }
    }

    /**
     * Thrown to indicate that the subarray being tokenized is not the
     * complete encoding of one or more characters, but might be if
     * more bytes were added.
     * @version $Revision: 1.2 $ $Date: 1998/02/17 04:24:11 $
     */
    public class PartialCharException : PartialTokenException
    {
        private int leadByteIndex;
  
        /// <summary>
        /// 
        /// </summary>
        /// <param name="leadByteIndex"></param>
        public PartialCharException(int leadByteIndex) 
        {
            this.leadByteIndex = leadByteIndex;
        }

        /**
         * Returns the index of the first byte that is not part of the complete
         * encoding of a character.
         */
        public int LeadByteIndex 
        {
            get { return leadByteIndex; }
        }
    }

    /// <summary>
    /// A partial token was received.  Try again, after you add more bytes to the buffer.
    /// </summary>
    public class PartialTokenException : TokenException
    {
    }
}