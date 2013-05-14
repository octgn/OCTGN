namespace Octgn.Core
{
    using System;
    using System.Reflection;

    using Ninject;
    using Ninject.Selection.Heuristics;
    using Ninject.Syntax;

    using Octgn.Core.Injection;

    public class K
    {
        #region KSingleton

        public static K C { get { lock (KSingletonLocker) return Context ?? (Context = new K()); } }

        internal static K Context { get; set; }

        private static readonly object KSingletonLocker = new object();

        internal K()
        {
            this.Kernel = new StandardKernel();
            this.Kernel.Settings.InjectNonPublic = true;
            this.Kernel.Components.Add<IInjectionHeuristic, InjectedComponent>();
        }

        #endregion Singleton

        internal StandardKernel Kernel { get; set; }

        public T Get<T>(params Ninject.Parameters.IParameter[] param)
        {
            return C.Kernel.Get<T>(param);
        }

        public object Get(Type type, params Ninject.Parameters.IParameter[] param)
        {
            return C.Kernel.Get(type, param);
        }

        public void Load(params Assembly[] assemblies)
        {
            C.Kernel.Load(assemblies);
        }

        public IBindingToSyntax<T> Bind<T>()
        {
            return C.Kernel.Bind<T>();
        }

        public IBindingToSyntax<object> Bind(Type t)
        {
            return C.Kernel.Bind(t);
        }

        public void Unbind(Type t)
        {
            C.Kernel.Unbind(t);
        }

        public void Unbind<T>()
        {
            C.Kernel.Unbind<T>();
        }

        public void Unload()
        {
            K.Context = null;
        }
    }
}