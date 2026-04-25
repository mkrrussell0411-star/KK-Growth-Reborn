using System;
using BepInEx.Configuration;
using HarmonyLib;

namespace KK_Growth
{
	// Token: 0x02000008 RID: 8
	internal interface IFeature
	{
		// Token: 0x06000025 RID: 37
		bool Install(Harmony instance, ConfigFile config);
	}
}
