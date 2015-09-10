using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campaigns.Core.Data
{
    public interface IEntityStore<out T> where  T : BaseEntity
    {
        T GetById(int id);

        IQueryable<T> AsQueryable { get; }

        IQueryable<T> AsQueryableNoTracking { get; }
    }

    public interface IEntityRepository<T> : IEntityStore<T> where T : BaseEntity
    {
        void Add(T entity);

        void AddRange(IEnumerable<T> entities);

        void Update(T entity);

        void Remove(T entity);
    }
}
