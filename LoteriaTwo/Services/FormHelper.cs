using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace LoteriaTwo.Services
{
    public static class FormHelper
    {
        public static string Gv(this Dictionary<string, string> d, string key)
            => d.TryGetValue(key, out var v) ? v : string.Empty;

        public static void RestoreCombo(ComboBox cmb, string value)
        {
            if (string.IsNullOrEmpty(value)) return;
            foreach (var item in cmb.Items)
            {
                var text = item is ComboBoxItem cbi
                    ? cbi.Content?.ToString()
                    : item?.ToString();
                if (text == value) { cmb.SelectedItem = item; return; }
            }
        }

        public static void SetCheckedRadio(DependencyObject root, string groupName, string value)
        {
            if (!string.IsNullOrEmpty(value))
                FindAndCheck(root, groupName, value);
        }

        private static bool FindAndCheck(DependencyObject root, string groupName, string value)
        {
            if (root is RadioButton rb && rb.GroupName == groupName && rb.Content?.ToString() == value)
            {
                rb.IsChecked = true;
                return true;
            }
            foreach (object child in LogicalTreeHelper.GetChildren(root))
                if (child is DependencyObject dep && FindAndCheck(dep, groupName, value))
                    return true;
            return false;
        }
    }
}
