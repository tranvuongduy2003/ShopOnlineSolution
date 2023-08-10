﻿using Microsoft.EntityFrameworkCore;
using ShopOnline.Api.Data;
using ShopOnline.Api.Entities;
using ShopOnline.Api.Repositories.Contracts;
using ShopOnline.Models.Dtos;

namespace ShopOnline.Api.Repositories
{
	public class ShoppingCartRepository : IShoppingCartRepository
	{
		private readonly ShopOnlineDbContext shopOnlineDbContext;

		public ShoppingCartRepository(ShopOnlineDbContext shopOnlineDbContext)
        {
			this.shopOnlineDbContext = shopOnlineDbContext;
		}

		private async Task<bool> CartItemExists(int cartId, int productId)
		{
			return await this.shopOnlineDbContext.CartItems.AnyAsync(c => c.CartId == cartId && c.ProductId == productId);
		}

		public async Task<CartItem> AddItem(CartItemToAddDto cartItemToAddDto)
		{
			var isCartItemExists = await CartItemExists(cartItemToAddDto.CartId, cartItemToAddDto.ProductId);

			if (!isCartItemExists)
			{
				var item = await (from product in this.shopOnlineDbContext.Products
								  where product.Id == cartItemToAddDto.ProductId
								  select new CartItem
								  {
									  CartId = cartItemToAddDto.CartId,
									  ProductId = product.Id,
									  Qty = cartItemToAddDto.Qty,
								  }).SingleOrDefaultAsync();

				if (item != null)
				{
					var results = await this.shopOnlineDbContext.CartItems.AddAsync(item);
					await shopOnlineDbContext.SaveChangesAsync();
					return results.Entity;
				}
			}

			return null;
		}

		public async Task<CartItem> DeleteItem(int id)
		{
			var item = await this.shopOnlineDbContext.CartItems.FindAsync(id);

			if (item != null)
			{
				this.shopOnlineDbContext.CartItems.Remove(item);
				await this.shopOnlineDbContext.SaveChangesAsync();
			}

			return item;
		}

		public async Task<CartItem> GetItem(int id)
		{
			return await (from cartItem in this.shopOnlineDbContext.CartItems
						  join cart in this.shopOnlineDbContext.Carts
						  on cartItem.CartId equals cart.Id
						  where cartItem.Id == id
						  select new CartItem
						  {
							  Id = cartItem.Id,
							  ProductId = cartItem.ProductId,
							  Qty = cartItem.Qty,
							  CartId = cartItem.CartId
						  }).FirstOrDefaultAsync();
		}

		public async Task<IEnumerable<CartItem>> GetItems(int userId)
		{
			return await (from cart in this.shopOnlineDbContext.Carts
						  join cartItem in this.shopOnlineDbContext.CartItems
						  on cart.Id equals cartItem.CartId
						  where cart.UserId == userId
						  select new CartItem 
						  { 
							  Id = cartItem.Id,
							  ProductId = cartItem.ProductId,
							  Qty = cartItem.Qty,
							  CartId = cartItem.CartId
						  }).ToListAsync();
		}

		public async Task<CartItem> UpdateQty(int id, CartItemQtyUpdateDto cartItemQtyUpdateDto)
		{
			var item = await this.shopOnlineDbContext.CartItems.FindAsync(id);

			if (item != null)
			{
				item.Qty = cartItemQtyUpdateDto.Qty;
				await this.shopOnlineDbContext.SaveChangesAsync();
				return item;
			}

			return null;
		}
	}
}
