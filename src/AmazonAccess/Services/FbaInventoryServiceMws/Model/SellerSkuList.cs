/*******************************************************************************
 * Copyright 2009-2014 Amazon Services. All Rights Reserved.
 * Licensed under the Apache License, Version 2.0 (the "License"); 
 *
 * You may not use this file except in compliance with the License. 
 * You may obtain a copy of the License at: http://aws.amazon.com/apache2.0
 * This file is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
 * CONDITIONS OF ANY KIND, either express or implied. See the License for the 
 * specific language governing permissions and limitations under the License.
 *******************************************************************************
 * Seller Sku List
 * API Version: 2010-10-01
 * Library Version: 2014-09-30
 * Generated: Fri Sep 26 16:01:24 GMT 2014
 */

using System.Collections.Generic;
using System.Xml.Serialization;
using AmazonAccess.Services.Common;

namespace AmazonAccess.Services.FbaInventoryServiceMws.Model
{
    [XmlType(Namespace = "http://mws.amazonaws.com/FulfillmentInventory/2010-10-01/")]
    [XmlRoot(Namespace = "http://mws.amazonaws.com/FulfillmentInventory/2010-10-01/", IsNullable = false)]
    public class SellerSkuList : AbstractMwsObject
    {

        private List<string> _member;

        /// <summary>
        /// Gets and sets the member property.
        /// </summary>
        [XmlElement(ElementName = "member")]
        public List<string> member
        {
            get
            {
                if(this._member == null)
                {
                    this._member = new List<string>();
                }
                return this._member;
            }
            set { this._member = value; }
        }

        /// <summary>
        /// Sets the member property.
        /// </summary>
        /// <param name="member">member property.</param>
        /// <returns>this instance.</returns>
        public SellerSkuList Withmember(string[] member)
        {
            this._member.AddRange(member);
            return this;
        }

        /// <summary>
        /// Checks if member property is set.
        /// </summary>
        /// <returns>true if member property is set.</returns>
        public bool IsSetmember()
        {
            return this.member.Count > 0;
        }


        public override void ReadFragmentFrom(IMwsReader reader)
        {
            this._member = reader.ReadList<string>("member");
        }

        public override void WriteFragmentTo(IMwsWriter writer)
        {
            writer.WriteList("member", this._member);
        }

        public override void WriteTo(IMwsWriter writer)
        {
            writer.Write("http://mws.amazonaws.com/FulfillmentInventory/2010-10-01/", "SellerSkuList", this);
        }

        public SellerSkuList() : base()
        {
        }
    }
}
