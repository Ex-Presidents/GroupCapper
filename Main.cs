using Rocket.Core.Plugins;
using Rocket.Unturned.Chat;
using SDG.Unturned;
using Steamworks;
using Rocket.Unturned;
using Rocket.Unturned.Player;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace GroupCapper
{
	public class Main : RocketPlugin<Config>
	{
        public static Main Instance;
		private int _maxPlayers;
		private bool _changePlayersGroup;
		private Color _chatColor;
		private bool _notfiyPlayer;

		protected override void Load()
		{
            Instance = this;
			//Dank ass ASCII and some other shit
			Rocket.Core.Logging.Logger.LogError("\n  _________                    __          \n /   _____/ ____  ____   _____/  |_ ___.__.\n \\_____  \\_/ ___\\/  _ \\ /  _ \\   __<   |  |\n /        \\  \\__(  <_> (  <_> |  |  \\___  |\n/_______  /\\___  \\____/ \\____/|__|  / ____|\n        \\/     \\/                   \\/     ");
			Rocket.Core.Logging.Logger.Log("\n.___        \n|   | ______\n|   |/  ___/\n|   |\\___ \\ \n|___/____  >\n         \\/ ");
			Rocket.Core.Logging.Logger.LogWarning("\n___.                  \n\\_ |__ _____    ____  \n | __ \\\\__  \\ _/ __ \\ \n | \\_\\ \\/ __ \\\\  ___/ \n |___  (____  /\\___  >\n     \\/     \\/     \\/ ");
		

			_maxPlayers = Instance.Configuration.Instance.maxGroupSize;
			_changePlayersGroup = Instance.Configuration.Instance.changePlayerGroups;
			_chatColor =  UnturnedChat.GetColorFromName(Instance.Configuration.Instance.chatColor, Color.green);
			_notfiyPlayer = Instance.Configuration.Instance.notifyPlayer;

			U.Events.OnPlayerConnected += Join;
		    U.Events.OnPlayerDisconnected += Leave;


            //If the plugin is reloaded, re add the player events
		    try
            { 
		        foreach (var steamPlayer in Provider.clients)
		        {
		            steamPlayer.player.quests.groupIDChanged += GroupChanged;
		        }
            }
            catch { }
		}



	    protected override void Unload()
	    {
	        U.Events.OnPlayerDisconnected -= Leave;
			U.Events.OnPlayerConnected -= Join;


            //If the plugin is unloaded remove the events
	        try
	        {
	            foreach (var steamPlayer in Provider.clients)
	            {
	                steamPlayer.player.quests.groupIDChanged -= GroupChanged;
	            }
	        }
	        catch { }

        }
        
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


        private void Join(UnturnedPlayer player)
        {
            player.Player.quests.groupIDChanged += GroupChanged;
            CheckRemoveGroup(player);
			
		}

	    private void Leave(UnturnedPlayer player)
	    {
	        player.Player.quests.groupIDChanged -= GroupChanged;
        }

        private void GroupChanged(PlayerQuests sender, CSteamID oldgroupid, CSteamID newgroupid)
	    {
            if(newgroupid != CSteamID.Nil)
                CheckRemoveGroup(UnturnedPlayer.FromPlayer(sender.player));
	    }


	    private void CheckRemoveGroup(UnturnedPlayer player)
	    {
	        if (GroupManager.getGroupInfo(player.Player.quests.groupID).members <= _maxPlayers) return;

	        player.Player.quests.channel.send("tellSetGroup", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER,
	            new object[]
	            {
	                CSteamID.Nil,
	                (byte) 0
	            });

	        var mGroupInfo = typeof(PlayerQuests).GetField("inMainGroup",
	            BindingFlags.Instance | BindingFlags.NonPublic);
	        if (mGroupInfo != null)
	            mGroupInfo
	                .SetValue(player.Player.quests, false);

	        //Notifying the player
	        if (_notfiyPlayer)
	            UnturnedChat.Say(player, Translate("reason"), _chatColor);
        }

	}
}

