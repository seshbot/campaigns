using Services.Rules.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Campaigns.Models.API
{
    public static class ViewModelHelpers
    {
        public static AttributeViewModel AttribViewModel(this RulesDbContext db, int id)
        {
            var attrib = db.Attributes.FirstOrDefault(a => a.Id == id);
            if (null == attrib)
            {
                return null;
            }

            return new AttributeViewModel
            {
                Attribute = attrib,
            };
        }
    }
}
