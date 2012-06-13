using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Db4objects.Db4o;
using Db4objects.Db4o.Ext;
using Db4objects.Db4o.Query;

namespace Octgn.Data
{
	public class ObjectContainerStub: IObjectContainer
	{
		public bool Close() { return true; }

		public IObjectSet QueryByExample(object template) { return default(IObjectSet); }
		public IObjectSet Query(Type clazz) { return default(IObjectSet); }
		public IObjectSet Query(Predicate predicate) { return default(IObjectSet); }
		public IObjectSet Query(Predicate predicate, IQueryComparator comparator) { return default(IObjectSet); }
		public IObjectSet Query(Predicate predicate, IComparer comparer) { return default(IObjectSet); }

		public IExtObjectContainer Ext() { return default(IExtObjectContainer); }

		IQuery IObjectContainer.Query() { return default(IQuery); }

		public virtual IList<Extent> Query<Extent>() { return default(IList<Extent>); }
		public virtual IList<Extent> Query<Extent>(Predicate<Extent> match) { return default(IList<Extent>); }
		public virtual IList<Extent> Query<Extent>(Predicate<Extent> match, IComparer<Extent> comparer) { return default(IList<Extent>); }
		public virtual IList<Extent> Query<Extent>(Predicate<Extent> match, Comparison<Extent> comparison) { return default(IList<Extent>); }
		public virtual IList<Extent> Query<Extent>(IComparer<Extent> comparer) { return default(IList<Extent>); }

		public IList<ElementType> Query<ElementType>(Type extent) { return default(IList<ElementType>); }

		IQuery ISodaQueryFactory.Query() { return default(IQuery); }

		#region NoReturnValue

		public void Dispose() { }
		public void Activate(object obj, int depth) { }
		public void Commit() { }
		public void Deactivate(object obj, int depth) { }
		public void Delete(object obj) { }
		public void Rollback() { }
		public void Store(object obj) { }

		#endregion
	}
}
