using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AwesomeGIC_App
{
    public class Transaction
    {
        public DateTime Date { get; set; }
        public string TxnId { get; set; }
        public string Type { get; set; } // D, W, I
        public decimal Amount { get; set; }
    }
}
