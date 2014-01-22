using Octgn.Play;

namespace Octgn.Scripting
{
    public class PythonConverter
    {
        internal static string GroupCtor(Group group)
        {
            if (group is Table) return "table";
            if (group is Hand) return string.Format("Hand({0}, Player({1}))", group.Id, group.Owner.Id);
            return string.Format("Pile({0}, '{1}', Player({2}))", group.Id, group.Name.Replace("'", @"\'"),
                                 group.Owner.Id);
        }
    }
}