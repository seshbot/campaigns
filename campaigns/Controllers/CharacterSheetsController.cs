using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Campaigns.Models;
using Campaigns.Helpers;
using System.Linq.Expressions;
using Campaigns.Models.CharacterSheet;
using Services.Rules;
using Campaigns.Core.Data;
using AutoMapper;
using Campaigns.Model;

namespace Campaigns.Controllers
{
    public class CharacterSheetsController : Controller
    {
        private IRulesService _rules;

        public CharacterSheetsController(IRulesService rules)
        {
            _rules = rules;
        }

        [NonAction]
        private CreateCharacterViewModel PrepareCreateCharacterViewModel()
        {
            return new CreateCharacterViewModel
            {
                Level = 1,
                RaceId = _rules.GetAttributesByCategory("races").First().Id,
                InitialClassId = _rules.GetAttributesByCategory("classes").First().Id,
            };
        }

#if false
        [NonAction]
        private CharacterViewModel PrepareDetailedCharacterViewModel(Character character, IDictionary<int, Model.Attribute> attributesById, IDictionary<string, IList<int>> attributeIdsByCategory)
        {
            var raceIds = attributeIdsByCategory["races"];
            var raceAttributeId = character.Sheet.AttributeAllocations
                ?.FirstOrDefault(a => raceIds.Contains(a.AttributeId))
                ?.AttributeId;
            var raceName = raceAttributeId.HasValue ? attributesById[raceAttributeId.Value].LongName : "";

            var classIds = attributeIdsByCategory["classes"];
            var classAttributeId = character.Sheet.AttributeAllocations
                ?.FirstOrDefault(a => classIds.Contains(a.AttributeId))
                ?.AttributeId;
            var className = classAttributeId.HasValue ? attributesById[classAttributeId.Value].LongName : "";

            var attribVals =
               (from attribVal in character.Sheet.AttributeValues
                select new
                {
                    AttributeId = attribVal.AttributeId,
                    Value = attribVal.Value,
                    AttributeIdsContributingTo = attribVal.Contributions.Select(c => c.SourceId)
                }).ToList();

            IEnumerable<AttributeValueViewModel> attribValViewModels;
            using (new Tracer("Creating Attrib Value View Models"))
            {
                attribValViewModels =
                   (from attribVal in attribVals
                    let attrib = attributesById[attribVal.AttributeId]
                    let contribsTo = attribVal.AttributeIdsContributingTo
                        .Where(id => id.HasValue)
                        .Select(id =>
                            new AttributeContributionViewModel
                            {
                                SourceId = id.Value,
                                SourceName = attributesById[id.Value].Name
                            })
                    select new AttributeValueViewModel
                    {
                        AttributeId = attribVal.AttributeId,
                        AttributeCategory = attrib.Category,
                        AttributeName = attrib.Name,
                        AttributeLongName = attrib.LongName,
                        AttributeSortOrder = attrib.SortOrder,
                        Value = attribVal.Value,
                        Contributions = contribsTo,
                    }).ToList();
            }

            var viewModel = new CharacterViewModel
            {
                Id = character.Id,
                Name = character.Name,
                Description = character.Description,
                ClassName = className,
                RaceName = raceName,
                AttributeValues = attribValViewModels
            };

            return viewModel;
        }
#endif

        [NonAction]
        private CharacterViewModel PrepareCharacterViewModel(Character character)
        {
            var raceName = character.Sheet.AttributeAllocations
                ?.FirstOrDefault(a => "races" == a.Attribute.Category)
                ?.Attribute.LongName;

            var className = character.Sheet.AttributeAllocations
                ?.FirstOrDefault(a => "classes" == a.Attribute.Category)
                ?.Attribute.LongName;
            
            Mapper.CreateMap<AttributeValue, AttributeValueViewModel>();
            Mapper.CreateMap<Character, CharacterViewModel>()
                    .ForMember(dest => dest.AttributeValues, o => o.MapFrom(src => src.Sheet.AttributeValues));

            var viewModel = Mapper.Map<CharacterViewModel>(character);
            viewModel.RaceName = raceName;
            viewModel.ClassName = className;
            return viewModel;
        }

        [NonAction]
        private Models.CharacterSheet.IndexViewModel PrepareIndexViewModel()
        {
            Mapper.CreateMap<Model.Attribute, SelectListItem>()
            .ForMember(dest => dest.Value, o => o.MapFrom(src => src.Id))
            .ForMember(dest => dest.Text, o => o.MapFrom(src => src.LongName));
            
            IEnumerable<CharacterViewModel> characterViewModels =
                _rules.GetCharacters().Select(PrepareCharacterViewModel).ToList();

            return new Models.CharacterSheet.IndexViewModel
            {
                Characters = characterViewModels,
                CreateCharacter = PrepareCreateCharacterViewModel(),
                Races = _rules.GetAttributesByCategory("races").Select(Mapper.Map<SelectListItem>).ToList(),
                Classes = _rules.GetAttributesByCategory("classes").Select(Mapper.Map<SelectListItem>).ToList(),
            };
        }

