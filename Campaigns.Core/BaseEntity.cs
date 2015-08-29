using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campaigns.Core
{
    /// <summary>
    /// Entities have an identity derived from their ID (not the value of their properties).
    /// This class enforces that by overriding the .Net comparator operations
    /// to use the ID, so different instances of an entity still compare as equal.
    /// </summary>
    public class BaseEntity
    {
        /// <summary>
        /// Identity is nullable to allow us to have temporary entities (not yet written)
        /// </summary>
        public int Id { get; set; }

        #region System operators

        public bool IsTemporary()
        {
            return Id == 0;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as BaseEntity);
        }

        public virtual bool Equals(BaseEntity other)
        {
            if (other == null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (!IsTemporary() && !other.IsTemporary() && Id == other.Id)
            {
                var type = GetType();
                var otherType = other.GetType();
                return type.IsAssignableFrom(otherType) ||
                       otherType.IsAssignableFrom(type);
            }

            return false;
        }

        public override int GetHashCode()
        {
            if (IsTemporary())
                return base.GetHashCode();
            return Id.GetHashCode();
        }

        public static bool operator==(BaseEntity lhs, BaseEntity rhs)
        {
            return Equals(lhs, rhs);
        }

        public static bool operator!=(BaseEntity lhs, BaseEntity rhs)
        {
            return !(lhs == rhs);
        }

        #endregion
    }
}
