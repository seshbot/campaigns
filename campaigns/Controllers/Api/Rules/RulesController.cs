using AutoMapper;
using Campaigns.Core.Data;
using Campaigns.Model.Data;
using Campaigns.Models.API;
using Services.Rules;
using Services.Rules.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;

namespace Campaigns.Controllers.API.Rules
{
    static class ViewModelExtensions
    {
        public static AttributeViewModel CreateViewModel(this IEntityStore<Campaigns.Model.AttributeContribution> db, Campaigns.Model.Attribute attribute)
        {
            if (null == attribute)
                return null;
            
            Mapper.CreateMap<Campaigns.Model.Attribute, AttributeViewModel>();
            Mapper.CreateMap<Campaigns.Model.AttributeContribution, AttributeContributionViewModel>();

            var viewModel = Mapper.Map<AttributeViewModel>(attribute);
            viewModel.ContributionsFrom = db.GetContributionsFromAttribute(attribute.Id).Select(Mapper.Map<AttributeContributionViewModel>);
            viewModel.ContributionsTo = db.GetContributionsToAttribute(attribute.Id).Select(Mapper.Map<AttributeContributionViewModel>);
        
            return viewModel;
        }

        //public static Models.API.CharacterSheetViewModel CreateViewModel(Campaigns.Model.CharacterSheet characterSheet)
        //{
        //    if (null == characterSheet)
        //        return null;

        //    Mapper.CreateMap<Campaigns.Model.CharacterSheet, Models.API.CharacterSheetViewModel>();
        //    var viewModel = Mapper.Map<Models.API.CharacterSheetViewModel>(characterSheet);
        //    return viewModel;
        //}
    }

    [RoutePrefix("api/rules")]
    public class RulesController : ApiController
    {
        IEntityStore<Campaigns.Model.Attribute> _attributesDb;
        IEntityStore<Campaigns.Model.AttributeContribution> _contributionsDb;

        public RulesController(
            IEntityStore<Campaigns.Model.Attribute> attributesDb,
            IEntityStore<Campaigns.Model.AttributeContribution> contributionsDb)
        {
            _attributesDb = attributesDb;
            _contributionsDb = contributionsDb;
        }

        [ResponseType(typeof(IEnumerable<AttributeViewModel>))]
        [Route("attributes")]
        public IHttpActionResult GetAllAttributes()
        {
            var results = _attributesDb.AsQueryable.Select(_contributionsDb.CreateViewModel);
            return Ok(results);
        }

        [ResponseType(typeof(AttributeViewModel))]
        [Route("attributes/{id:int}")]
        public IHttpActionResult GetAttribute(int id)
        {
            var attrib = _attributesDb.GetById(id);
            if (null == attrib)
            {
                return NotFound();
            }

            return Ok(_contributionsDb.CreateViewModel(attrib));
        }

        [ResponseType(typeof(IEnumerable<AttributeCategoryViewModel>))]
        [Route("categories")]
        public IHttpActionResult GetCategories()
        {
            var categories = _attributesDb.AsQueryable
                .GroupBy(a => a.Category)
                .Select(g => new AttributeCategoryViewModel { Name = g.Key, AttributeCount = g.Count() });

            return Ok(categories);
        }

        [ResponseType(typeof(IEnumerable<AttributeViewModel>))]
        [Route("{category}")]
        public IHttpActionResult GetCategory(string category)
        {
            var attribs = _attributesDb.GetAttributesInCategory(category);
            if (null == attribs || attribs.Count() == 0)
            {
                return NotFound();
            }
            return Ok(attribs.Select(_contributionsDb.CreateViewModel));
        }

        [ResponseType(typeof(AttributeViewModel))]
        [Route("{category}/{id:int}")]
        public IHttpActionResult GetAttribute(string category, int id)
        {
            var attrib = _attributesDb.AsQueryable.FirstOrDefault(a => 
                a.Id == id && 0 == string.Compare(category, a.Category, true));
            if (null == attrib)
            {
                return NotFound();
            }

            return Ok(_contributionsDb.CreateViewModel(attrib));
        }

        [ResponseType(typeof(AttributeViewModel))]
        [Route("{category}/{name}")]
        public IHttpActionResult GetAttribute(string category, string name)
        {
            var attrib = _attributesDb.GetAttribute(name, category);
            if (null == attrib)
            {
                return NotFound();
            }

            return Ok(_contributionsDb.CreateViewModel(attrib));
        }
    }

    [RoutePrefix("api/characters")]
    public class CharactersV2Controller : ApiController
    {
        private IRulesService _rules;

        public CharactersV2Controller(IRulesService rules)
        {
            _rules = rules;
        }
        
        //[ResponseType(typeof(Models.API.CharacterSheetViewModel))]
        //[Route("")]
        //public IHttpActionResult GetAll()
        //{
        //    var attrib = _rulesContext.GetAttribute(name, category);
        //    if (null == attrib)
        //    {
        //        return NotFound();
        //    }

        //    return Ok(_rulesContext.CreateAttributeViewModel(attrib));
        //}
    }
}
