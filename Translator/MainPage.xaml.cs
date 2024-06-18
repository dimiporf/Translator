using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace Translator
{
    public partial class MainPage : ContentPage
    {
        private HttpClient _httpClient; // HttpClient for API requests
        private bool _isEnglishToGerman = true; // Flag for translation direction
        private int _languageIndex = 0; // Index for rotating through language icons

        // Array of language icons
        private string[] _languageIcons = { "eng.png", "de.png" };

        public MainPage()
        {
            InitializeComponent();
            _httpClient = new HttpClient();
            Title = "Welcome to Translator App";
            UpdateLanguageUI(); // Initialize language icons on UI
        }

        // Event handler for Translate button click
        private async void OnTranslateClicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(InputEditor.Text))
            {
                await TranslateAndDisplayText(); // Call translation method if input is not empty
            }
        }

        // Event handler for Switch Language button click
        private async void OnSwitchLanguageClicked(object sender, EventArgs e)
        {
            _isEnglishToGerman = !_isEnglishToGerman; // Toggle translation direction

            // Increment language index to rotate through language icons
            _languageIndex = (_languageIndex + 1) % _languageIcons.Length;

            UpdateLanguageUI(); // Update UI with new language settings

            // Translate only if there is text in the editor
            if (!string.IsNullOrWhiteSpace(InputEditor.Text))
            {
                await TranslateAndDisplayText(); // Re-translate with new direction
            }
        }

        // Event handler for Editor's Completed event (Enter key press)
        private async void OnEditorCompleted(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(InputEditor.Text))
            {
                await TranslateAndDisplayText(); // Translate text if input is not empty
                InputEditor.Unfocus(); // Dismiss keyboard after translation
            }
        }

        // Method to handle translation and update UI
        private async Task TranslateAndDisplayText()
        {
            string inputText = InputEditor.Text;
            string sourceLanguage, targetLanguage;

            // Determine source and target languages based on translation direction
            if (_isEnglishToGerman)
            {
                sourceLanguage = "en";
                targetLanguage = "de";
            }
            else
            {
                sourceLanguage = "de";
                targetLanguage = "en";
            }

            try
            {
                // Call translation API
                var translatedText = await TranslateTextAsync(inputText, sourceLanguage, targetLanguage);
                TranslatedLabel.Text = translatedText; // Update translated text on UI
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Translation failed: {ex.Message}", "OK"); // Display error message if translation fails
            }
        }

        // Method to call the translation API
        private async Task<string> TranslateTextAsync(string text, string sourceLanguage, string targetLanguage)
        {
            string url = $"https://api.mymemory.translated.net/get?q={Uri.EscapeDataString(text)}&langpair={sourceLanguage}|{targetLanguage}";

            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var translationResult = JsonSerializer.Deserialize<MyMemoryResponse>(jsonResponse);
                return translationResult.responseData.translatedText; // Return translated text from API response
            }
            else
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error: {response.StatusCode}, Details: {errorResponse}"); // Throw exception if API call fails
            }
        }

        // Method to update language UI elements (flags)
        private void UpdateLanguageUI()
        {
            // Update language flags based on current language index
            EnglishFlag.Source = _languageIcons[_languageIndex];
            GermanFlag.Source = _languageIcons[(_languageIndex + 1) % _languageIcons.Length];
        }
    }

    // Classes representing the JSON response from the translation API
    public class MyMemoryResponse
    {
        public ResponseData responseData { get; set; }
    }

    public class ResponseData
    {
        public string translatedText { get; set; }
    }
}
