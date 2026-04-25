using System;
using System.Reflection;
using ExtensibleSaveFormat;

namespace KK_Growth
{
	// Token: 0x02000006 RID: 6
	public sealed class PregnancyData
	{
		// Token: 0x06000017 RID: 23 RVA: 0x000027A4 File Offset: 0x000009A4
		public static PregnancyData Load(PluginData data)
		{
			bool flag = ((data != null) ? data.data : null) == null;
			PregnancyData pregnancyData;
			if (flag)
			{
				pregnancyData = null;
			}
			else
			{
				PregnancyData result = new PregnancyData();
				foreach (FieldInfo fieldInfo in PregnancyData._serializedFields)
				{
					object val;
					bool flag2 = data.data.TryGetValue(fieldInfo.Name, out val);
					if (flag2)
					{
						try
						{
							bool isEnum = fieldInfo.FieldType.IsEnum;
							if (isEnum)
							{
								val = (int)val;
							}
							fieldInfo.SetValue(result, val);
						}
						catch (Exception ex)
						{
							Console.WriteLine(ex);
						}
					}
				}
				bool isPregnant = result.IsPregnant;
				if (isPregnant)
				{
					result.WeeksSinceLastPregnancy = 0;
					bool flag3 = result.PregnancyCount == 0;
					if (flag3)
					{
						result.PregnancyCount = 1;
					}
				}
				pregnancyData = result;
			}
			return pregnancyData;
		}

		// Token: 0x06000018 RID: 24 RVA: 0x0000288C File Offset: 0x00000A8C
		public PluginData Save()
		{
			PluginData result = new PluginData
			{
				version = 1
			};
			foreach (FieldInfo fieldInfo in PregnancyData._serializedFields)
			{
				object value = fieldInfo.GetValue(this);
				object defaultValue = fieldInfo.GetValue(PregnancyData._default);
				bool flag = !object.Equals(defaultValue, value);
				if (flag)
				{
					result.data.Add(fieldInfo.Name, value);
				}
			}
			return (result.data.Count > 0) ? result : null;
		}

		// Token: 0x17000002 RID: 2
		// (get) Token: 0x06000019 RID: 25 RVA: 0x00002916 File Offset: 0x00000B16
		public bool IsPregnant
		{
			get
			{
				return this.Week > 0;
			}
		}

		// Token: 0x0600001A RID: 26 RVA: 0x00002921 File Offset: 0x00000B21
		public void StartPregnancy()
		{
			this.Week++;
			this.CanTellAboutPregnancy = true;
		}

		// Token: 0x0600001B RID: 27 RVA: 0x0000293C File Offset: 0x00000B3C
		public void StopPregnancy()
		{
			bool flag = this.GameplayEnabled && this.IsPregnant;
			if (flag)
			{
				bool flag2 = this.Week <= 1;
				if (flag2)
				{
					this.PregnancyCount--;
				}
				this.Week = 0;
				this.CanAskForAfterpill = false;
				this.CanTellAboutPregnancy = false;
			}
		}

		// Token: 0x04000010 RID: 16
		public static readonly float DefaultFertility = 0.3f;

		// Token: 0x04000011 RID: 17
		public static readonly int LeaveSchoolWeek = GrowthPlugin.NutTotal.Value;

		// Token: 0x04000012 RID: 18
		public static readonly int ReturnToSchoolWeek = PregnancyData.LeaveSchoolWeek + 7;

		// Token: 0x04000013 RID: 19
		public float Fertility = 0.3f;

		// Token: 0x04000014 RID: 20
		public bool GameplayEnabled = true;

		// Token: 0x04000015 RID: 21
		public MenstruationSchedule MenstruationSchedule = MenstruationSchedule.Default;

		// Token: 0x04000016 RID: 22
		public int Week;

		// Token: 0x04000017 RID: 23
		public int PregnancyCount;

		// Token: 0x04000018 RID: 24
		public int WeeksSinceLastPregnancy;

		// Token: 0x04000019 RID: 25
		public bool AlwaysLactates;

		// Token: 0x0400001A RID: 26
		private static readonly PregnancyData _default = new PregnancyData();

		// Token: 0x0400001B RID: 27
		private static readonly FieldInfo[] _serializedFields = typeof(PregnancyData).GetFields(BindingFlags.Instance | BindingFlags.Public);

		// Token: 0x0400001C RID: 28
		public bool CanAskForAfterpill;

		// Token: 0x0400001D RID: 29
		public bool CanTellAboutPregnancy;
	}
}
