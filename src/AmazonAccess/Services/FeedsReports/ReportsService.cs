﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AmazonAccess.Misc;
using AmazonAccess.Models;
using AmazonAccess.Services.FeedsReports.Model;
using CuttingEdge.Conditions;
using LINQtoCSV;

namespace AmazonAccess.Services.FeedsReports
{
	public class ReportsService
	{
		private readonly IFeedReportServiceClient _client;
		private readonly AmazonCredentials _credentials;
		private readonly Throttler _requestReportThrottler = new Throttler( 15, 61 );
		private readonly Throttler _getReportRequestListThrottler = new Throttler( 10, 46 );
		private readonly Throttler _getReportListThrottler = new Throttler( 10, 61 );
		private readonly Throttler _getReportListByNextTokenThrottler = new Throttler( 30, 3 );
		private readonly Throttler _getReportThrottler = new Throttler( 15, 61 );

		public ReportsService( IFeedReportServiceClient client, AmazonCredentials credentials )
		{
			Condition.Requires( client, "client" ).IsNotNull();
			Condition.Requires( credentials, "credentials" ).IsNotNull();

			this._client = client;
			this._credentials = credentials;
		}

		public IEnumerable< T > GetReport< T >( ReportType reportType, DateTime startDate, DateTime endDate, string marker ) where T : class, new()
		{
			AmazonLogger.Trace( "GetReport", this._credentials.SellerId, marker, "Begin invoke" );

			var reportRequestId = this.GetReportRequestId( reportType, startDate, endDate, marker );

			var reportId = this.GetNewReportId( reportRequestId, marker );
			if( string.IsNullOrEmpty( reportId ) )
				reportId = this.GetExistingReportId( reportType, marker );
			if( string.IsNullOrEmpty( reportId ) )
				throw AmazonLogger.Error( "GetReport", this._credentials.SellerId, marker, "Can't request new report or find existing" );

			var reportString = this.GetReportById( reportId, marker );
			if( reportString == null )
				throw AmazonLogger.Error( "GetReport", this._credentials.SellerId, marker, "Can't get report" );

			var report = this.ConvertReport< T >( reportString );
			AmazonLogger.Trace( "GetReport", this._credentials.SellerId, marker, "End invoke" );
			return report;
		}

		private string GetReportRequestId( ReportType reportType, DateTime startDate, DateTime endDate, string marker )
		{
			AmazonLogger.Trace( "GetReportRequestId", this._credentials.SellerId, marker, "Begin invoke" );

			var request = new RequestReportRequest
			{
				SellerId = this._credentials.SellerId,
				MWSAuthToken = this._credentials.MwsAuthToken,
				MarketplaceId = this._credentials.AmazonMarketplaces.GetMarketplaceIdAsList(),
				ReportType = reportType.Description,
				StartDate = startDate,
				EndDate = endDate
			};
			var response = ActionPolicies.Get.Get( () => this._requestReportThrottler.Execute( () => this._client.RequestReport( request, marker ) ) );
			if( response.IsSetRequestReportResult() && response.RequestReportResult.IsSetReportRequestInfo() )
				return response.RequestReportResult.ReportRequestInfo.ReportRequestId;

			return string.Empty;
		}

		private string GetNewReportId( string reportRequestId, string marker )
		{
			AmazonLogger.Trace( "GetNewReportId", this._credentials.SellerId, marker, "Begin invoke" );

			var request = new GetReportRequestListRequest
			{
				SellerId = this._credentials.SellerId,
				MWSAuthToken = this._credentials.MwsAuthToken,
				MarketplaceId = this._credentials.AmazonMarketplaces.GetMarketplaceIdAsList(),
				ReportRequestIdList = new List< string > { reportRequestId },
				RequestedFromDate = DateTime.MinValue.ToUniversalTime(),
				RequestedToDate = DateTime.UtcNow.ToUniversalTime()
			};
			while( true )
			{
				ActionPolicies.CreateApiDelay( 30 ).Wait();

				var response = ActionPolicies.Get.Get( () => this._getReportRequestListThrottler.Execute( () => this._client.GetReportRequestList( request, marker ) ) );
				if( !response.IsSetGetReportRequestListResult() || !response.GetReportRequestListResult.IsSetReportRequestInfo() )
					break;
				var info = response.GetReportRequestListResult.ReportRequestInfo.FirstOrDefault( i => i.ReportRequestId.Equals( reportRequestId ) );
				if( info == null || !info.IsSetReportProcessingStatus() || info.ReportProcessingStatus.Equals( "_CANCELLED_", StringComparison.InvariantCultureIgnoreCase ) )
					break;

				if( !string.IsNullOrEmpty( info.GeneratedReportId ) )
					return info.GeneratedReportId;
			}

			return string.Empty;
		}

