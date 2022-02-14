using HmacServer.Filters;
using Microsoft.AspNetCore.Mvc;
using System;

namespace HmacServer.Controllers
{
    [ServiceFilter(typeof(HmacAuthentication))]
    [Route("v1/[controller]")]
    public class ProductController : ControllerBase
    {
        [HttpPost]
        public IActionResult Index([FromBody] Product model)
        {
            var x = 0;
            return Ok(model);
        }
    }

    public class Product
    {
        public Product()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; private set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
}
