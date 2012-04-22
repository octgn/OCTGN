/*
 * Created by SharpDevelop.
 * User: dschachtler
 * Date: 22.04.2012
 * Time: 12:04
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace Octgn.Data
{
    /// <summary>
    /// Description of EventArgs.
    /// </summary>
    public class EventArgs<T> : EventArgs
    {
        public EventArgs(T value)
        {
            Value = value;
        }
        
        public T Value
        {
            get;
            private set;
        }
    }
}
