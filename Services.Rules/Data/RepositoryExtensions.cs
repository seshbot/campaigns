using Campaigns.Core.Data;
using Campaigns.Model;
using Campaigns.Model.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Rules.Data
{
    public static class RepositoryExtensions
    {
        public static IQueryable<Campaigns.Model.AttributeContribution> ContributionsFrom(this IEntityStore<AttributeContribution> db, Campaigns.Model.Attribute source)
        {
            return db.AsQueryable.Where(c => c.SourceId == source.Id);
        }

        public static IQueryable<Campaigns.Model.AttributeContribution> ContributionsTo(this IEntityStore<AttributeContribution> db, Campaigns.Model.Attribute target)
        {
            return db.AsQueryable.Where(c => c.TargetId == target.Id);
        }
        
        public static Campaigns.Model.Attribute GetAttribute(this CampaignsDbContext db, string name, string category)
        {
            return db.Attributes
                .FirstOrDefault(a => 0 == string.Compare(a.Name, name, true) && 0 == string.Compare(a.Category, category, true));
        }

        public static IEnumerable<Campaigns.Model.Attribute> GetAttributesInCategory(this CampaignsDbContext db, string category)
        {
            return db.Attributes.Where(a => 0 == string.Compare(a.Category, category, true));
        }

        public static IEnumerable<Campaigns.Model.Attribute> GetStandardAttributes(this CampaignsDbContext db)
        {
            return db.Attributes.Where(a => a.IsStandard);
        }

        public static IEnumerable<Campaigns.Model.AttributeContribution> GetContributionsToAttribute(this CampaignsDbContext db, int id)
        {
            return db.AttributeContributions.Where(c => c.TargetId == id);
        }

        public static IEnumerable<Campaigns.Model.AttributeContribution> GetContributionsFromAttribute(this CampaignsDbContext db, int id)
        {
            return db.AttributeContributions.Where(c => c.SourceId == id);
        }
    }
}
