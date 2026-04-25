using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using ExtensibleSaveFormat;
using HarmonyLib;
using KKAPI.Chara;
using KKAPI.MainGame;
using KKAPI.Utilities;
using Manager;
using Studio;
using UnityEngine;

namespace KK_Growth
{
	// Token: 0x0200000B RID: 11
	[BepInPlugin("KK_Growth", "KK_Growth", "1.2")]
	[BepInDependency("KKABMX.Core", "4.4.1")]
	[BepInDependency("marco.kkapi", "1.30")]
	public class GrowthPlugin : BaseUnityPlugin
	{
		// Token: 0x17000006 RID: 6
		// (get) Token: 0x0600003F RID: 63 RVA: 0x000035E1 File Offset: 0x000017E1
		// (set) Token: 0x06000040 RID: 64 RVA: 0x000035E8 File Offset: 0x000017E8
		public static ConfigEntry<bool> ShowPregnancyIconEarly { get; private set; }

		// Token: 0x17000007 RID: 7
		// (get) Token: 0x06000041 RID: 65 RVA: 0x000035F0 File Offset: 0x000017F0
		// (set) Token: 0x06000042 RID: 66 RVA: 0x000035F7 File Offset: 0x000017F7
		public static ConfigEntry<bool> HSceneMenstrIconOverride { get; private set; }

		// Token: 0x17000008 RID: 8
		// (get) Token: 0x06000043 RID: 67 RVA: 0x000035FF File Offset: 0x000017FF
		// (set) Token: 0x06000044 RID: 68 RVA: 0x00003606 File Offset: 0x00001806
		public static ConfigEntry<bool> ResidualGrowth { get; private set; }

		// Token: 0x17000009 RID: 9
		// (get) Token: 0x06000045 RID: 69 RVA: 0x0000360E File Offset: 0x0000180E
		// (set) Token: 0x06000046 RID: 70 RVA: 0x00003615 File Offset: 0x00001815
		public static ConfigEntry<int> ResidualGrowthRate { get; private set; }

		// Token: 0x1700000A RID: 10
		// (get) Token: 0x06000047 RID: 71 RVA: 0x0000361D File Offset: 0x0000181D
		// (set) Token: 0x06000048 RID: 72 RVA: 0x00003624 File Offset: 0x00001824
		public static ConfigEntry<bool> InflationEnable { get; private set; }

		// Token: 0x1700000B RID: 11
		// (get) Token: 0x06000049 RID: 73 RVA: 0x0000362C File Offset: 0x0000182C
		// (set) Token: 0x0600004A RID: 74 RVA: 0x00003633 File Offset: 0x00001833
		public static ConfigEntry<int> NutTotal { get; private set; }

		// Token: 0x1700000C RID: 12
		// (get) Token: 0x0600004B RID: 75 RVA: 0x0000363B File Offset: 0x0000183B
		// (set) Token: 0x0600004C RID: 76 RVA: 0x00003642 File Offset: 0x00001842
		public static ConfigEntry<bool> HShrink { get; private set; }

		// Token: 0x1700000D RID: 13
		// (get) Token: 0x0600004D RID: 77 RVA: 0x0000364A File Offset: 0x0000184A
		// (set) Token: 0x0600004E RID: 78 RVA: 0x00003651 File Offset: 0x00001851
		public static ConfigEntry<float> BreastExpansion { get; private set; }

		// Token: 0x1700000E RID: 14
		// (get) Token: 0x0600004F RID: 79 RVA: 0x00003659 File Offset: 0x00001859
		// (set) Token: 0x06000050 RID: 80 RVA: 0x00003660 File Offset: 0x00001860
		public static ConfigEntry<float> AssExpansion { get; private set; }

		// Token: 0x1700000F RID: 15
		// (get) Token: 0x06000051 RID: 81 RVA: 0x00003668 File Offset: 0x00001868
		// (set) Token: 0x06000052 RID: 82 RVA: 0x0000366F File Offset: 0x0000186F
		public static ConfigEntry<float> HeightScale { get; private set; }

		// Token: 0x17000010 RID: 16
		// (get) Token: 0x06000053 RID: 83 RVA: 0x00003677 File Offset: 0x00001877
		// (set) Token: 0x06000054 RID: 84 RVA: 0x0000367E File Offset: 0x0000187E
		public static ConfigEntry<float> GrowthSpeed { get; private set; }

		// Token: 0x17000011 RID: 17
		// (get) Token: 0x06000055 RID: 85 RVA: 0x00003686 File Offset: 0x00001886
		// (set) Token: 0x06000056 RID: 86 RVA: 0x0000368D File Offset: 0x0000188D
		public static ConfigEntry<bool> InflationOpenClothAtMax { get; private set; }

