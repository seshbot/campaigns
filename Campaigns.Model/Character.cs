using Campaigns.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campaigns.Model
{
    public class Character : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        [ForeignKey("Sheet")]
        public int SheetId { get; set; }
        public virtual CharacterSheet Sheet { get; set; }
    }
}
