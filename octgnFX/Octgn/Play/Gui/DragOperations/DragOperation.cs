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
        private Point oldPos;
        protected T target;

        protected DragOperation(T target)
        {
            this.target = target;
            oldPos = Mouse.GetPosition(target);
            target.CaptureMouse();
            StartDragCore(oldPos);
        }

        #region IDragOperation Members

        public void Dragging(MouseEventArgs e)
        {
            Point newPos = e.GetPosition(target);
            Vector delta = newPos - oldPos;
            oldPos = newPos;
            DraggingCore(newPos, delta);
            e.Handled = true;
        }

        public void EndDrag()
        {
            target.ReleaseMouseCapture();
            EndDragCore();
        }

        #endregion

        protected abstract void StartDragCore(Point position);

        protected abstract void DraggingCore(Point position, Vector delta);

        protected abstract void EndDragCore();
    }
}