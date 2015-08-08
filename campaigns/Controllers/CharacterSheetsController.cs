using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using campaigns.Models;

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
            return View(characterSheet);
        }

        // GET: CharacterSheets/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: CharacterSheets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Name,Description,Experience,Level,ProficiencyBonus")] CharacterSheet characterSheet)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.CharacterSheets.Add(characterSheet);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            catch (DataException)
            {
                ModelState.AddModelError("", "Save failed - an error occurred while trying to save changes");
            }
            return View(characterSheet);
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
            return View(characterSheet);
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

            var characterSheetToUpdate = db.CharacterSheets.Find(id);
            if (TryUpdateModel(characterSheetToUpdate, "",
                new string[] { "Name", "Description", "Experience", "Level", "ProficiencyBonus" }))
            {
                try
                {
                    db.SaveChanges();

                    return RedirectToAction("Index");
                }
                catch (DataException)
                {
                    ModelState.AddModelError("", "Edit failed - an error occurred while trying to save changes");
                }
            }
            return View(characterSheetToUpdate);
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
            return View(characterSheet);
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
