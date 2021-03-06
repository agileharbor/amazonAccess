﻿using System;
using System.Collections.Generic;
using System.Linq;
using AmazonAccess.Misc;
using AmazonAccess.Models;
using AmazonAccess.Services.Products.Model;
using CuttingEdge.Conditions;
using Netco.Extensions;

namespace AmazonAccess.Services.Products
{
	public class ProductsBySkuService
	{
		private readonly IProductsServiceClient _client;
		private readonly AmazonCredentials _credentials;
		private readonly Throttler _throttler = new Throttler( 20, 1 ); //TODO: Optimization - Restore rate is five items every second

		/// <param name="client">Instance of IProductsServiceClient client</param>
		/// <param name="credentials">credentials</param>
		public ProductsBySkuService( IProductsServiceClient client, AmazonCredentials credentials )
		{
			Condition.Requires( client, "client" ).IsNotNull();
			Condition.Requires( credentials, "credentials" ).IsNotNull();

			this._client = client;
			this._credentials = credentials;
		}

		public Dictionary< AmazonMarketplace, List< string > > GetProductsBySkus( string marker, List< string > skus, bool skipDuplicates, Action< Product > processProductAction )
		{
			AmazonLogger.Trace( "GetProductsBySkus", this._credentials.SellerId, marker, "Begin invoke" );

			var skusCopy = skus.ToList();
			var result = new Dictionary< AmazonMarketplace, List< string > >();
			foreach( var marketplace in this._credentials.AmazonMarketplaces.Marketplaces )
			{
				var receivedSkus = this.GetProductsBySkusForMarketplace( marker, skusCopy, marketplace.MarketplaceId, processProductAction );
				result.Add( marketplace, receivedSkus );
				if( skipDuplicates )
				{
					foreach( var receivedSku in receivedSkus )
					{
						skusCopy.Remove( receivedSku );
					}
				}
			}

			AmazonLogger.Trace( "GetProductsBySkus", this._credentials.SellerId, marker, "End invoke" );
			return result;
		}

		private List< string > GetProductsBySkusForMarketplace( string marker, IEnumerable< string > skus, string marketplace, Action< Product > processProductAction )
		{
			AmazonLogger.Trace( "GetProductsBySkusForMarketplace", this._credentials.SellerId, marker, "Begin invoke" );

			var result = new List< string >();
			var skusParts = skus.Slice( 5 );
			foreach( var skusPart in skusParts )
			{
				var request = new GetMatchingProductForIdRequest
				{
					SellerId = this._credentials.SellerId,
					MWSAuthToken = this._credentials.MwsAuthToken,
					MarketplaceId = marketplace,
					IdType = "SellerSKU",
					IdList = new IdListType { Id = skusPart.ToList() }
				};

				var response = ActionPolicies.Get.Get( () => this._throttler.Execute( () => this._client.GetMatchingProductForId( request, marker ) ) );
				if( !response.IsSetGetMatchingProductForIdResult() )
					continue;

				foreach( var productResult in response.GetMatchingProductForIdResult )
				{
					if( productResult.status.Equals( "ClientError" ) || productResult.status.Equals( "ServerError" ) )
						continue;
					if( !productResult.status.Equals( "Success" ) || !productResult.IsSetProducts() )
						continue;
					if( productResult.Products.Product.Count != 1 )
						continue;

					result.Add( productResult.Id );
					var product = productResult.Products.Product.First();
					if( !product.Identifiers.IsSetSKUIdentifier() )
					{
						product.Identifiers.SKUIdentifier = new SellerSKUIdentifier
						{
							MarketplaceId = marketplace,
							SellerId = this._credentials.SellerId,
							SellerSKU = productResult.Id
						};
					}
					processProductAction( product );
				}
			}

			AmazonLogger.Trace( "GetProductsBySkusForMarketplace", this._credentials.SellerId, marker, "End invoke" );
			return result;
		}
	}
}