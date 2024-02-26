using Mango.Web.Models.Dto;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mango.Web.Models.Dto
{
    public class OrderDetailDto
    {
        public int OrderDetailId { get; set; }
        public int OrderHeaderId { get; set; }
        public int ProductId { get; set; }
        public ProductDto? Product{ get; set; }
        public int Qty { get; set; }
        public string ProductName { get; set; }
        public double Price { get; set; }
    }
}

