/*******************************************************************************
 * Copyright 2009-2015 Amazon Services. All Rights Reserved.
 * Licensed under the Apache License, Version 2.0 (the "License"); 
 *
 * You may not use this file except in compliance with the License. 
 * You may obtain a copy of the License at: http://aws.amazon.com/apache2.0
 * This file is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
 * CONDITIONS OF ANY KIND, either express or implied. See the License for the 
 * specific language governing permissions and limitations under the License.
 *******************************************************************************
 * List Order Items Request
 * API Version: 2013-09-01
 * Library Version: 2015-09-24
 * Generated: Fri Sep 25 20:06:25 GMT 2015
 */

using AmazonAccess.Services.Common;

namespace AmazonAccess.Services.Orders.Model
{
	public class ListOrderItemsRequest: AbstractMwsObject
	{
		/// <summary>
		/// Gets and sets the SellerId property.
		/// </summary>
		public string SellerId{ get; set; }

		/// <summary>
		/// Sets the SellerId property.
		/// </summary>
		/// <param name="sellerId">SellerId property.</param>
		/// <returns>this instance.</returns>
		public ListOrderItemsRequest WithSellerId( string sellerId )
		{
			this.SellerId = sellerId;
			return this;
		}

		/// <summary>
		/// Checks if SellerId property is set.
		/// </summary>
		/// <returns>true if SellerId property is set.</returns>
		public bool IsSetSellerId()
		{
			return this.SellerId != null;
		}

		/// <summary>
		/// Gets and sets the MWSAuthToken property.
		/// </summary>
		public string MWSAuthToken{ get; set; }

		/// <summary>
		/// Sets the MWSAuthToken property.
		/// </summary>
		/// <param name="mwsAuthToken">MWSAuthToken property.</param>
		/// <returns>this instance.</returns>
		public ListOrderItemsRequest WithMWSAuthToken( string mwsAuthToken )
		{
			this.MWSAuthToken = mwsAuthToken;
			return this;
		}

		/// <summary>
		/// Checks if MWSAuthToken property is set.
		/// </summary>
		/// <returns>true if MWSAuthToken property is set.</returns>
		public bool IsSetMWSAuthToken()
		{
			return this.MWSAuthToken != null;
		}

		/// <summary>
		/// Gets and sets the AmazonOrderId property.
		/// </summary>
		public string AmazonOrderId{ get; set; }

		/// <summary>
		/// Sets the AmazonOrderId property.
		/// </summary>
		/// <param name="amazonOrderId">AmazonOrderId property.</param>
		/// <returns>this instance.</returns>
		public ListOrderItemsRequest WithAmazonOrderId( string amazonOrderId )
		{
			this.AmazonOrderId = amazonOrderId;
			return this;
		}

		/// <summary>
		/// Checks if AmazonOrderId property is set.
		/// </summary>
		/// <returns>true if AmazonOrderId property is set.</returns>
		public bool IsSetAmazonOrderId()
		{
			return this.AmazonOrderId != null;
		}

		public override void ReadFragmentFrom( IMwsReader reader )
		{
			this.SellerId = reader.Read< string >( "SellerId" );
			this.MWSAuthToken = reader.Read< string >( "MWSAuthToken" );
			this.AmazonOrderId = reader.Read< string >( "AmazonOrderId" );
		}

		public override void WriteFragmentTo( IMwsWriter writer )
		{
			writer.Write( "SellerId", this.SellerId );
			writer.Write( "MWSAuthToken", this.MWSAuthToken );
			writer.Write( "AmazonOrderId", this.AmazonOrderId );
		}

		public override void WriteTo( IMwsWriter writer )
		{
			writer.Write( "https://mws.amazonservices.com/Orders/2013-09-01", "ListOrderItemsRequest", this );
		}

		public ListOrderItemsRequest(): base()
		{
		}
	}
}