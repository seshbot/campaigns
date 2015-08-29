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
using Campaigns.Models.DTO;
using Services.Calculation;
using Campaigns.Models.DAL;
using System.Linq.Expressions;

namespace Campaigns.Controllers
{
    public class CharacterSheetsController : Controller
    {
        private CharacterSheetDbContext _charDb = new CharacterSheetDbContext();
        private ICharacterSheetService _service;

        public CharacterSheetsController()
        {
            _service = new CharacterSheetService(_charDb);
        }

        public CharacterSheetsController(ICharacterSheetService service)
        {
            _service = service;
        }

        // GET: CharacterSheets
        public ActionResult Index()
        {
            var characterSheetsWithDerivedInfo =
               (from sheet in _charDb.CharacterSheets.ToList()
                select _service.AddCalculatedStatisticsTo(sheet)).ToList();

            return View(characterSheetsWithDerivedInfo);
        }

        //Expression<Func<AttributeValue, AttributeViewModel<int>>> ToAttributeViewModel = a => new AttributeViewModel<int>
        //{
        //    AttributeId = a.Attribute.Id,
        //    IsSet = true,
        //    Name = a.Attribute.LongName,
        //    ShortName = a.Attribute.Name,
        //    Value = a.Value
        //};
        //Expression<Func<CharacterSheet, CharacterSheetViewModel>> ToViewModel = c => new CharacterSheetViewModel
        //{
        //    Mode = Mode.View,
        //    Name = c.Description.Name,
        //    ShortDescription = c.Description.ShortText,
        //    Description = c.Description.Text,
        //    Xp = c.Experience,
        //    Level = c.Level,
        //    Race = ,
        //    Class = ,
        //    ProficiencyBonus = ,
        //    Abilities = c.AbilityAllocations.Select(ToAttributeViewModel),
        //};

        // GET: CharacterSheets/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CharacterSheet characterSheet = _charDb.CharacterSheets.Find(id);
            if (characterSheet == null)
            {
                return HttpNotFound();
            }

            return View(_service.AddCalculatedStatisticsTo(characterSheet));
        }

        // GET: CharacterSheets/Create
        public ActionResult Create([Bind(Exclude = "Id")] CharacterSheet characterSheet)
        {
            if (null == characterSheet)
            {
                characterSheet = _service.CreateCharacterSheet();
            }
            else
            {
                _service.EnsureValid(characterSheet);
            }

            //newCharacterSheet = DtoHelper.UpdateFromDTO(_charDb, newCharacterSheet, characterSheetInfo);
            return View(_service.AddCalculatedStatisticsTo(characterSheet));
        }

        // POST: CharacterSheets/Create
        [HttpPost]
        [ActionName("Create")]
        [ValidateAntiForgeryToken]
        public ActionResult CreateConfirm([Bind(Exclude = "Id")] CharacterSheet characterSheet)
        {
            //var newCharacterSheet = DtoHelper.CreateFromDTO(_charDb, characterSheet);
            try
            {
                if (ModelState.IsValid)
                {
                    _service.EnsureValid(characterSheet);
                    _charDb.CharacterSheets.Add(characterSheet);
                    _charDb.SaveChanges();
                    return RedirectToAction("Details", new { Id = characterSheet.Id });
                }
            }
            catch (DataException)
            {
                ModelState.AddModelError("", "Save failed - an error occurred while trying to save changes");
            }

            characterSheet = _service.AddCalculatedStatisticsTo(characterSheet);

            //var characterSheetEx = ToExperimental(characterSheet);
            //_charDb.CharacterSheetsEx.Add(characterSheetEx);
            //_charDb.SaveChanges();

            return View(characterSheet);
        }

        // GET: CharacterSheets/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CharacterSheet characterSheet = _charDb.CharacterSheets.Find(id);
            if (characterSheet == null)
            {
                return HttpNotFound();
            }
            return View(_service.AddCalculatedStatisticsTo(characterSheet));
        }

        // POST: CharacterSheets/Edit/5
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public ActionResult EditConfirmed(int? id)
        {
            // TODO: track only fields that change? send viewmodel through and use http://automapper.org/ ?
            // Use db.Entry on the entity instance to set its state to Unchanged, and then set 
            // Property("PropertyName").IsModified to true on each entity property that is included in the view model
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var characterSheetToUpdate = _charDb.CharacterSheets.Find(id);
            _charDb.Entry(characterSheetToUpdate).State = EntityState.Detached;
            if (TryUpdateModel(characterSheetToUpdate))
            {
                _service.EnsureValid(characterSheetToUpdate);
                try
                {
                    // create new entry, dont update existing entry
                    _charDb.CharacterSheets.Add(characterSheetToUpdate);
                    _charDb.SaveChanges();
                    return RedirectToAction("Details", new { Id = characterSheetToUpdate.Id });
                }
                catch (DataException)
                {
                    ModelState.AddModelError("", "Edit failed - an error occurred while trying to save changes");
                }
            }
            return View(_service.AddCalculatedStatisticsTo(characterSheetToUpdate));
        }

        // GET: CharacterSheets/Delete/5
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
            CharacterSheet characterSheet = _charDb.CharacterSheets.Find(id);
            if (characterSheet == null)
            {
                return HttpNotFound();
            }
            return View(_service.AddCalculatedStatisticsTo(characterSheet));
        }

        // POST: CharacterSheets/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            try
            {
                CharacterSheet characterSheet = _charDb.CharacterSheets.Find(id);
                foreach (var o in characterSheet.AbilityAllocations.ToList())
                    _charDb.Entry(o).State = EntityState.Deleted;
                foreach (var o in characterSheet.SkillAllocations.ToList())
                    _charDb.Entry(o).State = EntityState.Deleted;
                _charDb.CharacterSheets.Remove(characterSheet);
                _charDb.SaveChanges();
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
                _charDb.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
