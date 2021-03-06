﻿using System;
using CuttingEdge.Conditions;
using Netco.Extensions;

namespace AmazonAccess.Models
{
	public sealed class AmazonMarketplace
	{
		public string MarketplaceId{ get; private set; }
		public bool IsAmazonMarketplace{ get; private set; }
		public string Endpoint{ get; private set; }
		public string OrdersServiceUrl{ get; private set; }
		public string ProductsServiceUrl{ get; private set; }
		public string FbaInventoryServiceUrl{ get; private set; }
		public string FbaInboundServiceUrl{ get; private set; }
		public string FeedsServiceUrl{ get; private set; }
		public string SellersServiceUrl{ get; private set; }
		public string FinancesServiceUrl { get; private set; }
		public AmazonRegionCodeEnum RegionCode{ get; private set; }
		public AmazonCountryCodeEnum CountryCode{ get; private set; }

		public AmazonMarketplace( string countryCode, string marketplaceId = null )
			: this( countryCode.ToEnum( AmazonCountryCodeEnum.Unknown ), marketplaceId )
		{
		}

		public AmazonMarketplace( AmazonCountryCodeEnum countryCode, string marketplaceId = null )
		{
			Condition.Requires( countryCode, "countryCode" ).IsGreaterThan( AmazonCountryCodeEnum.Unknown );
			this.InitMarketplace( countryCode, marketplaceId );
		}

		public static AmazonMarketplace CreateForRegion( string region )
		{
			return CreateForRegion( region.ToEnum( AmazonRegionCodeEnum.Unknown ) );
		}

		public static AmazonMarketplace CreateForRegion( AmazonRegionCodeEnum regionCode )
		{
			Condition.Requires( regionCode, "regionCode" ).IsGreaterThan( AmazonRegionCodeEnum.Unknown );

			var countryCode = GetDefaultCountryCodeForRegion( regionCode );
			return new AmazonMarketplace( countryCode );
		}

		private static AmazonCountryCodeEnum GetDefaultCountryCodeForRegion( AmazonRegionCodeEnum regionCode )
		{
			AmazonCountryCodeEnum countryCode;
			switch( regionCode )
			{
				case AmazonRegionCodeEnum.Na:
					countryCode = AmazonCountryCodeEnum.Us;
					break;
				case AmazonRegionCodeEnum.Eu:
					countryCode = AmazonCountryCodeEnum.Uk;
					break;
				case AmazonRegionCodeEnum.Fe:
					countryCode = AmazonCountryCodeEnum.Jp;
					break;
				case AmazonRegionCodeEnum.Cn:
					countryCode = AmazonCountryCodeEnum.Cn;
					break;
				default:
					throw new Exception( "Incorrect region" );
			}
			return countryCode;
		}

