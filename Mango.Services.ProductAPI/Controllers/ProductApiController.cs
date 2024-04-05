using AutoMapper;
using Mango.Services.ProductAPI.Data;
using Mango.Services.ProductAPI.Models;
using Mango.Services.ProductAPI.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Net.Mime.MediaTypeNames;

namespace Mango.Services.ProductAPI.Controllers
{
    [Route("api/product")]
    [ApiController]
    public class ProductApiController : ControllerBase
    {
        private readonly AppDbContext _db;
        private ResponseDto _response;
        private IMapper _mapper;
        public ProductApiController(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _response = new ResponseDto();
            _mapper = mapper;
        }

        [HttpGet]
        public ResponseDto Get()
        {
            try
            {
                IEnumerable<Product> objList = _db.Products.ToList();

                objList.Where(obj => !string.IsNullOrEmpty(obj.ImageUrl) && !string.IsNullOrEmpty(obj.ImageLocalPath)).ToList() 
                   .ForEach(obj => obj.ImageUrl = ConvertToAbsoluteUrl(obj.ImageUrl));

                _response.Result = _mapper.Map<IEnumerable<ProductDto>>(objList); ;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        [HttpGet]
        [Route("{id:int}")]
        public ResponseDto Get(int id)
        {
            try
            {
                Product obj = _db.Products.First(x => x.ProductId == id);
                if (!string.IsNullOrEmpty(obj.ImageUrl) && !string.IsNullOrEmpty(obj.ImageLocalPath))
                    obj.ImageUrl = ConvertToAbsoluteUrl(obj.ImageUrl);
                _response.Result = _mapper.Map<ProductDto>(obj);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        private string ConvertToAbsoluteUrl(string? relativeUrl)
        {
            var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";
            var absoluteUrl = baseUrl + relativeUrl?.ToString();
            return absoluteUrl;
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public ResponseDto Post(ProductDto productDto)
        {
            try
            {
                Product obj = _mapper.Map<Product>(productDto);
                _db.Products.Add(obj);
                _db.SaveChanges();
                
                //Saving image
                if(productDto.Image != null)
                {
                    string localFilePath, imageUrl;
                    string fileName = obj.ProductId.ToString() + Path.GetExtension(productDto.Image.FileName);
                    SaveProductImageFile(productDto.Image, fileName, out localFilePath, out imageUrl);

                    obj.ImageUrl = imageUrl;
                    obj.ImageLocalPath = localFilePath;
                }
                else
                {
                    obj.ImageUrl = "https://placehold.co/600x400";
                }
                _db.Products.Update(obj);
                _db.SaveChanges();

                _response.Result = _mapper.Map<ProductDto>(obj);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        private void SaveProductImageFile(IFormFile formFile, string fileName, out string localFilePath, out string imageUrl)
        {
            string imageFolderName = "ProductImages";
            localFilePath = $"wwwroot\\{imageFolderName}\\" + fileName;
            imageUrl = $"/{imageFolderName}/{fileName}";
            string fullFilePath = Path.Combine(Directory.GetCurrentDirectory(), localFilePath);
            using (var fileStream = new FileStream(fullFilePath, FileMode.Create))
            {
                formFile.CopyTo(fileStream);
            }
        }

        [HttpPut]
        [Authorize(Roles = "ADMIN")]
        public ResponseDto Put(ProductDto productDto)
        {
            try
            {
                Product obj = _mapper.Map<Product>(productDto);
                
                if (productDto.Image != null)
                {
                    // replace image
                    DeleteProductImageFile(obj.ImageLocalPath);

                    string localFilePath, imageUrl;
                    string fileName = obj.ProductId.ToString() + Path.GetExtension(productDto.Image.FileName);
                    SaveProductImageFile(productDto.Image, fileName, out localFilePath, out imageUrl);

                    obj.ImageUrl = imageUrl;
                    obj.ImageLocalPath = localFilePath;
                }
                else
                {
                    //retain existing image
                    Product existing = _db.Products.AsNoTracking().First(x => x.ProductId == productDto.ProductId);
                    obj.ImageUrl= existing.ImageUrl;
                    obj.ImageLocalPath = existing.ImageLocalPath;
                }
                _db.Products.Update(obj);
                _db.SaveChanges();
                _response.Result = _mapper.Map<ProductDto>(obj);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public ResponseDto Delete(int id)
        {
            try
            {
                Product obj = _db.Products.First(x => x.ProductId == id);
                DeleteProductImageFile(obj.ImageLocalPath);
                _db.Products.Remove(obj);
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        private void DeleteProductImageFile(string? imageLocalPath)
        {
            if (!string.IsNullOrEmpty(imageLocalPath))
            {
                string filePathFull = Path.Combine(Directory.GetCurrentDirectory(), imageLocalPath);
                FileInfo file = new FileInfo(filePathFull);
                if (file.Exists)
                {
                    file.Delete();
                }
            }
        }
    }
}