		// Token: 0x17000012 RID: 18
		// (get) Token: 0x06000057 RID: 87 RVA: 0x00003695 File Offset: 0x00001895
		// (set) Token: 0x06000058 RID: 88 RVA: 0x0000369C File Offset: 0x0000189C
		public static ConfigEntry<GrowthTarget> GrowthTargetMode { get; private set; }

		internal static ManualLogSource Logger { get; private set; }

		// Token: 0x06000059 RID: 89 RVA: 0x000036A4 File Offset: 0x000018A4
		private void Start()
		{
			GrowthPlugin.Logger = base.Logger;
			GrowthPlugin.HeightScale = base.Config.Bind<float>("General", "Height Scaling slider (Requires Restart)", 50f, new ConfigDescription("Modifer for height scaling. Amount of growth per male orgasm is equal to this value/orgasms to final size. Values smaller than 1 cause shrinking, larger than 1 growth.", new AcceptableValueRange<float>(0.1f, 1000f), new object[0]));
			GrowthPlugin.BreastExpansion = base.Config.Bind<float>("General", "Breast Expansion slider (Requires Restart)", 10f, new ConfigDescription("Modifer for breast expansion. Values smaller than 1 cause shrinking, larger than 1 growth.", new AcceptableValueRange<float>(0.1f, 30f), new object[0]));
			GrowthPlugin.AssExpansion = base.Config.Bind<float>("General", "Ass Expansion slider (Requires Restart)", 10f, new ConfigDescription("Modifer for breast expansion. Values smaller than 1 cause shrinking, larger than 1 growth.", new AcceptableValueRange<float>(0.1f, 30f), new object[0]));
			GrowthPlugin.NutTotal = base.Config.Bind<int>("General", "Orgasms to final size", 1000, new ConfigDescription("Total number of orgasms needed for female to reach final size", new AcceptableValueRange<int>(1, 1000), new object[0]));
			GrowthPlugin.ResidualGrowth = base.Config.Bind<bool>("General", "Residual Growth Option", false, "Turning this option on will cause girls you've orgasmed in to grow over time.");
			GrowthPlugin.ResidualGrowthRate = base.Config.Bind<int>("General", "Rate of Residual Growth", 1, new ConfigDescription("Equivalent times of cumming in a girl per week", new AcceptableValueRange<int>(1, 5), new object[0]));
			GrowthPlugin.ShowPregnancyIconEarly = base.Config.Bind<bool>("Legacy", "Show pregnancy icon early", false, "By default pregnancy status icon in class roster is shown after a few days or weeks (the character had a chance to do the test or noticed something is wrong).\nTurning this on will always make the icon show up at the end of the current day.");
			GrowthPlugin.HShrink = base.Config.Bind<bool>("General", "Residual Growth Option", true, "Turning this option on will cause girls you've orgasmed in to shrink if you pull out in an H Scene.");
			GrowthPlugin.HSceneMenstrIconOverride = base.Config.Bind<bool>("Legacy", "Use custom safe/risky icons in H Scenes", true, "Replaces the standard safe/risky indicators with custom indicators that can also show pregnancy and unknown status. If the status is unknown you will have to listen for the voice cues instead.\nChanges take effect after game restart.");
			GrowthPlugin.InflationEnable = base.Config.Bind<bool>("General", "Enable H-scene growth.", true, "Turn on the in H-scene growth effect.");
			GrowthPlugin.GrowthSpeed = base.Config.Bind<float>("General", "Scaling speed modifier", 1f, new ConfigDescription("How quickly the girl's growth happens in scene (i.e. 3 = 3x faster).", new AcceptableValueRange<float>(0.1f, 3f), new object[0]));
			GrowthPlugin.InflationOpenClothAtMax = base.Config.Bind<bool>("Legacy", "Open clothes at max inflation", true, "If clothes are fully on, open them when inflation reaches the max value (they 'burst' open).");
			GrowthPlugin.GrowthTargetMode = base.Config.Bind<GrowthTarget>("General", "Growth Target", GrowthTarget.Both, "Who grows when orgasm occurs inside: Female (heroine only), Player (player character only), or Both.");
			CharacterApi.RegisterExtraBehaviour<PregnancyCharaController>("KK_Growth");
			GameAPI.RegisterExtraBehaviour<PregnancyGameController>("KK_Growth");
			Harmony hi = new Harmony("KK_Growth");
			GrowthPlugin.Hooks.InitHooks(hi);
			GrowthGui.Init(hi, this);
			this.LoadFeatures(hi);
			bool flag = TimelineCompatibility.IsTimelineAvailable();
			if (flag)
			{
				TimelineCompatibility.AddCharaFunctionInterpolable<int, PregnancyCharaController>("KK_Growth", "week", "Pregnancy week", delegate(OCIChar oci, PregnancyCharaController parameter, int leftValue, int rightValue, float factor)
				{
					parameter.Data.Week = Mathf.RoundToInt(Mathf.LerpUnclamped((float)leftValue, (float)rightValue, factor));
				}, null, (OCIChar oci, PregnancyCharaController parameter) => parameter.Data.Week, null, null, null, true, null, null);
			}
		}