		private void InitMarketplace( AmazonCountryCodeEnum countryCode, string marketplaceId )
		{
			this.CountryCode = countryCode;
			this.IsAmazonMarketplace = true;
			switch( countryCode )
			{
				case AmazonCountryCodeEnum.Ca:
					this.RegionCode = AmazonRegionCodeEnum.Na;
					this.MarketplaceId = "A2EUQ1WTGCTBG2";
					this.Endpoint = "https://mws.amazonservices.ca";
					break;
				case AmazonCountryCodeEnum.Us:
					this.RegionCode = AmazonRegionCodeEnum.Na;
					this.MarketplaceId = "ATVPDKIKX0DER";
					this.Endpoint = "https://mws.amazonservices.com";
					break;
				case AmazonCountryCodeEnum.Mx:
					this.RegionCode = AmazonRegionCodeEnum.Na;
					this.MarketplaceId = "A1AM78C64UM0Y8";
					this.Endpoint = "https://mws.amazonservices.com.mx";
					break;
				case AmazonCountryCodeEnum.Br:
					this.RegionCode = AmazonRegionCodeEnum.Na;
					this.MarketplaceId = "A2Q3Y263D00KWC";
					this.Endpoint = "https://mws.amazonservices.com";
					break;

				case AmazonCountryCodeEnum.Ae:
					this.RegionCode = AmazonRegionCodeEnum.Eu;
					this.MarketplaceId = "A2VIGQ35RCS4UG";
					this.Endpoint = "https://mws.amazonservices.ae";
					break;
				case AmazonCountryCodeEnum.De:
					this.RegionCode = AmazonRegionCodeEnum.Eu;
					this.MarketplaceId = "A1PA6795UKMFR9";
					this.Endpoint = "https://mws-eu.amazonservices.com";
					break;
				case AmazonCountryCodeEnum.Es:
					this.RegionCode = AmazonRegionCodeEnum.Eu;
					this.MarketplaceId = "A1RKKUPIHCS9HS";
					this.Endpoint = "https://mws-eu.amazonservices.com";
					break;
				case AmazonCountryCodeEnum.Fr:
					this.RegionCode = AmazonRegionCodeEnum.Eu;
					this.MarketplaceId = "A13V1IB3VIYZZH";
					this.Endpoint = "https://mws-eu.amazonservices.com";
					break;
				case AmazonCountryCodeEnum.In:
					this.RegionCode = AmazonRegionCodeEnum.Eu;
					this.MarketplaceId = "A21TJRUUN4KGV";
					this.Endpoint = "https://mws.amazonservices.in";
					break;
				case AmazonCountryCodeEnum.It:
					this.RegionCode = AmazonRegionCodeEnum.Eu;
					this.MarketplaceId = "APJ6JRA9NG5V4";
					this.Endpoint = "https://mws-eu.amazonservices.com";
					break;
				case AmazonCountryCodeEnum.Tr:
					this.RegionCode = AmazonRegionCodeEnum.Eu;
					this.MarketplaceId = "A33AVAJ2PDY3EV";
					this.Endpoint = "https://mws-eu.amazonservices.com";
					break;
				case AmazonCountryCodeEnum.Uk:
				case AmazonCountryCodeEnum.Gb:
					this.RegionCode = AmazonRegionCodeEnum.Eu;
					this.MarketplaceId = "A1F83G8C2ARO7P";
					this.Endpoint = "https://mws-eu.amazonservices.com";
					break;
				case AmazonCountryCodeEnum.Nl:
					this.RegionCode = AmazonRegionCodeEnum.Eu;
					this.MarketplaceId = "A1805IZSGTT6HS";
					this.Endpoint = "https://mws-eu.amazonservices.com";
					break;
				case AmazonCountryCodeEnum.Se:
					this.RegionCode = AmazonRegionCodeEnum.Eu;
					this.MarketplaceId = "A2NODRKZP88ZB9";
					this.Endpoint = "https://mws-eu.amazonservices.com";
					break;
				case AmazonCountryCodeEnum.Sa:
					this.RegionCode = AmazonRegionCodeEnum.Eu;
					this.MarketplaceId = "A17E79C6D8DWNP";
					this.Endpoint = "https://mws-eu.amazonservices.com";
					break;
				case AmazonCountryCodeEnum.Eg:
					this.RegionCode = AmazonRegionCodeEnum.Eu;
					this.MarketplaceId = "ARBP9OOSHTCHU";
					this.Endpoint = "https://mws-eu.amazonservices.com";
					break;
				case AmazonCountryCodeEnum.Pl:
					this.RegionCode = AmazonRegionCodeEnum.Eu;
					this.MarketplaceId = "A1C3SOZRARQ6R3";
					this.Endpoint = "https://mws-eu.amazonservices.com";
					break;

				case AmazonCountryCodeEnum.Au:
					this.RegionCode = AmazonRegionCodeEnum.Fe;
					this.MarketplaceId = "A39IBJ37TRP1C6";
					this.Endpoint = "https://mws.amazonservices.com.au";
					break;
				case AmazonCountryCodeEnum.Jp:
					this.RegionCode = AmazonRegionCodeEnum.Fe;
					this.MarketplaceId = "A1VC38T7YXB528";
					this.Endpoint = "https://mws.amazonservices.jp";
					break;
				case AmazonCountryCodeEnum.Sg:
					this.RegionCode = AmazonRegionCodeEnum.Fe;
					this.MarketplaceId = "A19VAU5U5O7RUS";
					this.Endpoint = "https://mws-fe.amazonservices.com";
					break;

				case AmazonCountryCodeEnum.Cn:
					this.RegionCode = AmazonRegionCodeEnum.Cn;
					this.MarketplaceId = "AAHKV2X7AFYLW";
					this.Endpoint = "https://mws.amazonservices.com.cn";
					break;
				default:
					throw new Exception( "Incorrect country code" );
			}

			if( !string.IsNullOrWhiteSpace( marketplaceId ) && !this.MarketplaceId.Equals( marketplaceId, StringComparison.InvariantCultureIgnoreCase ) )
			{
				this.MarketplaceId = marketplaceId;
				this.IsAmazonMarketplace = false;
			}

			this.OrdersServiceUrl = this.Endpoint + "/Orders/2013-09-01";
			this.ProductsServiceUrl = this.Endpoint + "/Products/2011-10-01";
			this.FbaInventoryServiceUrl = this.Endpoint + "/FulfillmentInventory/2010-10-01";
			this.FbaInboundServiceUrl = this.Endpoint + "/FulfillmentInboundShipment/2010-10-01";
			this.FeedsServiceUrl = this.Endpoint;
			this.SellersServiceUrl = this.Endpoint + "/Sellers/2011-07-01";
			this.FinancesServiceUrl = this.Endpoint + "/Finances/2015-05-01";
		}
	}

	public enum AmazonRegionCodeEnum
	{
		Unknown,
		Na,
		Eu,
		Fe,
		Cn
	}

	public enum AmazonCountryCodeEnum
	{
		Unknown,
		Ca,
		Us,
		Mx,
		Br,
		Ae,
		De,
		Es,
		Fr,
		In,
		It,
		Tr,
		Uk,
		Gb,
		Jp,
		Au,
		Cn,
		Nl,
		Se,
		Eg,
		Sa,
		Sg,
		Pl
	}
}