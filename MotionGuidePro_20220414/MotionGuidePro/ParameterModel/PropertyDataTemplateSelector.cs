using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Data;
using System.Globalization;
using CrevisLibrary;

namespace CrevisLibrary
{
    #region - shseol0618: 컨트롤 짤렸을 때 쓰임 :  MathConverter => <local:MathConverter x:Key="MathConverter" />
    // Does a math equation on the bound value.
    // Use @VALUE in your mathEquation as a substitute for bound value
    // Operator order is parenthesis first, then Left-To-Right (no operator precedence)
    public class MathConverter : IValueConverter
    {
        private static readonly char[] _allOperators = new[] { '+', '-', '*', '/', '%', '(', ')' };

        private static readonly List<String> _grouping = new List<String> { "(", ")" };
        private static readonly List<String> _operators = new List<String> { "+", "-", "*", "/", "%" };

        #region IValueConverter Members

        public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            // Parse value into equation and remove spaces
            var mathEquation = parameter as String;
            mathEquation = mathEquation.Replace(" ", "");
            mathEquation = mathEquation.Replace("@VALUE", value.ToString());

            // Validate values and get list of numbers in equation
            var numbers = new List<double>();
            double tmp;

            foreach (String s in mathEquation.Split(_allOperators))
            {
                if (s != String.Empty)
                {
                    if (double.TryParse(s, out tmp))
                    {
                        numbers.Add(tmp);
                    }
                    else
                    {
                        // Handle Error - Some non-numeric, operator, or grouping character found in String
                        throw new InvalidCastException();
                    }
                }
            }

            // Begin parsing method
            EvaluateMathString(ref mathEquation, ref numbers, 0);

            // After parsing the numbers list should only have one value - the total
            return numbers[0];
        }

        public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion

        // Evaluates a mathematical String and keeps track of the results in a List<double> of numbers
        private void EvaluateMathString(ref String mathEquation, ref List<double> numbers, int index)
        {
            // Loop through each mathemtaical token in the equation
            String token = GetNextToken(mathEquation);

            while (token != String.Empty)
            {
                // Remove token from mathEquation
                mathEquation = mathEquation.Remove(0, token.Length);

                // If token is a grouping character, it affects program flow
                if (_grouping.Contains(token))
                {
                    switch (token)
                    {
                        case "(":
                            EvaluateMathString(ref mathEquation, ref numbers, index);
                            break;

                        case ")":
                            return;
                    }
                }

                // If token is an operator, do requested operation
                if (_operators.Contains(token))
                {
                    // If next token after operator is a parenthesis, call method recursively
                    String nextToken = GetNextToken(mathEquation);
                    if (nextToken == "(")
                    {
                        EvaluateMathString(ref mathEquation, ref numbers, index + 1);
                    }

                    // Verify that enough numbers exist in the List<double> to complete the operation
                    // and that the next token is either the number expected, or it was a ( meaning
                    // that this was called recursively and that the number changed
                    if (numbers.Count > (index + 1) &&
                        (double.Parse(nextToken) == numbers[index + 1] || nextToken == "("))
                    {
                        switch (token)
                        {
                            case "+":
                                numbers[index] = numbers[index] + numbers[index + 1];
                                break;
                            case "-":
                                numbers[index] = numbers[index] - numbers[index + 1];
                                break;
                            case "*":
                                numbers[index] = numbers[index] * numbers[index + 1];
                                break;
                            case "/":
                                numbers[index] = numbers[index] / numbers[index + 1];
                                break;
                            case "%":
                                numbers[index] = numbers[index] % numbers[index + 1];
                                break;
                        }
                        numbers.RemoveAt(index + 1);
                    }
                    else
                    {
                        // Handle Error - Next token is not the expected number
                        throw new FormatException("Next token is not the expected number");
                    }
                }

                token = GetNextToken(mathEquation);
            }
        }

