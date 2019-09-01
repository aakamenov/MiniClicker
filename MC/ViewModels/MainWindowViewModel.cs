using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Gma.System.MouseKeyHook;
using MC.Commands;

namespace MC.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        public ICommand TextBoxShortcutClickCommand { get; }
        public ICommand TextBoxShortcutButtonPressedCommand { get; }
        public ICommand TextBoxShortcutLostFocusCommand { get; }

        public string TextBoxShortcutText
        {
            get => textBoxShortcutText;
            set
            {
                textBoxShortcutText = value;
                NotifyPropertyChanged();
            }
        }

        public string TextBoxIntervalText
        {
            get => textBoxIntervalText;
            set
            {
                if (IsInputNumeric(value))
                {
                    textBoxIntervalText = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string LabelIntervalText
        {
            get => labelIntervalText;
            set
            {
                labelIntervalText = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsRunning
        {
            get => isRunning;
            set
            {
                isRunning = value;
                NotifyPropertyChanged();
            }
        }

        private string textBoxShortcutText;
        private string textBoxIntervalText;
        private string labelIntervalText;

        private const int MaxInterval = 10000;
        private const int DefaultInterval = 1000;
        private const int MinInterval = 50;

        private bool isRunning;
        private bool isShortcutPending;
        private IKeyboardMouseEvents hook;
        private Key shortcutKey;

        public MainWindowViewModel()
        {
            shortcutKey = Key.F1;
            TextBoxShortcutText = shortcutKey.ToString();
            TextBoxIntervalText = DefaultInterval.ToString();
            LabelIntervalText = $"Click interval in milliseconds (Min: {MinInterval}, Max: {MaxInterval})";
            
            TextBoxShortcutClickCommand = new RelayCommand<MouseButtonEventArgs>(TextBoxShortcut_PreviewMouseDown);
            TextBoxShortcutButtonPressedCommand = new RelayCommand<System.Windows.Input.KeyEventArgs>(TextBoxShortcut_ButtonPressed);
            TextBoxShortcutLostFocusCommand = new RelayCommand<RoutedEventArgs>(TextBoxShortcut_LostFocus);
            
            SetupKeyHook();
        }

        private bool IsInputNumeric(string input)
        {
            foreach(var c in input)
            {
                if (!char.IsDigit(c))
                    return false;
            }

            return true;
        }

        private void SetupKeyHook()
        {
            if (hook != null)
                hook.Dispose();

            //The MouseKeyHook library uses the Forms "Keys" enum so we have to convert first
            var formsKey = (Keys)KeyInterop.VirtualKeyFromKey(shortcutKey);
            var triggerClicks = Combination.TriggeredBy(formsKey).With(Keys.ControlKey);

            var assignments = new Dictionary<Combination, Action>() { { triggerClicks, PerformClicks } };

            hook = Hook.GlobalEvents();
            hook.OnCombination(assignments);
        }

        private async void PerformClicks()
        {
            if(IsRunning)
            {
                IsRunning = false;
                return;
            }

            if (int.TryParse(TextBoxIntervalText, out int interval))
            {
                if (interval < MinInterval)
                {
                    interval = MinInterval;
                    TextBoxIntervalText = interval.ToString();
                }
                else if (interval > MaxInterval)
                {
                    interval = MaxInterval;
                    TextBoxIntervalText = interval.ToString();
                }
            }
            else
                interval = DefaultInterval;

            IsRunning = true;

            while(isRunning)
            {
                Mouse.Click();
                await Task.Delay(interval);
            }
        }

        private void TextBoxShortcut_PreviewMouseDown(MouseButtonEventArgs args)
        {
            isShortcutPending = true;
            TextBoxShortcutText = "Press any key";
        }

        private void TextBoxShortcut_ButtonPressed(System.Windows.Input.KeyEventArgs args)
        {
            if(isShortcutPending)
            {
                var forbiddenKeys = new Key[] { Key.LeftCtrl, Key.RightCtrl };

                if (forbiddenKeys.Contains(args.Key))
                {
                    ResetCurrentShortcutKey();
                    return;
                }

                shortcutKey = args.Key;
                TextBoxShortcutText = shortcutKey.ToString();

                SetupKeyHook();
            }
        }

        private void TextBoxShortcut_LostFocus(RoutedEventArgs args)
        {
            if(isShortcutPending)
                ResetCurrentShortcutKey();
        }

        private void ResetCurrentShortcutKey()
        {
            TextBoxShortcutText = shortcutKey.ToString();
        }
    }
}
