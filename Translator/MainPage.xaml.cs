using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace Translator
{
    public partial class MainPage : ContentPage
    {
        // HttpClient for making web requests
        private readonly HttpClient _httpClient;

        public MainPage()
        {
            InitializeComponent();
            // Initialize the HttpClient
            _httpClient = new HttpClient();
        }

        // Event handler for the Translate button click
        private async void OnTranslateClicked(object sender, EventArgs e)
        {
            // Perform translation and update UI
            await TranslateAndDisplayText();
        }

        // Event handler for the Editor's Completed event (triggered by pressing Enter key)
        private async void OnEditorCompleted(object sender, EventArgs e)
        {
            // Perform translation and update UI
            await TranslateAndDisplayText();
            // Dismiss the keyboard
            InputEditor.Unfocus();
        }

        // Method to perform translation and update the translated text label
        private async Task TranslateAndDisplayText()
        {
            // Get the text from the Editor
            string inputText = InputEditor.Text;

            // Check if input text is not empty or whitespace
            if (string.IsNullOrWhiteSpace(inputText))
            {
                await DisplayAlert("Error", "Please enter some text to translate.", "OK");
                return;
            }

            try
            {
                // Translate text from English to German
                var translatedText = await TranslateTextAsync(inputText, "en", "de");
                // Update the Label with the translated text
                TranslatedLabel.Text = translatedText;
            }
            catch (Exception ex)
            {
                // Display an error message if translation fails
                await DisplayAlert("Error", $"Translation failed: {ex.Message}", "OK");
            }
        }

        // Method to translate text using MyMemory API
        private async Task<string> TranslateTextAsync(string text, string sourceLanguage, string targetLanguage)
        {
            // Construct the API request URL
            string url = $"https://api.mymemory.translated.net/get?q={Uri.EscapeDataString(text)}&langpair={sourceLanguage}|{targetLanguage}";

            // Send a GET request to the API
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            // Check if the response is successful
            if (response.IsSuccessStatusCode)
            {
                // Parse the JSON response
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var translationResult = JsonSerializer.Deserialize<MyMemoryResponse>(jsonResponse);
                // Return the translated text
                return translationResult.responseData.translatedText;
            }
            else
            {
                // Get the error message from the response and throw an exception
                var errorResponse = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error: {response.StatusCode}, Details: {errorResponse}");
            }
        }
    }

    // Class representing the JSON response from the MyMemory API
    public class MyMemoryResponse
    {
        public ResponseData responseData { get; set; }
    }

    // Class representing the response data containing the translated text
    public class ResponseData
    {
        public string translatedText { get; set; }
    }
}
