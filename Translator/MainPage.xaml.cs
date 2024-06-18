using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Translator
{
    public partial class MainPage : ContentPage
    {
        private readonly HttpClient _httpClient;

        public MainPage()
        {
            InitializeComponent();
            _httpClient = new HttpClient();
        }

        private async void OnTranslateClicked(object sender, EventArgs e)
        {
            await TranslateAndDisplayText();
        }

        private async void OnEditorCompleted(object sender, EventArgs e)
        {
            await TranslateAndDisplayText();
            InputEditor.Unfocus(); // Dismiss the keyboard
        }

        private async Task TranslateAndDisplayText()
        {
            string inputText = InputEditor.Text;
            if (string.IsNullOrWhiteSpace(inputText))
            {
                await DisplayAlert("Error", "Please enter some text to translate.", "OK");
                return;
            }

            try
            {
                // Translate text from English to German
                var translatedText = await TranslateTextAsync(inputText, "en", "de");
                TranslatedLabel.Text = translatedText;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Translation failed: {ex.Message}", "OK");
            }
        }

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
    }

    public class MyMemoryResponse
    {
        public ResponseData responseData { get; set; }
    }

    public class ResponseData
    {
        public string translatedText { get; set; }
    }
}
