using AutoMapper;
using Campaigns.Models.Sessions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Campaigns.Controllers
{
    public class SessionsController : Controller
    {
        private ISessionService _sessions;

        public SessionsController(ISessionService sessions)
        {
            _sessions = sessions;
        }

        // GET: Session
        public ActionResult Index()
        {
            Mapper.CreateMap<Session, SessionViewModel>();
            var viewModels = _sessions.GetSessions().Select(Mapper.Map<SessionViewModel>);

            return View(viewModels);
        }

        public ActionResult Create()
        {
            var newSession = _sessions.CreateSession();
            return RedirectToAction("Join", new { id = newSession.Id } );
        }

        public ActionResult Join(string id)
        {
            var session = _sessions.GetSession(id);
            if (null == session)
            {
                return HttpNotFound();
            }
            Mapper.CreateMap<Session, SessionViewModel>();

            var viewModel = Mapper.Map<SessionViewModel>(session);
            return View(viewModel);
        }
    }
}