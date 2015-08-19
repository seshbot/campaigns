﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace campaigns.Models
{
    public class CharacterSheetDbContext : DbContext
    {
        public DbSet<CharacterSheet> CharacterSheets { get; set; }
        public DbSet<Ability> Abilities { get; set; }
        public DbSet<Skill> Skills { get; set; }
        public DbSet<Race> Races { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<Rules> Rules { get; set; }
    }
}