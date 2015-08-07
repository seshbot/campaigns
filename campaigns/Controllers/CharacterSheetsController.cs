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
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,Description,Experience,Level,ProficiencyBonus")] CharacterSheet characterSheet)
        {
            if (ModelState.IsValid)
            {
                db.CharacterSheets.Add(characterSheet);
                db.SaveChanges();
                return RedirectToAction("Index");
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
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Description,Experience,Level,ProficiencyBonus")] CharacterSheet characterSheet)
        {
            if (ModelState.IsValid)
            {
                db.Entry(characterSheet).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(characterSheet);
        }

        // GET: CharacterSheets/Delete/5
        public ActionResult Delete(int? id)
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

        // POST: CharacterSheets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            CharacterSheet characterSheet = db.CharacterSheets.Find(id);
            db.CharacterSheets.Remove(characterSheet);
            db.SaveChanges();
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
