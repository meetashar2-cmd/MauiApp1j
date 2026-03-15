using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace MauiApp1;

public partial class DashboardPage : ContentPage
{
    // Keep all data cached; filters work on this list
    private List<Transaction> _all = new();

    public DashboardPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _all = await Db.GetAll();
        await ApplyFilterAsync(FilterMode.ThisMonth); // default view
    }

    // =========================
    // Filtering
    // =========================
    private enum FilterMode { ThisMonth, LastMonth, All }
    private FilterMode _mode = FilterMode.ThisMonth;

    private async Task ApplyFilterAsync(FilterMode mode)
    {
        _mode = mode;
        var now = DateTime.Now;

        IEnumerable<Transaction> list = _all;

        if (mode == FilterMode.ThisMonth)
            list = _all.Where(t => t.Date.Year == now.Year && t.Date.Month == now.Month);
        else if (mode == FilterMode.LastMonth)
        {
            var prev = now.AddMonths(-1);
            list = _all.Where(t => t.Date.Year == prev.Year && t.Date.Month == prev.Month);
        }

        var income = list.Where(x => x.Type == "Income").Sum(x => x.Amount);
        var expenses = list.Where(x => x.Type == "Expense").Sum(x => x.Amount);

        // These names must exist in XAML
        IncomeLabel.Text = $"${income:0,0.00}";
        ExpenseLabel.Text = $"${expenses:0,0.00}";
        BalanceLabel.Text = $"${(income - expenses):0,0.00}";

        RecentList.ItemsSource = list.Take(10).ToList();

        await CheckBudgetsAsync(list);
        await Task.CompletedTask;
    }

    private async Task CheckBudgetsAsync(IEnumerable<Transaction> list)
    {
        // Budget checks only for the currently visible (filtered) expenses
        var over = list.Where(t => t.Type == "Expense")
            .GroupBy(t => t.Category)
            .Select(g => new
            {
                Category = g.Key,
                Sum = g.Sum(x => x.Amount),
                Cap = Budget.Caps.GetValueOrDefault(g.Key, 0m)
            })
            .Where(x => x.Cap > 0 && x.Sum > x.Cap)
            .ToList();

        if (over.Any())
        {
            var message = string.Join("\n", over.Select(x => $"{x.Category}: ${x.Sum:0} / ${x.Cap:0} (over)"));
            await DisplayAlert("Budget warning", message, "OK");
        }
    }

    // =========================
    // Event handlers required by your XAML
    // =========================

    // Buttons (Clicked)
    private async void OnFilterThisMonth(object sender, EventArgs e)
        => await ApplyFilterAsync(FilterMode.ThisMonth);

    private async void OnFilterLastMonth(object sender, EventArgs e)
        => await ApplyFilterAsync(FilterMode.LastMonth);

    private async void OnFilterAll(object sender, EventArgs e)
        => await ApplyFilterAsync(FilterMode.All);

    // Quick Action “Add” (TapGestureRecognizer.Tapped)
    private async void OnAddClicked(object sender, TappedEventArgs e)
        => await Shell.Current.GoToAsync(nameof(AddTransactionPage));

    // Swipe to delete (SwipeItem.Invoked)
    private Transaction? _lastDeleted;

    private async void OnDeleteSwipe(object sender, EventArgs e)
    {
        if (sender is SwipeItem swipe && swipe.BindingContext is Transaction t)
        {
            _lastDeleted = t;
            await Db.Delete(t);
            _all.RemoveAll(x => x.Id == t.Id);
            await ApplyFilterAsync(_mode);

            bool undo = await DisplayAlert("Deleted", $"Removed '{t.Title}'. Undo?", "Undo", "OK");
            if (undo && _lastDeleted != null)
            {
                _lastDeleted.Id = 0; // re-insert as new
                await Db.Save(_lastDeleted);
                _all = await Db.GetAll();
                _lastDeleted = null;
                await ApplyFilterAsync(_mode);
            }
        }
    }

    // Tap row to edit (TapGestureRecognizer.Tapped)
    private async void OnRowTapped(object sender, TappedEventArgs e)
    {
        if (sender is Grid grid && grid.BindingContext is Transaction t)
        {
            await Navigation.PushModalAsync(new EditTransactionPage(t));

            // Small delay then refresh after closing
            grid.Dispatcher.StartTimer(TimeSpan.FromMilliseconds(200), () =>
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    _all = await Db.GetAll();
                    await ApplyFilterAsync(_mode);
                });
                return false;
            });
        }
    }
}