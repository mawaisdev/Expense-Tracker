using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expense_Tracker.Models
{
    public class Transaction
    {
        [Key]
        public long Id { get; set; }
        [Range(1,int.MaxValue, ErrorMessage = "Please Select a Category")]
        public long CategoryId { get; set; }
        public Category? Category { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Amount Should be Greater than 0")]
        public decimal Amount { get; set; }
        [Column(TypeName = "nvarchar(50)")]
        public string? Note { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;
        
        [NotMapped]
        public string? CategoryTitleWithIcon 
        { 
            get
            {
                return Category == null ? "" : Category.Icon + " " + Category.Title;
            }
        }
        [NotMapped]
        public string? FormattedAmount {
            get
            {
                return ((Category == null || Category.Type == "Expense") ? "- " : "+ ") +Amount.ToString("C0");
            }
        }

    }
}
