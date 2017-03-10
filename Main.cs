using Rocket.Core.Plugins;
using Rocket.Unturned.Chat;
using SDG.Unturned;
using Steamworks;
using Rocket.Unturned;
using Rocket.Unturned.Player;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace GroupCapper
{
	public class Main : RocketPlugin<Config>
	{ 
		private int maxPlayers;
		private bool changePlayersGroup;
		private Color chatColor;
		private List<CSteamID> toMessage = new List<CSteamID>();
		private bool notfiyPlayer;

		protected override void Load()
		{
			//Dank ass ASCII and some other shit
			Rocket.Core.Logging.Logger.LogError("\n  _________                    __          \n /   _____/ ____  ____   _____/  |_ ___.__.\n \\_____  \\_/ ___\\/  _ \\ /  _ \\   __<   |  |\n /        \\  \\__(  <_> (  <_> |  |  \\___  |\n/_______  /\\___  \\____/ \\____/|__|  / ____|\n        \\/     \\/                   \\/     ");
			Rocket.Core.Logging.Logger.Log("\n.___        \n|   | ______\n|   |/  ___/\n|   |\\___ \\ \n|___/____  >\n         \\/ ");
			Rocket.Core.Logging.Logger.LogWarning("\n___.                  \n\\_ |__ _____    ____  \n | __ \\\\__  \\ _/ __ \\ \n | \\_\\ \\/ __ \\\\  ___/ \n |___  (____  /\\___  >\n     \\/     \\/     \\/ ");
		

			maxPlayers = this.Configuration.Instance.maxGroupSize;
			changePlayersGroup = this.Configuration.Instance.changePlayerGroups;
			chatColor =  UnturnedChat.GetColorFromName(this.Configuration.Instance.chatColor, Color.green);
			notfiyPlayer = this.Configuration.Instance.notifyPlayer;

			Rocket.Unturned.Permissions.UnturnedPermissions.OnJoinRequested += JoinT;
			U.Events.OnPlayerConnected += Join;
		}

		protected override void Unload()
		{

			//Closing the events so we dont loop through code twice if a rocket reload were to happen
			Rocket.Unturned.Permissions.UnturnedPermissions.OnJoinRequested -= JoinT;
			U.Events.OnPlayerConnected -= Join;
		}

		//Translation Allow Users To Change The Messages in the translation file
		public override Rocket.API.Collections.TranslationList DefaultTranslations
		{
			get
			{
				return new Rocket.API.Collections.TranslationList
				{
					{"reason", "You were moved out of your group"}
				};
			}
		}

		//This is called before any sort of player is initialized (Ty Trojaner you're a gun for showing me steam pending)
		void JoinT(CSteamID player, ref ESteamRejection? rejectionReason)
		{
			if (!changePlayersGroup)
			{
				rejectionReason = ESteamRejection.SERVER_FULL;
				return;
			}

			//Put this in try so it doesnt spam errors/unload when something bad happens
			try
			{
				SteamPending pending = null;
				int playersInMyGroup = 0;

				//Finding SteamPending (player which we can make changes to). 
				pending = Provider.pending.Find((c) => c.playerID.steamID == player);
				//Looping through steamplayers with the same group as the pending
				for (int i = 0; i < Provider.clients.Count; i++) {
					SteamPlayer ply = Provider.clients[i];

					if(ply.playerID.group.m_SteamID != pending.playerID.group.m_SteamID || ply.playerID.group == CSteamID.Nil)
						continue;
					//This bit is untested idk how it works
					playersInMyGroup++;

				}


				//Checking if there is anymore team members in the queueeueueue with the same group
				for (int b = 0, index = int.MaxValue; b < Provider.pending.Count; b++)
				{

					//Finding the index of the player 
					if (Provider.pending[b] == pending)
					{
						index = b;
						continue;
					}

					//If the player is going to join before us to add to playersInMyGroup
					if (b < index && Provider.pending[b].playerID.group == pending.playerID.group)
						playersInMyGroup++;
				}

				//If players in group is greater than or equal to the max players set  it to nil (none)
				if (playersInMyGroup >= maxPlayers)
				{
					pending.playerID.group = CSteamID.Nil;
					//Adding To list so we can notify the player upon joining
					toMessage.Add(pending.playerID.steamID);
				}
			}
			catch { Rocket.Core.Logging.Logger.Log("Something bad happened while changing the players info"); }
		}


		private void Join(UnturnedPlayer player)
		{
			if (!notfiyPlayer)
				return;
			
			//Notifying the player
			if (toMessage.Contains(player.CSteamID))
			{
				//Translation Allow Users To Change The Messages in the translation file
				UnturnedChat.Say(player, Translate("reason"), chatColor);
				toMessage.Remove(player.CSteamID);
			}
		}

	}
}

