using System.Globalization;

namespace MauiApp1;

public partial class EditTransactionPage : ContentPage
{
    private Transaction _model;

    public EditTransactionPage(Transaction model)
    {
        InitializeComponent();
        _model = model;

        // Initialize UI from model
        TitleEntry.Text = _model.Title;
        AmountEntry.Text = _model.Amount.ToString(CultureInfo.InvariantCulture);
        TypePicker.SelectedIndex = _model.Type == "Income" ? 0 : 1;
        CategoryPicker.SelectedItem = _model.Category;
        DatePicker.Date = _model.Date;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TitleEntry.Text))
        {
            await DisplayAlert("Missing", "Title required", "OK");
            return;
        }

        if (!decimal.TryParse(AmountEntry.Text?.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var amt))
        {
            await DisplayAlert("Invalid", "Amount invalid", "OK");
            return;
        }

        // Update model and save
        _model.Title = TitleEntry.Text!.Trim();
        _model.Amount = Math.Abs(amt);
        _model.Type = TypePicker.SelectedIndex == 0 ? "Income" : "Expense";
        _model.Category = CategoryPicker.SelectedItem?.ToString() ?? "Other";
        // DatePicker.Date can be DateTime? in newer MAUI; this works for both:
        _model.Date = DatePicker.Date.GetValueOrDefault(DateTime.Today);

        await Db.Save(_model);
        await Navigation.PopModalAsync();
    }
}