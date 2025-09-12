namespace Viper.IconBrowser
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    internal class NotNullToVisibleConverter : IValueConverter
    {
        /// <summary>
        /// Converts the null status of a value to a <see cref="Visibility"/> property value.
        /// </summary>
        /// <param name="value">The Boolean value to convert. This value can be a standard Boolean value or a nullable Boolean value.</param>
        /// <param name="targetType">Type: <see cref="Type"/>. <para>This parameter is not used.</para></param>
        /// <param name="parameter">Type: <see cref="object"/>. <para>This parameter is not used.</para></param>
        /// <param name="culture">Type: <see cref="CultureInfo"/>. <para>This parameter is not used.</para></param>
        /// <returns><c>false</c> if <paramref name="value"/> is <c>true</c>; otherwise, <c>true</c>.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null || value is bool b && !b ? Visibility.Collapsed : Visibility.Visible;
        }

        /// <summary>
        /// Converts an inverted <see cref="bool"/> to its original value.
        /// </summary>
        /// <param name="value">The Boolean value to convert. This value can be a standard Boolean value or a nullable Boolean value.</param>
        /// <param name="targetType">Type: <see cref="Type"/>. <para>This parameter is not used.</para></param>
        /// <param name="parameter">Type: <see cref="object"/>. <para>This parameter is not used.</para></param>
        /// <param name="culture">Type: <see cref="CultureInfo"/>. <para>This parameter is not used.</para></param>
        /// <returns><c>false</c> if <paramref name="value"/> is <c>true</c>; otherwise, <c>true</c>.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }
    }
}