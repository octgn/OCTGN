using Octgn.Play;

namespace Octgn.Scripting
{
    public class PythonConverter
    {
        internal static string GroupCtor(Group group)
        {
            if (group is Table) return "table";
            return string.Format("Pile({0}, '{1}', Player({2}))", group.Id, group.Name.Replace("'", @"\'"),
                                 group.Owner.Id);
        }
    }
}