using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TicketSystemAPI.Models
{
    public class Coupons
    {
        public int CouponId { get; set; }
        public int TicketId { get; set; }
        public DateTime ExpirationDate { get; set; }
    }
}
