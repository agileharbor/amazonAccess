﻿using System;
using System.Linq;
using AmazonAccess;
using AmazonAccess.Models;
using FluentAssertions;
using LINQtoCSV;
using NUnit.Framework;

namespace AmazonAccessTests.Orders
{
	internal class OrdersTests
	{
		private IAmazonFactory AmazonFactory;
		private TestConfig Config;

		[ SetUp ]
		public void Init()
		{
			const string credentialsFilePath = @"..\..\Files\AmazonCredentials.csv";

			var cc = new CsvContext();
			this.Config = cc.Read< TestConfig >( credentialsFilePath, new CsvFileDescription { FirstLineHasColumnNames = true } ).FirstOrDefault();
			var marketplace = new AmazonMarketplace( CountryCodesEnum.Us );

			if( this.Config != null )
				this.AmazonFactory = new AmazonFactory( this.Config.AccessKeyId, this.Config.SecretAccessKeyId, marketplace );
		}

		[ Test ]
		public void LoadOrders()
		{
			var service = this.AmazonFactory.CreateService( this.Config.SellerId );

			var orders = service.GetOrders( DateTime.UtcNow - TimeSpan.FromDays( 2 ), DateTime.UtcNow );
			orders.Count().Should().BeGreaterThan( 0 );
		}
	}
}