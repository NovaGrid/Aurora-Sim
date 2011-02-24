﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenSim.Framework;
using Aurora.Simulation.Base;
using OpenSim.Services.Interfaces;
using Nini.Config;
using OpenMetaverse;
using OpenMetaverse.StructuredData;
using OpenSim.Region.Framework.Interfaces;
using OpenSim.Region.Framework.Scenes;

namespace OpenSim.Services.MessagingService.MessagingModules.GridWideMessage
{
    public class GridWideMessageModule : IService
    {
        #region Declares

        protected IRegistryCore m_registry;

        #endregion

        #region IService Members

        public void Initialize(IConfigSource config, IRegistryCore registry)
        {
        }

        public void Start(IConfigSource config, IRegistryCore registry)
        {
            m_registry = registry;
            MainConsole.Instance.Commands.AddCommand("GridWideMessagingModule", true, "send grid alert",
                "send grid alert <message>", "Sends a message to all users in the grid", SendGridAlert);
            MainConsole.Instance.Commands.AddCommand("GridWideMessagingModule", true, "send grid message",
                "send grid message <first> <last> <message>", "Sends a message to a user in the grid", SendGridMessage);
            MainConsole.Instance.Commands.AddCommand("GridWideMessagingModule", true, "kick user",
                "kick user <first> <last> <message>", "Kicks a user from the grid", KickUserMessage);

            //Also look for incoming messages to display
            registry.RequestModuleInterface<IAsyncMessageRecievedService>().OnMessageReceived += OnMessageReceived;
        }

        public void FinishedStartup()
        {
        }

        #endregion

        #region Commands

        protected void SendGridAlert(string module, string[] cmd)
        {
            //Combine the params and figure out the message
            string message = CombineParams(cmd, 3);

            //Get required interfaces
            IAsyncMessagePostService messagePost = m_registry.RequestModuleInterface<IAsyncMessagePostService>();
            ICapsService capsService = m_registry.RequestModuleInterface<ICapsService>();
            List<IClientCapsService> clients = capsService.GetClientsCapsServices();

            //Go through all clients, and send the message asyncly to all agents that are root
            foreach (IClientCapsService client in clients)
            {
                foreach (IRegionClientCapsService regionClient in client.GetCapsServices())
                {
                    if (regionClient.RootAgent)
                    {
                        //Send the message to the client
                        messagePost.Post(regionClient.RegionHandle, BuildRequest("GridWideMessage", message, regionClient.AgentID.ToString()));
                    }
                }
            }
        }

        protected void SendGridMessage(string module, string[] cmd)
        {
            //Combine the params and figure out the message
            string user = CombineParams(cmd, 3, 5);
            string message = CombineParams(cmd, 5);

            //Get required interfaces
            IAsyncMessagePostService messagePost = m_registry.RequestModuleInterface<IAsyncMessagePostService>();
            ICapsService capsService = m_registry.RequestModuleInterface<ICapsService>();
            IUserAccountService userService = m_registry.RequestModuleInterface<IUserAccountService>();
            UserAccount account = userService.GetUserAccount(UUID.Zero, user.Split(' ')[0], user.Split(' ')[1]);
            if (account == null)
            {
                MainConsole.Instance.Output("User does not exist.");
                return;
            }
            IClientCapsService client = capsService.GetClientCapsService(account.PrincipalID);
            if (client != null)
            {
                IRegionClientCapsService regionClient = client.GetRootCapsService();
                if (regionClient != null)
                {
                    //Send the message to the client
                    messagePost.Post(regionClient.RegionHandle, BuildRequest("GridWideMessage", message, regionClient.AgentID.ToString()));
                    MainConsole.Instance.Output("Message sent.");
                    return;
                }
            }
            MainConsole.Instance.Output("Could not find user to send message to.");
        }

        protected void KickUserMessage(string module, string[] cmd)
        {
            //Combine the params and figure out the message
            string user = CombineParams(cmd, 3, 5);
            string message = CombineParams(cmd, 5);

            //Get required interfaces
            IAsyncMessagePostService messagePost = m_registry.RequestModuleInterface<IAsyncMessagePostService>();
            ICapsService capsService = m_registry.RequestModuleInterface<ICapsService>();
            IUserAccountService userService = m_registry.RequestModuleInterface<IUserAccountService>();
            UserAccount account = userService.GetUserAccount(UUID.Zero, user.Split(' ')[0], user.Split(' ')[1]);
            if (account == null)
            {
                MainConsole.Instance.Output("User does not exist.");
                return;
            }
            IClientCapsService client = capsService.GetClientCapsService(account.PrincipalID);
            if (client != null)
            {
                IRegionClientCapsService regionClient = client.GetRootCapsService();
                if (regionClient != null)
                {
                    //Send the message to the client
                    messagePost.Post(regionClient.RegionHandle, BuildRequest("KickUserMessage", message, regionClient.AgentID.ToString()));
                    client.Close();
                    capsService.RemoveCAPS(client.AgentID);
                    MainConsole.Instance.Output("User Kicked sent.");
                    return;
                }
            }
            MainConsole.Instance.Output("Could not find user to send message to.");
        }

        private string CombineParams(string[] commandParams, int pos)
        {
            string result = string.Empty;
            for (int i = pos; i < commandParams.Length; i++)
            {
                result += commandParams[i] + " ";
            }

            return result;
        }

        private string CombineParams(string[] commandParams, int pos, int end)
        {
            string result = string.Empty;
            for (int i = pos; i < commandParams.Length && i < end; i++)
            {
                result += commandParams[i] + " ";
            }

            return result;
        }

        private OSDMap BuildRequest(string name, string value, string user)
        {
            OSDMap map = new OSDMap();

            map["Method"] = name;
            map["Value"] = value;
            map["User"] = user;

            return map;
        }

        #endregion

        #region Message Received

        protected OSDMap OnMessageReceived(OSDMap message)
        {
            if (message.ContainsKey("Method") && message["Method"] == "GridWideMessage")
            {
                //We got a message, now display it
                string user = message["User"].AsString();
                string value = message["Value"].AsString();

                //Get the Scene registry since IDialogModule is a region module, and isn't in the ISimulationBase registry
                SceneManager manager = m_registry.RequestModuleInterface<SceneManager>();
                if (manager != null && manager.Scenes.Count > 0)
                {
                    IDialogModule dialogModule = manager.Scenes[0].RequestModuleInterface<IDialogModule>();
                    if (dialogModule != null)
                    {
                        //Send the message to the user now
                        dialogModule.SendAlertToUser(UUID.Parse(user), value);
                    }
                }
            }
            else if (message.ContainsKey("Method") && message["Method"] == "KickUserMessage")
            {
                //We got a message, now display it
                string user = message["User"].AsString();
                string value = message["Value"].AsString();

                //Get the Scene registry since IDialogModule is a region module, and isn't in the ISimulationBase registry
                SceneManager manager = m_registry.RequestModuleInterface<SceneManager>();
                if (manager != null && manager.Scenes.Count > 0)
                {
                    foreach (Scene scene in manager.Scenes)
                    {
                        ScenePresence sp = null;
                        if (scene.TryGetScenePresence(UUID.Parse(user), out sp))
                        {
                            sp.ControllingClient.Kick(value);
                            scene.IncomingCloseAgent(sp.UUID);
                        }
                    }
                }
            }
            return null;
        }

        #endregion
    }
}