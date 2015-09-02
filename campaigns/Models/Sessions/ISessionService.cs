using AutoMapper;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace Campaigns.Models.Sessions
{
    public class User
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public interface ISessionService
    {
        Session CreateSession();
        Session GetSession(string id);
        IEnumerable<Session> GetSessions(bool publicOnly = true);

        void AddUser(User user);
        void RemoveUser(User user);

        IEnumerable<User> GetUsers();
        User GetUser(string id);
        bool TryGetUser(string id, out User user);
        void UpdateUser(User user);

        void AddMessage(Session session, Message message);
        IEnumerable<Message> GetSessionMessages(Session session);
    }

    public class SessionService : ISessionService
    {
        private static int _nextSeed = new Random().Next();
        private static readonly string KEY_DICT = "abcdefghijklmnopqrstuvwxyz0123456789";
        private static int nextSeed() { return Interlocked.Increment(ref _nextSeed); }

        private static string createKey()
        {
            var rand = new Random(nextSeed());
            var sb = new StringBuilder();
            for (var i = 0; i < 5; i++)
            {
                var idx = rand.Next(KEY_DICT.Length - 1);
                sb.Append(KEY_DICT[idx]);
            }
            return sb.ToString();
        }

        private IHubConnectionContext<dynamic> _clients;
        private ConcurrentDictionary<string, Session> _sessionsById = new ConcurrentDictionary<string, Session>();
        private ConcurrentDictionary<string, IList<Message>> _messagesBySessionId = new ConcurrentDictionary<string, IList<Message>>();

        private ConcurrentDictionary<string, User> _usersById = new ConcurrentDictionary<string, User>();

        public SessionService(IHubConnectionContext<dynamic> clients)
        {
            if (null == clients)
            {
                throw new ArgumentNullException("clients");
            }

            _clients = clients;
        }

        public Session CreateSession()
        {
            var key = createKey();
            var newSession = new Session();
            var retryAttempts = 0;
            while (!_sessionsById.TryAdd(key, newSession))
            {
                key = createKey();
                if (retryAttempts++ >= 100)
                    throw new Exception("Cannot create new session - cannot find a new unique sesion key!");
            }

            newSession.Id = key;
            return newSession;
        }

        public IEnumerable<Session> GetSessions(bool publicOnly = true)
        {
            var sessions = _sessionsById.Values.ToList();
            if (publicOnly)
            {
                sessions = sessions.Where(s => s.IsPublic).ToList();
            }

            return sessions;
        }

        public Session GetSession(string id)
        {
            if (null == id)
            {
                return null;
            }
            Session result;
            _sessionsById.TryGetValue(id, out result);
            return result;
        }

        private int _nextMessageId = 1;
        public void AddMessage(Session session, Message message)
        {
            message.Id = _nextMessageId++;

            var messages = _messagesBySessionId.GetOrAdd(session.Id, k => new List<Message>());
            lock (messages)
            {
                messages.Add(message);
            }

            Mapper.CreateMap<Message, MessageViewModel>();

            var viewModel = Mapper.Map<MessageViewModel>(message);
            _clients.All.onNewMessage(viewModel);
        }

        public IEnumerable<Message> GetSessionMessages(Session session)
        {
            var messages = _messagesBySessionId
                .GetOrAdd(session.Id, k => new List<Message>())
                .ToList();

            return messages;
        }

        public void AddUser(User user)
        {
            _usersById.TryAdd(user.Id, user);

            _clients.All.onUsersUpdated(GetUsers().Count());
        }

        public void RemoveUser(User user)
        {
            User userRemoved;
            _usersById.TryRemove(user.Id, out userRemoved);

            _clients.All.onUsersUpdated(GetUsers().Count());
        }

        public IEnumerable<User> GetUsers()
        {
            return _usersById.Values;
        }

        public User GetUser(string id)
        {
            return _usersById[id];
        }

        public bool TryGetUser(string id, out User user)
        {
            return _usersById.TryGetValue(id, out user);
        }

        public void UpdateUser(User user)
        {
            _usersById.TryUpdate(user.Id, user, user);
        }
    }
}