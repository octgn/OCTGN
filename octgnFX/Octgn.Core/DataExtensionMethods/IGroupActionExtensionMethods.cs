namespace Octgn.Core.DataExtensionMethods
{
    using Octgn.DataNew.Entities;

    public static class GroupActionExtensionMethods
    {
        public static GroupAction AsAction(this IGroupAction action)
        {
            if (action is GroupAction)
            {
                return action as GroupAction;
            }
            return null;
        }
    }
}