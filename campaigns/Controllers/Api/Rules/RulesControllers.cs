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
using campaigns.Models;

namespace campaigns.Controllers.Api.Rules
{
    public class RootController : ApiController
    {
        private CharacterSheetDbContext db = new CharacterSheetDbContext();

        // GET: api/rules/Root
        [ResponseType(typeof(Models.Rules))]
        public IHttpActionResult Get()
        {
            return Ok(db.Rules.First());
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

    public class RacesController : ApiController
    {
        private CharacterSheetDbContext db = new CharacterSheetDbContext();

        // GET: api/rules/Races
        public IQueryable<Race> GetAll()
        {
            return db.Races;
        }

        // GET: api/rules/Races/5
        [ResponseType(typeof(Race))]
        public IHttpActionResult GetSingle(int id)
        {
            var race = db.Races.Find(id);
            if (race == null)
            {
                return NotFound();
            }

            return Ok(race);
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

    public class ClassesController : ApiController
    {
        private CharacterSheetDbContext db = new CharacterSheetDbContext();

        // GET: api/rules/Classes
        public IQueryable<Class> GetAll()
        {
            return db.Classes;
        }

        // GET: api/rules/Classes/5
        [ResponseType(typeof(Class))]
        public IHttpActionResult GetSingle(int id)
        {
            var Class = db.Classes.Find(id);
            if (Class == null)
            {
                return NotFound();
            }

            return Ok(Class);
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