namespace Viper.IconBrowser
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Data;

    public class CaseInsensitiveStringCompareConverter : IValueConverter, IMultiValueConverter
    {
        /// <summary>
        /// Compares two <see cref="string"/> values for equality, ignoring case.
        /// </summary>
        /// <param name="value">The first <see cref="string"/> value to compare.</param>
        /// <param name="targetType">Type: <see cref="Type"/>. <para>This parameter is not used.</para></param>
        /// <param name="parameter">The second <see cref="string"/> value to compare.</param>
        /// <param name="culture">Type: <see cref="CultureInfo"/>. <para>This parameter is not used.</para></param>
        /// <returns><see cref="bool"/><see langword="true"/> if the strings are equal, otherwise, <see langword="false"/>.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string first && parameter is string second)
            {
                return first.Equals(second, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Compares two bound <see cref="string"/> values for equality, ignoring case.
        /// </summary>
        /// <param name="values">The collection of values to compare.</param>
        /// <param name="targetType">The Type.<para>This parameter is not used.</para></param>
        /// <param name="parameter">Optional <see langword="bool"/> when used with more than two <paramref name="values"/> to only test for 'Any' matches, rather than 'All'.</param>
        /// <param name="culture">The Culture.<para>This parameter is not used.</para></param>
        /// <returns><see cref="bool"/><see langword="true"/> if the strings are equal, otherwise, <see langword="false"/>.</returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return parameter is bool anyStringsMatch && anyStringsMatch ? values.Distinct().Count() < values.Length : values.Distinct().Count() == 1;
        }

        /// <summary>
        /// This method is not supported.
        /// </summary>
        /// <param name="value">The value.<para>This parameter is not used.</para></param>
        /// <param name="targetTypes">The Target Types.<para>This parameter is not used.</para></param>
        /// <param name="parameter">The Parameter.<para>This parameter is not used.</para></param>
        /// <param name="culture">The Culture.<para>This parameter is not used.</para></param>
        /// <exception cref="NotImplementedException">If this method is called.</exception>
        /// <returns>Nothing will be returned.</returns>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This method is not supported.
        /// </summary>
        /// <param name="value">The value.<para>This parameter is not used.</para></param>
        /// <param name="targetType">The Target Type.<para>This parameter is not used.</para></param>
        /// <param name="parameter">The Parameter.<para>This parameter is not used.</para></param>
        /// <param name="culture">The Culture.<para>This parameter is not used.</para></param>
        /// <exception cref="NotImplementedException">If this method is called.</exception>
        /// <returns>Nothing will be returned.</returns>
        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}