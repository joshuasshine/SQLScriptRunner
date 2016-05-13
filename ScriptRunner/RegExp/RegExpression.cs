using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ScriptRunner.RegExp
{
    public static class RegExpression
    {
        /// <summary>
        /// This function makes and returns a RegEx object
        /// depending on user input
        /// </summary>
        /// <returns></returns>
        public static Regex GetRegExpression(string txt)
        {
            Regex result;
            String regExString;
            

            // Get what the user entered
            regExString = txt;

            // replace escape characters
            regExString = Regex.Escape(regExString);           

            // Is whole word check box checked?
            result = new Regex(regExString, RegexOptions.IgnoreCase);

            return result;
        }

    }
}
