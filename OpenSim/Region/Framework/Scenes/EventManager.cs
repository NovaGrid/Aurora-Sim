/*
 * Copyright (c) Contributors, http://opensimulator.org/
 * See CONTRIBUTORS.TXT for a full list of copyright holders.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the OpenSimulator Project nor the
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
using System.Reflection;
using log4net;
using OpenMetaverse;
using OpenSim.Framework;
using OpenSim.Framework.Client;
using OpenSim.Region.Framework.Interfaces;
using OpenSim.Services.Interfaces;
using GridRegion = OpenSim.Services.Interfaces.GridRegion;
using OpenMetaverse.StructuredData;
using OpenSim.Framework.Servers.HttpServer;

namespace OpenSim.Region.Framework.Scenes
{
    /// <summary>
    /// A class for triggering remote scene events.
    /// </summary>
    public class EventManager
    {
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public delegate void OnFrameDelegate();

        public event OnFrameDelegate OnFrame;

        public delegate void ClientMovement(ScenePresence client);

        public event ClientMovement OnClientMovement;

        public delegate void OnClientConnectCoreDelegate(IClientCore client);

        public event OnClientConnectCoreDelegate OnClientConnect;

        public delegate void OnNewClientDelegate(IClientAPI client);

        /// <summary>
        /// Deprecated in favour of OnClientConnect.
        /// Will be marked Obsolete after IClientCore has 100% of IClientAPI interfaces.
        /// </summary>
        public event OnNewClientDelegate OnNewClient;
        public event OnNewClientDelegate OnClosingClient;

        public delegate void OnClientLoginDelegate(IClientAPI client);
        public event OnClientLoginDelegate OnClientLogin;

        public delegate void OnNewPresenceDelegate(ScenePresence presence);

        public event OnNewPresenceDelegate OnNewPresence;

        public event OnNewPresenceDelegate OnRemovePresence;

        public delegate void OnPluginConsoleDelegate(string[] args);

        public event OnPluginConsoleDelegate OnPluginConsole;

        public delegate void OnPermissionErrorDelegate(UUID user, string reason);

        /// <summary>
        /// Fired when an object is touched/grabbed.
        /// </summary>
        /// The child is the part that was actually touched.
        public event ObjectGrabDelegate OnObjectGrab;
        public delegate void ObjectGrabDelegate(SceneObjectPart part, SceneObjectPart child, Vector3 offsetPos, IClientAPI remoteClient, SurfaceTouchEventArgs surfaceArgs);

        public event ObjectGrabDelegate OnObjectGrabbing;
        public event ObjectDeGrabDelegate OnObjectDeGrab;
        public delegate void ObjectDeGrabDelegate(SceneObjectPart part, SceneObjectPart child, IClientAPI remoteClient, SurfaceTouchEventArgs surfaceArgs);

        public event OnPermissionErrorDelegate OnPermissionError;

        /// <summary>
        /// Fired when a new script is created.
        /// </summary>
        public event NewRezScript OnRezScript;
        public delegate void NewRezScript(SceneObjectPart part, UUID itemID, string script, int startParam, bool postOnRez, int stateSource);

        public event NewRezScripts OnRezScripts;
        public delegate void NewRezScripts(SceneObjectPart part, TaskInventoryItem[] taskInventoryItem, int startParam, bool postOnRez, int stateSource, UUID RezzedFrom);

        public delegate void RemoveScript(uint localID, UUID itemID);
        public event RemoveScript OnRemoveScript;

        public delegate bool SceneGroupMoved(UUID groupID, Vector3 delta);
        public event SceneGroupMoved OnSceneGroupMove;

        public delegate void SceneGroupGrabed(UUID groupID, Vector3 offset, UUID userID);
        public event SceneGroupGrabed OnSceneGroupGrab;

        public delegate bool SceneGroupSpinStarted(UUID groupID);
        public event SceneGroupSpinStarted OnSceneGroupSpinStart;

        public delegate bool SceneGroupSpun(UUID groupID, Quaternion rotation);
        public event SceneGroupSpun OnSceneGroupSpin;

        public delegate void LandObjectAdded(LandData newParcel);
        public event LandObjectAdded OnLandObjectAdded;

        public delegate void LandObjectRemoved(UUID RegionID, UUID globalID);
        public event LandObjectRemoved OnLandObjectRemoved;

        public delegate void AvatarEnteringNewParcel(ScenePresence avatar, int localLandID, UUID regionID);
        public event AvatarEnteringNewParcel OnAvatarEnteringNewParcel;

        public delegate void SignificantClientMovement(IClientAPI remote_client);
        public event SignificantClientMovement OnSignificantClientMovement;

        public delegate void SignificantObjectMovement(SceneObjectGroup group);
        public event SignificantObjectMovement OnSignificantObjectMovement;

        public delegate void IncomingInstantMessage(GridInstantMessage message);
        public event IncomingInstantMessage OnIncomingInstantMessage;

        public event IncomingInstantMessage OnUnhandledInstantMessage;

        public delegate void ClientClosed(UUID clientID, Scene scene);

        public event ClientClosed OnClientClosed;

        /// <summary>
        /// This is fired when a scene object property that a script might be interested in (such as color, scale or
        /// inventory) changes.  Only enough information is sent for the LSL changed event
        /// (see http://lslwiki.net/lslwiki/wakka.php?wakka=changed)
        /// </summary>
        public event ScriptChangedEvent OnScriptChangedEvent;
        public delegate void ScriptChangedEvent(SceneObjectPart part, uint change);

        public event ScriptMovingStartEvent OnScriptMovingStartEvent;
        public delegate void ScriptMovingStartEvent(SceneObjectPart part);

        public event ScriptMovingEndEvent OnScriptMovingEndEvent;
        public delegate void ScriptMovingEndEvent(SceneObjectPart part);

        public delegate void ScriptControlEvent(SceneObjectPart part, UUID item, UUID avatarID, uint held, uint changed);
        public event ScriptControlEvent OnScriptControlEvent;

        public delegate void ScriptAtTargetEvent(uint localID, uint handle, Vector3 targetpos, Vector3 atpos);
        public event ScriptAtTargetEvent OnScriptAtTargetEvent;

        public delegate void ScriptNotAtTargetEvent(uint localID);
        public event ScriptNotAtTargetEvent OnScriptNotAtTargetEvent;

        public delegate void ScriptAtRotTargetEvent(uint localID, uint handle, Quaternion targetrot, Quaternion atrot);
        public event ScriptAtRotTargetEvent OnScriptAtRotTargetEvent;

        public delegate void ScriptNotAtRotTargetEvent(uint localID);
        public event ScriptNotAtRotTargetEvent OnScriptNotAtRotTargetEvent;

        public delegate void ScriptColliding(SceneObjectPart part, ColliderArgs colliders);
        public event ScriptColliding OnScriptColliderStart;
        public event ScriptColliding OnScriptColliding;
        public event ScriptColliding OnScriptCollidingEnd;
        public event ScriptColliding OnScriptLandColliderStart;
        public event ScriptColliding OnScriptLandColliding;
        public event ScriptColliding OnScriptLandColliderEnd;

        public delegate void OnMakeChildAgentDelegate(ScenePresence presence);
        public event OnMakeChildAgentDelegate OnMakeChildAgent;

        public delegate void OnMakeRootAgentDelegate(ScenePresence presence);
        public event OnMakeRootAgentDelegate OnMakeRootAgent;

        public delegate void NewInventoryItemUploadComplete(UUID avatarID, UUID assetID, string name, int userlevel);

        public event NewInventoryItemUploadComplete OnNewInventoryItemUploadComplete;

        public delegate void RequestChangeWaterHeight(float height);

        public event RequestChangeWaterHeight OnRequestChangeWaterHeight;

        public delegate void AddToStartupQueue(string name);
        public delegate void FinishedStartup(string name, List<string> data);
        public delegate void StartupComplete(IScene scene, List<string> data);
        public event FinishedStartup OnModuleFinishedStartup;
        public event AddToStartupQueue OnAddToStartupQueue;
        public event StartupComplete OnStartupComplete;
        //This is called after OnStartupComplete is done, it should ONLY be registered to the Scene
        public event StartupComplete OnStartupFullyComplete;

        public delegate void EstateToolsSunUpdate(ulong regionHandle, bool FixedTime, bool EstateSun, float LindenHour);
        public event EstateToolsSunUpdate OnEstateToolsSunUpdate;

        public delegate void ObjectBeingRemovedFromScene(SceneObjectGroup obj);
        public event ObjectBeingRemovedFromScene OnObjectBeingRemovedFromScene;

        public event ObjectBeingRemovedFromScene OnObjectBeingAddedToScene;

        public delegate void IncomingLandDataFromStorage(List<LandData> data);
        public event IncomingLandDataFromStorage OnIncomingLandDataFromStorage;

        /// <summary>
        /// RegisterCapsEvent is called by Scene after the Caps object
        /// has been instantiated and before it is return to the
        /// client and provides region modules to add their caps.
        /// </summary>
        public delegate OSDMap RegisterCapsEvent(UUID agentID, IHttpServer httpServer);
        public event RegisterCapsEvent OnRegisterCaps;

        /// <summary>
        /// DeregisterCapsEvent is called by Scene when the caps
        /// handler for an agent are removed.
        /// </summary>
        public delegate void DeregisterCapsEvent(UUID agentID, IRegionClientCapsService caps);
        public event DeregisterCapsEvent OnDeregisterCaps;

        /// <summary>
        /// ChatFromWorldEvent is called via Scene when a chat message
        /// from world comes in.
        /// </summary>
        public delegate void ChatFromWorldEvent(Object sender, OSChatMessage chat);
        public event ChatFromWorldEvent OnChatFromWorld;

        /// <summary>
        /// ChatFromClientEvent is triggered via ChatModule (or
        /// substitutes thereof) when a chat message
        /// from the client  comes in.
        /// </summary>
        public delegate void ChatFromClientEvent(Object sender, OSChatMessage chat);
        public event ChatFromClientEvent OnChatFromClient;

        /// <summary>
        /// ChatBroadcastEvent is called via Scene when a broadcast chat message
        /// from world comes in
        /// </summary>
        public delegate void ChatBroadcastEvent(Object sender, OSChatMessage chat);
        public event ChatBroadcastEvent OnChatBroadcast;

        /// <summary>
        /// Called when oar file has finished loading, although
        /// the scripts may not have started yet
        /// Message is non empty string if there were problems loading the oar file
        /// </summary>
        public delegate void OarFileLoaded(Guid guid, string message);
        public event OarFileLoaded OnOarFileLoaded;

        /// <summary>
        /// Called when an oar file has finished saving
        /// Message is non empty string if there were problems saving the oar file
        /// If a guid was supplied on the original call to identify, the request, this is returned.  Otherwise 
        /// Guid.Empty is returned.
        /// </summary>
        public delegate void OarFileSaved(Guid guid, string message);
        public event OarFileSaved OnOarFileSaved;

        /// <summary>
        /// Called when the script compile queue becomes empty
        /// Returns the number of scripts which failed to start
        /// </summary>
        public delegate void EmptyScriptCompileQueue(int numScriptsFailed, string message);
        public event EmptyScriptCompileQueue OnEmptyScriptCompileQueue;

        /// <summary>
        /// Called whenever an object is attached, or detached from an in-world presence.
        /// </summary>
        /// If the object is being attached, then the avatarID will be present.  If the object is being detached then
        /// the avatarID is UUID.Zero (I know, this doesn't make much sense but now it's historical).
        public delegate void Attach(uint localID, UUID itemID, UUID avatarID);
        public event Attach OnAttach;

        public delegate void RegionUp(GridRegion region);
        public event RegionUp OnRegionUp;
        public event RegionUp OnRegionDown;

        public class LandBuyArgs : EventArgs
        {
            public UUID agentId = UUID.Zero;

            public UUID groupId = UUID.Zero;

            public UUID parcelOwnerID = UUID.Zero;

            public bool final = false;
            public bool groupOwned = false;
            public bool removeContribution = false;
            public int parcelLocalID = 0;
            public int parcelArea = 0;
            public int parcelPrice = 0;
            public bool authenticated = false;
            public bool landValidated = false;
            public bool economyValidated = false;
            public int transactionID = 0;
            public int amountDebited = 0;

            public LandBuyArgs(UUID pagentId, UUID pgroupId, bool pfinal, bool pgroupOwned,
                bool premoveContribution, int pparcelLocalID, int pparcelArea, int pparcelPrice,
                bool pauthenticated)
            {
                agentId = pagentId;
                groupId = pgroupId;
                final = pfinal;
                groupOwned = pgroupOwned;
                removeContribution = premoveContribution;
                parcelLocalID = pparcelLocalID;
                parcelArea = pparcelArea;
                parcelPrice = pparcelPrice;
                authenticated = pauthenticated;
            }
        }

        public delegate bool LandBuy(LandBuyArgs e);

        public event LandBuy OnValidateBuyLand;

        public void TriggerOnAttach(uint localID, UUID itemID, UUID avatarID)
        {
            Attach handlerOnAttach = OnAttach;
            if (handlerOnAttach != null)
            {
                foreach (Attach d in handlerOnAttach.GetInvocationList())
                {
                    try
                    {
                        d(localID, itemID, avatarID);
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for TriggerOnAttach failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
            }
        }

        public void TriggerOnScriptChangedEvent(SceneObjectPart part, uint change)
        {
            ScriptChangedEvent handlerScriptChangedEvent = OnScriptChangedEvent;
            if (handlerScriptChangedEvent != null)
            {
                foreach (ScriptChangedEvent d in handlerScriptChangedEvent.GetInvocationList())
                {
                    try
                    {
                        d(part, change);
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for TriggerOnScriptChangedEvent failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
            }
        }

        public void TriggerOnScriptMovingStartEvent(SceneObjectPart part)
        {
            ScriptMovingStartEvent handlerScriptMovingStartEvent = OnScriptMovingStartEvent;
            if (handlerScriptMovingStartEvent != null)
            {
                foreach (ScriptMovingStartEvent d in handlerScriptMovingStartEvent.GetInvocationList())
                {
                    try
                    {
                        d(part);
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for TriggerOnScriptMovingStartEvent failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
            }
        }

        public void TriggerOnScriptMovingEndEvent(SceneObjectPart part)
        {
            ScriptMovingEndEvent handlerScriptMovingEndEvent = OnScriptMovingEndEvent;
            if (handlerScriptMovingEndEvent != null)
            {
                foreach (ScriptMovingEndEvent d in handlerScriptMovingEndEvent.GetInvocationList())
                {
                    try
                    {
                        d(part);
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for TriggerOnScriptMovingEndEvent failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
            }
        }

        public void TriggerOnClientMovement(ScenePresence avatar)
        {
            ClientMovement handlerClientMovement = OnClientMovement;
            if (handlerClientMovement != null)
            {
                foreach (ClientMovement d in handlerClientMovement.GetInvocationList())
                {
                    try
                    {
                        d(avatar);
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for TriggerOnClientMovement failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
            }
        }

        public void TriggerPermissionError(UUID user, string reason)
        {
            OnPermissionErrorDelegate handlerPermissionError = OnPermissionError;
            if (handlerPermissionError != null)
            {
                foreach (OnPermissionErrorDelegate d in handlerPermissionError.GetInvocationList())
                {
                    try
                    {
                        d(user, reason);
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for TriggerPermissionError failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
            }
        }

        public void TriggerOnPluginConsole(string[] args)
        {
            OnPluginConsoleDelegate handlerPluginConsole = OnPluginConsole;
            if (handlerPluginConsole != null)
            {
                foreach (OnPluginConsoleDelegate d in handlerPluginConsole.GetInvocationList())
                {
                    try
                    {
                        d(args);
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for TriggerOnPluginConsole failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
            }
        }

        public void TriggerOnFrame()
        {
            OnFrameDelegate handlerFrame = OnFrame;
            if (handlerFrame != null)
            {
                foreach (OnFrameDelegate d in handlerFrame.GetInvocationList())
                {
                    try
                    {
                        d();
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for TriggerOnFrame failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
            }
        }

        public void TriggerOnClosingClient(IClientAPI client)
        {
            OnNewClientDelegate handlerClosingClient = OnClosingClient;
            if (handlerClosingClient != null)
            {
                foreach (OnNewClientDelegate d in handlerClosingClient.GetInvocationList())
                {
                    try
                    {
                        d(client);
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for TriggerOnClosingClient failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
            }
        }

        public void TriggerOnNewClient(IClientAPI client)
        {
            OnNewClientDelegate handlerNewClient = OnNewClient;
            if (handlerNewClient != null)
            {
                foreach (OnNewClientDelegate d in handlerNewClient.GetInvocationList())
                {
                    try
                    {
                        d(client);
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for TriggerOnNewClient failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
            }

            if (client is IClientCore)
            {
                OnClientConnectCoreDelegate handlerClientConnect = OnClientConnect;
                if (handlerClientConnect != null)
                {
                    foreach (OnClientConnectCoreDelegate d in handlerClientConnect.GetInvocationList())
                    {
                        try
                        {
                            d((IClientCore)client);
                        }
                        catch (Exception e)
                        {
                            m_log.ErrorFormat(
                                "[EVENT MANAGER]: Delegate for TriggerOnNewClient (IClientCore) failed - continuing.  {0} {1}",
                                e.ToString(), e.StackTrace);
                        }
                    }
                }
            }
        }

        public void TriggerOnClientLogin(IClientAPI client)
        {
            OnClientLoginDelegate handlerClientLogin = OnClientLogin;
            if (handlerClientLogin != null)
            {
                foreach (OnClientLoginDelegate d in handlerClientLogin.GetInvocationList())
                {
                    try
                    {
                        d(client);
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for TriggerOnClientLogin failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
            }

        }

        public void TriggerOnNewPresence(ScenePresence presence)
        {
            OnNewPresenceDelegate handlerNewPresence = OnNewPresence;
            if (handlerNewPresence != null)
            {
                foreach (OnNewPresenceDelegate d in handlerNewPresence.GetInvocationList())
                {
                    try
                    {
                        d(presence);
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for TriggerOnNewPresence failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
            }
        }

        public void TriggerOnRemovePresence(ScenePresence presence)
        {
            OnNewPresenceDelegate handlerRemovePresence = OnRemovePresence;
            if (handlerRemovePresence != null)
            {
                foreach (OnNewPresenceDelegate d in handlerRemovePresence.GetInvocationList())
                {
                    try
                    {
                        d(presence);
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for TriggerOnRemovePresence failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
            }
        }

        public void TriggerObjectBeingAddedToScene(SceneObjectGroup obj)
        {
            ObjectBeingRemovedFromScene handlerObjectBeingAddedToScene = OnObjectBeingAddedToScene;
            if (handlerObjectBeingAddedToScene != null)
            {
                foreach (ObjectBeingRemovedFromScene d in handlerObjectBeingAddedToScene.GetInvocationList())
                {
                    try
                    {
                        d(obj);
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for TriggerObjectBeingAddToScene failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
            }
        }

        public void TriggerObjectBeingRemovedFromScene(SceneObjectGroup obj)
        {
            ObjectBeingRemovedFromScene handlerObjectBeingRemovedFromScene = OnObjectBeingRemovedFromScene;
            if (handlerObjectBeingRemovedFromScene != null)
            {
                foreach (ObjectBeingRemovedFromScene d in handlerObjectBeingRemovedFromScene.GetInvocationList())
                {
                    try
                    {
                        d(obj);
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for TriggerObjectBeingRemovedFromScene failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
            }
        }

        public void TriggerObjectGrab(SceneObjectPart part, SceneObjectPart child, Vector3 offsetPos, IClientAPI remoteClient, SurfaceTouchEventArgs surfaceArgs)
        {
            ObjectGrabDelegate handlerObjectGrab = OnObjectGrab;
            if (handlerObjectGrab != null)
            {
                foreach (ObjectGrabDelegate d in handlerObjectGrab.GetInvocationList())
                {
                    try
                    {
                        d(part, child, offsetPos, remoteClient, surfaceArgs);
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for TriggerObjectGrab failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
            }
        }

        public void TriggerObjectGrabbing(SceneObjectPart part, SceneObjectPart child, Vector3 offsetPos, IClientAPI remoteClient, SurfaceTouchEventArgs surfaceArgs)
        {
            ObjectGrabDelegate handlerObjectGrabbing = OnObjectGrabbing;
            if (handlerObjectGrabbing != null)
            {
                foreach (ObjectGrabDelegate d in handlerObjectGrabbing.GetInvocationList())
                {
                    try
                    {
                        d(part, child, offsetPos, remoteClient, surfaceArgs);
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for TriggerObjectGrabbing failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
            }
        }

        public void TriggerObjectDeGrab(SceneObjectPart part, SceneObjectPart child, IClientAPI remoteClient, SurfaceTouchEventArgs surfaceArgs)
        {
            ObjectDeGrabDelegate handlerObjectDeGrab = OnObjectDeGrab;
            if (handlerObjectDeGrab != null)
            {
                foreach (ObjectDeGrabDelegate d in handlerObjectDeGrab.GetInvocationList())
                {
                    try
                    {
                        d(part, child, remoteClient, surfaceArgs);
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for TriggerObjectDeGrab failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
            }
        }

        public void TriggerRezScript(SceneObjectPart part, UUID itemID, string script, int startParam, bool postOnRez, int stateSource)
        {
            NewRezScript handlerRezScript = OnRezScript;
            if (handlerRezScript != null)
            {
                foreach (NewRezScript d in handlerRezScript.GetInvocationList())
                {
                    try
                    {
                        d(part, itemID, script, startParam, postOnRez, stateSource);
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for TriggerRezScript failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
            }
        }

        public void TriggerRezScripts(SceneObjectPart part, TaskInventoryItem[] taskInventoryItem, int startParam, bool postOnRez, int stateSource, UUID RezzedFrom)
        {
            NewRezScripts handlerRezScripts = OnRezScripts;
            if (handlerRezScripts != null)
            {
                foreach (NewRezScripts d in handlerRezScripts.GetInvocationList())
                {
                    try
                    {
                        d(part, taskInventoryItem, startParam, postOnRez, stateSource, RezzedFrom);
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for TriggerRezScript failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
            }
        }

        public void TriggerRemoveScript(uint localID, UUID itemID)
        {
            RemoveScript handlerRemoveScript = OnRemoveScript;
            if (handlerRemoveScript != null)
            {
                foreach (RemoveScript d in handlerRemoveScript.GetInvocationList())
                {
                    try
                    {
                        d(localID, itemID);
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for TriggerRemoveScript failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
            }
        }

        public bool TriggerGroupMove(UUID groupID, Vector3 delta)
        {
            bool result = true;

            SceneGroupMoved handlerSceneGroupMove = OnSceneGroupMove;
            if (handlerSceneGroupMove != null)
            {
                foreach (SceneGroupMoved d in handlerSceneGroupMove.GetInvocationList())
                {
                    try
                    {
                        if (d(groupID, delta) == false)
                            result = false;
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for TriggerOnAttach failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
            }

            return result;
        }

        public bool TriggerGroupSpinStart(UUID groupID)
        {
            bool result = true;

            SceneGroupSpinStarted handlerSceneGroupSpinStarted = OnSceneGroupSpinStart;
            if (handlerSceneGroupSpinStarted != null)
            {
                foreach (SceneGroupSpinStarted d in handlerSceneGroupSpinStarted.GetInvocationList())
                {
                    try
                    {
                        if (d(groupID) == false)
                            result = false;
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for TriggerGroupSpinStart failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
            }

            return result;
        }

        public bool TriggerGroupSpin(UUID groupID, Quaternion rotation)
        {
            bool result = true;

            SceneGroupSpun handlerSceneGroupSpin = OnSceneGroupSpin;
            if (handlerSceneGroupSpin != null)
            {
                foreach (SceneGroupSpun d in handlerSceneGroupSpin.GetInvocationList())
                {
                    try
                    {
                        if (d(groupID, rotation) == false)
                            result = false;
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for TriggerGroupSpin failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
            }

            return result;
        }

        public void TriggerGroupGrab(UUID groupID, Vector3 offset, UUID userID)
        {
            SceneGroupGrabed handlerSceneGroupGrab = OnSceneGroupGrab;
            if (handlerSceneGroupGrab != null)
            {
                foreach (SceneGroupGrabed d in handlerSceneGroupGrab.GetInvocationList())
                {
                    try
                    {
                        d(groupID, offset, userID);
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for TriggerGroupGrab failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
            }
        }

        public void TriggerLandObjectAdded(LandData newParcel)
        {
            LandObjectAdded handlerLandObjectAdded = OnLandObjectAdded;
            if (handlerLandObjectAdded != null)
            {
                foreach (LandObjectAdded d in handlerLandObjectAdded.GetInvocationList())
                {
                    try
                    {
                        d(newParcel);
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for TriggerLandObjectAdded failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
            }
        }

        public void TriggerLandObjectRemoved(UUID regionID, UUID globalID)
        {
            LandObjectRemoved handlerLandObjectRemoved = OnLandObjectRemoved;
            if (handlerLandObjectRemoved != null)
            {
                foreach (LandObjectRemoved d in handlerLandObjectRemoved.GetInvocationList())
                {
                    try
                    {
                        d(regionID, globalID);
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for TriggerLandObjectRemoved failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
            }
        }

        public void TriggerAvatarEnteringNewParcel(ScenePresence avatar, int localLandID, UUID regionID)
        {
            AvatarEnteringNewParcel handlerAvatarEnteringNewParcel = OnAvatarEnteringNewParcel;
            if (handlerAvatarEnteringNewParcel != null)
            {
                foreach (AvatarEnteringNewParcel d in handlerAvatarEnteringNewParcel.GetInvocationList())
                {
                    try
                    {
                        d(avatar, localLandID, regionID);
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for TriggerAvatarEnteringNewParcel failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
            }
        }

        public void TriggerIncomingInstantMessage(GridInstantMessage message)
        {
            IncomingInstantMessage handlerIncomingInstantMessage = OnIncomingInstantMessage;
            if (handlerIncomingInstantMessage != null)
            {
                foreach (IncomingInstantMessage d in handlerIncomingInstantMessage.GetInvocationList())
                {
                    try
                    {
                        d(message);
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for TriggerIncomingInstantMessage failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
            }
        }

        public void TriggerUnhandledInstantMessage(GridInstantMessage message)
        {
            IncomingInstantMessage handlerUnhandledInstantMessage = OnUnhandledInstantMessage;
            if (handlerUnhandledInstantMessage != null)
            {
                foreach (IncomingInstantMessage d in handlerUnhandledInstantMessage.GetInvocationList())
                {
                    try
                    {
                        d(message);
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for TriggerOnAttach failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
            }
        }

        public void TriggerClientClosed(UUID ClientID, Scene scene)
        {
            ClientClosed handlerClientClosed = OnClientClosed;
            if (handlerClientClosed != null)
            {
                foreach (ClientClosed d in handlerClientClosed.GetInvocationList())
                {
                    try
                    {
                        d(ClientID, scene);
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for TriggerClientClosed failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
            }
        }

        public void TriggerOnMakeChildAgent(ScenePresence presence)
        {
            OnMakeChildAgentDelegate handlerMakeChildAgent = OnMakeChildAgent;
            if (handlerMakeChildAgent != null)
            {
                foreach (OnMakeChildAgentDelegate d in handlerMakeChildAgent.GetInvocationList())
                {
                    try
                    {
                        d(presence);
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for TriggerOnMakeChildAgent failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
            }
        }

        public void TriggerOnMakeRootAgent(ScenePresence presence)
        {
            OnMakeRootAgentDelegate handlerMakeRootAgent = OnMakeRootAgent;
            if (handlerMakeRootAgent != null)
            {
                foreach (OnMakeRootAgentDelegate d in handlerMakeRootAgent.GetInvocationList())
                {
                    try
                    {
                        d(presence);
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for TriggerOnMakeRootAgent failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
            }
        }

        public OSDMap TriggerOnRegisterCaps(UUID agentID)
        {
            OSDMap retVal = new OSDMap();
            RegisterCapsEvent handlerRegisterCaps = OnRegisterCaps;
            if (handlerRegisterCaps != null)
            {
                foreach (RegisterCapsEvent d in handlerRegisterCaps.GetInvocationList())
                {
                    try
                    {
                        OSDMap r = d(agentID, MainServer.Instance);
                        if (r != null)
                        {
                            foreach (KeyValuePair<string, OSD> kvp in r)
                            {
                                retVal[kvp.Key] = kvp.Value;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for TriggerOnRegisterCaps failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
            }
            return retVal;
        }

        public void TriggerOnDeregisterCaps(UUID agentID, IRegionClientCapsService caps)
        {
            DeregisterCapsEvent handlerDeregisterCaps = OnDeregisterCaps;
            if (handlerDeregisterCaps != null)
            {
                foreach (DeregisterCapsEvent d in handlerDeregisterCaps.GetInvocationList())
                {
                    try
                    {
                        d(agentID, caps);
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for TriggerOnDeregisterCaps failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
            }
        }

        public void TriggerOnNewInventoryItemUploadComplete(UUID agentID, UUID AssetID, String AssetName, int userlevel)
        {
            NewInventoryItemUploadComplete handlerNewInventoryItemUpdateComplete = OnNewInventoryItemUploadComplete;
            if (handlerNewInventoryItemUpdateComplete != null)
            {
                foreach (NewInventoryItemUploadComplete d in handlerNewInventoryItemUpdateComplete.GetInvocationList())
                {
                    try
                    {
                        d(agentID, AssetID, AssetName, userlevel);
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for TriggerOnNewInventoryItemUploadComplete failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
            }
        }

        public bool TriggerValidateBuyLand(LandBuyArgs args)
        {
            LandBuy handlerLandBuy = OnValidateBuyLand;
            if (handlerLandBuy != null)
            {
                foreach (LandBuy d in handlerLandBuy.GetInvocationList())
                {
                    try
                    {
                        if (!d(args))
                            return false;
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for TriggerLandBuy failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
            }
            return true;
        }

        public void TriggerAtTargetEvent(uint localID, uint handle, Vector3 targetpos, Vector3 currentpos)
        {
            ScriptAtTargetEvent handlerScriptAtTargetEvent = OnScriptAtTargetEvent;
            if (handlerScriptAtTargetEvent != null)
            {
                foreach (ScriptAtTargetEvent d in handlerScriptAtTargetEvent.GetInvocationList())
                {
                    try
                    {
                        d(localID, handle, targetpos, currentpos);
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for TriggerAtTargetEvent failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
            }
        }

        public void TriggerNotAtTargetEvent(uint localID)
        {
            ScriptNotAtTargetEvent handlerScriptNotAtTargetEvent = OnScriptNotAtTargetEvent;
            if (handlerScriptNotAtTargetEvent != null)
            {
                foreach (ScriptNotAtTargetEvent d in handlerScriptNotAtTargetEvent.GetInvocationList())
                {
                    try
                    {
                        d(localID);
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for TriggerNotAtTargetEvent failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
            }
        }

        public void TriggerAtRotTargetEvent(uint localID, uint handle, Quaternion targetrot, Quaternion currentrot)
        {
            ScriptAtRotTargetEvent handlerScriptAtRotTargetEvent = OnScriptAtRotTargetEvent;
            if (handlerScriptAtRotTargetEvent != null)
            {
                foreach (ScriptAtRotTargetEvent d in handlerScriptAtRotTargetEvent.GetInvocationList())
                {
                    try
                    {
                        d(localID, handle, targetrot, currentrot);
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for TriggerAtRotTargetEvent failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
            }
        }

        public void TriggerNotAtRotTargetEvent(uint localID)
        {
            ScriptNotAtRotTargetEvent handlerScriptNotAtRotTargetEvent = OnScriptNotAtRotTargetEvent;
            if (handlerScriptNotAtRotTargetEvent != null)
            {
                foreach (ScriptNotAtRotTargetEvent d in handlerScriptNotAtRotTargetEvent.GetInvocationList())
                {
                    try
                    {
                        d(localID);
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for TriggerNotAtRotTargetEvent failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
            }
        }

        public void TriggerRequestChangeWaterHeight(float height)
        {
            RequestChangeWaterHeight handlerRequestChangeWaterHeight = OnRequestChangeWaterHeight;
            if (handlerRequestChangeWaterHeight != null)
            {
                foreach (RequestChangeWaterHeight d in handlerRequestChangeWaterHeight.GetInvocationList())
                {
                    try
                    {
                        d(height);
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for TriggerRequestChangeWaterHeight failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
            }
        }

        public void TriggerSignificantClientMovement(IClientAPI client)
        {
            SignificantClientMovement handlerSignificantClientMovement = OnSignificantClientMovement;
            if (handlerSignificantClientMovement != null)
            {
                foreach (SignificantClientMovement d in handlerSignificantClientMovement.GetInvocationList())
                {
                    try
                    {
                        d(client);
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for TriggerSignificantClientMovement failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
            }
        }

        public void TriggerSignificantObjectMovement(SceneObjectGroup group)
        {
            SignificantObjectMovement handlerSignificantObjectMovement = OnSignificantObjectMovement;
            if (handlerSignificantObjectMovement != null)
            {
                foreach (SignificantObjectMovement d in handlerSignificantObjectMovement.GetInvocationList())
                {
                    try
                    {
                        d(group);
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for TriggerSignificantObjectMovement failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
            }
        }

        public void TriggerOnChatFromWorld(Object sender, OSChatMessage chat)
        {
            ChatFromWorldEvent handlerChatFromWorld = OnChatFromWorld;
            if (handlerChatFromWorld != null)
            {
                foreach (ChatFromWorldEvent d in handlerChatFromWorld.GetInvocationList())
                {
                    try
                    {
                        d(sender, chat);
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for TriggerOnChatFromWorld failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
            }
        }

        public void TriggerOnChatFromClient(Object sender, OSChatMessage chat)
        {
            ChatFromClientEvent handlerChatFromClient = OnChatFromClient;
            if (handlerChatFromClient != null)
            {
                foreach (ChatFromClientEvent d in handlerChatFromClient.GetInvocationList())
                {
                    try
                    {
                        d(sender, chat);
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for TriggerOnChatFromClient failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
            }
        }

        public void TriggerOnChatBroadcast(Object sender, OSChatMessage chat)
        {
            ChatBroadcastEvent handlerChatBroadcast = OnChatBroadcast;
            if (handlerChatBroadcast != null)
            {
                foreach (ChatBroadcastEvent d in handlerChatBroadcast.GetInvocationList())
                {
                    try
                    {
                        d(sender, chat);
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for TriggerOnChatBroadcast failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
            }
        }

        internal void TriggerControlEvent(SceneObjectPart part, UUID scriptUUID, UUID avatarID, uint held, uint _changed)
        {
            ScriptControlEvent handlerScriptControlEvent = OnScriptControlEvent;
            if (handlerScriptControlEvent != null)
            {
                foreach (ScriptControlEvent d in handlerScriptControlEvent.GetInvocationList())
                {
                    try
                    {
                        d(part, scriptUUID, avatarID, held, _changed);
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for TriggerControlEvent failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
            }
        }

        public void TriggerIncomingLandDataFromStorage(List<LandData> landData)
        {
            IncomingLandDataFromStorage handlerIncomingLandDataFromStorage = OnIncomingLandDataFromStorage;
            if (handlerIncomingLandDataFromStorage != null)
            {
                foreach (IncomingLandDataFromStorage d in handlerIncomingLandDataFromStorage.GetInvocationList())
                {
                    try
                    {
                        d(landData);
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for TriggerIncomingLandDataFromStorage failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
            }
        }

        /// <summary>
        /// Updates the system as to how the position of the sun should be handled.
        /// </summary>
        /// <param name="regionHandle"></param>
        /// <param name="FixedTime">True if the Sun Position is fixed</param>
        /// <param name="useEstateTime">True if the Estate Settings should be used instead of region</param>
        /// <param name="FixedSunHour">The hour 0.0 <= FixedSunHour <= 24.0 at which the sun is fixed at. Sun Hour 0 is sun-rise, when Day/Night ratio is 1:1</param>
        public void TriggerEstateToolsSunUpdate(ulong regionHandle, bool FixedTime, bool useEstateTime, float FixedSunHour)
        {
            EstateToolsSunUpdate handlerEstateToolsSunUpdate = OnEstateToolsSunUpdate;
            if (handlerEstateToolsSunUpdate != null)
            {
                foreach (EstateToolsSunUpdate d in handlerEstateToolsSunUpdate.GetInvocationList())
                {
                    try
                    {
                        d(regionHandle, FixedTime, useEstateTime, FixedSunHour);
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for TriggerEstateToolsSunUpdate failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
            }
        }

        public void TriggerOarFileLoaded(Guid requestId, string message)
        {
            OarFileLoaded handlerOarFileLoaded = OnOarFileLoaded;
            if (handlerOarFileLoaded != null)
            {
                foreach (OarFileLoaded d in handlerOarFileLoaded.GetInvocationList())
                {
                    try
                    {
                        d(requestId, message);
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for TriggerOarFileLoaded failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
            }
        }

        public void TriggerOarFileSaved(Guid requestId, string message)
        {
            OarFileSaved handlerOarFileSaved = OnOarFileSaved;
            if (handlerOarFileSaved != null)
            {
                foreach (OarFileSaved d in handlerOarFileSaved.GetInvocationList())
                {
                    try
                    {
                        d(requestId, message);
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for TriggerOarFileSaved failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
            }
        }

        public void TriggerEmptyScriptCompileQueue(int numScriptsFailed, string message)
        {
            EmptyScriptCompileQueue handlerEmptyScriptCompileQueue = OnEmptyScriptCompileQueue;
            if (handlerEmptyScriptCompileQueue != null)
            {
                foreach (EmptyScriptCompileQueue d in handlerEmptyScriptCompileQueue.GetInvocationList())
                {
                    try
                    {
                        d(numScriptsFailed, message);
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for TriggerEmptyScriptCompileQueue failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
            }
        }

        public void TriggerScriptCollidingStart(SceneObjectPart part, ColliderArgs colliders)
        {
            ScriptColliding handlerCollidingStart = OnScriptColliderStart;
            if (handlerCollidingStart != null)
            {
                foreach (ScriptColliding d in handlerCollidingStart.GetInvocationList())
                {
                    try
                    {
                        d(part, colliders);
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for TriggerScriptCollidingStart failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
            }
        }

        public void TriggerScriptColliding(SceneObjectPart part, ColliderArgs colliders)
        {
            ScriptColliding handlerColliding = OnScriptColliding;
            if (handlerColliding != null)
            {
                foreach (ScriptColliding d in handlerColliding.GetInvocationList())
                {
                    try
                    {
                        d(part, colliders);
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for TriggerScriptColliding failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
            }
        }

        public void TriggerScriptCollidingEnd(SceneObjectPart part, ColliderArgs colliders)
        {
            ScriptColliding handlerCollidingEnd = OnScriptCollidingEnd;
            if (handlerCollidingEnd != null)
            {
                foreach (ScriptColliding d in handlerCollidingEnd.GetInvocationList())
                {
                    try
                    {
                        d(part, colliders);
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for TriggerScriptCollidingEnd failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
            }
        }

        public void TriggerScriptLandCollidingStart(SceneObjectPart part, ColliderArgs colliders)
        {
            ScriptColliding handlerLandCollidingStart = OnScriptLandColliderStart;
            if (handlerLandCollidingStart != null)
            {
                foreach (ScriptColliding d in handlerLandCollidingStart.GetInvocationList())
                {
                    try
                    {
                        d(part, colliders);
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for TriggerScriptLandCollidingStart failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
            }
        }

        public void TriggerScriptLandColliding(SceneObjectPart part, ColliderArgs colliders)
        {
            ScriptColliding handlerLandColliding = OnScriptLandColliding;
            if (handlerLandColliding != null)
            {
                foreach (ScriptColliding d in handlerLandColliding.GetInvocationList())
                {
                    try
                    {
                        d(part, colliders);
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for TriggerScriptLandColliding failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
            }
        }

        public void TriggerScriptLandCollidingEnd(SceneObjectPart part, ColliderArgs colliders)
        {
            ScriptColliding handlerLandCollidingEnd = OnScriptLandColliderEnd;
            if (handlerLandCollidingEnd != null)
            {
                foreach (ScriptColliding d in handlerLandCollidingEnd.GetInvocationList())
                {
                    try
                    {
                        d(part, colliders);
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for TriggerScriptLandCollidingEnd failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
            }
        }

        public void TriggerOnRegionUp(GridRegion otherRegion)
        {
            RegionUp handlerOnRegionUp = OnRegionUp;
            if (handlerOnRegionUp != null)
            {
                foreach (RegionUp d in handlerOnRegionUp.GetInvocationList())
                {
                    try
                    {
                        d(otherRegion);
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for TriggerOnRegionUp failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
            }
        }

        public void TriggerOnRegionDown(GridRegion otherRegion)
        {
            RegionUp handlerOnRegionDown = OnRegionDown;
            if (handlerOnRegionDown != null)
            {
                foreach (RegionUp d in handlerOnRegionDown.GetInvocationList())
                {
                    try
                    {
                        d(otherRegion);
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for TriggerOnRegionUp failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
            }
        }

        public void TriggerModuleFinishedStartup(string name, List<string> data)
        {
            FinishedStartup handlerOnFinishedStartup = OnModuleFinishedStartup;
            if (handlerOnFinishedStartup != null)
            {
                foreach (FinishedStartup d in handlerOnFinishedStartup.GetInvocationList())
                {
                    try
                    {
                        d(name, data);
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for FinishedStartup failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
            }
        }

        public void TriggerAddToStartupQueue(string name)
        {
            AddToStartupQueue handlerOnAddToStartupQueue = OnAddToStartupQueue;
            if (handlerOnAddToStartupQueue != null)
            {
                foreach (AddToStartupQueue d in handlerOnAddToStartupQueue.GetInvocationList())
                {
                    try
                    {
                        d(name);
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for AddToStartupQueue failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
            }
        }

        public void TriggerStartupComplete(IScene scene, List<string> StartupData)
        {
            StartupComplete handlerOnStartupComplete = OnStartupComplete;
            StartupComplete handlerOnStartupFullyComplete = OnStartupFullyComplete;
            if (handlerOnStartupComplete != null)
            {
                foreach (StartupComplete d in handlerOnStartupComplete.GetInvocationList())
                {
                    try
                    {
                        d(scene, StartupData);
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[EVENT MANAGER]: Delegate for StartupComplete failed - continuing.  {0} {1}",
                            e.ToString(), e.StackTrace);
                    }
                }
                if (handlerOnStartupFullyComplete != null)
                {
                    foreach (StartupComplete d in handlerOnStartupFullyComplete.GetInvocationList())
                    {
                        try
                        {
                            d(scene, StartupData);
                        }
                        catch (Exception e)
                        {
                            m_log.ErrorFormat(
                                "[EVENT MANAGER]: Delegate for StartupComplete failed - continuing.  {0} {1}",
                                e.ToString(), e.StackTrace);
                        }
                    }
                }
            }
        }
    }
}
