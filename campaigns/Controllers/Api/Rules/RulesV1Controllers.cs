using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using Campaigns.Models;
using System.Linq.Expressions;

namespace Campaigns.Controllers.API.Rules
{
    // http://pluralsight.com/training/Player?author=scott-allen&name=aspdotnet-mvc5-fundamentals-m5-webapi2&mode=live&clip=0&course=aspdotnet-mvc5-fundamentals
    // TODO: JSON data should be prepended with ")]}',#chr( 10 )#" 
    // to prevent malicious attacks
    [RoutePrefix("api/v1/rules")]
    public class RulesV1Controller : ApiController
    {
        private CharacterSheetDbContext db = new CharacterSheetDbContext();

        private Expression<Func<Models.DAL.RuleSet, bool>> RuleSetIsNamed(string name)
        {
            return set => 0 == string.Compare(set.Name, name, true);
        }
        
        // GET: api/rules/dnd5e
        [ResponseType(typeof(Models.DAL.RuleSet))]
        [Route("{ruleset}")]
        [HttpGet]
        public IHttpActionResult GetRoot(string ruleset)
        {
            var result = db.RuleSets
                .Include(r => r.Abilities)
                .Include(r => r.Classes)
                .Include(r => r.Races)
                .Include(r => r.Skills)
                .FirstOrDefault(RuleSetIsNamed(ruleset));

            if (null == result)
            {
                return NotFound();
            }

            return Ok(result);
        }

        // GET: api/rules/dnd5e/abilities
        [ResponseType(typeof(IEnumerable<Models.DAL.Ability>))]
        [Route("{ruleset}/abilities/")]
        [HttpGet]
        public IHttpActionResult GetAbilities(string ruleset)
        {
            var results = db.RuleSets.FirstOrDefault(RuleSetIsNamed(ruleset));
            if (null == results)
            {
                return NotFound();
            }

            return Ok(results.Abilities);
        }

        // GET: api/rules/dnd5e/skills
        [ResponseType(typeof(IEnumerable<Models.DAL.Skill>))]
        [Route("{ruleset}/skills/")]
        [HttpGet]
        public IHttpActionResult GetSkills(string ruleset)
        {
            var results = db.RuleSets.FirstOrDefault(RuleSetIsNamed(ruleset));
            if (null == results)
            {
                return NotFound();
            }

            return Ok(results.Skills);
        }
        
        [ResponseType(typeof(Models.DAL.Ability))]
        [Route("{ruleset}/abilities/{id:int}")]
        [HttpGet]
        public IHttpActionResult GetAbilityByID(string ruleset, int id)
        {
            var ruleSets = db.RuleSets.FirstOrDefault(RuleSetIsNamed(ruleset));
            if (null == ruleSets)
            {
                return NotFound();
            }

            var ability = ruleSets.Abilities.FirstOrDefault(a => a.Id == id);
            if (null == ability)
            {
                return NotFound();
            }

            return Ok(ability);
        }
        
        [ResponseType(typeof(Models.DAL.Ability))]
        [Route("{ruleset}/abilities/{name}")]
        [HttpGet]
        public IHttpActionResult GetAbilityByName(string ruleset, string name)
        {
            var ruleSets = db.RuleSets.FirstOrDefault(RuleSetIsNamed(ruleset));
            if (null == ruleSets)
            {
                return NotFound();
            }

            var ability = ruleSets.Abilities
                .FirstOrDefault(a => 
                    0 == string.Compare(a.ShortName, name, true) || 
                    0 == string.Compare(a.Name, name, true));

            if (null == ability)
            {
                return NotFound();
            }

            return Ok(ability);
        }
        
        [ResponseType(typeof(Models.DAL.Skill))]
        [Route("{ruleset}/skills/{id:int}")]
        [HttpGet]
        public IHttpActionResult GetSkillByID(string ruleset, int id)
        {
            var ruleSets = db.RuleSets.FirstOrDefault(RuleSetIsNamed(ruleset));
            if (null == ruleSets)
            {
                return NotFound();
            }

            var ability = ruleSets.Skills.FirstOrDefault(a => a.Id == id);
            if (null == ability)
            {
                return NotFound();
            }

            return Ok(ability);
        }
        
        [ResponseType(typeof(Models.DAL.Skill))]
        [Route("{ruleset}/skills/{name}")]
        [HttpGet]
        public IHttpActionResult GetSkillByName(string ruleset, string name)
        {
            var ruleSets = db.RuleSets.FirstOrDefault(RuleSetIsNamed(ruleset));
            if (null == ruleSets)
            {
                return NotFound();
            }

            var skill = ruleSets.Skills.FirstOrDefault(a => 0 == string.Compare(a.Name, name, true));
            if (null == skill)
            {
                return NotFound();
            }

            return Ok(skill);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}