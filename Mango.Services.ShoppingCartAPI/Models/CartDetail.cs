﻿using Mango.Services.ShoppingCartAPI.Models.Dto;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mango.Services.ShoppingCartAPI.Models
{
    public class CartDetail
    {
        [Key]
        public int CartDetailId { get; set; }
        public int CartHeaderId { get; set; }
        [ForeignKey("CartHeaderId")]
        public CartHeader? CartHeader { get; set; }
        public int ProductId { get; set; }
        [NotMapped]
        public ProductDto? Product{ get; set; }
        public int Qty { get; set; }
    }
}