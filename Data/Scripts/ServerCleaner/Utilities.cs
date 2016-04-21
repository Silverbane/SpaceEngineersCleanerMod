﻿using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace ServerCleaner
{
	public static class Utilities
	{
		public const string ServerNameInChat = "Server";
		public const int MaxDisplayedMessageLength = 400; // the chat window can fit about 200 W characters
		public const string MessageSnip = " [...]";

		public static bool IsGameRunning()
		{
			return
				MyAPIGateway.Entities != null &&
				MyAPIGateway.Multiplayer != null &&
				MyAPIGateway.Players != null &&
				MyAPIGateway.Session != null &&
				MyAPIGateway.Utilities != null;
		}

		public static void ShowMessageFromServer(string text)
		{
			Logger.WriteLine("{0}: {1}", ServerNameInChat, text);

			if (!MyAPIGateway.Multiplayer.MultiplayerActive)
			{
				ShowMessageFromServerOnClient(text);
			}
			else  if (MyAPIGateway.Multiplayer.IsServer)
			{
				var bytes = Encoding.Unicode.GetBytes(text);

				var players = new List<IMyPlayer>();
				MyAPIGateway.Players.GetPlayers(players, p => p != null);

				foreach (var player in players)
					MyAPIGateway.Multiplayer.SendMessageTo(MessageIds.MessageFromServer, bytes, player.SteamUserId);
			}
		}

		public static void ShowMessageFromServer(string format, params object[] args)
		{
			ShowMessageFromServer(string.Format(format, args));
		}

		public static void ShowMessageFromServerOnClient(string text)
		{
			if (text.Length > MaxDisplayedMessageLength)
				text = text.Substring(0, MaxDisplayedMessageLength - MessageSnip.Length) + MessageSnip;

			MyAPIGateway.Utilities.ShowMessage(ServerNameInChat, text);
		}

		public static bool AnyWithinDistance(Vector3D position, List<Vector3D> otherPositions, double threshold)
		{
			foreach (var otherPosition in otherPositions)
				if ((otherPosition - position).Length() < threshold)
					return true;

			return false;
		}

		public static bool IsConnectableToOtherGrids(IMySlimBlock slimBlock)
		{
			var fatBlock = slimBlock.FatBlock;

			if (fatBlock == null)
				return false;

			return
				fatBlock is IMyMotorBase ||
				fatBlock is IMyMotorRotor ||
				fatBlock is IMyPistonBase ||
				fatBlock is IMyPistonTop;
		}

		public static string GetOwnerNameString(IMyEntity entity, List<IMyIdentity> playerIdentities)
		{
			var cubeGrid = entity as IMyCubeGrid;
			return cubeGrid == null ? "???" : GetOwnerNameString(cubeGrid.SmallOwners, playerIdentities);
		}

		public static string GetOwnerNameString(List<long> ownerIds, List<IMyIdentity> playerIdentities)
		{
			var result = string.Join(" & ", playerIdentities
				.Where(identity => ownerIds.Contains(identity.PlayerId))
				.Select(identity => identity.DisplayName));

			return result.Length > 0 ? result : "noone";
		}
	}
}
