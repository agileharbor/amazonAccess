﻿using System;
using System.Collections.Generic;
using System.Linq;
using AmazonAccess.Misc;
using AmazonAccess.Models;
using AmazonAccess.Services;
using AmazonAccess.Services.FbaInventoryServiceMws;
using AmazonAccess.Services.FbaInventoryServiceMws.Model;
using AmazonAccess.Services.MarketplaceWebServiceFeedsReports;
using AmazonAccess.Services.MarketplaceWebServiceFeedsReports.Model;
using AmazonAccess.Services.MarketplaceWebServiceFeedsReports.ReportModel;
using AmazonAccess.Services.MarketplaceWebServiceOrders;
using AmazonAccess.Services.MarketplaceWebServiceOrders.Model;
using AmazonAccess.Services.MarketplaceWebServiceSellers;
using AmazonAccess.Services.MarketplaceWebServiceSellers.Model;
using CuttingEdge.Conditions;

namespace AmazonAccess
{
	public sealed class AmazonService: IAmazonService
	{
		private const int Updateitemslimit = 3000;
		private readonly AmazonCredentials _credentials;
		private readonly IAmazonClientsFactory _factory;

		public AmazonService( AmazonCredentials credentials )
		{
			Condition.Requires( credentials, "credentials" ).IsNotNull();

			this._credentials = credentials;
			this._factory = new AmazonClientsFactory( credentials );
		}

		#region Orders
		/// <summary>
		/// This operation takes up to 50 order ids and returns the corresponding orders.
		/// </summary>
		/// <param name="ids"></param>
		/// <returns></returns>
		public IEnumerable< ComposedOrder > GetOrdersById( List< string > ids )
		{
			var client = this._factory.CreateOrdersClient( "SkuVault", "1.0" );
			var request = new GetOrderRequest
			{
				SellerId = this._credentials.SellerId,
				MWSAuthToken = this._credentials.MwsAuthToken,
				AmazonOrderId = ids
			};

			AmazonLogger.Log.Trace( "[amazon] Loading orders by id for seller {0}", this._credentials.SellerId );

			var service = new OrdersByIdService( client, request );
			foreach( var order in service.LoadOrders() )
			{
				yield return order;
			}

			AmazonLogger.Log.Trace( "[amazon] Orders by id for seller {0} loaded", this._credentials.SellerId );
		}

		public IEnumerable< ComposedOrder > GetOrders( DateTime dateFrom, DateTime dateTo )
		{
			var client = this._factory.CreateOrdersClient( "SkuVault", "1.0" );
			var request = new ListOrdersRequest
			{
				SellerId = this._credentials.SellerId,
				LastUpdatedAfter = dateFrom,
				//LastUpdatedBefore = dateTo,
				MarketplaceId = this._credentials.AmazonMarketplaces.GetMarketplaceIdAsList(),
				MWSAuthToken = this._credentials.MwsAuthToken
			};

			AmazonLogger.Log.Trace( "[amazon] Loading orders for seller {0}", this._credentials.SellerId );

			var service = new OrdersService( client, request );
			foreach( var order in service.LoadOrders() )
			{
				yield return order;
			}

			AmazonLogger.Log.Trace( "[amazon] Orders for seller {0} loaded", this._credentials.SellerId );
		}

		public bool IsOrdersReceived( DateTime? dateFrom = null, DateTime? dateTo = null )
		{
			try
			{
				dateFrom = dateFrom ?? DateTime.UtcNow.AddHours( -1 );
				dateTo = dateTo ?? DateTime.UtcNow.AddMinutes( -10 );
				var client = this._factory.CreateOrdersClient( "SkuVault", "1.0" );
				var request = new ListOrdersRequest
				{
					SellerId = this._credentials.SellerId,
					LastUpdatedAfter = dateFrom.Value,
					//LastUpdatedBefore = dateTo,
					MarketplaceId = this._credentials.AmazonMarketplaces.GetMarketplaceIdAsList(),
					MWSAuthToken = this._credentials.MwsAuthToken
				};

				AmazonLogger.Log.Trace( "[amazon] Checking orders for seller {0}", this._credentials.SellerId );

				var service = new OrdersService( client, request );
				if( service.IsOrdersReceived() )
				{
					AmazonLogger.Log.Trace( "[amazon] Checking orders for seller {0} finished", this._credentials.SellerId );
					return true;
				}
				AmazonLogger.Log.Warn( "[amazon] Checking orders for seller {0} failed", this._credentials.SellerId );
				return false;
			}
			catch( Exception ex )
			{
				AmazonLogger.Log.Warn( ex, "[amazon] Checking orders for seller {0} failed", this._credentials.SellerId );
				return false;
			}
		}
		#endregion

