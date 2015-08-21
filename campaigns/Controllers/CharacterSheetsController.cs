using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using campaigns.Models;
using campaigns.Helpers;
using campaigns.Models.DTO;
using Model.Calculations;
using campaigns.Models.DAL;

namespace campaigns.Controllers
{
    public class CharacterSheetsController : Controller
    {
        private CharacterSheetDbContext _charDb = new CharacterSheetDbContext();
        private RulesDbContext _rulesDb = new RulesDbContext();
        private ICharacterSheetService _service;
        private ICalculationService _calcService = new CalculationService();

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
                select CharacterSheetCalculator.AddDerivedStatisticsTo(sheet)).ToList();

            return View(characterSheetsWithDerivedInfo);
        }

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
            return View(CharacterSheetCalculator.AddDerivedStatisticsTo(characterSheet));
        }

        private void EnsureValid(CharacterSheet characterSheet)
        {
            characterSheet.Description = characterSheet.Description ?? new CharacterDescription
            {
                Name = "",
                Text = ""
            };

            // TODO: this shouldnt reference the rules directly
            var levelInfo = LevelInfo.FindBestFit(characterSheet.Experience, characterSheet.Level);
            characterSheet.Experience = levelInfo.XP;
            characterSheet.Level = levelInfo.Level;

            _service.AddStandardAttributesTo(characterSheet);
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
                EnsureValid(characterSheet);
            }

            //newCharacterSheet = DtoHelper.UpdateFromDTO(_charDb, newCharacterSheet, characterSheetInfo);
            var calculationContext = new RulesCalculationContext(_rulesDb, characterSheet);

            var calculatedResults = _calcService.Calculate(calculationContext);

            return View(CharacterSheetCalculator.AddDerivedStatisticsTo(characterSheet));
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
                    EnsureValid(characterSheet);
                    _charDb.CharacterSheets.Add(characterSheet);
                    _charDb.SaveChanges();
                    return RedirectToAction("Details", new { Id = characterSheet.Id });
                }
            }
            catch (DataException)
            {
                ModelState.AddModelError("", "Save failed - an error occurred while trying to save changes");
            }
            
            return View(CharacterSheetCalculator.AddDerivedStatisticsTo(characterSheet));
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
            return View(CharacterSheetCalculator.AddDerivedStatisticsTo(characterSheet));
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
                EnsureValid(characterSheetToUpdate);
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
            return View(CharacterSheetCalculator.AddDerivedStatisticsTo(characterSheetToUpdate));
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
            return View(CharacterSheetCalculator.AddDerivedStatisticsTo(characterSheet));
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
