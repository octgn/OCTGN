namespace Octgn.Core.Play
{
    using Octgn.Play;

    public class ControllableObjectStateMachine
    {

        public IPlayControllableObject Find(int id)
        {
            switch ((byte)(id >> 24))
            {
                case 0:
                    return K.C.Get<CardStateMachine>().Find(id);
                case 1:
                    return K.C.Get<GroupStateMachine>().Find(id);
                case 2:
                    //TODO: make counters controllable objects    
                    //return Counter.Find(id);
                    return null;
                default:
                    return null;
            }
        }
    }
}