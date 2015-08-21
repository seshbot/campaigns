using campaigns.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace campaigns.Controllers.Api.Rules
{
    [RoutePrefix("api/attributes")]
    public class AttributesController : ApiController
    {
        private CharacterSheetDbContext _charDb = new CharacterSheetDbContext();
        private RulesDbContext _attribDb = new RulesDbContext();

        [Route("category/{name}")]
        [HttpGet]
        public IEnumerable<Model.Calculations.Attribute> GetByCategory(string name)
        {
            return _attribDb.Attributes.Where(a => 0 == String.Compare(name, a.Category));
        }

        [Route("")]
        [HttpGet]
        public IEnumerable<Model.Calculations.Attribute> Get()
        {
            return _attribDb.Attributes;
        }
    }
}