		#region update inventory
		public void UpdateInventory( IEnumerable< AmazonInventoryItem > inventoryItems )
		{
			var client = this._factory.CreateFeedsReportsClient();

			AmazonLogger.Log.Trace( "[amazon] Updating inventory for seller {0}", this._credentials.SellerId );

			if( inventoryItems.Count() > Updateitemslimit )
			{
				var partsCount = inventoryItems.Count() / Updateitemslimit + 1;
				var parts = inventoryItems.Split( partsCount );

				foreach( var part in parts )
				{
					this.SubmitInventoryUpdateRequest( client, part );
				}
			}
			else
				this.SubmitInventoryUpdateRequest( client, inventoryItems );

			AmazonLogger.Log.Trace( "[amazon] Inventory for seller {0} loaded", this._credentials.SellerId );
		}

		private SubmitFeedRequest InitInventoryFeedRequest( IEnumerable< AmazonInventoryItem > inventoryItems )
		{
			var xmlService = new InventoryFeedXmlService( inventoryItems, this._credentials.SellerId );
			var contentString = xmlService.GetDocumentString();
			var contentStream = xmlService.GetDocumentStream();

			AmazonLogger.Log.Trace( "[amazon] Inventory document for seller {0}\n{1}", this._credentials.SellerId, contentString );

			var request = new SubmitFeedRequest
			{
				MarketplaceIdList = new IdList { Id = this._credentials.AmazonMarketplaces.GetMarketplaceIdAsList() },
				Merchant = this._credentials.SellerId,
				FeedType = FeedType.InventoryQuantityUpdate.Description,
				FeedContent = contentStream,
				ContentMD5 = MarketplaceWebServiceFeedsReportsClient.CalculateContentMD5( contentStream ),
				MWSAuthToken = this._credentials.MwsAuthToken
			};

			return request;
		}

		private void SubmitInventoryUpdateRequest( IMarketplaceWebServiceFeedsReports client, IEnumerable< AmazonInventoryItem > inventoryItems )
		{
			var request = this.InitInventoryFeedRequest( inventoryItems );
			var service = new FeedsService( client );

			ActionPolicies.Submit.Do( () =>
			{
				service.SubmitFeed( request );
				request.FeedContent.Close();
				ActionPolicies.CreateApiDelay( 2 ).Wait();
			} );
		}
		#endregion

		#region Get FBA inventory
		public IEnumerable< InventorySupply > GetFbaInventory()
		{
			var inventory = new List< InventorySupply >();

			ActionPolicies.Get.Do( () =>
			{
				var client = this._factory.CreateFbaInventoryClient();
				var request = new ListInventorySupplyRequest
				{
					SellerId = this._credentials.SellerId,
					QueryStartDateTime = DateTime.MinValue,
					ResponseGroup = "Detailed",
					MWSAuthToken = this._credentials.MwsAuthToken
				};
				var service = new FbaInventorySupplyService( client, request );

				AmazonLogger.Log.Trace( "[amazon] Loading FBA inventory for seller {0}", this._credentials.SellerId );

				inventory.AddRange( service.LoadInventory() );

				AmazonLogger.Log.Trace( "[amazon] FBA inventiry for seller {0} loaded", this._credentials.SellerId );
			} );

			return inventory;
		}

		public bool IsFbaInventoryReceived()
		{
			try
			{
				var client = this._factory.CreateFbaInventoryClient();
				var request = new ListInventorySupplyRequest
				{
					SellerId = this._credentials.SellerId,
					QueryStartDateTime = DateTime.MinValue,
					ResponseGroup = "Detailed",
					MWSAuthToken = this._credentials.MwsAuthToken
				};
				var service = new FbaInventorySupplyService( client, request );
				return service.IsInventoryReceived();
			}
			catch( Exception ex )
			{
				AmazonLogger.Log.Warn( ex, "[amazon] Checking FBA inventory for seller {0} failed", this._credentials.SellerId );
				return false;
			}
		}

		public IEnumerable< FbaManageInventory > GetDetailedFbaInventory()
		{
			var inventory = new List< FbaManageInventory >();

			ActionPolicies.Get.Do( () =>
			{
				var client = this._factory.CreateFeedsReportsClient();
				var service = new ReportsService( client, this._credentials );

				AmazonLogger.Log.Trace( "[amazon] Loading Detailed FBA inventory for seller {0}", this._credentials.SellerId );

				inventory.AddRange( service.GetReport< FbaManageInventory >(
					ReportType.FbaManageInventoryArchived,
					DateTime.UtcNow.AddDays( -90 ).ToUniversalTime(),
					DateTime.UtcNow.ToUniversalTime() ) );

				AmazonLogger.Log.Trace( "[amazon] Detailed FBA inventiry for seller {0} loaded", this._credentials.SellerId );
			} );

			return inventory;
		}
		#endregion

		#region Sellers
		public MarketplaceParticipations GetMarketplaceParticipations()
		{
			var marker = Guid.NewGuid().ToString();
			var client = this._factory.CreateSellersClient();
			var service = new SellerMarketplaceService( client, this._credentials );
			var result = service.GetMarketplaceParticipations( marker );
			return result;
		}
		#endregion
	}
}