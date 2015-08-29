using Campaigns.Core.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campaigns.Core.Testing
{
    public class InMemoryEntityRepository<T> : IEntityRepository<T> where T : BaseEntity
    {
        IDictionary<int, T> _entities = new Dictionary<int, T>();
        IDictionary<Type, IEntityStore<BaseEntity>> _foreignStores = new Dictionary<Type, IEntityStore<BaseEntity>>();

        int _nextId = 1;
        int AllocId() { return _nextId++; }

        public void AddForeignStore<OtherT>(IEntityStore<OtherT> foreignTable) where OtherT : BaseEntity
        {
            _foreignStores.Add(typeof(OtherT), foreignTable);
        }

        private IEntityStore<BaseEntity> GetForeignStore(Type entityType)
        {
            if (!_foreignStores.ContainsKey(entityType))
                throw new Exception("InMemoryRepository does not know how to look up entity of type '" + entityType.Name + "'");

            return _foreignStores[entityType];
        }

        public IQueryable<T> EntityTable
        {
            get
            {
                return _entities.Values.AsQueryable();
            }
        }

        public void Add(T entity)
        {
            if (!entity.IsTemporary())
                throw new Exception("cannot add entity that already has an ID");

            entity.Id = AllocId();
            _entities.Add(entity.Id, entity);

            SyncForeignKeys(entity);
        }

        public void AddRange(IEnumerable<T> entities)
        {
            foreach (var e in entities)
            {
                Add(e);
            }
        }

        public T GetById(int id)
        {
            T result;
            _entities.TryGetValue(id, out result);
            return result;
        }

        public void Remove(T entity)
        {
            if (entity.IsTemporary())
                throw new Exception("cannot update entity that does not have an ID");

            _entities.Remove(entity.Id);
        }

        public void Update(T entity)
        {
            if (entity.IsTemporary())
                throw new Exception("cannot update entity that does not have an ID");

            _entities[entity.Id] = entity;
        }

        private void SyncForeignKeys(T entity)
        {
            var type = entity.GetType();
            var fkProps =
                from prop in type.GetProperties()
                where Attribute.IsDefined(prop, typeof(ForeignKeyAttribute))
                select prop;

            foreach (var prop in fkProps)
            {
                var attrib = (ForeignKeyAttribute)Attribute.GetCustomAttribute(prop, typeof(ForeignKeyAttribute));
                var otherProp = type.GetProperty(attrib.Name);

                SyncForeignKeyProperties(entity, prop, otherProp);
            }
        }

        private void SyncForeignKeyProperties(T entity, System.Reflection.PropertyInfo prop1, System.Reflection.PropertyInfo prop2)
        {
            var navProp = IsNavigationProperty(prop1) ? prop1 : prop2;
            var fkIdProp = prop1 == navProp ? prop2 : prop1;

            if (IsNavigationProperty(fkIdProp))
                throw new Exception("two foreign key navigation properties referring to each other");

            // get foreign entity ID
            var navEntityId = GetForeignKeyId(entity, navProp);
            var fkId = GetForeignKeyId(entity, fkIdProp);

            if (navEntityId.HasValue == fkId.HasValue)
            {
                if (navEntityId != fkId)
                    throw new Exception(
                        string.Format("Foreign Keys property '{0}' ({1}) doesnt match '{2}' ({3})", navProp.Name, navEntityId.Value, fkIdProp.Name, fkId.Value));

                return;
            }

            var foreignEntityId = navEntityId.HasValue ? navEntityId.Value : fkId.Value;

            fkIdProp.SetValue(entity, foreignEntityId);
            if (null == navProp.GetValue(entity))
            {
                var foreignEntity = GetForeignEntity(entity, navProp, foreignEntityId);
                navProp.SetValue(entity, foreignEntity);
            }
        }

        private static bool IsNavigationProperty(System.Reflection.PropertyInfo prop)
        {
            if (prop.PropertyType.IsSubclassOf(typeof(BaseEntity)))
            {
                return true;
            }

            return false;
        }
        
        private BaseEntity GetForeignEntity(T entity, System.Reflection.PropertyInfo prop, int foreignEntityId)
        {
            var entityType = prop.PropertyType;
            var entityStore = GetForeignStore(entityType);
            return entityStore.GetById(foreignEntityId);
        }

        private static int? GetForeignKeyId(T entity, System.Reflection.PropertyInfo prop)
        {
            var value = prop.GetValue(entity);
            if (null == value)
                return null;

            if (IsNavigationProperty(prop))
            {
                return ((BaseEntity)value).Id;
            }

            if (typeof(int?) == value.GetType())
            {
                return (int?)value;
            }

            if (typeof(int) == value.GetType())
            {
                var intval = (int)value;
                return (0 != intval) ? intval : (int?)null;
            }
            
            throw new Exception("Cannot recognise type of foreign key property '" + prop.Name + "'");
        }
    }
}
