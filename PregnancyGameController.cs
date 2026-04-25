using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ActionGame;
using ExtensibleSaveFormat;
using KKAPI.MainGame;
using Manager;
using UnityEngine;

namespace KK_Growth
{
	// Token: 0x02000003 RID: 3
	public class PregnancyGameController : GameCustomFunctionController
	{
		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000005 RID: 5 RVA: 0x00002394 File Offset: 0x00000594
		// (set) Token: 0x06000006 RID: 6 RVA: 0x0000239B File Offset: 0x0000059B
		internal static bool InsideHScene { get; private set; }

		// Token: 0x06000007 RID: 7 RVA: 0x000023A3 File Offset: 0x000005A3
		public static void StartPregnancy(SaveData.CharaData heroine)
		{
			PregnancyGameController._startedPregnancies.Add(heroine);
		}

		// Token: 0x06000008 RID: 8 RVA: 0x000023B2 File Offset: 0x000005B2
		public static void StopPregnancy(SaveData.CharaData heroine)
		{
			PregnancyGameController._stoppedPregnancies.Add(heroine);
		}

		// Token: 0x06000009 RID: 9 RVA: 0x000023C4 File Offset: 0x000005C4
		protected override void OnDayChange(Cycle.Week day)
		{
			bool flag = (int)day == 6;
			if (flag)
			{
				bool value = GrowthPlugin.ResidualGrowth.Value;
				if (value)
				{
					for (int i = GrowthPlugin.ResidualGrowthRate.Value; i > 0; i--)
					{
						PregnancyGameController.ApplyToAllDatas((SaveData.CharaData chara, PregnancyData data) => PregnancyGameController.AddPregnancyWeek(data));
						PregnancyGameController.ProcessPendingChanges();
					}
				}
			}
		}

		// Token: 0x0600000A RID: 10 RVA: 0x00002432 File Offset: 0x00000632
		protected override void OnStartH(BaseLoader proc, HFlag hFlag, bool vr)
		{
			PregnancyGameController.InsideHScene = true;
		}

		// Token: 0x0600000B RID: 11 RVA: 0x0000243C File Offset: 0x0000063C
		protected override void OnEndH(BaseLoader proc, HFlag hFlag, bool vr)
		{
			PregnancyGameController.InsideHScene = false;
			bool flag = (int)hFlag.mode == 6 || (int)hFlag.mode == 7;
			if (!flag)
			{
				int cumCount = hFlag.count.sonyuInside + hFlag.count.houshiDrink + hFlag.count.sonyuAnalInside;
				GrowthTarget target = GrowthPlugin.GrowthTargetMode.Value;
				if (target == GrowthTarget.Female || target == GrowthTarget.Both)
				{
					SaveData.Heroine heroine = hFlag.lstHeroine.First((SaveData.Heroine x) => x != null);
					for (int i = cumCount; i > 0; i--)
					{
						PregnancyGameController.StartPregnancy(heroine);
						PregnancyGameController.ProcessPendingChanges();
					}
				}
				if (target == GrowthTarget.Player || target == GrowthTarget.Both)
				{
					SaveData.Player player = Singleton<Game>.Instance.Player;
					if (player != null)
					{
						for (int i = cumCount; i > 0; i--)
						{
							PregnancyGameController.StartPregnancy(player);
							PregnancyGameController.ProcessPendingChanges();
						}
					}
				}
			}
		}

		// Token: 0x0600000C RID: 12 RVA: 0x000024E6 File Offset: 0x000006E6
		protected override void OnGameLoad(GameSaveLoadEventArgs args)
		{
			PregnancyGameController._startedPregnancies.Clear();
		}

		// Token: 0x0600000D RID: 13 RVA: 0x000024F4 File Offset: 0x000006F4
		protected override void OnGameSave(GameSaveLoadEventArgs args)
		{
			PregnancyGameController.ProcessPendingChanges();
		}

		// Token: 0x0600000E RID: 14 RVA: 0x000024FD File Offset: 0x000006FD
		protected override void OnPeriodChange(Cycle.Type period)
		{
			PregnancyGameController.ProcessPendingChanges();
		}

