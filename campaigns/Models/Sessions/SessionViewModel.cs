using System;
using System.Collections.Generic;

namespace Campaigns.Models.Sessions
{
    public class SessionViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool IsPublic { get; set; }
        public DateTime StartTime { get; set; }
        public TimeSpan TimeRunning { get { return DateTime.UtcNow - StartTime; } }
    }
}
