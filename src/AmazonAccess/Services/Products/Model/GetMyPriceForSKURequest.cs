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
 * Get My Price For SKU Request
 * API Version: 2011-10-01
 * Library Version: 2015-09-01
 * Generated: Thu Sep 10 06:52:19 PDT 2015
 */

using System.Xml.Serialization;
using AmazonAccess.Services.Common;

namespace AmazonAccess.Services.Products.Model
{
	[ XmlType( Namespace = "http://mws.amazonservices.com/schema/Products/2011-10-01" ) ]
	[ XmlRoot( Namespace = "http://mws.amazonservices.com/schema/Products/2011-10-01", IsNullable = false ) ]
	public class GetMyPriceForSKURequest: AbstractMwsObject
	{
		/// <summary>
		/// Gets and sets the SellerId property.
		/// </summary>
		[ XmlElement( ElementName = "SellerId" ) ]
		public string SellerId{ get; set; }

		/// <summary>
		/// Sets the SellerId property.
		/// </summary>
		/// <param name="sellerId">SellerId property.</param>
		/// <returns>this instance.</returns>
		public GetMyPriceForSKURequest WithSellerId( string sellerId )
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
		[ XmlElement( ElementName = "MWSAuthToken" ) ]
		public string MWSAuthToken{ get; set; }

		/// <summary>
		/// Sets the MWSAuthToken property.
		/// </summary>
		/// <param name="mwsAuthToken">MWSAuthToken property.</param>
		/// <returns>this instance.</returns>
		public GetMyPriceForSKURequest WithMWSAuthToken( string mwsAuthToken )
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
		/// Gets and sets the MarketplaceId property.
		/// </summary>
		[ XmlElement( ElementName = "MarketplaceId" ) ]
		public string MarketplaceId{ get; set; }

		/// <summary>
		/// Sets the MarketplaceId property.
		/// </summary>
		/// <param name="marketplaceId">MarketplaceId property.</param>
		/// <returns>this instance.</returns>
		public GetMyPriceForSKURequest WithMarketplaceId( string marketplaceId )
		{
			this.MarketplaceId = marketplaceId;
			return this;
		}

		/// <summary>
		/// Checks if MarketplaceId property is set.
		/// </summary>
		/// <returns>true if MarketplaceId property is set.</returns>
		public bool IsSetMarketplaceId()
		{
			return this.MarketplaceId != null;
		}

		/// <summary>
		/// Gets and sets the SellerSKUList property.
		/// </summary>
		[ XmlElement( ElementName = "SellerSKUList" ) ]
		public SellerSKUListType SellerSKUList{ get; set; }

		/// <summary>
		/// Sets the SellerSKUList property.
		/// </summary>
		/// <param name="sellerSKUList">SellerSKUList property.</param>
		/// <returns>this instance.</returns>
		public GetMyPriceForSKURequest WithSellerSKUList( SellerSKUListType sellerSKUList )
		{
			this.SellerSKUList = sellerSKUList;
			return this;
		}

		/// <summary>
		/// Checks if SellerSKUList property is set.
		/// </summary>
		/// <returns>true if SellerSKUList property is set.</returns>
		public bool IsSetSellerSKUList()
		{
			return this.SellerSKUList != null;
		}

		public override void ReadFragmentFrom( IMwsReader reader )
		{
			this.SellerId = reader.Read< string >( "SellerId" );
			this.MWSAuthToken = reader.Read< string >( "MWSAuthToken" );
			this.MarketplaceId = reader.Read< string >( "MarketplaceId" );
			this.SellerSKUList = reader.Read< SellerSKUListType >( "SellerSKUList" );
		}

		public override void WriteFragmentTo( IMwsWriter writer )
		{
			writer.Write( "SellerId", this.SellerId );
			writer.Write( "MWSAuthToken", this.MWSAuthToken );
			writer.Write( "MarketplaceId", this.MarketplaceId );
			writer.Write( "SellerSKUList", this.SellerSKUList );
		}

		public override void WriteTo( IMwsWriter writer )
		{
			writer.Write( "http://mws.amazonservices.com/schema/Products/2011-10-01", "GetMyPriceForSKURequest", this );
		}

		public GetMyPriceForSKURequest(): base()
		{
		}
	}
}