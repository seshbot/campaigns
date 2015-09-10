using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campaigns.Models.CharacterSheet
{
    public class ViewEditCharacterViewModel
    {
        public bool IsEditing { get; set; }
        public CharacterViewModel Character { get; set; }
    }
}
