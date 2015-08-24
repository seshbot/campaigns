using campaigns.Models;
using campaigns.Models.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace campaigns.Controllers.Api.Rules
{
    static class RulesDbViewModelExtensions
    {
        public static AttributeViewModel CreateAttributeViewModel(this RulesDbContext db, int attribId)
        {
            var attrib = db.Attributes.FirstOrDefault(a => a.Id == attribId);
            if (null == attrib)
            {
                return null;
            }

            return CreateAttributeViewModel(db, attrib);
        }

        public static AttributeViewModel CreateAttributeViewModel(this RulesDbContext db, Model.Calculations.Attribute attribute)
        {
            if (null == attribute)
            {
                return null;
            }

            var viewmodel = new AttributeViewModel
            {
                Attribute = attribute,
                ContributedToBy = db.GetContributionsToAttribute(attribute.Id),
                ContributesTo = db.GetContributionsByAttribute(attribute.Id),
            };

            return viewmodel;
        }
    }

    [RoutePrefix("api/v2/rules")]
    public class RulesV2Controller : ApiController
    {
        private RulesDbContext _db = new RulesDbContext();

        [ResponseType(typeof(IEnumerable<AttributeViewModel>))]
        [Route("attributes")]
        public IHttpActionResult GetAll()
        {
            return Ok(_db.Attributes.Select(_db.CreateAttributeViewModel));
        }

        [ResponseType(typeof(AttributeViewModel))]
        [Route("attributes/{id:int}")]
        public IHttpActionResult Get(int id)
        {
            var viewmodel = _db.CreateAttributeViewModel(id);
            if (null == viewmodel)
            {
                return NotFound();
            }

            return Ok(viewmodel);
        }

        [ResponseType(typeof(IEnumerable<AttributeCategoryViewModel>))]
        [Route("categories")]
        public IHttpActionResult GetCategories()
        {
            var categories = _db.Attributes
                .GroupBy(a => a.Category)
                .Select(g => new AttributeCategoryViewModel { Name = g.Key, AttributeCount = g.Count() });

            return Ok(categories);
        }

        [ResponseType(typeof(IEnumerable<AttributeViewModel>))]
        [Route("{category}")]
        public IHttpActionResult Get(string category)
        {
            var attribs = _db.GetAttributesInCategory(category);
            if (null == attribs || attribs.Count() == 0)
            {
                return NotFound();
            }
            return Ok(attribs.Select(_db.CreateAttributeViewModel));
        }

        [ResponseType(typeof(AttributeViewModel))]
        [Route("{category}/{name}")]
        public IHttpActionResult Get(string category, string name)
        {
            var attrib = _db.GetAttribute(name, category);
            if (null == attrib)
            {
                return NotFound();
            }

            return Ok(_db.CreateAttributeViewModel(attrib));
        }
    }
}
