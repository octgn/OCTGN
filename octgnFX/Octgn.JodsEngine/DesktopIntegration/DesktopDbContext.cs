//using Octgn.DataNew;
//using Octgn.DataNew.Entities;
//using Octgn.DataNew.FileDB;
//using Octgn.ProxyGenerator;
//using System;
//using System.Collections.Generic;

//namespace Octgn.DesktopIntegration
//{
//    public class DesktopDbContext : IDbContext
//    {
//        private bool _isDisposed;

//        public IEnumerable<Card> Cards => throw new NotImplementedException();

//        public IEnumerable<Game> Games => throw new NotImplementedException();

//        public IEnumerable<ProxyDefinition> ProxyDefinitions => throw new NotImplementedException();

//        public IEnumerable<GameScript> Scripts => throw new NotImplementedException();

//        public CollectionQuery<Set> SetQuery => throw new NotImplementedException();

//        public IEnumerable<Set> Sets => throw new NotImplementedException();

//        public Game GameById(Guid gameId) => throw new NotImplementedException();
//        public void Save(Game game) => throw new NotImplementedException();
//        public void Save(Set set) => throw new NotImplementedException();
//        public IEnumerable<Set> SetsById(Guid setId) => throw new NotImplementedException();

//        protected virtual void Dispose(bool disposing) {
//            if (!_isDisposed) {
//                if (disposing) {
//                    // TODO: dispose managed state (managed objects)
//                }

//                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
//                // TODO: set large fields to null
//                _isDisposed = true;
//            }
//        }

//        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
//        // ~DesktopDbContext()
//        // {
//        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
//        //     Dispose(disposing: false);
//        // }

//        public void Dispose() {
//            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
//            Dispose(disposing: true);
//            GC.SuppressFinalize(this);
//        }
//    }
//}
