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
    // http://pluralsight.com/training/Player?author=scott-allen&name=aspdotnet-mvc5-fundamentals-m5-webapi2&mode=live&clip=0&course=aspdotnet-mvc5-fundamentals
    // TODO: JSON data should be prepended with ")]}',#chr( 10 )#" 
    // to prevent malicious attacks
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
}