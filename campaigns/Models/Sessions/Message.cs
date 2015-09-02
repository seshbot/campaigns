using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campaigns.Models.Sessions
{
    public class Message
    {
        public int Id { get; set; }
        public User Sender { get; set; }
        public string Text { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
