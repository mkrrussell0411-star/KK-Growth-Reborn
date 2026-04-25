using System;
using System.Collections.Generic;
using System.Linq;
using ExtensibleSaveFormat;
using KKAPI.MainGame;

namespace KK_Growth
{
	// Token: 0x02000007 RID: 7
	public static class PregnancyDataUtils
	{
		// Token: 0x0600001E RID: 30 RVA: 0x00002A0C File Offset: 0x00000C0C
		public static PregnancyData GetPregnancyData(this ChaFileControl c)
		{
			bool flag = c == null;
			PregnancyData pregnancyData;
			if (flag)
			{
				pregnancyData = null;
			}
			else
			{
				PluginData d = ExtendedSave.GetExtendedDataById(c, "KK_Growth");
				bool flag2 = d == null;
				if (flag2)
				{
					pregnancyData = null;
				}
				else
				{
					pregnancyData = PregnancyData.Load(d);
				}
			}
			return pregnancyData;
		}

		// Token: 0x0600001F RID: 31 RVA: 0x00002A48 File Offset: 0x00000C48
		[Obsolete]
		public static PregnancyData GetPregnancyData(SaveData.Heroine heroine)
		{
			return heroine.GetPregnancyData();
		}

		// Token: 0x06000020 RID: 32 RVA: 0x00002A50 File Offset: 0x00000C50
		public static PregnancyData GetPregnancyData(this SaveData.CharaData chara)
		{
			bool flag = chara == null;
			PregnancyData pregnancyData;
			if (flag)
			{
				pregnancyData = new PregnancyData();
			}
			else
			{
				pregnancyData = chara.charFile.GetPregnancyData() ?? new PregnancyData();
			}
			return pregnancyData;
		}

		// Token: 0x06000021 RID: 33 RVA: 0x00002A86 File Offset: 0x00000C86
		[Obsolete]
		public static HeroineStatus GetHeroineStatus(this SaveData.CharaData chara, PregnancyData pregData = null)
		{
			return chara.GetCharaStatus(pregData);
		}

		// Token: 0x06000022 RID: 34 RVA: 0x00002A90 File Offset: 0x00000C90
		public static HeroineStatus GetCharaStatus(this SaveData.CharaData chara, PregnancyData pregData = null)
		{
			SaveData.Heroine heroine = chara as SaveData.Heroine;
			bool flag = heroine != null;
			if (flag)
			{
				bool flag2 = pregData == null;
				if (flag2)
				{
					pregData = heroine.GetPregnancyData();
				}
				bool flag3 = heroine.intimacy >= 80 || heroine.hCount >= 5 || (heroine.parameter.attribute.bitch && heroine.favor > 50) || ((heroine.isGirlfriend || heroine.favor >= 90) && (!heroine.isVirgin || heroine.hCount >= 2 || heroine.intimacy >= 40));
				if (flag3)
				{
					int pregnancyWeek = pregData.Week;
					bool flag4 = pregnancyWeek > 0;
					if (flag4)
					{
						bool flag5 = pregnancyWeek >= PregnancyData.LeaveSchoolWeek;
						if (flag5)
						{
							return HeroineStatus.OnLeave;
						}
						bool value = GrowthPlugin.ShowPregnancyIconEarly.Value;
						if (value)
						{
							return HeroineStatus.Pregnant;
						}
						bool flag6 = PregnancyDataUtils._earlyDetectPersonalities.Contains(heroine.personality);
						if (flag6)
						{
							bool flag7 = pregnancyWeek > 1;
							if (flag7)
							{
								return HeroineStatus.Pregnant;
							}
						}
						else
						{
							bool flag8 = PregnancyDataUtils._lateDetectPersonalities.Contains(heroine.personality);
							if (flag8)
							{
								bool flag9 = pregnancyWeek > 11;
								if (flag9)
								{
									return HeroineStatus.Pregnant;
								}
							}
							else
							{
								bool flag10 = pregnancyWeek > 5;
								if (flag10)
								{
									return HeroineStatus.Pregnant;
								}
							}
						}
					}
					return (HFlag.GetMenstruation(heroine.MenstruationDay) == null) ? HeroineStatus.Safe : HeroineStatus.Risky;
				}
			}
			else
			{
				SaveData.Player player = chara as SaveData.Player;
				bool flag11 = player != null;
				if (flag11)
				{
					bool flag12 = pregData == null;
					if (flag12)
					{
						pregData = player.GetPregnancyData();
					}
					return pregData.IsPregnant ? HeroineStatus.Pregnant : HeroineStatus.Safe;
				}
			}
			return HeroineStatus.Unknown;
		}

		// Token: 0x06000023 RID: 35 RVA: 0x00002C3C File Offset: 0x00000E3C
		internal static IEnumerable<ChaFileControl> GetRelatedChaFiles(this SaveData.CharaData character)
		{
			SaveData.Heroine h = character as SaveData.Heroine;
			IEnumerable<ChaFileControl> enumerable;
			if (h == null)
			{
				SaveData.Player p = character as SaveData.Player;
				enumerable = ((p != null) ? GameExtensions.GetRelatedChaFiles(p) : null);
			}
			else
			{
				enumerable = GameExtensions.GetRelatedChaFiles(h);
			}
			return enumerable;
		}

		// Token: 0x0400001E RID: 30
		private static readonly int[] _earlyDetectPersonalities = new int[] { 0, 11, 12, 13, 19, 24, 31, 33 };

		// Token: 0x0400001F RID: 31
		private static readonly int[] _lateDetectPersonalities = new int[] { 3, 5, 8, 20, 25, 26, 37 };
	}
}
