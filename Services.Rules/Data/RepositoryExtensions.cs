using Campaigns.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Rules.Data
{
    public static class RepositoryExtensions
    {
        public static IQueryable<Calculation.AttributeContribution> ContributionsFrom(this IEntityStore<Calculation.AttributeContribution> db, Calculation.Attribute source)
        {
            return db.EntityTable.Where(c => c.SourceId == source.Id);
        }

        public static IQueryable<Calculation.AttributeContribution> ContributionsTo(this IEntityStore<Calculation.AttributeContribution> db, Calculation.Attribute target)
        {
            return db.EntityTable.Where(c => c.TargetId == target.Id);
        }
        
        public static Calculation.Attribute GetAttribute(this RulesDbContext db, string name, string category)
        {
            return db.Attributes
                .FirstOrDefault(a => 0 == string.Compare(a.Name, name, true) && 0 == string.Compare(a.Category, category, true));
        }

        public static IEnumerable<Calculation.Attribute> GetAttributesInCategory(this RulesDbContext db, string category)
        {
            return db.Attributes.Where(a => 0 == string.Compare(a.Category, category, true));
        }

        public static IEnumerable<Calculation.Attribute> GetStandardAttributes(this RulesDbContext db)
        {
            return db.Attributes.Where(a => a.IsStandard);
        }

        public static IEnumerable<Calculation.AttributeContribution> GetContributionsToAttribute(this RulesDbContext db, int id)
        {
            return db.AttributeContributions.Where(c => c.SourceId == id);
        }

        public static IEnumerable<Calculation.AttributeContribution> GetContributionsByAttribute(this RulesDbContext db, int id)
        {
            return db.AttributeContributions.Where(c => c.TargetId == id);
        }
    }
}
