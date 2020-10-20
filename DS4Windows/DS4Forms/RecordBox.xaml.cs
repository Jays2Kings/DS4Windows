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
using System.Windows.Navigation;
using System.Windows.Shapes;
using NonFormTimer = System.Timers.Timer;
using Microsoft.Win32;
using Xceed.Wpf.Toolkit;
using DS4WinWPF.DS4Forms.ViewModels;

namespace DS4WinWPF.DS4Forms
{
    /// <summary>
    /// Interaction logic for RecordBox.xaml
    /// </summary>
    public partial class RecordBox : UserControl
    {
        private RecordBoxViewModel recordBoxVM;
        public RecordBoxViewModel RecordBoxVM { get => recordBoxVM; }

        private bool saved;
        public bool Saved { get => saved; }

        public event EventHandler Save;
        public event EventHandler Cancel;

        private ColorPickerWindow colorDialog;
        private NonFormTimer ds4 = new NonFormTimer();

        public RecordBox(int deviceNum, DS4Windows.DS4ControlSettings controlSettings, bool shift, bool showscan = true)
        {
            InitializeComponent();

            recordBoxVM = new RecordBoxViewModel(deviceNum, controlSettings, shift);
            mouseButtonsPanel.Visibility = Visibility.Hidden;
            extraConPanel.Visibility = Visibility.Hidden;
            if (!showscan)
            {
                useScanCode.Visibility = Visibility.Collapsed;
            }

            ds4.Elapsed += Ds4_Tick;
            ds4.Interval = 10;
            DataContext = recordBoxVM;
            SetupLateEvents();
        }

        private void Ds4_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            recordBoxVM.ProcessDS4Tick();
        }

        private void SetupLateEvents()
        {
            macroListBox.SelectionChanged += MacroListBox_SelectionChanged;
            recordBoxVM.MacroSteps.CollectionChanged += MacroSteps_CollectionChanged;
        }

