using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
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

        public IQueryable<T> AsQueryable { get { return _db; } }

        public IQueryable<T> AsQueryableNoTracking { get { return _db.AsNoTracking(); } }

        public IQueryable<T> AsQueryableIncluding(params string[] paths)
        {
            DbQuery<T> db = _db;
            foreach (var path in paths)
            {
                db = db.Include(path);
            }
            return db;
        }

        public IQueryable<T> AsQueryableNoTrackingIncluding(params string[] paths)
        {
            DbQuery<T> db = _db;
            foreach (var path in paths)
            {
                db = db.AsNoTracking().Include(path);
            }
            return db;
        }

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

        public Task GetByIdAsync(int id)
        {
            return _db.FindAsync(id);
        }

        public T GetByIdNoTracking(int id)
        {
            return _db.AsNoTracking()
                .FirstOrDefault(e => e.Id == id);
        }

        public T GetByIdIncluding(int id, params string[] paths)
        {
            return AsQueryableIncluding(paths)
                .FirstOrDefault(e => e.Id == id);
        }

        public Task GetByIdIncludingAsync(int id, params string[] paths)
        {
            DbQuery<T> db = _db;
            foreach (var path in paths)
            {
                db = db.Include(path);
            }
            return db.FirstOrDefaultAsync(e => e.Id == id);
        }

        public T GetByIdNoTrackingIncluding(int id, params string[] paths)
        {
            return AsQueryableNoTrackingIncluding(paths)
                .FirstOrDefault(e => e.Id == id);
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
