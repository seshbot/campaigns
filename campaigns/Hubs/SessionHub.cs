using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Campaigns.Models.Sessions;
using AutoMapper;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Campaigns
{
    public class SessionHub : Hub
    {
        ISessionService _services;

        public SessionHub(ISessionService services)
        {
            if (null == services)
            {
                throw new ArgumentNullException("services");
            }
            _services = services;
        }
        
        public void SendMessage(string sessionId, string messageText)
        {
            var userId = Context.ConnectionId;
            User user;
            _services.TryGetUser(userId, out user);

            var model = new Message { Sender = user, Text = messageText, TimeStamp = DateTime.UtcNow };

            var session = _services.GetSession(sessionId);
            _services.AddMessage(session, model);
        }

        public void SetUserHandle(string handle)
        {
            var userId = Context.ConnectionId;

            User user;
            _services.TryGetUser(userId, out user);
            user.Name = string.IsNullOrEmpty(handle) ? "Anonymous" : handle;

            _services.UpdateUser(user);
        }

        public override Task OnConnected()
        {
            var userId = Context.ConnectionId;
            var userName = Context.User?.Identity?.Name ?? "Anonymous";

            _services.AddUser(new User { Id = userId, Name = userName });

            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            var userId = Context.ConnectionId;

            // TODO: mark as 'removed' so we can remember their username if they reconnect
            var user = _services.GetUser(userId);
            _services.RemoveUser(user);
            
            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            var userId = Context.ConnectionId;
            var userName = Context.User?.Identity?.Name ?? "Anonymous";

            User user;
            if (!_services.TryGetUser(userId, out user))
            {
                _services.AddUser(new User { Id = userId, Name = userName });
            }

            return base.OnReconnected();
        }
    }
}