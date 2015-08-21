﻿using campaigns.Models.DAL;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace campaigns.Models
{
    public class CharacterSheetInitializer : DropCreateDatabaseIfModelChanges<CharacterSheetDbContext>
    {
        protected override void Seed(CharacterSheetDbContext context)
        {
            var rules = new DnD5.RulesDescription();
            context.Races.AddRange(rules.Races);
            context.Classes.AddRange(rules.Classes);
            context.Abilities.AddRange(rules.Abilities);
            context.Skills.AddRange(rules.Skills);

            context.RuleSets.Add(new RuleSet { Name = "Dungeons and Dragons 5th Edition", Races = rules.Races, Classes = rules.Classes, Abilities = rules.Abilities, Skills = rules.Skills });

            context.SaveChanges();
        }
    }
}