		// Token: 0x0600000F RID: 15 RVA: 0x00002506 File Offset: 0x00000706
		private static void ProcessPendingChanges()
		{
			PregnancyGameController.ApplyToAllDatas(delegate(SaveData.CharaData chara, PregnancyData data)
			{
				bool flag = PregnancyGameController._stoppedPregnancies.Contains(chara) && data.IsPregnant;
				bool flag2;
				if (flag)
				{
					data.StopPregnancy();
					flag2 = true;
				}
				else
				{
					bool flag3 = PregnancyGameController._startedPregnancies.Contains(chara);
					if (flag3)
					{
						data.StartPregnancy();
						flag2 = true;
					}
					else
					{
						flag2 = false;
					}
				}
				return flag2;
			});
			PregnancyGameController._startedPregnancies.Clear();
			PregnancyGameController._stoppedPregnancies.Clear();
		}

		// Token: 0x06000010 RID: 16 RVA: 0x00002544 File Offset: 0x00000744
		private static void ApplyToAllDatas(Func<SaveData.CharaData, PregnancyData, bool> action)
		{
			foreach (SaveData.Heroine heroine in Singleton<Game>.Instance.HeroineList)
			{
				ApplyToDatas(heroine, action);
			}
			ApplyToDatas(Singleton<Game>.Instance.Player, action);
			foreach (PregnancyCharaController controller in UnityEngine.Object.FindObjectsOfType<PregnancyCharaController>())
			{
				controller.ReadData();
			}
		}

		private static void ApplyToDatas(SaveData.CharaData character, Func<SaveData.CharaData, PregnancyData, bool> action)
		{
			IEnumerable<ChaFileControl> chafiles = character.GetRelatedChaFiles();
			if (chafiles == null) return;
			foreach (ChaFileControl chaFile in chafiles)
			{
				PluginData data = ExtendedSave.GetExtendedDataById(chaFile, "KK_Growth");
				PregnancyData pd = PregnancyData.Load(data) ?? new PregnancyData();
				if (action(character, pd))
				{
					ExtendedSave.SetExtendedDataById(chaFile, "KK_Growth", pd.Save());
				}
			}
		}

		// Token: 0x06000011 RID: 17 RVA: 0x000025E4 File Offset: 0x000007E4
		private static bool AddPregnancyWeek(PregnancyData pd)
		{
			bool flag = pd == null || !pd.GameplayEnabled;
			bool flag2;
			if (flag)
			{
				flag2 = false;
			}
			else
			{
				bool flag3 = pd.Week < GrowthPlugin.NutTotal.Value && pd.Week > 0;
				if (flag3)
				{
					pd.Week++;
				}
				else
				{
					bool flag4 = pd.Week >= GrowthPlugin.NutTotal.Value;
					if (flag4)
					{
						return true;
					}
				}
				flag2 = true;
			}
			return flag2;
		}

		// Token: 0x06000012 RID: 18 RVA: 0x00002660 File Offset: 0x00000860
		private static bool AddPregnancyWeekAll(PregnancyData pd)
		{
			bool flag = pd == null || !pd.GameplayEnabled;
			bool flag2;
			if (flag)
			{
				flag2 = false;
			}
			else
			{
				bool flag3 = pd.Week < GrowthPlugin.NutTotal.Value;
				if (flag3)
				{
					pd.Week++;
				}
				else
				{
					bool flag4 = pd.Week >= GrowthPlugin.NutTotal.Value;
					if (flag4)
					{
						return true;
					}
				}
				flag2 = true;
			}
			return flag2;
		}

		// Token: 0x06000013 RID: 19 RVA: 0x000026D0 File Offset: 0x000008D0
		private static bool DoNothing(PregnancyData pd)
		{
			return true;
		}

		// Token: 0x04000002 RID: 2
		private static readonly HashSet<SaveData.CharaData> _startedPregnancies = new HashSet<SaveData.CharaData>();

		// Token: 0x04000003 RID: 3
		private static readonly HashSet<SaveData.CharaData> _stoppedPregnancies = new HashSet<SaveData.CharaData>();
	}
}
