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
using Aurora.Framework;
using Aurora.DataManager;
using OpenMetaverse;
using OpenSim.Framework;
using Nini.Config;
using OpenMetaverse.StructuredData;

namespace Aurora.Services.DataService
{
    /// <summary>
    /// Some background to this class
    /// 
    /// This class saves any class that implements the IDataTransferable interface.
    ///   When implementing the IDataTransferable interface, it is heavily recommending to implement ToOSD and FromOSD first, then use the Utility methods to convert OSDMaps into Dictionarys, as shown in the LandData class.
    /// 
    /// This method of saving uses 4 columns in the database, OwnerID, Type, Key, and Value
    /// 
    ///   - OwnerID : This is a way to be able to save Agent or Region or anything with a UUID into the database and have it be set to that UUID only.
    ///   - Type : What made this data? This just tells what module created the given row in the database.
    ///   - Key : Another identifying setting so that you can store more than one row under an OwnerID and Type
    ///   - Value : The value of the row
    /// 
    /// This class deals with the Getting/Setting/Removing of these generic interfaces.
    /// 
    /// </summary>
    public class LocalGenericsConnector : IGenericsConnector
	{
		private IGenericData GD = null;

        public void Initialize(IGenericData GenericData, IConfigSource source, IRegistryCore simBase, string defaultConnectionString)
        {
            if(source.Configs["AuroraConnectors"].GetString("GenericsConnector", "LocalConnector") == "LocalConnector")
            {
                GD = GenericData;

                if (source.Configs[Name] != null)
                    defaultConnectionString = source.Configs[Name].GetString("ConnectionString", defaultConnectionString);

                GD.ConnectToDatabase(defaultConnectionString, "Generics", source.Configs["AuroraConnectors"].GetBoolean("ValidateTables", true));

                DataManager.DataManager.RegisterPlugin(Name, this);
            }
        }

        public string Name
        {
            get { return "IGenericsConnector"; }
        }

        public void Dispose()
        {
        }

        /// <summary>
        /// Gets a Generic type as set by the ownerID, Type, and Key
        /// </summary>
        /// <typeparam name="T">return value of type IDataTransferable</typeparam>
        /// <param name="OwnerID"></param>
        /// <param name="Type"></param>
        /// <param name="Key"></param>
        /// <param name="data">a default T to copy all data into</param>
        /// <returns></returns>
        public T GetGeneric<T>(UUID OwnerID, string Type, string Key, T data) where T : IDataTransferable
        {
            return GenericUtils.GetGeneric<T>(OwnerID, Type, Key, GD, data);
        }

        /// <summary>
        /// Gets a list of generic T's from the database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="OwnerID"></param>
        /// <param name="Type"></param>
        /// <param name="data">a default T</param>
        /// <returns></returns>
        public List<T> GetGenerics<T>(UUID OwnerID, string Type, T data) where T : IDataTransferable
        {
            return GenericUtils.GetGenerics<T>(OwnerID, Type, GD, data);
        }

        /// <summary>
        /// Adds a generic IDataTransferable into the database
        /// </summary>
        /// <param name="AgentID"></param>
        /// <param name="Type"></param>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        public void AddGeneric(UUID AgentID, string Type, string Key, OSDMap Value)
        {
            GenericUtils.AddGeneric(AgentID, Type, Key, Value, GD);
        }

        /// <summary>
        /// Removes a generic IDataTransferable from the database
        /// </summary>
        /// <param name="AgentID"></param>
        /// <param name="Type"></param>
        /// <param name="Key"></param>
        public void RemoveGeneric(UUID AgentID, string Type, string Key)
        {
            GenericUtils.RemoveGeneric(AgentID, Type, Key, GD);
        }

        /// <summary>
        /// Removes a generic IDataTransferable from the database
        /// </summary>
        /// <param name="AgentID"></param>
        /// <param name="Type"></param>
        public void RemoveGeneric(UUID AgentID, string Type)
        {
            GenericUtils.RemoveGeneric(AgentID, Type, GD);
        }
    }
}