        // Gets the next mathematical token in the equation
        private String GetNextToken(String mathEquation)
        {
            // If we're at the end of the equation, return String.empty
            if (mathEquation == String.Empty)
            {
                return String.Empty;
            }

            // Get next operator or numeric value in equation and return it
            String tmp = "";
            foreach (char c in mathEquation)
            {
                if (_allOperators.Contains(c))
                {
                    return (tmp == "" ? c.ToString() : tmp);
                }
                else
                {
                    tmp += c;
                }
            }

            return tmp;
        }
    }
    #endregion

    internal class RegImageConverter : IValueConverter
    {
        public Object Convert(Object o, Type type, Object parameter, CultureInfo culture)
        {
            if (o is DevParam)
            {
                if (((DevParam)o).ParamInfo.ParamType == ParamType.Category)
                { 
                    //return "/Images/category.png";
                    return "/Images/Category_New.ico";
                }
                else
                {
                    //return "/Images/parameter.png";
                    return "/Images/Parameter_New.ico";
                }
            }
            return null;
        }

        public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PropertyDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate CategoryDataTemplate        { get; set; }
        public DataTemplate StringDataTemplate          { get; set; }
        public DataTemplate BooleanDataTemplate         { get; set; }
        public DataTemplate EnumDataTemplate            { get; set; }
        public DataTemplate ButtonDataTemplate          { get; set; }

        public DataTemplate IntegerPureDataTemplate     { get; set; }
        public DataTemplate IntegerHexDataTemplate      { get; set; }
        public DataTemplate IntegerLinearDataTemplate   { get; set; }

        public DataTemplate FloatDataTemplate           { get; set; }
        public DataTemplate EnumPopupDataTemplate       { get; set; }

        public DataTemplate EnumDataReadOnlyTemplate    { get; set; }

        public DataTemplate ShortPureDataTemplate       { get; set; }
        public DataTemplate ShortHexDataTemplate        { get; set; }
        public DataTemplate ShortLienarDataTemplate     { get; set; }

        public DataTemplate ByteDataTemplate            { get; set; }
        public DataTemplate ByteArrayDataTemplate       { get; set; }

        public DataTemplate FileSelectTemplate { get; set; }

        public DataTemplate TextModifyTemplate { get; set; }

        public override DataTemplate SelectTemplate(Object item, DependencyObject container)
        {
            DevParam devParam = item as DevParam;

            if (devParam.ParamInfo.ParamType == ParamType.Category)
            {
                return CategoryDataTemplate;
            }
            else if (devParam.ParamInfo.ParamType == ParamType.Integer)
            {
                if (devParam.Representation == ParamRepresentation.PureNumber)
                {
                    return IntegerPureDataTemplate;
                }
                else if (devParam.Representation == ParamRepresentation.HexNumber)
                {
                    return IntegerHexDataTemplate;
                }
                else if (devParam.Representation == ParamRepresentation.Linear)
                {
                    return IntegerLinearDataTemplate;
                }
                return IntegerPureDataTemplate;
            }
            else if (devParam.ParamInfo.ParamType == ParamType.String)
            {
                return StringDataTemplate;
            }
            else if (devParam.ParamInfo.ParamType == ParamType.Command)
            {
                return ButtonDataTemplate;
            }
            else if (devParam.ParamInfo.ParamType == ParamType.Enum)
            {
                if (devParam.Representation == ParamRepresentation.PopupCombo)
                {
                    return EnumPopupDataTemplate;
                }
                if (devParam.AccessMode == AccessMode.ReadOnly &&
                    devParam.Representation != ParamRepresentation.PopupCombo)
                    return EnumDataReadOnlyTemplate; 

                return EnumDataTemplate;
            }
            else if (devParam.ParamInfo.ParamType == ParamType.Float)
            {
                return FloatDataTemplate;
            }
            else if (devParam.ParamInfo.ParamType == ParamType.Boolean)
            {
                return BooleanDataTemplate;
            }
            else if (devParam.ParamInfo.ParamType == ParamType.Short)
            {
                if (devParam.Representation == ParamRepresentation.PureNumber)
                {
                    return ShortPureDataTemplate;
                }
                else if (devParam.Representation == ParamRepresentation.HexNumber)
                {
                    return ShortHexDataTemplate;
                }
                else if (devParam.Representation == ParamRepresentation.Linear)
                {
                    return ShortLienarDataTemplate;
                }
                return ShortPureDataTemplate;
            }
            else if (devParam.ParamInfo.ParamType == ParamType.Byte)
            {
                return ByteDataTemplate;
            }
            else if (devParam.ParamInfo.ParamType == ParamType.ByteArray)
            {
                return ByteArrayDataTemplate;
            }
            else if (devParam.ParamInfo.ParamType == ParamType.FileSelect)
            {
                return FileSelectTemplate;
            }
            else if (devParam.ParamInfo.ParamType == ParamType.TextModify)
            {
                return TextModifyTemplate;
            }
            return StringDataTemplate;
        }
    }
}
