﻿using System.Collections.Generic;

namespace AmazonAccess.Services.Sellers.Model
{
	public class MarketplaceParticipations
	{
		public List< Marketplace > Marketplaces{ get; set; }
		public List< Participation > Participations{ get; set; }

		public MarketplaceParticipations()
		{
			this.Marketplaces = new List< Marketplace >();
			this.Participations = new List< Participation >();
		}

		public MarketplaceParticipations( ListMarketplaces listMarketplaces, ListParticipations listParticipations )
		{
			this.Marketplaces = listMarketplaces.Marketplace;
			this.Participations = listParticipations.Participation;
		}

		public void Add( ListMarketplaces listMarketplaces, ListParticipations listParticipations )
		{
			if( listMarketplaces != null )
				this.Marketplaces.AddRange( listMarketplaces.Marketplace );
			if( listParticipations != null )
				this.Participations.AddRange( listParticipations.Participation );
		}
	}
}