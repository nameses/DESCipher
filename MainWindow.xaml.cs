using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;
using Windows.Storage;
using WinRT.Interop;
using Cipher;
using Windows.Globalization;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DESCipher
{
    public sealed partial class MainWindow : Window
    {
        private string OriginalTextValue { get; set; } = string.Empty;
        private string ProcessedTextValue { get; set; } = string.Empty;
        private string Key { get; set; } = string.Empty;
        private string IV { get; set; } = string.Empty;

        private ObservableCollection<string> ComboBoxItems { get; set; } = new ObservableCollection<string>();

        private ICipher _cipherInstance;
        private CipherType _lastSelectedItem;

        public MainWindow()
        {
            this.InitializeComponent();

            OriginalTextBox.DataContext = OriginalTextValue;
            OriginalTextBox.SetBinding(TextBox.TextProperty, new Binding { Source = OriginalTextValue });

            ProcessedTextBox.DataContext = ProcessedTextValue;
            ProcessedTextBox.SetBinding(TextBox.TextProperty, new Binding { Source = ProcessedTextValue });

            KeyTextBox.DataContext = Key;
            KeyTextBox.SetBinding(TextBox.TextProperty, new Binding { Source = Key });

            IVTextBox.DataContext = IV;
            IVTextBox.SetBinding(TextBox.TextProperty, new Binding { Source = IV });

            ComboBoxItems.Add("DES");
            ComboBoxItems.Add("TripleDES");
            ComboBoxItems.Add("AES");

            CipherChoiceComboBox.ItemsSource = ComboBoxItems;
            CipherChoiceComboBox.SelectionChanged += (s, e) =>
            {
                var selectedItem = CipherChoiceComboBox.SelectedItem as string;
                if(Enum.TryParse<CipherType>(selectedItem, out var result))
                {
                    _cipherInstance = createCipherInstance(result);
                    _lastSelectedItem = result; 
                }
            };
        }

        private ICipher createCipherInstance(CipherType cipherType) => CipherFactory.CreateCipher(cipherType,
            KeyTextBox.GetValue(TextBox.TextProperty).ToString(),
            IVTextBox.GetValue(TextBox.TextProperty).ToString());

        private void ClearOriginalText_Click(object sender, RoutedEventArgs e)
        {
            OriginalTextBox.ClearValue(TextBox.TextProperty);
        }

        private void ClearProcessedText_Click(object sender, RoutedEventArgs e)
        {
            ProcessedTextBox.ClearValue(TextBox.TextProperty);
        }

        private void Encrypt_Click(object sender, RoutedEventArgs e)
        {
            ProcessText(_cipherInstance.EncryptText);
        }

        private void Decrypt_Click(object sender, RoutedEventArgs e)
        {
            ProcessText(_cipherInstance.DecryptText);
        }

        private void ProcessText(Func<string, string> processFunction)
        {
            ProcessedTextBox.ClearValue(TextBox.TextProperty);
            var originalText = OriginalTextBox.GetValue(TextBox.TextProperty).ToString();

            var processedText = processFunction(originalText);

            ProcessedTextBox.SetValue(TextBox.TextProperty, string.IsNullOrEmpty(processedText) ? "Не вірно задані параметри шифрування" : processedText);
        }


#region file processing
        private async void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            var stringFileContent = await readFileAsync();

            if (string.IsNullOrEmpty(stringFileContent))
            {
                return;
            }

            OriginalTextBox.SetValue(TextBox.TextProperty, stringFileContent);
        }

        private async void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            await saveFileAsync(ProcessedTextBox.GetValue(TextBox.TextProperty).ToString());
        }

        private async Task saveFileAsync(string fileContent)
        {
            // Створення діалогу для вибору файлу
            var savePicker = new FileSavePicker()
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                FileTypeChoices = { { "Текстовий файл", new List<string>() { ".txt" } } },
                SuggestedFileName = "Нове_ім'я"
            };

            var handledWindow = WindowNative.GetWindowHandle(App.Window);
            InitializeWithWindow.Initialize(savePicker, handledWindow);

            StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                // Збереження вмісту файлу
                await FileIO.WriteTextAsync(file, fileContent);
            }
        }

        private async Task<string> readFileAsync()
        {
            var openPicker = new FileOpenPicker()
            {
                SuggestedStartLocation = PickerLocationId.Downloads,
                FileTypeFilter = { ".txt" }
            };

            var handledWindow = WindowNative.GetWindowHandle(App.Window);
            InitializeWithWindow.Initialize(openPicker, handledWindow);

            var file = await openPicker.PickSingleFileAsync();

            if (file == null)
            {
                return string.Empty;
            }

            return await FileIO.ReadTextAsync(file);
        }
        #endregion

        private void UpdateInstance_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            if(_lastSelectedItem != CipherType.None)
            {
                _cipherInstance = createCipherInstance(_lastSelectedItem);
            }
        }
    }
}