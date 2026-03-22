using System.Globalization;
using MauiApp1.Models;
using MauiApp1.Services;
using MauiApp1.Helpers;

namespace MauiApp1;

public partial class EditTransactionPage : ContentPage
{
    private TransactionRecord _model;
    private readonly ITransactionDatabase _db = ServiceHelper.GetService<ITransactionDatabase>();

    public EditTransactionPage(TransactionRecord model)
    {
        InitializeComponent();

        _model = model;

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

        if (!double.TryParse(AmountEntry.Text, out var amt))
        {
            await DisplayAlert("Invalid", "Amount invalid", "OK");
            return;
        }

        _model.Title = TitleEntry.Text.Trim();
        _model.Amount = Math.Abs(amt);
        _model.Type = TypePicker.SelectedIndex == 0 ? "Income" : "Expense";
        _model.Category = CategoryPicker.SelectedItem?.ToString() ?? "Other";
        _model.Date = DatePicker.Date ?? DateTime.Today;

        await _db.UpdateAsync(_model);

        await Navigation.PopModalAsync();
    }
}