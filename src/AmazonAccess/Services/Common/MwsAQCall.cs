﻿/******************************************************************************* 
 * Copyright 2009-2012 Amazon Services. All Rights Reserved.
 * Licensed under the Apache License, Version 2.0 (the "License"); 
 *
 * You may not use this file except in compliance with the License. 
 * You may obtain a copy of the License at: http://aws.amazon.com/apache2.0
 * This file is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
 * CONDITIONS OF ANY KIND, either express or implied. See the License for the 
 * specific language governing permissions and limitations under the License.
 *******************************************************************************
 * Marketplace Web Service Runtime Client Library
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;

namespace AmazonAccess.Services.Common
{
	/// <summary>
	/// AWS query call implementation
	/// </summary>
	public class MwsAQCall: IMwsCall
	{
		private readonly MwsConnection connection;
		private readonly IDictionary< string, string > parameters = new SortedDictionary< string, string >( StringComparer.Ordinal );
		private readonly MwsConnection.ServiceEndpoint serviceEndPoint;

		public HttpWebRequest request;

		private readonly string operationName;
		private MwsResponseHeaderMetadata ResponseHeaderMetadata;

		public MwsAQCall( MwsConnection connection, MwsConnection.ServiceEndpoint serviceEndpoint, string operationName )
		{
			this.connection = connection;
			this.serviceEndPoint = serviceEndpoint;
			this.operationName = operationName;
		}

		/// <summary>
		/// Creates a request and invokes it 
		/// </summary>
		/// <returns></returns>
		/// <exception cref="MwsException">Exceptions from invoking the request</exception>
		public IMwsReader invoke()
		{
			// Add required request parameters 
			this.AddRequiredParameters();
			var queryString = this.GetParametersAsString( this.parameters );

			string responseBody;
			HttpStatusCode statusCode;
			try
			{
				this.request = this.connection.GetHttpClient( this.serviceEndPoint.URI );
				var requestData = new UTF8Encoding().GetBytes( queryString );
				this.request.ContentLength = requestData.Length;
				using( var requestStream = this.request.GetRequestStream() )
					requestStream.Write( requestData, 0, requestData.Length );
				string message;
				using( var httpResponse = ( HttpWebResponse )this.request.GetResponse() )
				{
					statusCode = httpResponse.StatusCode;
					message = httpResponse.StatusDescription;
					this.ResponseHeaderMetadata = GetResponseHeaderMetadata( httpResponse );
					var reader = new StreamReader( httpResponse.GetResponseStream(), Encoding.UTF8 );
					responseBody = reader.ReadToEnd();
				}
				if( statusCode == HttpStatusCode.OK )
					return new MwsXmlReader( responseBody );

				throw new MwsException( ( int )statusCode, message, null, null, responseBody, this.ResponseHeaderMetadata );
			}
			catch( WebException we )
			{
				// Web exception is thrown on unsuccessful responses
				using( var httpErrorResponse = we.Response as HttpWebResponse )
				{
					if( httpErrorResponse == null )
						throw new MwsException( we );
					statusCode = httpErrorResponse.StatusCode;
					using( var reader = new StreamReader( httpErrorResponse.GetResponseStream(), Encoding.UTF8 ) )
						responseBody = reader.ReadToEnd();
				}
				throw new MwsException( ( int )statusCode, null, null, null, responseBody, null );
			}
			catch( Exception e )
			{
				// Catch other exceptions, attempt to convert to formatted exception, else rethrow wrapped exception 
				throw new MwsException( e );
			}
		}

		/// <summary>
		/// Extracts and assigns the response header metadata
		/// </summary>
		/// <param name="httpResponse"></param>
		/// <returns></returns>
		private static MwsResponseHeaderMetadata GetResponseHeaderMetadata( HttpWebResponse httpResponse )
		{
			var requestId = httpResponse.GetResponseHeader( "x-mws-request-id" );
			var timestamp = httpResponse.GetResponseHeader( "x-mws-timestamp" );
			var contextStr = httpResponse.GetResponseHeader( "x-mws-response-context" );
			var context = new List< string >( contextStr.Split( ',' ) );

			double? quotaMax;
			try
			{
				string quotaMaxStr = httpResponse.GetResponseHeader( "x-mws-quota-max" );
				quotaMax = Double.Parse( quotaMaxStr );
			}
			catch( Exception )
			{
				quotaMax = null;
			}

			double? quotaRemaining;
			try
			{
				string quotaRemainingStr = httpResponse.GetResponseHeader( "x-mws-quota-remaining" );
				quotaRemaining = Double.Parse( quotaRemainingStr );
			}
			catch( Exception )
			{
				quotaRemaining = null;
			}

			DateTime? quotaResetsAt;
			try
			{
				string quotaResetsAtStr = httpResponse.GetResponseHeader( "x-mws-quota-resetsOn" );
				quotaResetsAt = MwsUtil.ParseTimestamp( quotaResetsAtStr );
			}
			catch( Exception )
			{
				quotaResetsAt = null;
			}

			return new MwsResponseHeaderMetadata( requestId, context, timestamp, quotaMax, quotaRemaining, quotaResetsAt );
		}

		/// <summary>
		/// Constructs the parameters as string 
		/// </summary>
		/// <param name="parameters"></param>
		/// <returns></returns>
		private string GetParametersAsString( IDictionary< string, string > parameters )
		{
			var data = new StringBuilder();
			foreach( var key in parameters.Keys )
			{
				var value = parameters[ key ];
				if( value != null )
				{
					data.Append( key );
					data.Append( '=' );
					data.Append( MwsUtil.UrlEncode( value, false ) );
					data.Append( '&' );
				}
			}
			string result = data.ToString();
			return result.Remove( result.Length - 1 );
		}

		/// <summary>
		/// Add authentication related and version parameters
		/// </summary>
		private void AddRequiredParameters()
		{
			this.parameters.Add( "AWSAccessKeyId", this.connection.AwsAccessKeyId );
			this.parameters.Add( "Action", this.operationName );
			this.parameters.Add( "Timestamp", MwsUtil.GetFormattedTimestamp() );
			this.parameters.Add( "Version", this.serviceEndPoint.version );
			string signature = MwsUtil.SignParameters( this.serviceEndPoint.URI, this.connection.SignatureVersion, this.connection.SignatureMethod, this.parameters, this.connection.AwsSecretKeyId );
			this.parameters.Add( "Signature", signature );
		}

		private void putValue( object value )
		{
			if( value == null )
				return;
			if( value is IMwsObject )
			{
				this.parameterPrefix.Append( '.' );
				( value as IMwsObject ).WriteFragmentTo( this );
				return;
			}
			var name = this.parameterPrefix.ToString();
			if( value is DateTime )
			{
				this.parameters.Add( name, MwsUtil.GetFormattedTimestamp( ( DateTime )value ) );
				return;
			}
			var valueStr = value.ToString();
			if( string.IsNullOrEmpty( valueStr ) )
				return;
			if( value is bool )
				valueStr = valueStr.ToLower();
			this.parameters.Add( name, valueStr );
		}

		public MwsResponseHeaderMetadata GetResponseMetadataHeader()
		{
			return this.ResponseHeaderMetadata;
		}

		/** The parameter prefix */
		private readonly StringBuilder parameterPrefix = new StringBuilder();

		public void Write( string name, object value )
		{
			int holdParameterPrefixLen = this.parameterPrefix.Length;
			this.parameterPrefix.Append( name );
			this.putValue( value );
			this.parameterPrefix.Length = holdParameterPrefixLen;
		}

		public void Write( string xmlNamespace, string name, IMwsObject value )
		{
			if( value != null )
				value.WriteFragmentTo( this );
		}

		public void WriteAttribute( string name, object value )
		{
			this.Write( name, value );
		}

		public void WriteList< T >( string name, string memberName, ICollection< T > list )
		{
			if( list == null )
				return;
			if( name == null && memberName == null )
				throw new ArgumentNullException( "Both name and memberName cannot be null." );
			var holdParameterPrefixLen = this.parameterPrefix.Length;
			if( name != null )
				this.parameterPrefix.Append( name );
			if( name != null && memberName != null )
				this.parameterPrefix.Append( '.' );
			if( memberName != null )
				this.parameterPrefix.Append( memberName );
			this.parameterPrefix.Append( '.' );
			int dotLen = this.parameterPrefix.Length;
			int i = 1;
			foreach( Object v in list )
			{
				this.parameterPrefix.Length = dotLen;
				this.parameterPrefix.Append( i );
				this.putValue( v );
				i++;
			}
			this.parameterPrefix.Length = holdParameterPrefixLen;
		}

		public void WriteList< T >( string name, ICollection< T > list )
		{
			this.WriteList( null, name, list );
		}

		public void WriteAny( ICollection< XmlElement > elements )
		{
			throw new NotSupportedException( "WriteAny not supported" );
		}

		public void WriteValue( object value )
		{
			throw new NotSupportedException( "WriteValue not supported" );
		}

		public void BeginObject( string name )
		{
			throw new NotSupportedException( "Complex object writing not supported" );
		}

		public void close()
		{
			//nothing to do
		}

		public void EndObject( string name )
		{
			throw new NotSupportedException( "Complex object writing not supported" );
		}
	}
}