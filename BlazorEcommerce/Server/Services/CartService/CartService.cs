﻿using BlazorEcommerce.Client.Shared;
using System.Security.Claims;

namespace BlazorEcommerce.Server.Services.CartService
{
    public class CartService : ICartService
    {
        private readonly DataContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartService(DataContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private int GetUserId() => int.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));

        public async Task<ServiceResponse<List<CartProductResponse>>> GetCartProducts(List<CartItem> cartItems)
        {
            var result = new ServiceResponse<List<CartProductResponse>>
            {
                Data = new List<CartProductResponse>()
            };

            foreach(var item in cartItems)
            {
                var product = await _context.Products
                    .Where( p => p.Id == item.ProductId)
                    .FirstOrDefaultAsync();
                if ( product == null)
                {
                    continue;
                }

                var productVariant = await _context.ProductVariants
                .Where(v => v.ProductId == item.ProductId
                 && v.ProductTypeId == item.ProductTypeId)
                .Include( v => v.ProductType)
                .FirstOrDefaultAsync();

                if ( productVariant == null)
                {
                    continue;
                }

                var cartProduct = new CartProductResponse
                {
                    ProductId = product.Id,
                    Title = product.Title,
                    ImageUrl = product.ImageUrl,
                    Price = productVariant.Price,
                    ProductType = productVariant.ProductType.Name,
                    ProductTypeId = productVariant.ProductTypeId,
                    Quantity = item.Quantity
                };

                result.Data.Add(cartProduct);
            }


            return result;
        }

        public async Task<ServiceResponse<List<CartProductResponse>>> StoreCartItems(List<CartItem> cartItems)
        {
            //  cartItems.ForEach(cartItem => cartItem.UserId = userId);

            cartItems.ForEach(cartItem => cartItem.UserId = GetUserId());

            _context.CartItems.AddRange(cartItems);
            await _context.SaveChangesAsync();

            //return await GetCartProducts(
            //            await _context.CartItems
            //            .Where( ci => ci.UserId == userId).ToListAsync());

            //return await GetCartProducts(
            //          await _context.CartItems
            //          .Where(ci => ci.UserId == GetUserId()).ToListAsync());

            return await GetDbCartProducts();
        }

        public async Task<ServiceResponse<int>> GetCartItemsCount()
        {
            var count = (await _context.CartItems.Where(ci => ci.UserId == GetUserId()).ToListAsync()).Count;
            return new ServiceResponse<int> {  Data = count };
        }

        public async Task<ServiceResponse<List<CartProductResponse>>> GetDbCartProducts()
        {
            return await GetCartProducts(await _context.CartItems.Where( ci => ci.UserId == GetUserId()).ToListAsync() );
        }

        public async Task<ServiceResponse<bool>> AddToCart(CartItem cartItem)
        {
            cartItem.UserId = GetUserId();

            var sameItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.ProductId == cartItem.ProductId &&
                 ci.ProductTypeId == cartItem.ProductTypeId && ci.UserId == cartItem.UserId);

            if ( sameItem == null)
            {
                _context.CartItems.Add(cartItem);
            }
            else
            {
                sameItem.Quantity += cartItem.Quantity;
            }

            await _context.SaveChangesAsync();  

            return new ServiceResponse<bool> { Data = true };
        }

    }

}
