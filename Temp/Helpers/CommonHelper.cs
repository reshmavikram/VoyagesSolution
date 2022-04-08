using Data.Solution.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Data.Solution.Helpers
{
    public static class CommonHelper
    {
        public static string GetRequiredFieldValidations(Type type)
        {
            StringBuilder sb = new StringBuilder();
            var properties = type.GetProperties().Where(prop => prop.IsDefined(typeof(RequiredAttribute), false));
            foreach (var prop in properties)
            {
                var attr = prop.GetCustomAttributes(typeof(DisplayAttribute), true).FirstOrDefault() as DisplayAttribute;
                sb.Append(attr.Name + ", ");
            }
            return string.Format(MessagesResource.RequiredField, sb.ToString().Substring(0, sb.Length - 2));
        }
    }
}
