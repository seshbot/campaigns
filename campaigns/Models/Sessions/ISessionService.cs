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
using System.Threading.Tasks;

namespace Campaigns.Models.Sessions
{
    public class Client
    {
        public string ConnectionId { get; set; }
        public string SessionId { get; set; }
        public string Name { get; set; }
    }

    public interface ISessionService
    {
        Session CreateSession();
        Session GetSession(string id);
        IEnumerable<Session> GetSessions(bool publicOnly = true);

        void AddClient(Client client);
        void RemoveClient(Client client);

        IEnumerable<Client> GetClients();
        Client GetClient(string id);
        bool TryGetClient(string id, out Client client);
        void UpdateClient(Client client);

        void HandleCommand(Session session, SendMessageCommand command);
        void HandleCommand(Session session, RollDiceCommand command);
        IEnumerable<BaseClientEvent> GetSessionEvents(Session session);
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
        private ConcurrentDictionary<string, IList<BaseClientEvent>> _eventsBySessionId = new ConcurrentDictionary<string, IList<BaseClientEvent>>();

        private ConcurrentDictionary<string, Client> _clientsById = new ConcurrentDictionary<string, Client>();

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
        
        private int _nextEventId = 1;
        private void storeEvent(Session session, BaseClientEvent clientEvent)
        {
            clientEvent.Id = _nextEventId++;

            var events = _eventsBySessionId.GetOrAdd(session.Id, k => new List<BaseClientEvent>());
            lock (events)
            {
                events.Add(clientEvent);
            }
        }

        private void setBaseEventDetails(BaseClientEvent baseEvent, BaseCommand baseCommand)
        {
            // TODO: get client details from the hub context (dont have them in the command?)
            baseEvent.Sender = baseCommand.Sender;
            baseEvent.TimeStamp = baseCommand.TimeStamp;
        }

        private ClientSentMessageEvent createEvent(SendMessageCommand command)
        {
            var result = new ClientSentMessageEvent { Text = command.Text };
            setBaseEventDetails(result, command);
            return result;
        }

        private ClientRolledDiceEvent createEvent(RollDiceCommand command)
        {
            var result = new ClientRolledDiceEvent();
            setBaseEventDetails(result, command);
            return result;
        }

        public void HandleCommand(Session session, SendMessageCommand command)
        {
            var newEvent = createEvent(command);
            storeEvent(session, newEvent);

            //Mappings
            Mapper.CreateMap<BaseClientEvent, BaseClientEventViewModel>()
                  .Include<ClientSentMessageEvent, ClientSentMessageEventViewModel>();
            Mapper.CreateMap<ClientSentMessageEvent, ClientSentMessageEventViewModel>();
           
            var viewModel = Mapper.Map(newEvent, newEvent.GetType(), typeof(ClientSentMessageEventViewModel));
            _clients.Group(session.Id).onNewMessage(viewModel);
        }
        
        public void HandleCommand(Session session, RollDiceCommand command)
        {
            var rollSpec = DiceService.ParseFormula(command.DiceRollFormula);
            var roll = DiceService.RollDice(rollSpec);

            var newEvent = createEvent(command);
            newEvent.Roll = roll;
            storeEvent(session, newEvent);

            //Mappings
            Mapper.CreateMap<BaseClientEvent, BaseClientEventViewModel>()
                  .Include<ClientRolledDiceEvent, ClientRolledDiceEventViewModel>();
            Mapper.CreateMap<ClientRolledDiceEvent, ClientRolledDiceEventViewModel>();
            Mapper.CreateMap<DiceGroupRoll, DiceGroupRollViewModel>();

            var viewModel = Mapper.Map(newEvent, newEvent.GetType(), typeof(ClientRolledDiceEventViewModel));
            _clients.Group(session.Id).onNewMessage(viewModel);
        }

        public IEnumerable<BaseClientEvent> GetSessionEvents(Session session)
        {
            var events = _eventsBySessionId
                .GetOrAdd(session.Id, k => new List<BaseClientEvent>())
                .ToList();

            return events;
        }

        public void AddClient(Client client)
        {
            _clientsById.TryAdd(client.ConnectionId, client);

            _clients.All.onUsersUpdated(GetClients().Count());
        }

        public void RemoveClient(Client client)
        {
            Client clientRemoved;
            _clientsById.TryRemove(client.ConnectionId, out clientRemoved);

            _clients.All.onUsersUpdated(GetClients().Count());
        }

        public IEnumerable<Client> GetClients()
        {
            return _clientsById.Values;
        }

        public Client GetClient(string id)
        {
            Client client;
            _clientsById.TryGetValue(id, out client);
            return client;
        }

        public bool TryGetClient(string id, out Client client)
        {
            return _clientsById.TryGetValue(id, out client);
        }

        public void UpdateClient(Client client)
        {
            _clientsById.TryUpdate(client.ConnectionId, client, client);
        }
    }
}