		// Token: 0x0600005A RID: 90 RVA: 0x0000398C File Offset: 0x00001B8C
		private void LoadFeatures(Harmony hi)
		{
			Type featureT = typeof(IFeature);
			IEnumerable<Type> types = from x in typeof(GrowthPlugin).Assembly.GetTypes()
				where featureT.IsAssignableFrom(x) && x.IsClass
				select x;
			List<string> successful = new List<string>();
			foreach (Type type in types)
			{
				IFeature feature = (IFeature)Activator.CreateInstance(type);
				bool flag = feature.Install(hi, base.Config);
				if (flag)
				{
					successful.Add(type.Name);
				}
			}
			GrowthPlugin.Logger.LogInfo("Loaded features: " + string.Join(", ", successful.ToArray()));
		}

		// Token: 0x0600005B RID: 91 RVA: 0x00003A6C File Offset: 0x00001C6C
		internal static PregnancyCharaController GetEffectController(SaveData.Heroine heroine)
		{
			return (((heroine != null) ? heroine.chaCtrl : null) != null) ? heroine.chaCtrl.GetComponent<PregnancyCharaController>() : null;
		}

		internal static PregnancyCharaController GetPlayerController()
		{
			SaveData.Player player = (Singleton<Game>.Instance != null) ? Singleton<Game>.Instance.Player : null;
			return (player != null && player.chaCtrl != null) ? player.chaCtrl.GetComponent<PregnancyCharaController>() : null;
		}

		// Token: 0x0400002B RID: 43
		public const string GUID = "KK_Growth";

		// Token: 0x0400002C RID: 44
		public const string Version = "1.2";

		// Token: 0x02000012 RID: 18
		private static class Hooks
		{
			// Token: 0x0600007B RID: 123 RVA: 0x000048E0 File Offset: 0x00002AE0
			public static void InitHooks(Harmony harmonyInstance)
			{
				harmonyInstance.PatchAll(typeof(GrowthPlugin.Hooks));
				GrowthPlugin.Hooks.PatchNPCLoadAll(harmonyInstance);
			}

			// Token: 0x0600007C RID: 124 RVA: 0x000048FB File Offset: 0x00002AFB
			[HarmonyPostfix]
			[HarmonyPatch(typeof(SaveData.Heroine), "MenstruationDay", MethodType.Getter)]
			private static void LastAccessedHeroinePatch(SaveData.Heroine __instance)
			{
				GrowthPlugin.Hooks._lastHeroine = __instance;
			}

			// Token: 0x0600007D RID: 125 RVA: 0x00004904 File Offset: 0x00002B04
			[HarmonyPrefix]
			[HarmonyPatch(typeof(HFlag), "GetMenstruation", new Type[] { typeof(byte) })]
			private static void GetMenstruationOverridePrefix()
			{
				bool flag = GrowthPlugin.Hooks._lastHeroine != null;
				if (flag)
				{
					MenstruationSchedule schedule = GrowthPlugin.Hooks._lastHeroine.GetRelatedChaFiles().Select(delegate(ChaFileControl c)
					{
						PregnancyData pregnancyData = PregnancyData.Load(ExtendedSave.GetExtendedDataById(c, "KK_Growth"));
						return (pregnancyData != null) ? pregnancyData.MenstruationSchedule : MenstruationSchedule.Default;
					}).FirstOrDefault((MenstruationSchedule x) => x > MenstruationSchedule.Default);
					GrowthPlugin.Hooks._menstruationsBackup = HFlag.menstruations;
					HFlag.menstruations = PregnancyCharaController.GetMenstruationsArr(schedule);
				}
			}

			// Token: 0x0600007E RID: 126 RVA: 0x00004988 File Offset: 0x00002B88
			[HarmonyPostfix]
			[HarmonyPatch(typeof(HFlag), "GetMenstruation", new Type[] { typeof(byte) })]
			private static void GetMenstruationOverridePostfix()
			{
				bool flag = GrowthPlugin.Hooks._menstruationsBackup != null;
				if (flag)
				{
					HFlag.menstruations = GrowthPlugin.Hooks._menstruationsBackup;
					GrowthPlugin.Hooks._menstruationsBackup = null;
				}
			}

