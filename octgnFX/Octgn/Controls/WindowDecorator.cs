/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Reflection;
using System.Windows.Controls;
using System.Windows.Documents;

using log4net;

namespace Octgn.Controls
{
    using Core;
    using WindowDecorators;

    /// <summary>
    /// Abstract base class for <see cref="DecorableWindow"/> decorators
    /// </summary>
    public abstract class WindowDecorator
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// <para>
        /// Factory method. Creates a <see cref="WindowDecorator"/> instance based on the current application preferences (<see cref="Prefs"/>).
        /// </para>
        /// <remarks>The type of the <paramref name="window"/> parameter may be used when deciding which decorator to use.</remarks>
        /// </summary>
        /// <param name="window">A <see cref="DecorableWindow"/> instance for which the decorator should be created.</param>
        /// <returns></returns>
        internal static WindowDecorator Create(DecorableWindow window)
        {
            WindowDecorator decorator;
            switch (Prefs.WindowBorderDecorator)
            {
                case "Native":
                    decorator = new NativeDecorator(window);
                    break;
                case "Octgn_1":
                    decorator = new OctgnDecorator(window);
                    break;
                case "Octgn_2":
                case "Octgn":
                default:
                    decorator = new OctgnDecorator_2(window);
                    break;
            }
            return decorator;
        }

        /// <summary>
        /// The <see cref="DecorableWindow"/> instance, to which this decorator is assigned to.
        /// </summary>
        protected DecorableWindow Decorated;

        /// <summary>
        /// Specifies whether this decorator can be undone.
        /// </summary>
        public bool IsUndoable { get; protected set; } = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowDecorator"/> class.
        /// </summary>
        /// <param name="decorated">The <see cref="DecorableWindow"/> instance to decorate.</param>
        protected WindowDecorator(DecorableWindow decorated)
        {
            Decorated = decorated;
        }

        /// <summary>
        /// Apply the decorations to the window.
        /// </summary>
        public abstract void Apply();

        /// <summary>
        /// Undo the decorations if possible.
        /// </summary>
        /// <returns>
        /// Returns false if the decorator cannot be undone or some error occured during the undo operation.
        /// Returns true when the window was successfully undecorated.
        /// </returns>
        public abstract bool Undo();

        /// <summary>
        /// Returns the outermost child of the <see cref="DecorableWindow"/>
        /// </summary>
        /// <returns></returns>
        protected Border GetContainer()
        {
            return Decorated.MainContainer;
        }

        /// <summary>
        /// Returns the child of the <see cref="DecorableWindow"/> which holds the window contents.
        /// </summary>
        /// <returns></returns>
        protected AdornerDecorator GetContentArea()
        {
            return Decorated.ContentArea;
        }
    }
}
