using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace HybridWebView9Bug
{

    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(Dictionary<string, string>))]
    [JsonSerializable(typeof(string))]
    public partial class HybridSampleJSContext : JsonSerializerContext {
        // This type's attributes specify JSON serialization info to preserve type structure
        // for trimmed builds.  
    }

    public partial class App : Application {

        protected override Window CreateWindow(IActivationState? activationState)
        {
            ContentPage mainPage = new();

            mainPage.BackgroundColor = Colors.DarkRed;

            AbsoluteLayout absDummy = new();
            mainPage.Content = absDummy;

            AbsoluteLayout absRoot = new();
            absRoot.BackgroundColor = Colors.DarkBlue;
            absDummy.Add(absRoot);

            HybridWebView hybridWebView = new();
            hybridWebView.BackgroundColor = Colors.DarkGreen;
            hybridWebView.HybridRoot = "webview";
            hybridWebView.DefaultFile = "index.html";
            absRoot.Add(hybridWebView);
            hybridWebView.RawMessageReceived += delegate (object sender, HybridWebViewRawMessageReceivedEventArgs e) {
                Debug.WriteLine("RECEIVED MESSAGE");
                Debug.WriteLine(e.Message);
            };

            //button row
            HorizontalStackLayout hor = new();
            absRoot.Add(hor);

            //change color button
            Button changeColorButton = new();
            changeColorButton.Text = "change color";
            changeColorButton.Clicked += async delegate {
                Debug.WriteLine("CLICKED");

                //just reads properties I think but iOS issues: https://github.com/dotnet/maui/issues/20288
                //var result = await hybridWebView.EvaluateJavaScriptAsync("setBackgroundRandomColor"); 

                //SETTING NULL JSON CHANGES COLOR (VISIBLE IN XAML LIVE VIEW) BUT CRASHES APP
                //android says: System.ArgumentNullException: 'Value cannot be null. (Parameter 'jsonTypeInfo')'

                //SEE https://github.com/dotnet/maui/issues/22303 other NORMAL functions should still work but don't
                //var result = await hybridWebView.InvokeJavaScriptAsync<string>("setBackgroundRandomColor", null);
                var result = await hybridWebView.InvokeJavaScriptAsync<string>("setBackgroundRandomColor", HybridSampleJSContext.Default.String);
                //var result = hybridWebView.InvokeJavaScriptAsync("setBackgroundRandomColor");

                Debug.WriteLine("INVOKED");
            };
            hor.Add(changeColorButton);

            //get color string button
            Button getColorButton = new();
            getColorButton.Text = "get color string";
            getColorButton.Clicked += async delegate {
                Debug.WriteLine("CLICKED");
                var result = await hybridWebView.InvokeJavaScriptAsync<string>("getRandomHexColor", null);
                Debug.WriteLine("INVOKED");
            };
            hor.Add(getColorButton);

            //screen resizing
            mainPage.SizeChanged += (sender, args) => {
                if (mainPage.Height > 0) {
                    double height = mainPage.Height;
                    double width = mainPage.Width;
                    absRoot.WidthRequest = width;
                    absRoot.HeightRequest = height;
                    hybridWebView.WidthRequest = width;
                    hybridWebView.HeightRequest = height;

                }
            };
            return new Window(mainPage);
        }
    }
}
