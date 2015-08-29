using Campaigns.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Services.Calculation;

namespace Services.Rules.Data
{
    public class AttributeRepositoryEF : IEntityRepository<Calculation.Attribute>
    {
        public IQueryable<Calculation.Attribute> EntityTable
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void Add(Calculation.Attribute entity)
        {
            throw new NotImplementedException();
        }

        public void AddRange(IEnumerable<Calculation.Attribute> entities)
        {
            throw new NotImplementedException();
        }

        public Calculation.Attribute GetById(int id)
        {
            throw new NotImplementedException();
        }

        public void Remove(Calculation.Attribute entity)
        {
            throw new NotImplementedException();
        }

        public void Update(Calculation.Attribute entity)
        {
            throw new NotImplementedException();
        }
    }
}
