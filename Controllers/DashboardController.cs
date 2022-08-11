using Expense_Tracker.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Expense_Tracker.Controllers
{
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            // Last 7 Days Transactions
            DateTime StartDate = DateTime.Today.AddDays(-6);
            DateTime EndDate = DateTime.Today;

            var transactions = await _context.Transactions
                .Include(c => c.Category)
                .Where(y => y.Date >= StartDate && y.Date <= EndDate)
                .ToListAsync();

            // Income

            var totalIncome = transactions
                .Where(i => i?.Category?.Type == "Income")
                .Sum(j => j.Amount);

            ViewBag.TotalIncome = totalIncome.ToString("C0");


            // Expenses

            var totalExpense = transactions
                .Where(i => i?.Category?.Type == "Expense")
                .Sum(j => j.Amount);

            ViewBag.TotalExpense = totalExpense.ToString("C0");

            var balanceAMount = totalIncome - totalExpense;
            var culture = CultureInfo.CreateSpecificCulture("en-US");
            culture.NumberFormat.CurrencyNegativePattern = 1;
            ViewBag.Balance = String.Format(culture, "{0:C0}", balanceAMount);


            // Doughnut Chart - Expense By Category
            ViewBag.DoughnutChartData = transactions
                .Where(i => i?.Category?.Type == "Expense")
                .GroupBy(j => j?.Category?.Id)
                .Select(k => new
                {
                    categoryTitleWithIcon = k.First().Category?.Icon + " " + k.First().Category?.Title,
                    amount = k.Sum(j => j.Amount),
                    formattedAmount = k.Sum(j => j.Amount).ToString("C0"),
                })
                .OrderByDescending(l => l.amount)
                .ToList();

            // Spline Chart - Income vs Expense

            // Income
            List<SplineChartData> IncomeSummery = transactions
                .Where(i => i.Category.Type == "Income")
                .GroupBy(j => j.Date)
                .Select(k => new SplineChartData()
                {
                    Day = k.First().Date.ToString("dd-MMM"),
                    Income = (int)k.Sum(i => i.Amount),
                }).ToList();

            // Expense
            List<SplineChartData> ExpenseSummery = transactions
                .Where(i => i.Category!.Type == "Expense")
                .GroupBy(j => j.Date)
                .Select(k => new SplineChartData()
                {
                    Day = k.First().Date.ToString("dd-MMM"),
                    Income = (int)k.Sum(i => i.Amount),
                }).ToList();


            // Combining Income and Expenses
            string[] Last7DaysDate = Enumerable.Range(0, 7)
                .Select(i => StartDate.AddDays(i).ToString("dd-MMM"))
                .ToArray();


            ViewBag.SplineChartData = from day in Last7DaysDate
                                      join income in IncomeSummery on day equals income.Day into dayIncomeJoined
                                      from income in dayIncomeJoined.DefaultIfEmpty()
                                      join expense in ExpenseSummery on day equals expense.Day into expenseJoined
                                      from expense in expenseJoined.DefaultIfEmpty()
                                      select new
                                      {
                                          day = day,
                                          income = income == null ? 0 : income.Income,
                                          expense = expense == null ? 0 : expense.Expense
                                      };
            // Recent Transactions
            ViewBag.RecentTransactions = await _context.Transactions
                .Include(c => c.Category)
                .OrderByDescending(d => d.Date)
                .Take(5)
                .ToListAsync();


            return View();
        }
    }
}


public class SplineChartData
{
    public string Day;
    public int Income;
    public int Expense;
}