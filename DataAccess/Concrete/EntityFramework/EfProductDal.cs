﻿using Core.DataAccess;
using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entities.Concrete;
using Entities.Concrete.DTOs;
using Entities.Concrete.MyProfiles;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Concrete.EntityFramework
{
    public class EfProductDal : EFEntityRepositoryBase<Product, T110Context>, IProductDal
    {
        public void AddProductWithLang(ProductDTO productDTO)
        {
            //ProductDTO,ProductRecordDTO
            Product newProduct = new()
            {
                Price = productDTO.Price,
                Discount = productDTO.Discount,
                CategoryId = productDTO.CategoryId,
                ProductRecords = new List<ProductRecord>()
            };
            
            //newProduct.ProductRecords.AddRange(productDTO.ProductRecords.Select(c => new ProductRecord()
            //{
            //    Description = c.Description,
            //    Name= c.Name,
            //    LanguageKey=c.LanguageKey
            //}));
            using T110Context context = new();
            context.Add(newProduct);
            context.SaveChanges();
        }

        public async Task<List<Product>> GetAllWithInclude(Expression<Func<Product, bool>>? filters,string lang)
        {
            using T110Context context = new();
            var products = context.Products
                .Include(c => c.ProductRecords.Where(c => c.LanguageKey == lang))
                .Include(c => c.Category)
                .ThenInclude(c => c.CategoryRecords)
                .AsQueryable();

            if (filters != null)
            {
                products = products.Where(filters);
            }
            return await products.ToListAsync();
        }

        public Product GetByIdWithInclude(Expression<Func<Product, bool>> filters,string lang)
        {
            using T110Context context = new();
            var product = context.Products
                .Include(c => c.ProductRecords.Where(c=>c.LanguageKey==lang))
                .Include(c => c.Category)
                .ThenInclude(c => c.CategoryRecords)
                .FirstOrDefault(filters);
            return product;
        }

        public async Task<List<Product>> SearchProducts(string? searchTerm,string langKey)
        {
            using T110Context context = new();
            var products = context.Products.AsQueryable();
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                products = products
                    .Include(c=>c.ProductRecords.Where(c=>c.LanguageKey==langKey))
                    .Where(c => c.ProductRecords
                  .Any(pr => pr.Name.ToUpper().Contains(searchTerm.ToUpper())));
            }

            //if (categoryId.HasValue)
            //{
            //    products = products.Where(c => c.CategoryId == categoryId);
            //}
            //if (minPrice.HasValue && maxPrice.HasValue)
            //{

            //    products = products.Where(c => (c.Discount != null && c.Discount > 0) ?
            //    (c.Discount >= minPrice && c.Discount <= maxPrice) :
            //    (c.Price >= minPrice && c.Price <= maxPrice)
            //    );
            //}
            return await products.ToListAsync();

        }
    }
}
