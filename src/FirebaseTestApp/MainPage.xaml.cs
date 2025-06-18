using FirebaseTestApp.Services.Analytics;

namespace FirebaseTestApp;

public partial class MainPage : ContentPage
{
	int count = 0;
	
	private IAnalytics? analytics;
	private ILogger<MainPage>? logger;

	public MainPage()
	{
		InitializeComponent();
		
		HandlerChanged += OnHandlerChanged;
	} 
		
	void OnHandlerChanged(object? sender, EventArgs e)
	{
		analytics = Handler?.MauiContext?.Services.GetService<IAnalytics>();
		logger = Handler?.MauiContext?.Services.GetService<ILogger<MainPage>>();
	}

	private void OnCounterClicked(object sender, EventArgs e)
	{
		count++;

		if (count == 1)
			CounterBtn.Text = $"Clicked {count} time";
		else
			CounterBtn.Text = $"Clicked {count} times";

		SemanticScreenReader.Announce(CounterBtn.Text);
		
		analytics?.TrackEvent("CounterClicked", ("Count", count));
	}

	private void OnCrashClicked(object sender, EventArgs e)
	{
		throw new Exception("Testing crash reporting");
	}

	private async void OnTaskErrorClicked(object sender, EventArgs e)
	{
		await Task.Delay(100);
		throw new Exception("Testing UnobservedTaskException reporting");
	}

	private void OnLogErrorClicked(object sender, EventArgs e)
	{
		var ex = new Exception("Testing exception logging");
		logger?.LogError(ex, "Testing out exception reporting");
	}
}
