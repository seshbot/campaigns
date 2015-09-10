using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campaigns.Model.Data
{
    public class CampaignsDbContext : DbContext
    {
        // rules
        public DbSet<Model.Attribute> Attributes { get; set; }
        public DbSet<Model.AttributeContribution> AttributeContributions { get; set; }

        // charsheets
        public DbSet<Model.AttributeAllocation> AttributeAllocations { get; set; }
        public DbSet<Model.AttributeValue> AttributeValues { get; set; }
        public DbSet<Model.CharacterSheet> CharacterSheets { get; set; }
        public DbSet<Model.Character> Characters { get; set; }
    }
}
