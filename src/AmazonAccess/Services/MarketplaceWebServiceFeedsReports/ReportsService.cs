﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AmazonAccess.Models;
using AmazonAccess.Services.MarketplaceWebServiceFeedsReports.Model;
using LINQtoCSV;

namespace AmazonAccess.Services.MarketplaceWebServiceFeedsReports
{
	public class ReportsService
	{
		private readonly IMarketplaceWebServiceFeedsReports _client;
		private readonly AmazonCredentials _credentials;

		public ReportsService( IMarketplaceWebServiceFeedsReports client, AmazonCredentials credentials )
		{
			this._client = client;
			this._credentials = credentials;
		}

		public IEnumerable< T > GetInventoryReport< T >( RequestReportRequest request ) where T : class, new()
		{
			var reportId = this.GetReportId( request );
			var getReportRequest = new GetReportRequest
			{
				Merchant = this._credentials.SellerId,
				MWSAuthToken = this._credentials.MwsAuthToken,
				ReportId = reportId,
				Report = new MemoryStream()
			};
			var response = this._client.GetReport( getReportRequest );

			if( response.IsSetGetReportResult() && getReportRequest.Report != null )
				return this.ConvertReport< T >( getReportRequest.Report );
			return new List< T >();
		}

		private IEnumerable< T > ConvertReport< T >( Stream stream ) where T : class, new()
		{
			var reader = new StreamReader( stream, Encoding.UTF8 );
			var cc = new CsvContext();
			var report = cc.Read< T >( reader, new CsvFileDescription { FirstLineHasColumnNames = true, SeparatorChar = '\t' } );
			return report;
		}

		private string GetReportId( RequestReportRequest request )
		{
			var reportRequestId = "278910016589";
			//this.GetReportRequestId( new RequestReportRequest
			//{
			//	Merchant = this._credentials.SellerId,
			//	MWSAuthToken = this._credentials.MwsAuthToken,
			//	MarketplaceIdList = new IdList { Id = new List< string > { this._credentials.AmazonMarketplace.MarketplaceId } },
			//	ReportType = request.ReportType,
			//	StartDate = request.StartDate,
			//	EndDate = request.EndDate
			//} );
			var reportId = this.GetGeneratedReportId( reportRequestId );
			if( !string.IsNullOrEmpty( reportId ) )
				return reportId;

			var reportListResponse = this._client.GetReportList( new GetReportListRequest
			{
				Merchant = this._credentials.SellerId,
				MWSAuthToken = this._credentials.MwsAuthToken,
				AvailableFromDate = request.StartDate,
				AvailableToDate = request.EndDate
			} );
			if( reportListResponse.IsSetGetReportListResult() )
			{
				var reportListResult = reportListResponse.GetReportListResult;
				if( reportListResult.IsSetReportInfo() )
				{
					var reportInfo = reportListResult.ReportInfo.FirstOrDefault( r => r.ReportRequestId.Equals( reportRequestId ) );
					if( reportInfo != null )
						reportId = reportInfo.ReportId;
					else if( reportListResult.IsSetNextToken() )
					{
						var nextResponse = this._client.GetReportListByNextToken( new GetReportListByNextTokenRequest
						{
							Merchant = this._credentials.SellerId,
							MWSAuthToken = this._credentials.MwsAuthToken,
							NextToken = reportListResult.NextToken
						} );

						reportId = this.GetReportIdInNextPages( nextResponse.GetReportListByNextTokenResult, reportRequestId, reportId );
					}
				}
			}
			return reportId;
		}

