﻿namespace Mango.Web.Models
{
    public class CouponDto
    {        
        public string? CouponId { get; set; }
        public string CouponCode { get; set; } 
        public double DiscountAmount { get; set; }
        public int MinAmount { get; set; }
    }
}