        [NonAction]
        private Campaigns.Model.Character CreateCharacter(CreateCharacterViewModel viewModel)
        {
            if (null == viewModel)
                return _rules.CreateCharacter("Unnamed", "");

            var standardAllocations = _rules.GetAttributesByCategory("abilities")
                .Where(a => a.IsStandard)
                .ToList()
                .Select(a => new AttributeAllocation { Attribute = a, Value = 8 });

            var characterAllocations = new[]
            {
                new AttributeAllocation { Attribute = _rules.GetAttributeById(viewModel.RaceId) },
                new AttributeAllocation { Attribute = _rules.GetAttributeById(viewModel.InitialClassId) },
            };

            var allocations = standardAllocations.Concat(characterAllocations);
            var newCharacter = _rules.CreateCharacter(viewModel.Name, viewModel.Description, allocations);
            return newCharacter;
        }

        // GET: CharacterSheets
        public ActionResult Index()
        {
            var viewModel = PrepareIndexViewModel();
            return View(viewModel);
        }

        [HttpPost]
        [ActionName("Index")]
        [ValidateAntiForgeryToken]
        public ActionResult IndexAddCharacter(Models.CharacterSheet.IndexViewModel viewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var newCharacter = CreateCharacter(viewModel.CreateCharacter);
                    return RedirectToAction("Index");
                }
            }
            catch (DataException)
            {
                ModelState.AddModelError("", "Creation failed - an error occurred while trying to save changes");
            }

            var newViewModel = PrepareIndexViewModel();
            newViewModel.IsModalOpen = true;
            newViewModel.CreateCharacter = viewModel.CreateCharacter;
            return View(newViewModel);
        }

        // GET: CharacterSheets/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var character = _rules.GetCharacter(id.Value);
            if (character == null)
            {
                return HttpNotFound();
            }
            
            var viewModel = new ViewEditCharacterViewModel
            {
                IsEditing = false,
                Character = PrepareCharacterViewModel(character)
            };

            return View(viewModel);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var character = _rules.GetCharacter(id.Value);
            if (character == null)
            {
                return HttpNotFound();
            }
            
            var viewModel = new ViewEditCharacterViewModel
            {
                IsEditing = true,                
                Character = PrepareCharacterViewModel(character)
            };

            return View(viewModel);
        }

        //[HttpPost, ActionName("Edit")]
        //[ValidateAntiForgeryToken]
        //public ActionResult EditConfirmed(int? id)
        //{
        //    // TODO: track only fields that change? send viewmodel through and use http://automapper.org/ ?
        //    // Use db.Entry on the entity instance to set its state to Unchanged, and then set 
        //    // Property("PropertyName").IsModified to true on each entity property that is included in the view model
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }

        //    var characterSheetToUpdate = _charDb.CharacterSheets.Find(id);
        //    _charDb.Entry(characterSheetToUpdate).State = EntityState.Detached;
        //    if (TryUpdateModel(characterSheetToUpdate))
        //    {
        //        _service.EnsureValid(characterSheetToUpdate);
        //        try
        //        {
        //            // create new entry, dont update existing entry
        //            _charDb.CharacterSheets.Add(characterSheetToUpdate);
        //            _charDb.SaveChanges();
        //            return RedirectToAction("Details", new { Id = characterSheetToUpdate.Id });
        //        }
        //        catch (DataException)
        //        {
        //            ModelState.AddModelError("", "Edit failed - an error occurred while trying to save changes");
        //        }
        //    }
        //    return View(_service.AddCalculatedStatisticsTo(characterSheetToUpdate));
        //}

        public ActionResult Delete(int? id, bool? saveChangesError = false)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (saveChangesError.GetValueOrDefault())
            {
                ViewBag.ErrorMessage = "Delete failed - an error occurred while trying to save changes";
            }
            Character character = _rules.GetCharacter(id.Value);
            if (character == null)
            {
                return HttpNotFound();
            }
            return View(PrepareCharacterViewModel(character));
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            try
            {
                _rules.DeleteCharacterById(id);
            }
            catch (DataException)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.
                return RedirectToAction("Delete", new { id = id, saveChangesError = true });
            }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
            base.Dispose(disposing);
        }
    }
    
    class Tracer : IDisposable
    {
        string _name;
        public Tracer(string name)
        {
            _name = name;
            System.Diagnostics.Trace.TraceInformation("Begin " + _name);
        }

        public void Dispose()
        {
            System.Diagnostics.Trace.TraceInformation("End " + _name);
        }
    }
}
