using System.Globalization;

namespace MauiApp1;

public partial class AddTransactionPage : ContentPage
{
    public AddTransactionPage()
    {
        InitializeComponent();
        TypePicker.SelectedIndex = 1;     // default Expense
        CategoryPicker.SelectedIndex = 0; // default first category
        DatePicker.Date = DateTime.Today;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        var title = TitleEntry.Text?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(title))
        {
            await DisplayAlert("Missing", "Please enter a title.", "OK");
            return;
        }

        if (!decimal.TryParse(AmountEntry.Text?.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var rawAmount))
        {
            await DisplayAlert("Invalid", "Enter a valid amount.", "OK");
            return;
        }

        var selectedType = TypePicker.SelectedIndex == 0 ? "Income" : "Expense";
        // Normalize: store absolute amount; type decides semantic sign
        var amount = Math.Abs(rawAmount);

        var t = new Transaction
        {
            Title = title,
            Amount = amount,
            Type = selectedType,
            Category = CategoryPicker.SelectedItem?.ToString() ?? "Other",
            // Some MAUI versions use DateTime? for DatePicker.Date -> GetValueOrDefault avoids CS0266
            Date = DatePicker.Date.GetValueOrDefault(DateTime.Today)
        };

        await Db.Save(t);
        await DisplayAlert("Saved", "Transaction saved.", "OK");
        await Shell.Current.GoToAsync("..");
    }
}