/*
 * Copyright (c) Contributors, http://aurora-sim.org/
 * See CONTRIBUTORS.TXT for a full list of copyright holders.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the Aurora-Sim Project nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE DEVELOPERS ``AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE CONTRIBUTORS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenMetaverse;
using OpenMetaverse.StructuredData;
using OpenSim.Framework;

namespace Aurora.Framework
{
    public interface IDirectoryServiceConnector : IAuroraDataPlugin
	{
        /// <summary>
        /// Adds a region into search
        /// </summary>
        /// <param name="args"></param>
        void AddRegion (List<LandData> args);

        /// <summary>
        /// Removes a region from search
        /// </summary>
        /// <param name="regionID"></param>
        /// <param name="args"></param>
        void ClearRegion (UUID regionID);
        
        /// <summary>
        /// Gets a parcel from the search database by Info UUID (the true cross instance parcel ID)
        /// </summary>
        /// <param name="ParcelID"></param>
        /// <returns></returns>
        LandData GetParcelInfo(UUID ParcelID);

        /// <summary>
        /// Gets all parcels owned by the given user
        /// </summary>
        /// <param name="OwnerID"></param>
        /// <returns></returns>
        LandData[] GetParcelByOwner(UUID OwnerID);

        /// <summary>
        /// Searches for parcels around the grid
        /// </summary>
        /// <param name="queryText"></param>
        /// <param name="category"></param>
        /// <param name="StartQuery"></param>
        /// <returns></returns>
        DirPlacesReplyData[] FindLand(string queryText, string category, int StartQuery, uint Flags);
		
        /// <summary>
        /// Searches for parcels for sale around the grid
        /// </summary>
        /// <param name="searchType"></param>
        /// <param name="price"></param>
        /// <param name="area"></param>
        /// <param name="StartQuery"></param>
        /// <returns></returns>
        DirLandReplyData[] FindLandForSale(string searchType, string price, string area, int StartQuery, uint Flags);
		
        /// <summary>
        /// Searches for events with the given parameters
        /// </summary>
        /// <param name="queryText"></param>
        /// <param name="flags"></param>
        /// <param name="StartQuery"></param>
        /// <returns></returns>
        DirEventsReplyData[] FindEvents(string queryText, string flags, int StartQuery);
		
        /// <summary>
        /// Retrives all events in the given region by their maturity level
        /// </summary>
        /// <param name="regionName"></param>
        /// <param name="maturity">Uses DirectoryManager.EventFlags to determine the maturity requested</param>
        /// <returns></returns>
        DirEventsReplyData[] FindAllEventsInRegion(string regionName, int maturity);

        /// <summary>
        /// Searches for classifieds
        /// </summary>
        /// <param name="queryText"></param>
        /// <param name="category"></param>
        /// <param name="queryFlags"></param>
        /// <param name="StartQuery"></param>
        /// <returns></returns>
		DirClassifiedReplyData[] FindClassifieds(string queryText, string category, string queryFlags, int StartQuery);
		
        /// <summary>
        /// Gets more info about the event by the events unique event ID
        /// </summary>
        /// <param name="EventID"></param>
        /// <returns></returns>
        EventData GetEventInfo(string EventID);

        /// <summary>
        /// Gets all classifieds in the given region
        /// </summary>
        /// <param name="regionName"></param>
        /// <returns></returns>
		Classified[] GetClassifiedsInRegion(string regionName);
    }
}
