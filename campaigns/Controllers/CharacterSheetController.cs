using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace campaigns.Controllers
{
    public class CharacterSheetController : Controller
    {
        // GET: CharacterSheet
        public ActionResult Index()
        {
            return View();
        }
    }
}