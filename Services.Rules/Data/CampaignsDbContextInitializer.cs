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
    public class CampaignsDbContextInitializer : DropCreateDatabaseIfModelChanges<CampaignsDbContext>
    {
        protected override void Seed(CampaignsDbContext context)
        {
            var dnd = new DnD5.RulesDescription();
            var raceInfo = DnD5.RaceInfo.Races;
            //
            // attributes
            //

            var intrinsics = new[]
            {
                new Campaigns.Model.Attribute { Category = "intrinsics", Name = "Level", LongName = "Level", IsStandard = true, Description = "The total character level" },
                new Campaigns.Model.Attribute { Category = "intrinsics", Name = "XP", LongName = "Experience", IsStandard = true, Description = "The total amount of experience" },
                new Campaigns.Model.Attribute { Category = "intrinsics", Name = "AC", LongName = "Armour Class", IsStandard = true, Description = "The difficulty to hit the character" },
                new Campaigns.Model.Attribute { Category = "intrinsics", Name = "Speed", LongName = "Speed", IsStandard = true, Description = "The character base travel speed" },
                new Campaigns.Model.Attribute { Category = "intrinsics", Name = "ProfBonus", LongName = "Proficiency Bonus", IsStandard = true, Description = "The character base travel speed" },
                new Campaigns.Model.Attribute { Category = "hitdie", Name = "d4", LongName = "Hit Die", IsStandard = true, Description = "Determines hit points for leveling up and recovery" },
                new Campaigns.Model.Attribute { Category = "hitdie", Name = "d6", LongName = "Hit Die", IsStandard = true, Description = "Determines hit points for leveling up and recovery" },
                new Campaigns.Model.Attribute { Category = "hitdie", Name = "d8", LongName = "Hit Die", IsStandard = true, Description = "Determines hit points for leveling up and recovery" },
                new Campaigns.Model.Attribute { Category = "hitdie", Name = "d10", LongName = "Hit Die", IsStandard = true, Description = "Determines hit points for leveling up and recovery" },
                new Campaigns.Model.Attribute { Category = "hitdie", Name = "d12", LongName = "Hit Die", IsStandard = true, Description = "Determines hit points for leveling up and recovery" },
            };

            var races = raceInfo
                .Select(r => new Campaigns.Model.Attribute { Category = "races", Name = r.Name, LongName = r.Name, IsStandard = false, Description = r.Description })
                .ToDictionary(r => r.Name.ToLower());

            var abilities = dnd.Abilities
                .Select(a => new Campaigns.Model.Attribute { Category = "abilities", Name = a.ShortName, LongName = a.Name, IsStandard = true, Description = a.Description, SortOrder = a.SortOrder })
                .ToDictionary(a => a.Name.ToLower());

            var abilityMods = dnd.Abilities
                .Select(a => new Campaigns.Model.Attribute { Category = "ability-modifiers", Name = a.ShortName, LongName = a.Name, IsStandard = true, Description = a.Description, SortOrder = a.SortOrder })
                .ToDictionary(a => a.Name.ToLower());

            var abilitySaves = dnd.Abilities
                .Select(a => new Campaigns.Model.Attribute { Category = "ability-saves", Name = a.ShortName, LongName = a.Name, IsStandard = true, Description = a.Description, SortOrder = a.SortOrder })
                .ToDictionary(a => a.Name.ToLower());

            var classes = dnd.Classes
                .Select(c => new Campaigns.Model.Attribute { Category = "classes", Name = c.Name, LongName = c.Name, IsStandard = false, Description = c.Description })
                .ToDictionary(a => a.Name.ToLower());

            var skills = dnd.Skills
                .Select(s => new Campaigns.Model.Attribute { Category = "skills", Name = s.Name, LongName = s.Name, IsStandard = true, Description = s.Description })
                .ToDictionary(a => a.Name.ToLower());

            context.Attributes.AddRange(intrinsics);
            context.Attributes.AddRange(races.Values);
            context.Attributes.AddRange(abilities.Values);
            context.Attributes.AddRange(abilityMods.Values);
            context.Attributes.AddRange(abilitySaves.Values);
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

            var abilitySaveContribs =
                from ability in dnd.Abilities
                let source = context.GetAttribute(ability.ShortName, "ability-modifiers")
                let target = context.GetAttribute(ability.ShortName, "ability-saves")
                select source.CopyContributionTo(target);

            var racialBonusContribs =
                from race in raceInfo
                let source = context.GetAttribute(race.Name, "races")
                from kvp in race.AbilityContributions
                let target = context.GetAttribute(kvp.Key, "abilities")
                select source.ConstantContributionTo(target, kvp.Value);

            var racialSpeedContribs =
                from race in raceInfo
                let source = context.GetAttribute(race.Name, "races")
                let target = context.GetAttribute("Speed", "intrinsics")
                select source.ConstantContributionTo(target, race.MaxSpeed);

            var hitDieByNumber = new Dictionary<int, Campaigns.Model.Attribute>
            {
                { 4,  context.GetAttribute("D4", "hitdie")},
                { 6,  context.GetAttribute("D6", "hitdie")},
                { 8,  context.GetAttribute("D8", "hitdie")},
                { 10,  context.GetAttribute("D10", "hitdie")},
                { 12,  context.GetAttribute("D12", "hitdie")},
            };

            var hitDieContribs =
                from xlass in dnd.Classes
                let source = context.GetAttribute(xlass.Name, "classes")
                let target = hitDieByNumber[xlass.HitDie]
                select source.CopyContributionTo(target);

            var levelContribs =
                from xlass in dnd.Classes
                let source = context.GetAttribute(xlass.Name, "classes")
                let target = context.GetAttribute("Level", "intrinsics")
                select source.CopyContributionTo(target);

            var levelAttrib = context.GetAttribute("Level", "intrinsics");
            var profBonusAttrib = context.GetAttribute("ProfBonus", "intrinsics");
            var profBonusContrib = levelAttrib.ContributionTo(profBonusAttrib, level =>
                level < 5 ? 2 :
                level < 10 ? 3 :
                level < 13 ? 4 :
                level < 17 ? 5 :
                6);

            var dexModAttrib = context.GetAttribute("dex", "ability-modifiers");
            var acAttrib = context.GetAttribute("AC", "intrinsics");
            var acContrib = dexModAttrib.ContributionTo(acAttrib, dex => 10 + dex);

            context.AttributeContributions.AddRange(abilityModContribs);
            context.AttributeContributions.AddRange(abilitySaveContribs);
            context.AttributeContributions.AddRange(skillContribs);
            context.AttributeContributions.AddRange(racialBonusContribs);
            context.AttributeContributions.AddRange(racialSpeedContribs);
            context.AttributeContributions.AddRange(hitDieContribs);
            context.AttributeContributions.AddRange(levelContribs);
            context.AttributeContributions.Add(profBonusContrib);
            context.AttributeContributions.Add(acContrib);

            context.SaveChanges();
        }
    }
}
