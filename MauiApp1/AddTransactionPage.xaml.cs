using MauiApp1.Models;
using MauiApp1.Services;

namespace MauiApp1;

public partial class AddTransactionPage : ContentPage
{
    private readonly ITransactionDatabase _db;

    public AddTransactionPage()
    {
        InitializeComponent();

        // ✅ Get database from DI
        _db = IPlatformApplication.Current.Services.GetService<ITransactionDatabase>();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_db == null)
            return;

        // ✅ Load accounts into picker
        var accounts = await _db.GetAccountsAsync();

        AccountPicker.ItemsSource = accounts;
        AccountPicker.ItemDisplayBinding = new Binding("Name");

        if (accounts.Any())
            AccountPicker.SelectedIndex = 0;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (_db == null)
        {
            await DisplayAlert("Error", "Database not available", "OK");
            return;
        }

        // ✅ Validate amount
        if (!double.TryParse(AmountEntry.Text, out var amount) || amount <= 0)
        {
            await DisplayAlert("Validation", "Enter a valid amount", "OK");
            return;
        }

        // ✅ Get selected account
        var selectedAccount = AccountPicker.SelectedItem as Account;

        if (selectedAccount == null)
        {
            await DisplayAlert("Validation", "Please select an account", "OK");
            return;
        }

        // ✅ Create transaction
        var tx = new TransactionRecord
        {
            Title = TitleEntry.Text?.Trim() ?? "No Title",
            Amount = amount,
            Category = CategoryPicker.SelectedItem?.ToString() ?? "Other",
            Type = TypePicker.SelectedItem?.ToString() ?? "Expense",
            Date = DatePicker.Date ?? DateTime.Now,
            AccountId = selectedAccount.Id
        };

        // ✅ Save to database
        await _db.AddAsync(tx);

        // ✅ Success message
        await DisplayAlert("Success", "Transaction saved!", "OK");

        // ✅ Navigate back to Dashboard
        await Shell.Current.GoToAsync("..");
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}