using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campaigns.Core.Data
{
    public class EFEntityRepository<T> : IEntityRepository<T> where T : BaseEntity
    {
        private DbContext _context;
        private DbSet<T> _db;

        public EFEntityRepository(DbContext context, DbSet<T> db)
        {
            _context = context;
            _db = db;
        }

        public IQueryable<T> EntityTable { get { return _db; } }

        public void Add(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            _db.Add(entity);
            _context.SaveChanges();
        }

        public void AddRange(IEnumerable<T> entities)
        {
            if (entities == null)
                throw new ArgumentNullException("entities");

            _db.AddRange(entities);
            _context.SaveChanges();
        }

        public T GetById(int id)
        {
            return _db.Find(id);
        }

        public void Remove(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            _db.Remove(entity);
            _context.SaveChanges();
        }

        public void Update(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            _context.SaveChanges();
        }
    }
}
