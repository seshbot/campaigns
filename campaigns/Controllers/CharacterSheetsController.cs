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
using campaigns.Models.Api;

namespace campaigns.Controllers
{
    public class CharacterSheetsController : Controller
    {
        private CharacterSheetDbContext db = new CharacterSheetDbContext();

        // GET: CharacterSheets
        public ActionResult Index()
        {
            return View(db.CharacterSheets.ToList());
        }

        // GET: CharacterSheets/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CharacterSheet characterSheet = db.CharacterSheets.Find(id);
            if (characterSheet == null)
            {
                return HttpNotFound();
            }
            return View(CharacterSheetCalculator.AddDerivedStatisticsTo(characterSheet));
        }

        // GET: CharacterSheets/Create
        public ActionResult Create([Bind(Exclude = "Id")] CharacterSheetDTO characterSheet)
        {
            var newCharacterSheet = db.CreateEmptyCharacterSheet();
            if (null != characterSheet)
            {
                newCharacterSheet = ApiHelper.UpdateFromApiData(db, newCharacterSheet, characterSheet);
            }

            return View(CharacterSheetCalculator.AddDerivedStatisticsTo(newCharacterSheet));
        }

        // POST: CharacterSheets/Create
        [HttpPost]
        [ActionName("Create")]
        [ValidateAntiForgeryToken]
        public ActionResult CreateConfirm([Bind(Exclude = "Id")] CharacterSheetDTO characterSheet)
        {
            var newCharacterSheet = ApiHelper.CreateFromApiData(db, characterSheet);
            try
            {
                if (ModelState.IsValid)
                {
                    db.CharacterSheets.Add(newCharacterSheet);
                    db.SaveChanges();
                    return RedirectToAction("Details", new { Id = newCharacterSheet.Id });
                }
            }
            catch (DataException)
            {
                ModelState.AddModelError("", "Save failed - an error occurred while trying to save changes");
            }
            
            return View(CharacterSheetCalculator.AddDerivedStatisticsTo(newCharacterSheet));
        }

        // GET: CharacterSheets/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CharacterSheet characterSheet = db.CharacterSheets.Find(id);
            if (characterSheet == null)
            {
                return HttpNotFound();
            }
            return View(CharacterSheetCalculator.AddDerivedStatisticsTo(characterSheet));
        }

        // POST: CharacterSheets/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CharacterSheetDTO characterSheet)
        {
            // TODO: track only fields that change? send viewmodel through and use http://automapper.org/ ?
            // Use db.Entry on the entity instance to set its state to Unchanged, and then set 
            // Property("PropertyName").IsModified to true on each entity property that is included in the view model
            if (!characterSheet.Id.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            
            var newCharacterSheet = db.CharacterSheets.Find(characterSheet.Id);
            db.Entry(newCharacterSheet).State = EntityState.Detached;
            try
            {
                var updatedCharacterSheet = ApiHelper.UpdateFromApiData(db, newCharacterSheet, characterSheet);
                db.CharacterSheets.Add(updatedCharacterSheet);
                db.SaveChanges();

                return RedirectToAction("Details", new { Id = updatedCharacterSheet.Id });
            }
            catch (DataException)
            {
                ModelState.AddModelError("", "Edit failed - an error occurred while trying to save changes");
            }

            return View(CharacterSheetCalculator.AddDerivedStatisticsTo(newCharacterSheet));
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
            CharacterSheet characterSheet = db.CharacterSheets.Find(id);
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
                CharacterSheet characterSheet = db.CharacterSheets.Find(id);
                db.CharacterSheets.Remove(characterSheet);
                db.SaveChanges();
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
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
