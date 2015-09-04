using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campaigns.Models.Sessions
{
    public class BaseCommand
    {
        public int Id { get; set; }
        public Client Sender { get; set; }
        public DateTime TimeStamp { get; set; }
    }

    public class SendMessageCommand : BaseCommand
    {
        public string Text { get; set; }
    }

    public class RollDiceCommand : BaseCommand
    {
        public string DiceRollFormula { get; set; }
    }
}
