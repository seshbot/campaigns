using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace campaigns.Models
{
#if false
    public class RulesDbContext : DbContext
    {
        public DbSet<Model.Calculations.Attribute> Attributes { get; set; }
        public DbSet<Model.Calculations.AttributeContribution> AttributeContributions{ get; set; }
    }

    public class RulesInitializer : DropCreateDatabaseIfModelChanges<RulesDbContext>
    {
        protected override void Seed(RulesDbContext context)
        {
            //
            // attributes
            //

            var races = new Dictionary<string, Model.Calculations.Attribute>
            {
                { "dwarf", new Model.Calculations.Attribute { Category = "race", Name = "dwarf" } },
            };

            context.Attributes.AddRange(races.Values);

            var abilities = new Dictionary<string, Model.Calculations.Attribute>
            {
                { "str", new Model.Calculations.Attribute { Category = "ability", Name = "str" } },
                { "dex", new Model.Calculations.Attribute { Category = "ability", Name = "dex" } },
                { "con", new Model.Calculations.Attribute { Category = "ability", Name = "con" } },
                { "int", new Model.Calculations.Attribute { Category = "ability", Name = "int" } },
                { "wis", new Model.Calculations.Attribute { Category = "ability", Name = "wis" } },
                { "cha", new Model.Calculations.Attribute { Category = "ability", Name = "cha" } },
            };

            context.Attributes.AddRange(abilities.Values);

            //
            // calculation contributions
            //

            var contributions = new[]
            {
                Model.Calculations.AttributeContributions.ConstantContributionTo(races["dwarf"], abilities["con"], 2),
            };

            context.SaveChanges();
        }
    }
#endif

    public class CharacterSheetInitializer : DropCreateDatabaseIfModelChanges<CharacterSheetDbContext>
    {
        protected override void Seed(CharacterSheetDbContext context)
        {
            var rules = new DnD5.RulesDescription();
            context.Races.AddRange(rules.Races);
            context.Classes.AddRange(rules.Classes);
            context.Abilities.AddRange(rules.Abilities);
            context.Skills.AddRange(rules.Skills);

            context.Rules.Add(new Rules { Name = "Dungeons and Dragons 5th Edition", Races = rules.Races, Classes = rules.Classes, Abilities = rules.Abilities, Skills = rules.Skills });

            context.SaveChanges();
        }
    }
}
