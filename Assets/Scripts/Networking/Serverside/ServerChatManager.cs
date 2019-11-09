using System;
using System.Collections.Generic;

using SMGCore.EventSys;
using Voxels.Networking.Events;

namespace Voxels.Networking.Serverside {
	public class ServerChatManager : ServerSideController<ServerChatManager> {
		public ServerChatManager(ServerGameManager owner) : base(owner) { }

		List<ChatMessage> _messages = new List<ChatMessage>();

		public override void PostLoad() {
			base.PostLoad();
			EventManager.Subscribe<OnServerReceivedChatMessage>(this, OnChatMessageReceived);
		}

		public override void Reset() {
			base.Reset();
			EventManager.Unsubscribe<OnServerReceivedChatMessage>(OnChatMessageReceived);
		}

		public void SendToClient(ClientState client, string senderName, string message) {
			var server = ServerController.Instance;
			server.SendNetMessage(client, ServerPacketID.ChatMessage, new S_ChatMessage { SenderName = senderName, MessageText = message });
		}

		public void BroadcastFromServer(string message) {
			var serverName = "Server";
			_messages.Add(new ChatMessage(serverName, message, DateTime.Now));
			SendToAll(serverName, message);
		}

		void SendToAll(string senderName, string message) {
			var clients = ServerController.Instance.Clients;
			foreach ( var pair in clients ) {
				var cli = pair.Value;
				SendToClient(cli, senderName, message);
			}
		}

		void OnChatMessageReceived(OnServerReceivedChatMessage e) {
			var msg = new ChatMessage(e.Sender.UserName, e.Message, DateTime.Now);
			_messages.Add(msg);
			SendToAll(msg.PlayerName, msg.Message);
		}
	}
}

