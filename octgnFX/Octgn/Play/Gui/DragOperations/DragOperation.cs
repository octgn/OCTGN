using System.Windows;
using System.Windows.Input;

namespace Octgn.Play.Gui.DragOperations
{
    internal interface IDragOperation
    {
        void Dragging(MouseEventArgs e);
        void EndDrag();
    }

    internal abstract class DragOperation<T> : IDragOperation where T : UIElement
    {
        protected T Target;
        private Point _oldPos;

        protected DragOperation(T target)
        {
            Target = target;
            _oldPos = Mouse.GetPosition(target);
            target.CaptureMouse();
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