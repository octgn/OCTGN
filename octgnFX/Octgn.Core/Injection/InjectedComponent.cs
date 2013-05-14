namespace Octgn.Core.Injection
{
    using System.Reflection;

    using Ninject.Components;
    using Ninject.Selection.Heuristics;

    using Octgn.Library.Injection;

    public class InjectedComponent : NinjectComponent, IInjectionHeuristic
    {
        public bool ShouldInject(MemberInfo member)
        {
            return member.IsDefined(
              typeof(Injected),
              true);
        }
    }
}