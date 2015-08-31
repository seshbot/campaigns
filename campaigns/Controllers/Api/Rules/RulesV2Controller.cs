using AutoMapper;
using Campaigns.Core.Data;
using Campaigns.Model.Data;
using Campaigns.Models;
using Campaigns.Models.API;
using Services.Rules;
using Services.Rules.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace Campaigns.Controllers.API.Rules
{
    static class ViewModelExtensions
    {
        public static AttributeViewModel CreateViewModel(this CampaignsDbContext db, Campaigns.Model.Attribute attribute)
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

        public static Models.API.CharacterSheetViewModel CreateViewModel(Campaigns.Model.CharacterSheet characterSheet)
        {
            if (null == characterSheet)
                return null;

            Mapper.CreateMap<Campaigns.Model.CharacterSheet, Models.API.CharacterSheetViewModel>();
            var viewModel = Mapper.Map<Models.API.CharacterSheetViewModel>(characterSheet);
            return viewModel;
        }
    }

    [RoutePrefix("api/v2/rules")]
    public class RulesV2Controller : ApiController
    {
        private CampaignsDbContext _rulesContext = new CampaignsDbContext();

        [ResponseType(typeof(IEnumerable<AttributeViewModel>))]
        [Route("attributes")]
        public IHttpActionResult GetAll()
        {
            return Ok(_rulesContext.Attributes.Select(_rulesContext.CreateViewModel));
        }

        [ResponseType(typeof(AttributeViewModel))]
        [Route("attributes/{id:int}")]
        public IHttpActionResult Get(int id)
        {
            var attrib = _rulesContext.Attributes.FirstOrDefault(a => a.Id == id);
            if (null == attrib)
            {
                return NotFound();
            }

            return Ok(_rulesContext.CreateViewModel(attrib));
        }

        [ResponseType(typeof(IEnumerable<AttributeCategoryViewModel>))]
        [Route("categories")]
        public IHttpActionResult GetCategories()
        {
            var categories = _rulesContext.Attributes
                .GroupBy(a => a.Category)
                .Select(g => new AttributeCategoryViewModel { Name = g.Key, AttributeCount = g.Count() });

            return Ok(categories);
        }

        [ResponseType(typeof(IEnumerable<AttributeViewModel>))]
        [Route("{category}")]
        public IHttpActionResult Get(string category)
        {
            var attribs = _rulesContext.GetAttributesInCategory(category);
            if (null == attribs || attribs.Count() == 0)
            {
                return NotFound();
            }
            return Ok(attribs.Select(_rulesContext.CreateViewModel));
        }

        [ResponseType(typeof(AttributeViewModel))]
        [Route("{category}/{name}")]
        public IHttpActionResult Get(string category, string name)
        {
            var attrib = _rulesContext.GetAttribute(name, category);
            if (null == attrib)
            {
                return NotFound();
            }

            return Ok(_rulesContext.CreateViewModel(attrib));
        }
    }

    [RoutePrefix("api/v2/characters")]
    public class CharactersV2Controller : ApiController
    {
        private CampaignsDbContext _dbContext = new CampaignsDbContext();

        private EFEntityRepository<Campaigns.Model.Attribute> _attributesDb;
        private EFEntityRepository<Campaigns.Model.AttributeContribution> _contributionsDb;
        private EFEntityRepository<Campaigns.Model.CharacterSheet> _sheetsDb;

        private IRulesService _rules;

        public CharactersV2Controller()
        {
            _attributesDb = new EFEntityRepository<Campaigns.Model.Attribute>(_dbContext, _dbContext.Attributes);
            _contributionsDb = new EFEntityRepository<Campaigns.Model.AttributeContribution>(_dbContext, _dbContext.AttributeContributions);
            _sheetsDb = new EFEntityRepository<Campaigns.Model.CharacterSheet>(_dbContext, _dbContext.CharacterSheets);

            _rules = new RulesService(_attributesDb, _contributionsDb, _sheetsDb);
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
