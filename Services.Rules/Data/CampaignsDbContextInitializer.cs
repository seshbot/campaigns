using Campaigns.Model;
using Campaigns.Model.Data;
using Services.Calculation;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Rules.Data
{
    public class CampaignsDbContextInitializer : DropCreateDatabaseAlways<CampaignsDbContext>
    {
        protected override void Seed(CampaignsDbContext context)
        {
            var dnd = new DnD5.RulesDescription();

            //
            // attributes
            //

            var races = dnd.Races
                .Select(r => new Campaigns.Model.Attribute { Category = "races", Name = r.Name, LongName = r.Name, IsStandard = false, Description = r.Description })
                .ToDictionary(r => r.Name.ToLower());

            var abilities = dnd.Abilities
                .Select(a => new Campaigns.Model.Attribute { Category = "abilities", Name = a.ShortName, LongName = a.Name, IsStandard = true, Description = a.Description, SortOrder = a.SortOrder })
                .ToDictionary(a => a.Name.ToLower());

            var abilityMods = dnd.Abilities
                .Select(a => new Campaigns.Model.Attribute { Category = "ability-modifiers", Name = a.ShortName, LongName = a.Name, IsStandard = true, Description = a.Description, SortOrder = a.SortOrder })
                .ToDictionary(a => a.Name.ToLower());

            var classes = dnd.Classes
                .Select(c => new Campaigns.Model.Attribute { Category = "classes", Name = c.Name, LongName = c.Name, IsStandard = false, Description = c.Description })
                .ToDictionary(a => a.Name.ToLower());

            var skills = dnd.Skills
                .Select(s => new Campaigns.Model.Attribute { Category = "skills", Name = s.Name, LongName = s.Name, IsStandard = true, Description = s.Description })
                .ToDictionary(a => a.Name.ToLower());

            context.Attributes.AddRange(races.Values);
            context.Attributes.AddRange(abilities.Values);
            context.Attributes.AddRange(abilityMods.Values);
            context.Attributes.AddRange(classes.Values);
            context.Attributes.AddRange(skills.Values);

            context.SaveChanges();

            //
            // contributions
            //

            var abilityModContribs = 
                from ability in dnd.Abilities
                let source = context.GetAttribute(ability.ShortName, "abilities")
                let target = context.GetAttribute(ability.ShortName, "ability-modifiers")
                select source.ContributionTo(target, ability => ability / 2 - 5);

            var skillContribs =
                from skill in dnd.Skills
                let source = context.GetAttribute(skill.Ability.ShortName, "ability-modifiers")
                let target = context.GetAttribute(skill.Name, "skills")
                select source.CopyContributionTo(target);

            var racialContribs =
                from bonus in dnd.RacialBonuses
                let source = context.GetAttribute(bonus.Race.Name, "races")
                from kvp in bonus.Bonuses
                let target = context.GetAttribute(kvp.Key.ShortName, "abilities")
                select source.ConstantContributionTo(target, kvp.Value);

            context.AttributeContributions.AddRange(abilityModContribs);
            context.AttributeContributions.AddRange(skillContribs);
            context.AttributeContributions.AddRange(racialContribs);
            
            context.SaveChanges();
        }
    }
}