		private string GetReportIdInNextPages( GetReportListByNextTokenResult reportListByNextTokenResult, string reportRequestId, string reportId )
		{
			if( !string.IsNullOrEmpty( reportId ) )
				return reportId;

			if( reportListByNextTokenResult.IsSetReportInfo() )
			{
				var reportInfo = reportListByNextTokenResult.ReportInfo.FirstOrDefault( r => r.ReportRequestId.Equals( reportRequestId ) );
				if( reportInfo != null )
					reportId = reportInfo.ReportId;
			}
			if( reportListByNextTokenResult.IsSetNextToken() && string.IsNullOrEmpty( reportId ) )
			{
				var nextResponse = this._client.GetReportListByNextToken( new GetReportListByNextTokenRequest
				{
					Merchant = this._credentials.SellerId,
					MWSAuthToken = this._credentials.MwsAuthToken,
					NextToken = reportListByNextTokenResult.NextToken
				} );

				reportId = this.GetReportIdInNextPages( nextResponse.GetReportListByNextTokenResult, reportRequestId, reportId );
			}
			return reportId;
		}

		private string GetGeneratedReportId( string reportRequestId )
		{
			var reportId = string.Empty;
			while( true )
			{
				var response = this._client.GetReportRequestList( new GetReportRequestListRequest
				{
					Merchant = this._credentials.SellerId,
					MWSAuthToken = this._credentials.MwsAuthToken,
					ReportRequestIdList = new IdList { Id = new List< string > { reportRequestId } },
					RequestedFromDate = DateTime.MinValue.ToUniversalTime(),
					RequestedToDate = DateTime.UtcNow.ToUniversalTime()
				} );
				var info = response.GetReportRequestListResult.ReportRequestInfo.FirstOrDefault( i => i.ReportRequestId.Equals( reportRequestId ) );
				if( info == null || !info.IsSetReportProcessingStatus() )
					break;

				if( info.ReportProcessingStatus.Equals( "_IN_PROGRESS_", StringComparison.InvariantCultureIgnoreCase ) )
				{
					Thread.Sleep( TimeSpan.FromSeconds( 30 ) );
					continue;
				}

				if( !string.IsNullOrEmpty( info.GeneratedReportId ) )
					return info.GeneratedReportId;
			}
			
			//else if( response.GetReportRequestListResult.IsSetNextToken() )
			//{
			//	var nextResponse = this._client.GetReportRequestListByNextToken( new GetReportRequestListByNextTokenRequest
			//	{
			//		Merchant = this._credentials.SellerId,
			//		MWSAuthToken = this._credentials.MwsAuthToken,
			//		NextToken = response.GetReportRequestListResult.NextToken
			//	} );
			//	reportId = this.GetGeneratedReportIdFromNextPages( nextResponse.GetReportRequestListByNextTokenResult, reportRequestId, reportId );
			//}

			return reportId;
		}

		private string GetGeneratedReportIdFromNextPages( GetReportRequestListByNextTokenResult reportRequestListByNextTokenResult, string reportRequestId, string reportId )
		{
			if( !string.IsNullOrEmpty( reportId ) )
				return reportId;

			if( reportRequestListByNextTokenResult.IsSetReportRequestInfo() )
			{
				var info = reportRequestListByNextTokenResult.ReportRequestInfo.FirstOrDefault( i => i.ReportRequestId.Equals( reportRequestId ) );
				if( info != null )
					reportId = info.GeneratedReportId;
			}
			if( reportRequestListByNextTokenResult.IsSetNextToken() && string.IsNullOrEmpty( reportId ) )
			{
				var nextResponse = this._client.GetReportRequestListByNextToken( new GetReportRequestListByNextTokenRequest
				{
					Merchant = this._credentials.SellerId,
					MWSAuthToken = this._credentials.MwsAuthToken,
					NextToken = reportRequestListByNextTokenResult.NextToken
				} );

				reportId = this.GetGeneratedReportIdFromNextPages( nextResponse.GetReportRequestListByNextTokenResult, reportRequestId, reportId );
			}
			return reportId;
		}

		private string GetReportRequestId( RequestReportRequest request )
		{
			var reportId = string.Empty;

			var response = this._client.RequestReport( request );

			if( response.IsSetRequestReportResult() )
				reportId = response.RequestReportResult.ReportRequestInfo.ReportRequestId;

			return reportId;
		}
	}
}