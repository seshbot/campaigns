using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;

namespace campaigns.Helpers
{
    public static class CharacterSheetHtmlHelper
    {
        static object FORM_LABEL_ATTRIBUTES = new { @class = "control-label col-md-2" };
        static object FORM_EDITOR_ATTRIBUTES = new { @class = "form-control" };
        static object FORM_VALIDATION_MESSAGE_ATTRIBUTES = new { @class = "text-danger" };
        
        private static object Merge(object item1, object item2)
        {
            if (item1 == null || item2 == null)
                return item1 ?? item2 ?? new { };

            dynamic expando = new ExpandoObject();
            var result = expando as IDictionary<string, object>;
            foreach (System.Reflection.PropertyInfo fi in item2.GetType().GetProperties())
            {
                result[fi.Name] = fi.GetValue(item2, null);
            }
            foreach (System.Reflection.PropertyInfo fi in item1.GetType().GetProperties())
            {
                if (!result.ContainsKey(fi.Name))
                    result[fi.Name] = fi.GetValue(item1, null);
            }
            return result;
        }

        public static object CustomLabelAttributes<TModel>(this HtmlHelper<TModel> htmlHelper, object extraAttributes = null)
        {
            return Merge(FORM_LABEL_ATTRIBUTES, extraAttributes);
        }

        public static object CustomEditorAttributes<TModel>(this HtmlHelper<TModel> htmlHelper, object extraAttributes = null)
        {
            return Merge(FORM_EDITOR_ATTRIBUTES, extraAttributes);
        }

        public static object CustomValidationAttributes<TModel>(this HtmlHelper<TModel> htmlHelper, object extraAttributes = null)
        {
            return Merge(FORM_VALIDATION_MESSAGE_ATTRIBUTES, extraAttributes);
        }
    }
}
