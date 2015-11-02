namespace Octgn.Controls.WindowDecorators
{
    class NativeDecorator : WindowDecorator
    {
        public NativeDecorator(DecorableWindow decorated) : base(decorated)
        {
            IsUndoable = true;
        }

        public override void Apply()
        {
        }

        public override bool Undo()
        {
            return true;
        }
    }
}
