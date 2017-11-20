using CommonServiceLocator;
using Ninject;
using System;
using System.Collections.Generic;

namespace Octide
{
    class NInjectServiceLocator : ServiceLocatorImplBase
    {
        private readonly IKernel _kernel;

        public NInjectServiceLocator(IKernel kernel) {
            _kernel = kernel;
        }

        protected override IEnumerable<object> DoGetAllInstances(Type serviceType) {
            return _kernel.GetAll(serviceType);
        }

        protected override object DoGetInstance(Type serviceType, string key) {
            if (key == null) {
                return _kernel.Get(serviceType);
            }

            return _kernel.Get(serviceType, key);
        }
    }
}
