using System;
using Rocket.API;
namespace GroupCapper
{
	public class Config : IRocketPluginConfiguration
	{
		public int maxGroupSize;
		public bool changePlayerGroups;
		public string chatColor;
		public bool notifyPlayer;
		public void LoadDefaults()
		{
			notifyPlayer = true;
			chatColor = "cyan";
			maxGroupSize = 5;
			changePlayerGroups = true;
		}
	}
}
