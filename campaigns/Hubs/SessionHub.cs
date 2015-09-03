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

        public async Task JoinSession(string sessionId)
        {
            if (null == sessionId)
            {
                throw new ArgumentNullException("sessionId");
            }

            // TODO: call the service to make sure we have permissions
            await Groups.Add(Context.ConnectionId, sessionId);

            var connectionId = Context.ConnectionId;
            var userName = Context.User?.Identity?.Name ?? "Anonymous";

            _services.AddClient(new Client { ConnectionId = connectionId, Name = userName, SessionId = sessionId });
        }

        public void SendMessage(string messageText)
        {
            var connectionId = Context.ConnectionId;
            Client client;
            if (!_services.TryGetClient(connectionId, out client))
            {
                throw new Exception("cannot send message - unrecognised client connection");
            }

            var sessionId = client.SessionId;
            if (string.IsNullOrEmpty(sessionId))
            {
                throw new Exception("cannot send message - client has not joined any sessions");
            }

            var model = new Message { Sender = client, Text = messageText, TimeStamp = DateTime.UtcNow };

            var session = _services.GetSession(sessionId);
            _services.AddMessage(session, model);
        }

        public void SetUserHandle(string handle)
        {
            var connectionId = Context.ConnectionId;

            Client user;
            _services.TryGetClient(connectionId, out user);
            user.Name = string.IsNullOrEmpty(handle) ? "Anonymous" : handle;

            _services.UpdateClient(user);
        }

        public override Task OnConnected()
        {
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            var connectionId = Context.ConnectionId;

            // TODO: mark as 'removed' so we can remember their username if they reconnect
            var client = _services.GetClient(connectionId);
            if (null != client)
            {
                _services.RemoveClient(client);
            }

            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            return base.OnReconnected();
        }
    }
}