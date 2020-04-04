using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DS4WinWPF.DS4Forms
{
    /// <summary>
    /// Interaction logic for ColorPickerWindow.xaml
    /// </summary>
    public partial class ColorPickerWindow : Window
    {
        public delegate void ColorChangedHandler(ColorPickerWindow sender, Color color);
        public event ColorChangedHandler ColorChanged;

        public ColorPickerWindow()
        {
            InitializeComponent();
        }

        private void ColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            ColorChanged?.Invoke(this, e.NewValue.GetValueOrDefault());
        }
    }
}
