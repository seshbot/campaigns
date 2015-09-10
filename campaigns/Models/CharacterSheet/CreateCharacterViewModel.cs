using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campaigns.Models.CharacterSheet
{
    public class CreateCharacterViewModel
    {
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        [Display(Name="Race")]
        public int RaceId { get; set; }
        [Display(Name = "Initial Class")]
        public int InitialClassId { get; set; }
        [Display(Name = "Starting Level")]
        public int Level { get; set; }
    }
}
