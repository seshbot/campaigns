using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Rules.Data
{
    public class RulesDbContext : DbContext
    {
        public DbSet<Calculation.Attribute> Attributes { get; set; }
        public DbSet<Calculation.AttributeContribution> AttributeContributions { get; set; }
    }
}