			// Token: 0x0600007F RID: 127 RVA: 0x000049B4 File Offset: 0x00002BB4
			private static void PatchNPCLoadAll(Harmony instance)
			{
				HarmonyMethod transpiler = new HarmonyMethod(typeof(GrowthPlugin.Hooks), "NPCLoadAllTpl", null);
				foreach (MethodInfo target in from x in typeof(ActionScene).GetMethods(AccessTools.all)
					where x.Name == "NPCLoadAll"
					select x)
				{
					GrowthPlugin.Logger.LogDebug("Patching " + GeneralExtensions.FullDescription(target));
					CoroutineUtils.PatchMoveNext(instance, target, null, null, transpiler, null, null);
				}
			}

			// Token: 0x06000080 RID: 128 RVA: 0x00004A70 File Offset: 0x00002C70
			private static IEnumerable<CodeInstruction> NPCLoadAllTpl(IEnumerable<CodeInstruction> instructions)
			{
				MethodInfo target = AccessTools.Property(typeof(Game), "HeroineList").GetGetMethod();
				MethodInfo customFilterM = AccessTools.Method(typeof(GrowthPlugin.Hooks), "GetFilteredHeroines", null, null);
				int count = 0;
				foreach (CodeInstruction instruction in instructions)
				{
					yield return instruction;
					bool flag = instruction.operand as MethodInfo == target;
					if (flag)
					{
						yield return new CodeInstruction(OpCodes.Call, customFilterM);
						int num = count;
						count = num + 1;
					}
				}
				GrowthPlugin.Logger.LogDebug("NPCLoadAllTpl calls injected count: " + count.ToString());
				yield break;
			}

			// Token: 0x06000081 RID: 129 RVA: 0x00004A80 File Offset: 0x00002C80
			private static bool CanGetSpawned(SaveData.Heroine heroine)
			{
				return true;
			}

			// Token: 0x06000082 RID: 130 RVA: 0x00004A94 File Offset: 0x00002C94
			private static List<SaveData.Heroine> GetFilteredHeroines(List<SaveData.Heroine> originalList)
			{
				return originalList.Where(new Func<SaveData.Heroine, bool>(GrowthPlugin.Hooks.CanGetSpawned)).ToList<SaveData.Heroine>();
			}

			// Token: 0x06000083 RID: 131 RVA: 0x00004AC0 File Offset: 0x00002CC0
			[HarmonyPrefix]
			[HarmonyPatch(typeof(HFlag), "AddSonyuInside")]
			[HarmonyPatch(typeof(HFlag), "AddSonyuAnalInside")]
			[HarmonyPatch(typeof(HFlag), "AddHoushiDrink")]
			public static void OnFinishInside(HFlag __instance)
			{
				GrowthTarget target = GrowthPlugin.GrowthTargetMode.Value;
				if (target == GrowthTarget.Female || target == GrowthTarget.Both)
				{
					SaveData.Heroine heroine = HSceneUtils.GetLeadingHeroine(__instance);
					PregnancyCharaController controller = GrowthPlugin.GetEffectController(heroine);
					if (controller != null) controller.AddInflation(1);
				}
				if (target == GrowthTarget.Player || target == GrowthTarget.Both)
				{
					PregnancyCharaController playerCtrl = GrowthPlugin.GetPlayerController();
					if (playerCtrl != null) playerCtrl.AddInflation(1);
				}
			}

			// Token: 0x06000084 RID: 132 RVA: 0x00004AE4 File Offset: 0x00002CE4
			[HarmonyPrefix]
			[HarmonyPatch(typeof(HFlag), "AddSonyuTare")]
			[HarmonyPatch(typeof(HFlag), "AddSonyuAnalTare")]
			public static void OnDrain(HFlag __instance)
			{
				bool value = GrowthPlugin.HShrink.Value;
				if (value)
				{
					int drainAmount = Mathf.Max(3, Mathf.CeilToInt((float)GrowthPlugin.NutTotal.Value / 2.2f));
					GrowthTarget target = GrowthPlugin.GrowthTargetMode.Value;
					if (target == GrowthTarget.Female || target == GrowthTarget.Both)
					{
						SaveData.Heroine heroine = HSceneUtils.GetLeadingHeroine(__instance);
						PregnancyCharaController controller = GrowthPlugin.GetEffectController(heroine);
						if (controller != null) controller.DrainInflation(drainAmount);
					}
					if (target == GrowthTarget.Player || target == GrowthTarget.Both)
					{
						PregnancyCharaController playerCtrl = GrowthPlugin.GetPlayerController();
						if (playerCtrl != null) playerCtrl.DrainInflation(drainAmount);
					}
				}
			}

			// Token: 0x04000059 RID: 89
			private static SaveData.Heroine _lastHeroine;

			// Token: 0x0400005A RID: 90
			private static byte[] _menstruationsBackup;
		}
	}
}
