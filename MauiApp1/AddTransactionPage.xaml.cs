namespace MauiApp1;

public partial class AddTransactionPage : ContentPage
{
    public AddTransactionPage()
    {
        InitializeComponent();
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        string title = TitleEntry.Text;
        string amountText = AmountEntry.Text;
        string type = TypePicker.SelectedItem?.ToString();

        // Convert amount safely
        double amount = 0;
        double.TryParse(amountText, out amount);

        // FIXED DATE ERROR HERE 👇
        DateTime selectedDate = DatePicker.Date ?? DateTime.Now;

        await DisplayAlert("Saved",
            $"Title: {title}\nAmount: {amount}\nType: {type}\nDate: {selectedDate.ToShortDateString()}",
            "OK");

        await Shell.Current.GoToAsync("..");
    }
}