		private string GetExistingReportId( ReportType reportType, string marker )
		{
			AmazonLogger.Trace( "GetExistingReportId", this._credentials.SellerId, marker, "Begin invoke" );

			var request = new GetReportListRequest
			{
				SellerId = this._credentials.SellerId,
				MWSAuthToken = this._credentials.MwsAuthToken,
				MarketplaceId = this._credentials.AmazonMarketplaces.GetMarketplaceIdAsList(),
				AvailableFromDate = DateTime.MinValue.ToUniversalTime(),
				AvailableToDate = DateTime.UtcNow.ToUniversalTime()
			};
			var reportListResponse = ActionPolicies.Get.Get( () => this._getReportListThrottler.Execute( () => this._client.GetReportList( request, marker ) ) );
			if( !reportListResponse.IsSetGetReportListResult() || !reportListResponse.GetReportListResult.IsSetReportInfo() )
				return string.Empty;

			var reportInfo = reportListResponse.GetReportListResult.ReportInfo.FirstOrDefault( r => r.ReportType.Equals( reportType.Description ) );
			if( reportInfo != null )
				return reportInfo.ReportId;

			return this.GetExistingReportIdInNextPages( reportListResponse.GetReportListResult.NextToken, reportType.Description, marker );
		}

		private string GetExistingReportIdInNextPages( string nextToken, string reportType, string marker )
		{
			while( !string.IsNullOrEmpty( nextToken ) )
			{
				AmazonLogger.Trace( "GetExistingReportIdInNextPages", this._credentials.SellerId, marker, "NextToken:{0}", nextToken );

				var request = new GetReportListByNextTokenRequest
				{
					SellerId = this._credentials.SellerId,
					MWSAuthToken = this._credentials.MwsAuthToken,
					MarketplaceId = this._credentials.AmazonMarketplaces.GetMarketplaceIdAsList(),
					NextToken = nextToken
				};
				var response = ActionPolicies.Get.Get( () => this._getReportListByNextTokenThrottler.Execute( () => this._client.GetReportListByNextToken( request, marker ) ) );
				if( !response.IsSetGetReportListByNextTokenResult() || !response.GetReportListByNextTokenResult.IsSetReportInfo() )
					return string.Empty;

				var reportInfo = response.GetReportListByNextTokenResult.ReportInfo.FirstOrDefault( r => r.ReportType.Equals( reportType ) );
				if( reportInfo != null )
					return reportInfo.ReportId;

				nextToken = response.GetReportListByNextTokenResult.NextToken;
			}
			return string.Empty;
		}

		private string GetReportById( string reportId, string marker )
		{
			AmazonLogger.Trace( "GetReportById", this._credentials.SellerId, marker, "Begin invoke" );

			var request = new GetReportRequest
			{
				SellerId = this._credentials.SellerId,
				MWSAuthToken = this._credentials.MwsAuthToken,
				MarketplaceId = this._credentials.AmazonMarketplaces.GetMarketplaceIdAsList(),
				ReportId = reportId
			};
			var response = ActionPolicies.Get.Get( () => this._getReportThrottler.Execute( () => this._client.GetReport( request, marker ) ) );
			if( response.IsSetGetReportResult() && response.GetReportResult.IsSetResult() )
				return response.GetReportResult.Result;

			return null;
		}

		private IEnumerable< T > ConvertReport< T >( string reportString ) where T : class, new()
		{
			using( var ms = new MemoryStream( Encoding.UTF8.GetBytes( reportString ) ) )
			{
				var reader = new StreamReader( ms );
				var cc = new CsvContext();
				var report = cc.Read< T >( reader, new CsvFileDescription { FirstLineHasColumnNames = true, SeparatorChar = '\t' } );
				return report.ToList();
			}
		}
	}
}