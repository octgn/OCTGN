using System.Collections.Generic;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Conventions;
using Nancy.TinyIoc;
using Nancy.ViewEngines.Razor;

namespace Octgn.Client
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        private UIBackend _uiBackend;
        public Bootstrapper(UIBackend server)
        {
            _uiBackend = server;
        }
        protected override void ConfigureConventions(NancyConventions nancyConventions)
        {
            base.ConfigureConventions(nancyConventions);

            nancyConventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("Scripts", "Scripts"));
        }

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);

            ServiceStack.Text.JsConfig.EmitCamelCaseNames = true;
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);

            container.Register<UIBackend, UIBackend>(_uiBackend);
        }
    }

    public class RazorConfig : IRazorConfiguration
    {
        public IEnumerable<string> GetAssemblyNames()
        {
            yield return "Octgn.Client";
        }

        public IEnumerable<string> GetDefaultNamespaces()
        {
            yield return "Octgn.Client";
        }

        public bool AutoIncludeModelNamespace
        {
            get { return true; }
        }
    }
}