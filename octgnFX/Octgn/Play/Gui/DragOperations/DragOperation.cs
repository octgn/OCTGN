using System.Windows;
using System.Windows.Input;

namespace Octgn.Play.Gui
{
    internal interface IDragOperation
    {
        void Dragging(MouseEventArgs e);
        void EndDrag();
    }

    internal abstract class DragOperation<T> : IDragOperation where T : UIElement
    {
        private Point _oldPos;
        protected T Target;

        protected DragOperation(T target)
        {
            this.Target = target;
            _oldPos = Mouse.GetPosition(target);
            target.CaptureMouse();
            // TODO: Calling a virtual constructure in a future object, bad news
            StartDragCore(_oldPos);
        }

        #region IDragOperation Members

        public void Dragging(MouseEventArgs e)
        {
            Point newPos = e.GetPosition(Target);
            Vector delta = newPos - _oldPos;
            _oldPos = newPos;
            DraggingCore(newPos, delta);
            e.Handled = true;
        }

        public void EndDrag()
        {
            Target.ReleaseMouseCapture();
            EndDragCore();
        }

        #endregion

        protected abstract void StartDragCore(Point position);

        protected abstract void DraggingCore(Point position, Vector delta);

        protected abstract void EndDragCore();
    }
}