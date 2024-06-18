﻿using System;
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
            UpdateLanguageUI(); // Initialize language icons
        }

        // Event handler for Translate button click
        private async void OnTranslateClicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(InputEditor.Text))
            {
                await TranslateAndDisplayText();
            }
        }

        // Event handler for Switch Language button click
        private async void OnSwitchLanguageClicked(object sender, EventArgs e)
        {
            _isEnglishToGerman = !_isEnglishToGerman; // Toggle translation direction
            _languageIndex = (_languageIndex + 1) % _languageIcons.Length; // Rotate through language icons
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
                await TranslateAndDisplayText();
                InputEditor.Unfocus(); // Dismiss keyboard
            }
        }

        // Method to handle translation and update UI
        private async Task TranslateAndDisplayText()
        {
            string inputText = InputEditor.Text;

            string sourceLanguage, targetLanguage;
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
                var translatedText = await TranslateTextAsync(inputText, sourceLanguage, targetLanguage);
                TranslatedLabel.Text = translatedText;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Translation failed: {ex.Message}", "OK");
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
                return translationResult.responseData.translatedText;
            }
            else
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error: {response.StatusCode}, Details: {errorResponse}");
            }
        }

        // Method to update language UI elements (flags)
        private void UpdateLanguageUI()
        {
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
