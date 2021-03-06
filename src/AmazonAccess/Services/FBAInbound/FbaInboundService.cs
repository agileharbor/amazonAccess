﻿/******************************************************************************* 
 *  Copyright 2009 Amazon Services. All Rights Reserved.
 *  Licensed under the Apache License, Version 2.0 (the "License"); 
 *  
 *  You may not use this file except in compliance with the License. 
 *  You may obtain a copy of the License at: http://aws.amazon.com/apache2.0
 *  This file is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
 *  CONDITIONS OF ANY KIND, either express or implied. See the License for the 
 *  specific language governing permissions and limitations under the License.
 * ***************************************************************************** 
 * 
 *  FBA Inventory Service MWS CSharp Library
 *  API Version: 2010-10-01
 *  Generated: Fri Oct 22 09:53:30 UTC 2010 
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using AmazonAccess.Misc;
using AmazonAccess.Models;
using AmazonAccess.Services.FBAInbound;
using AmazonAccess.Services.FBAInbound.Model;
using CuttingEdge.Conditions;

namespace AmazonAccess.Services.FbaInbound
{
	public class FbaInboundService
	{
		private readonly IFbaInboundServiceClient _client;
		private readonly AmazonCredentials _credentials;
		private readonly Throttler _listInboundShipmentsThrottler = new Throttler( 30, 1, 2 );
		private readonly Throttler _listInboundShipmentItemsThrottler = new Throttler( 30, 1, 2 );
		private readonly Throttler _createInboundShipmentPlanThrottler = new Throttler( 30, 1, 2 );
		private readonly Throttler _createInboundShipmentThrottler = new Throttler( 30, 1, 2 );
		private readonly Throttler _updateInboundShipmentThrottler = new Throttler( 30, 1, 2 );

		/// <param name="client">Instance of FBAInventoryServiceMWS client</param>
		/// <param name="credentials">credentials</param>
		public FbaInboundService( IFbaInboundServiceClient client, AmazonCredentials credentials )
		{
			Condition.Requires( client, "client" ).IsNotNull();
			Condition.Requires( credentials, "credentials" ).IsNotNull();

			this._client = client;
			this._credentials = credentials;
		}

		public List< InboundShipmentFullInfo > GetInboundShipmentsData( string marker, DateTime dateFrom, DateTime dateTo, List< string > shipmentStatusListForReceive, List< string > shipmentStatusListForReceiveItems )
		{
			var listInboundShipments = this.GetListInboundShipments( marker, dateFrom, dateTo, shipmentStatusListForReceive ).ToList();
			var result = new List< InboundShipmentFullInfo >();
			foreach( var inboundShipment in listInboundShipments )
			{
				if( shipmentStatusListForReceiveItems.Any( t => string.Equals( t, inboundShipment.ShipmentStatus, StringComparison.InvariantCultureIgnoreCase ) ) )
					result.Add( new InboundShipmentFullInfo( inboundShipment, this.GetListInboundShipmentItems( inboundShipment.ShipmentId, marker ).ToList() ) );
				else
					result.Add( new InboundShipmentFullInfo( inboundShipment ) );
			}

			return result;
		}

		private List< InboundShipmentInfo > GetListInboundShipments( string marker, DateTime dateFrom, DateTime dateTo, List< string > shipmentStatusListForReceive)
		{
			AmazonLogger.Trace( "ListInboundShipments", this._credentials.SellerId, marker, "Begin invoke" );

			var marketplaces = this._credentials.AmazonMarketplaces.GetMarketplaceIdAsList();
			var request = new ListInboundShipmentsRequest
			{
				SellerId = this._credentials.SellerId,
				MWSAuthToken = this._credentials.MwsAuthToken,
				LastUpdatedAfter = dateFrom,
				LastUpdatedBefore = dateTo,
				ShipmentStatusList = new ShipmentStatusList() { member = shipmentStatusListForReceive }
			};
			var result = new List< InboundShipmentInfo >();
			var response = ActionPolicies.Get.Get( () => this._listInboundShipmentsThrottler.Execute( () => this._client.ListInboundShipments( request, marker ) ) );
			if( response.IsSetListInboundShipmentsResult() )
			{
				if( response.ListInboundShipmentsResult.IsSetShipmentData() )
					result.AddRange( response.ListInboundShipmentsResult.ShipmentData.member );
				this.AddFbaInboundShipmentsFromOtherPages( marker, response.ListInboundShipmentsResult.NextToken, result );
			}

			AmazonLogger.Trace( "LoadFbaInventory", this._credentials.SellerId, marker, "End invoke" );
			return result;
		}

		private void AddFbaInboundShipmentsFromOtherPages( string marker, string nextToken, List< InboundShipmentInfo > result )
		{
			while( !string.IsNullOrEmpty( nextToken ) )
			{
				AmazonLogger.Trace( "AddFbaInboundShipmentsFromOtherPages", this._credentials.SellerId, marker, "NextToken:{0}", nextToken );

				var request = new ListInboundShipmentsByNextTokenRequest
				{
					SellerId = this._credentials.SellerId,
					MWSAuthToken = this._credentials.MwsAuthToken,
					NextToken = nextToken
				};
				var response = ActionPolicies.Get.Get( () => this._listInboundShipmentsThrottler.Execute( () => this._client.ListInboundShipmentsByNextToken( request, marker ) ) );
				if( response.IsSetListInboundShipmentsByNextTokenResult() )
				{
					if( response.ListInboundShipmentsByNextTokenResult.IsSetShipmentData() )
						result.AddRange( response.ListInboundShipmentsByNextTokenResult.ShipmentData.member );
					nextToken = response.ListInboundShipmentsByNextTokenResult.NextToken;
				}
			}
		}

		private IEnumerable< InboundShipmentItem > GetListInboundShipmentItems( string shipmentId, string marker )
		{
			AmazonLogger.Trace( "ListInboundShipmentItems", this._credentials.SellerId, marker, "Begin invoke" );

			var marketplaces = this._credentials.AmazonMarketplaces.GetMarketplaceIdAsList();
			var request = new ListInboundShipmentItemsRequest
			{
				SellerId = this._credentials.SellerId,
				MWSAuthToken = this._credentials.MwsAuthToken,
				ShipmentId = shipmentId
			};
			var result = new List< InboundShipmentItem >();
			var response = ActionPolicies.Get.Get( () => this._listInboundShipmentItemsThrottler.Execute( () => this._client.ListInboundShipmentItems( request, marker ) ) );
			if( response.IsSetListInboundShipmentItemsResult() )
			{
				if( response.ListInboundShipmentItemsResult.IsSetItemData() )
					result.AddRange( response.ListInboundShipmentItemsResult.ItemData.member );
				this.AddFbaInboundShipmentItemsFromOtherPages( marker, response.ListInboundShipmentItemsResult.NextToken, result );
			}

			AmazonLogger.Trace( "LoadFbaInventory", this._credentials.SellerId, marker, "End invoke" );
			return result;
		}

		private void AddFbaInboundShipmentItemsFromOtherPages( string marker, string nextToken, List< InboundShipmentItem > result )
		{
			while( !string.IsNullOrEmpty( nextToken ) )
			{
				AmazonLogger.Trace( "AddFbaInboundShipmentItemsFromOtherPages", this._credentials.SellerId, marker, "NextToken:{0}", nextToken );

				var request = new ListInboundShipmentItemsByNextTokenRequest
				{
					SellerId = this._credentials.SellerId,
					MWSAuthToken = this._credentials.MwsAuthToken,
					NextToken = nextToken
				};
				var response = ActionPolicies.Get.Get( () => this._listInboundShipmentItemsThrottler.Execute( () => this._client.ListInboundShipmentItemsByNextToken( request, marker ) ) );
				if( response.IsSetListInboundShipmentItemsByNextTokenResult() )
				{
					if( response.ListInboundShipmentItemsByNextTokenResult.IsSetItemData() )
						result.AddRange( response.ListInboundShipmentItemsByNextTokenResult.ItemData.member );
					nextToken = response.ListInboundShipmentItemsByNextTokenResult.NextToken;
				}
			}
		}

		public IEnumerable< InboundShipmentPlan > CreateInboundShipmentPlan( Address shipFromAddress, 
			string shipToCountryCode, 
			string shipToCountrySubdivisionCode, 
			string labelPrepPreference, 
			InboundShipmentPlanRequestItemList inboundShipmentPlanRequestItems, 
			string marker )
		{
			AmazonLogger.Trace( "CreateInboundShipmentPlan", this._credentials.SellerId, marker, "Begin invoke" );

			var request = new CreateInboundShipmentPlanRequest
			{
				SellerId = this._credentials.SellerId,
				MWSAuthToken = this._credentials.MwsAuthToken,
				InboundShipmentPlanRequestItems = inboundShipmentPlanRequestItems,
				LabelPrepPreference = labelPrepPreference,
				ShipFromAddress = shipFromAddress,
				ShipToCountryCode = shipToCountryCode,
				ShipToCountrySubdivisionCode = shipToCountrySubdivisionCode
			};

			var result = new List< InboundShipmentPlan >();
			var response = this._createInboundShipmentPlanThrottler.Execute( () => this._client.CreateInboundShipmentPlan( request, marker ) );
			if( response.IsSetCreateInboundShipmentPlanResult() )
			{
				if( response.CreateInboundShipmentPlanResult.IsSetInboundShipmentPlans() )
					result.AddRange( response.CreateInboundShipmentPlanResult.InboundShipmentPlans.member );
			}

			AmazonLogger.Trace( "CreateInboundShipmentPlan", this._credentials.SellerId, marker, "End invoke" );
			return result;
		}

		public string CreateInboundShipment( string shipmentId, InboundShipmentHeader header, InboundShipmentItemList items, string marker )
		{
			AmazonLogger.Trace( "CreateInboundShipment", this._credentials.SellerId, marker, "Begin invoke" );

			var request = new CreateInboundShipmentRequest
			{
				MWSAuthToken = this._credentials.MwsAuthToken,
				InboundShipmentHeader = header,
				InboundShipmentItems = items,
				ShipmentId = shipmentId,
				SellerId = this._credentials.SellerId
			};

			var result = string.Empty;
			var response = this._createInboundShipmentThrottler.Execute( () => this._client.CreateInboundShipment( request, marker ) );
			if( response.IsSetCreateInboundShipmentResult() )
			{
				if( response.CreateInboundShipmentResult.IsSetShipmentId() )
					result = response.CreateInboundShipmentResult.ShipmentId;
			}

			AmazonLogger.Trace( "CreateInboundShipment", this._credentials.SellerId, marker, "End invoke" );
			return result;
		}

		public string UpdateInboundShipment( string shipmentId, InboundShipmentHeader header, InboundShipmentItemList items, string marker )
		{
			AmazonLogger.Trace( "UpdateInboundShipment", this._credentials.SellerId, marker, "Begin invoke" );

			var request = new UpdateInboundShipmentRequest
			{
				SellerId = this._credentials.SellerId,
				MWSAuthToken = this._credentials.MwsAuthToken,
				InboundShipmentHeader = header,
				InboundShipmentItems = items,
				ShipmentId = shipmentId
			};

			var result = string.Empty;
			var response = this._updateInboundShipmentThrottler.Execute( () => this._client.UpdateInboundShipment( request, marker ) );
			if( response.IsSetUpdateInboundShipmentResult() )
			{
				if( response.UpdateInboundShipmentResult.IsSetShipmentId() )
					result = response.UpdateInboundShipmentResult.ShipmentId;
			}

			AmazonLogger.Trace( "UpdateInboundShipment", this._credentials.SellerId, marker, "End invoke" );
			return result;
		}
	}
}