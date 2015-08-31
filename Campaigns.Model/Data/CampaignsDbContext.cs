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
        public DbSet<Model.Attribute> Attributes { get; set; }
        public DbSet<Model.AttributeContribution> AttributeContributions { get; set; }
        public DbSet<Model.CharacterSheet> CharacterSheets { get; set; }
    }
}
