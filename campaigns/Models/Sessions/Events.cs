using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campaigns.Models.Sessions
{
    public class BaseClientEvent
    {
        public int Id { get; set; }
        public Client Sender { get; set; }
        public DateTime TimeStamp { get; set; }
    }

    public class ClientSentMessageEvent : BaseClientEvent
    {
        public string Text { get; set; }
    }

    public class ClientRolledDiceEvent : BaseClientEvent
    {
        public Roll Roll { get; set; }
    }
}
