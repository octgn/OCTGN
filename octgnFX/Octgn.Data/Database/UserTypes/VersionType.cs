namespace Octgn.Data.Database.UserTypes
{
    using System;
    using System.Data;
    using System.Reflection;

    using NHibernate;
    using NHibernate.SqlTypes;
    using NHibernate.UserTypes;

    public class VersionType : IUserType
    {
        public bool Equals(object x, object y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(object x)
        {
            return x.GetHashCode();
        }

        public object NullSafeGet(IDataReader rs, string[] names, object owner)
        {
            return new Version(NHibernateUtil.String.NullSafeGet(rs, names[0]).ToString());
        }

        public void NullSafeSet(IDbCommand cmd, object value, int index)
        {
            NHibernateUtil.String.NullSafeSet(cmd,value.ToString(),index);
        }

        public object DeepCopy(object value)
        {
            return value;
        }

        public object Replace(object original, object target, object owner)
        {
            return original;
        }

        public object Assemble(object cached, object owner)
        {
            return cached;
        }

        public object Disassemble(object value)
        {
            return value;
        }

        public SqlType[] SqlTypes
        {
            get
            {
                return new SqlType[] { new NHibernate.SqlTypes.StringSqlType() };
            }
        }

        public Type ReturnedType
        {
            get
            {
                return typeof(Version);
            }
        }

        public bool IsMutable
        {
            get
            {
                return true;
            }
        }
    }
}