        private void MacroSteps_CollectionChanged(object sender,
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    int count = recordBoxVM.MacroSteps.Count;
                    if (count > 0)
                    {
                        macroListBox.ScrollIntoView(recordBoxVM.MacroSteps[count - 1]);
                    }
                }));
            }
        }

        private void MacroListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!recordBoxVM.Recording)
            {
                if (recordBoxVM.MacroStepIndex >= 0)
                {
                    MacroStepItem item = recordBoxVM.MacroSteps[recordBoxVM.MacroStepIndex];
                    recordBtn.Content = $"Record Before {item.Step.Name}";
                }
                else
                {
                    recordBtn.Content = "Record";
                }

                if (recordBoxVM.EditMacroIndex > -1)
                {
                    UpdateDataRevertTemplate();
                }
            }
        }

        private void MacroListBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!recordBoxVM.Recording)
            {
                recordBtn.Content = "Record";
            }
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (recordBoxVM.EditMacroIndex > -1)
            {
                UpdateDataRevertTemplate();
            }

            saved = true;
            recordBoxVM.ExportMacro();
            Save?.Invoke(this, EventArgs.Empty);
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            Cancel?.Invoke(this, EventArgs.Empty);
        }

        private void RecordBtn_Click(object sender, RoutedEventArgs e)
        {
            bool recording = recordBoxVM.Recording = !recordBoxVM.Recording;
            if (recording)
            {
                DS4Windows.Program.rootHub.recordingMacro = true;
                recordBtn.Content = "Stop";
                if (recordBoxVM.MacroStepIndex == -1)
                {
                    // Don't clear macro steps in RECORD button because nowadays there is a separate CLEAR button. RECORD btn without a selection appends new steps to existing macro sequence
                    //recordBoxVM.MacroSteps.Clear();
                }
                else
                {
                    recordBoxVM.AppendIndex = recordBoxVM.MacroStepIndex;
                }

                mouseButtonsPanel.Visibility = Visibility.Visible;
                if (recordBoxVM.RecordDelays)
                {
                    extraConPanel.Visibility = Visibility.Visible;
                }

                ds4.Start();
                Enable_Controls(false);
                recordBoxVM.Sw.Restart();
                this.Focus();
            }
            else
            {
                DS4Windows.Program.rootHub.recordingMacro = false;
                recordBoxVM.AppendIndex = -1;
                ds4.Stop();
                recordBtn.Content = "Record";
                mouseButtonsPanel.Visibility = Visibility.Hidden;
                extraConPanel.Visibility = Visibility.Hidden;
                recordBoxVM.Sw.Stop();

                if (recordBoxVM.Toggle4thMouse)
                {
                    FourMouseBtnAction();
                }
                if (recordBoxVM.Toggle5thMouse)
                {
                    FiveMouseBtnAction();
                }
                if (recordBoxVM.ToggleLightbar)
                {
                    ChangeLightbarAction();
                }
                if (recordBoxVM.ToggleRumble)
                {
                    ChangeRumbleAction();
                }

                Enable_Controls(true);
            }

            recordBoxVM.EditMacroIndex = -1;
            recordBoxVM.ToggleLightbar = false;
            recordBoxVM.ToggleRumble = false;
            changeLightBtn.Content = "Change Lightbar Color";
            addRumbleBtn.Content = "Add Rumble";
            recordBoxVM.MacroStepIndex = -1;
        }

        private void ClearStepsBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!recordBoxVM.Recording)
            {
                recordBoxVM.MacroStepIndex = -1;
                recordBoxVM.MacroSteps.Clear();
            }
        }

        private void Enable_Controls(bool on)
        {
            macroListBox.IsEnabled = on;
            recordDelaysCk.IsEnabled = on;
            saveBtn.IsEnabled = on;
            cancelBtn.IsEnabled = on;
            loadPresetBtn.IsEnabled = on;
            savePresetBtn.IsEnabled = on;
            macroModeCombo.IsEnabled = on;
            clearStepsBtn.IsEnabled = on;
            addWaitTimeBtn.IsEnabled = on;
        }

        private void ChangeLightbarAction()
        {
            bool light = recordBoxVM.ToggleLightbar = !recordBoxVM.ToggleLightbar;
            if (light)
            {
                changeLightBtn.Content = "Reset Lightbar Color";
                DS4Windows.MacroStep step = new DS4Windows.MacroStep(1255255255, $"Lightbar Color: 255,255,255",
                            DS4Windows.MacroStep.StepType.ActDown, DS4Windows.MacroStep.StepOutput.Lightbar);
                recordBoxVM.AddMacroStep(step);
            }
            else
            {
                changeLightBtn.Content = "Change Lightbar Color";
                DS4Windows.MacroStep step = new DS4Windows.MacroStep(1000000000, $"Reset Lightbar",
                            DS4Windows.MacroStep.StepType.ActUp, DS4Windows.MacroStep.StepOutput.Lightbar);
                recordBoxVM.AddMacroStep(step);
            }
        }

        private void ChangeLightBtn_Click(object sender, RoutedEventArgs e)
        {
            ChangeLightbarAction();
        }

        private void ChangeRumbleAction()
        {
            bool rumble = recordBoxVM.ToggleRumble = !recordBoxVM.ToggleRumble;
            if (rumble)
            {
                addRumbleBtn.Content = "Stop Rumble";
                DS4Windows.MacroStep step = new DS4Windows.MacroStep(1255255, $"Rumble 255,255",
                            DS4Windows.MacroStep.StepType.ActDown, DS4Windows.MacroStep.StepOutput.Rumble);
                recordBoxVM.AddMacroStep(step);
            }
            else
            {
                addRumbleBtn.Content = "Add Rumble";
                DS4Windows.MacroStep step = new DS4Windows.MacroStep(1000000, $"Stop Rumble",
                            DS4Windows.MacroStep.StepType.ActUp, DS4Windows.MacroStep.StepOutput.Rumble);
                recordBoxVM.AddMacroStep(step);
            }
        }

        private void AddRumbleBtn_Click(object sender, RoutedEventArgs e)
        {
            ChangeRumbleAction();
        }

        private void LoadPresetBtn_Click(object sender, RoutedEventArgs e)
        {
            loadPresetBtn.ContextMenu.IsOpen = true;
        }

        private void SavePresetBtn_Click(object sender, RoutedEventArgs e)
        {
            // Reset selected index of macro list before removing item source
            macroListBox.SelectedIndex = -1;

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.AddExtension = true;
            dialog.DefaultExt = ".txt";
            dialog.Filter = "Text Documents (*.txt)|*.txt";
            dialog.Title = "Select Export File";
            dialog.InitialDirectory = $"{DS4Windows.Global.appdatapath}\\Macros";
            if (dialog.ShowDialog() == true)
            {
                //recordBoxVM.MacroSteps.Clear();
                recordBoxVM.SavePreset(dialog.FileName);
            }
        }

        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (recordBoxVM.Recording)
            {
                Key tempKey = e.SystemKey == Key.None ? e.Key : e.SystemKey;
                int value = KeyInterop.VirtualKeyFromKey(tempKey);
                recordBoxVM.KeysdownMap.TryGetValue(value, out bool isdown);
                if (!isdown)
                {
                    DS4Windows.MacroStep step = new DS4Windows.MacroStep(value, tempKey.ToString(),
                            DS4Windows.MacroStep.StepType.ActDown, DS4Windows.MacroStep.StepOutput.Key);
                    recordBoxVM.AddMacroStep(step);
                    recordBoxVM.KeysdownMap.Add(value, true);
                }

                e.Handled = true;
                //Console.WriteLine(e.Key);
                //Console.WriteLine(e.SystemKey);
            }
            else if (e.Key == Key.Delete && recordBoxVM.MacroStepIndex >= 0)
            {
                recordBoxVM.MacroSteps.RemoveAt(recordBoxVM.MacroStepIndex);
                e.Handled = true;
            }
        }

        private void UserControl_KeyUp(object sender, KeyEventArgs e)
        {
            if (recordBoxVM.Recording)
            {
                Key tempKey = e.SystemKey == Key.None ? e.Key : e.SystemKey;
                int value = KeyInterop.VirtualKeyFromKey(tempKey);
                recordBoxVM.KeysdownMap.TryGetValue(value, out bool isdown);
                if (isdown)
                {
                    DS4Windows.MacroStep step = new DS4Windows.MacroStep(value, tempKey.ToString(),
                            DS4Windows.MacroStep.StepType.ActUp, DS4Windows.MacroStep.StepOutput.Key);
                    recordBoxVM.AddMacroStep(step);
                    recordBoxVM.KeysdownMap.Remove(value);
                }
                else if (RecordBoxViewModel.KeydownOverrides.Contains(value))
                {
                    DS4Windows.MacroStep step = new DS4Windows.MacroStep(value, tempKey.ToString(),
                            DS4Windows.MacroStep.StepType.ActDown, DS4Windows.MacroStep.StepOutput.Key);
                    recordBoxVM.AddMacroStep(step, ignoreDelay: true);

                    step = new DS4Windows.MacroStep(value, tempKey.ToString(),
                            DS4Windows.MacroStep.StepType.ActUp, DS4Windows.MacroStep.StepOutput.Key);
                    recordBoxVM.AddMacroStep(step, ignoreDelay: true);
                }

                e.Handled = true;
                //Console.WriteLine(e.Key);
                //Console.WriteLine(e.SystemKey);
            }
        }

        private void MacroListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (recordBoxVM.MacroStepIndex >= 0)
            {
                MacroStepItem item = recordBoxVM.MacroSteps[recordBoxVM.MacroStepIndex];
                if (item.Step.ActType == DS4Windows.MacroStep.StepType.Wait)
                {
                    ListBoxItem lbitem = macroListBox.ItemContainerGenerator.ContainerFromIndex(recordBoxVM.MacroStepIndex)
                        as ListBoxItem;
                    lbitem.ContentTemplate = this.FindResource("EditTemplate") as DataTemplate;
                    recordBoxVM.EditMacroIndex = recordBoxVM.MacroStepIndex;
                }
                else if (item.Step.OutputType == DS4Windows.MacroStep.StepOutput.Rumble &&
                    item.Step.ActType == DS4Windows.MacroStep.StepType.ActDown)
                {
                    ListBoxItem lbitem = macroListBox.ItemContainerGenerator.ContainerFromIndex(recordBoxVM.MacroStepIndex)
                        as ListBoxItem;
                    lbitem.ContentTemplate = this.FindResource("EditRumbleTemplate") as DataTemplate;
                    recordBoxVM.EditMacroIndex = recordBoxVM.MacroStepIndex;
                }
                else if (item.Step.OutputType == DS4Windows.MacroStep.StepOutput.Lightbar &&
                    item.Step.ActType == DS4Windows.MacroStep.StepType.ActDown)
                {
                    colorDialog = new ColorPickerWindow();
                    colorDialog.Owner = Application.Current.MainWindow;
                    Color tempcolor = item.LightbarColorValue();
                    colorDialog.colorPicker.SelectedColor = tempcolor;
                    recordBoxVM.StartForcedColor(tempcolor);
                    colorDialog.ColorChanged += (sender2, color) =>
                    {
                        recordBoxVM.UpdateForcedColor(color);
                    };
                    colorDialog.ShowDialog();
                    recordBoxVM.EndForcedColor();
                    item.UpdateLightbarValue(colorDialog.colorPicker.SelectedColor.GetValueOrDefault());

                    FocusNavigationDirection focusDirection = FocusNavigationDirection.Next;
                    TraversalRequest request = new TraversalRequest(focusDirection);
                    UIElement elementWithFocus = Keyboard.FocusedElement as UIElement;
                    elementWithFocus?.MoveFocus(request);
                }
            }
        }

        private void RevertListItemTemplate()
        {
            if (recordBoxVM.EditMacroIndex >= 0)
            {
                ListBoxItem lbitem = macroListBox.ItemContainerGenerator.ContainerFromIndex(recordBoxVM.EditMacroIndex)
                        as ListBoxItem;
                lbitem.ContentTemplate = this.FindResource("DisplayTemplate") as DataTemplate;
                recordBoxVM.EditMacroIndex = -1;
            }
        }

        private void UpdateDataRevertTemplate()
        {
            ListBoxItem lbitem = macroListBox.ItemContainerGenerator.ContainerFromIndex(recordBoxVM.EditMacroIndex)
                        as ListBoxItem;
            ContentPresenter contentPresenter = UtilMethods.FindVisualChild<ContentPresenter>(lbitem);
            DataTemplate oldDataTemplate = contentPresenter.ContentTemplate;

            MacroStepItem item = recordBoxVM.MacroSteps[recordBoxVM.EditMacroIndex];
            if (item.Step.ActType == DS4Windows.MacroStep.StepType.Wait)
            {
                IntegerUpDown integerUpDown = oldDataTemplate.FindName("waitIUD", contentPresenter) as IntegerUpDown;
                if (integerUpDown != null)
                {
                    BindingExpression bindExp = integerUpDown.GetBindingExpression(IntegerUpDown.ValueProperty);
                    bindExp.UpdateSource();
                }
            }
            else if (item.Step.OutputType == DS4Windows.MacroStep.StepOutput.Rumble)
            {
                IntegerUpDown heavyRumble = oldDataTemplate.FindName("heavyRumbleUD", contentPresenter) as IntegerUpDown;
                IntegerUpDown lightRumble = oldDataTemplate.FindName("lightRumbleUD", contentPresenter) as IntegerUpDown;
                if (heavyRumble != null && lightRumble != null)
                {
                    BindingExpression bindExp = heavyRumble.GetBindingExpression(IntegerUpDown.ValueProperty);
                    bindExp.UpdateSource();

                    bindExp = lightRumble.GetBindingExpression(IntegerUpDown.ValueProperty);
                    bindExp.UpdateSource();
                }
            }

            lbitem.ContentTemplate = this.FindResource("DisplayTemplate") as DataTemplate;
            recordBoxVM.EditMacroIndex = -1;
        }

        private void CycleProgPresetMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Reset selected index of macro list before removing item source
            macroListBox.SelectedIndex = -1;

            macroListBox.ItemsSource = null;
            recordBoxVM.MacroSteps.Clear();
            recordBoxVM.WriteCycleProgramsPreset();
            macroListBox.ItemsSource = recordBoxVM.MacroSteps;
        }

        private void LoadPresetFromFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Reset selected index of macro list before removing item source
            macroListBox.SelectedIndex = -1;

            macroListBox.ItemsSource = null;
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.AddExtension = true;
            dialog.DefaultExt = ".txt";
            dialog.Filter = "Text Documents (*.txt)|*.txt";
            dialog.Title = "Select Preset File";
            dialog.InitialDirectory = $"{DS4Windows.Global.appdatapath}\\Macros";
            if (dialog.ShowDialog() == true)
            {
                recordBoxVM.MacroSteps.Clear();
                recordBoxVM.LoadPresetFromFile(dialog.FileName);
            }

            macroListBox.ItemsSource = recordBoxVM.MacroSteps;
        }

        private void WaitIUD_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                IntegerUpDown integerUpDown = sender as IntegerUpDown;
                BindingExpression bindExp = integerUpDown.GetBindingExpression(IntegerUpDown.ValueProperty);
                bindExp.UpdateSource();

                ListBoxItem lbitem = macroListBox.ItemContainerGenerator.ContainerFromIndex(recordBoxVM.EditMacroIndex)
                        as ListBoxItem;
                lbitem.ContentTemplate = this.FindResource("DisplayTemplate") as DataTemplate;
                recordBoxVM.EditMacroIndex = -1;
            }
        }

        private void FourMouseBtnAction()
        {
            int value = 259;
            recordBoxVM.KeysdownMap.TryGetValue(value, out bool isdown);
            if (!isdown)
            {
                DS4Windows.MacroStep step = new DS4Windows.MacroStep(value, DS4Windows.MacroParser.macroInputNames[value],
                            DS4Windows.MacroStep.StepType.ActDown, DS4Windows.MacroStep.StepOutput.Button);
                recordBoxVM.AddMacroStep(step);
                recordBoxVM.KeysdownMap.Add(value, true);
            }
            else
            {
                DS4Windows.MacroStep step = new DS4Windows.MacroStep(value, DS4Windows.MacroParser.macroInputNames[value],
                            DS4Windows.MacroStep.StepType.ActUp, DS4Windows.MacroStep.StepOutput.Button);
                recordBoxVM.AddMacroStep(step);
                recordBoxVM.KeysdownMap.Remove(value);
            }
        }

        private void FourMouseBtn_Click(object sender, RoutedEventArgs e)
        {
            FourMouseBtnAction();
        }

        private void FiveMouseBtnAction()
        {
            int value = 260;
            recordBoxVM.KeysdownMap.TryGetValue(value, out bool isdown);
            if (!isdown)
            {
                DS4Windows.MacroStep step = new DS4Windows.MacroStep(value, DS4Windows.MacroParser.macroInputNames[value],
                            DS4Windows.MacroStep.StepType.ActDown, DS4Windows.MacroStep.StepOutput.Button);
                recordBoxVM.AddMacroStep(step);
                recordBoxVM.KeysdownMap.Add(value, true);
            }
            else
            {
                DS4Windows.MacroStep step = new DS4Windows.MacroStep(value, DS4Windows.MacroParser.macroInputNames[value],
                            DS4Windows.MacroStep.StepType.ActUp, DS4Windows.MacroStep.StepOutput.Button);
                recordBoxVM.AddMacroStep(step);
                recordBoxVM.KeysdownMap.Remove(value);
            }
        }

        private void FiveMouseBtn_Click(object sender, RoutedEventArgs e)
        {
            FiveMouseBtnAction();
        }

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (recordBoxVM.Recording)
            {
                int value;
                switch (e.ChangedButton)
                {
                    case MouseButton.Left: value = 256; break;
                    case MouseButton.Right: value = 257; break;
                    case MouseButton.Middle: value = 258; break;
                    case MouseButton.XButton1: value = 259; break;
                    case MouseButton.XButton2: value = 260; break;
                    default: value = 0; break;
                }

                DS4Windows.MacroStep step = new DS4Windows.MacroStep(value, DS4Windows.MacroParser.macroInputNames[value],
                            DS4Windows.MacroStep.StepType.ActDown, DS4Windows.MacroStep.StepOutput.Button);
                recordBoxVM.AddMacroStep(step);
                recordBoxVM.KeysdownMap.Add(value, true);
                e.Handled = true;
            }
        }

        private void UserControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (recordBoxVM.Recording)
            {
                int value;
                switch (e.ChangedButton)
                {
                    case MouseButton.Left: value = 256; break;
                    case MouseButton.Right: value = 257; break;
                    case MouseButton.Middle: value = 258; break;
                    case MouseButton.XButton1: value = 259; break;
                    case MouseButton.XButton2: value = 260; break;
                    default: value = 0; break;
                }

                DS4Windows.MacroStep step = new DS4Windows.MacroStep(value, DS4Windows.MacroParser.macroInputNames[value],
                            DS4Windows.MacroStep.StepType.ActUp, DS4Windows.MacroStep.StepOutput.Button);
                recordBoxVM.AddMacroStep(step);
                recordBoxVM.KeysdownMap.Remove(value);
                e.Handled = true;
            }
        }

        private void AddWaitTimeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (recordBoxVM.MacroStepIndex >= 0)
            {
                DS4Windows.MacroStep step = new DS4Windows.MacroStep(400, "Wait 100ms",
                            DS4Windows.MacroStep.StepType.Wait, DS4Windows.MacroStep.StepOutput.None);
                recordBoxVM.InsertMacroStep(recordBoxVM.MacroStepIndex, step);
            }
        }
    }
}
