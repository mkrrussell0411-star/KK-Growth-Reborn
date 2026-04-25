using System;
using KKABMX.Core;
using KKAPI;
using KKAPI.Chara;
using KKAPI.MainGame;
using KKAPI.Maker;
using UnityEngine;

namespace KK_Growth
{
	public class PregnancyCharaController : CharaCustomFunctionController
	{
		public PregnancyData Data { get; private set; }

		public PregnancyCharaController()
		{
			this.Data = new PregnancyData();
			this._boneEffect = new PregnancyBoneEffect(this);
		}

		[Obsolete]
		public float GetPregnancyEffectPercent()
		{
			return this._boneEffect.GetPregnancyEffectPercent();
		}

		public bool CanGetDangerousDays()
		{
			return this.Data.Week <= 1;
		}

		public void SaveData()
		{
			base.SetExtendedData(this.Data.Save());
		}

		public void ReadData()
		{
			SaveData.Heroine heroine = (((int)KoikatuAPI.GetCurrentGameMode() == 3) ? GameExtensions.GetHeroine(base.ChaControl) : null);
			bool flag = ((heroine != null) ? heroine.charFile : null) != null;
			if (flag)
			{
				this.Data = heroine.charFile.GetPregnancyData() ?? new PregnancyData();
			}
			else
			{
				this.Data = PregnancyData.Load(base.GetExtendedData()) ?? new PregnancyData();
			}
			bool flag2 = !this.CanGetDangerousDays();
			if (flag2)
			{
				bool flag3 = heroine != null;
				if (flag3)
				{
					HFlag.SetMenstruation(heroine, 0);
				}
			}
		}

		protected override void OnCardBeingSaved(GameMode currentGameMode)
		{
			this.SaveData();
		}

		protected override void OnReload(GameMode currentGameMode)
		{
			bool flag = !GameAPI.InsideHScene;
			if (flag)
			{
				this._inflationChange = 0f;
				this._inflationAmount = 0;
			}
			bool flag2;
			if (MakerAPI.InsideAndLoaded)
			{
				CharacterLoadFlags characterLoadFlags = MakerAPI.GetCharacterLoadFlags();
				flag2 = characterLoadFlags == null || characterLoadFlags.Parameters;
			}
			else
			{
				flag2 = true;
			}
			bool flag3 = flag2;
			if (flag3)
			{
				this.ReadData();
				base.GetComponent<BoneController>().AddBoneEffect(this._boneEffect);
			}
		}

		internal static byte[] GetMenstruationsArr(MenstruationSchedule menstruationSchedule)
		{
			byte[] array;
			switch (menstruationSchedule)
			{
			case MenstruationSchedule.MostlyRisky:
				array = PregnancyCharaController._menstruationsRisky;
				break;
			case MenstruationSchedule.AlwaysSafe:
				array = PregnancyCharaController._menstruationsAlwaysSafe;
				break;
			case MenstruationSchedule.AlwaysRisky:
				array = PregnancyCharaController._menstruationsAlwaysRisky;
				break;
			default:
				array = HFlag.menstruations;
				break;
			}
			return array;
		}

		public int InflationAmount
		{
			get
			{
				return this._inflationAmount;
			}
			set
			{
				this._inflationAmount = Mathf.Clamp(value, 0, GrowthPlugin.NutTotal.Value);
			}
		}

		public bool IsInflated
		{
			get
			{
				return (float)this.InflationAmount + this._inflationChange > 0.01f;
			}
		}

		public float GetInflationEffectPercent()
		{
			return Mathf.Clamp01(((float)this.InflationAmount + this._inflationChange) / (float)GrowthPlugin.NutTotal.Value);
		}

		public void AddInflation(int amount)
		{
			int orig = this.InflationAmount;
			this.InflationAmount += amount;
			int change = this.InflationAmount - orig;
			this._inflationChange -= (float)change;
		}

		public void DrainInflation(int amount)
		{
			int orig = this.InflationAmount;
			this.InflationAmount -= amount;
			int change = orig - this.InflationAmount;
			this._inflationChange += (float)change;
		}

		protected override void Update()
		{
			base.Update();
			bool insideHScene = GameAPI.InsideHScene;
			if (insideHScene)
			{
				bool value = GrowthPlugin.InflationEnable.Value;
				if (value)
				{
					bool flag = this._inflationChange > 0.05f;
					if (flag)
					{
						this._inflationChange = Mathf.Max(0f, this._inflationChange - this.GetInflationChange());
					}
					else
					{
						bool flag2 = this._inflationChange < -0.05f;
						if (flag2)
						{
							this._inflationChange = Mathf.Min(0f, this._inflationChange + this.GetInflationChange());
							bool flag3 = GrowthPlugin.InflationOpenClothAtMax.Value && this.InflationAmount >= GrowthPlugin.NutTotal.Value;
							if (flag3)
							{
								bool flag4 = base.ChaControl.fileStatus.clothesState[0] == 0;
								if (flag4)
								{
									base.ChaControl.SetClothesStateNext(0);
								}
							}
						}
					}
				}
				else
				{
					this._inflationChange = 0f;
					this._inflationAmount = 0;
				}
			}
		}

		static PregnancyCharaController()
		{
			byte[] array = new byte[15];
			array[4] = 1;
			PregnancyCharaController._menstruationsAlwaysSafe = array;
			PregnancyCharaController._menstruationsAlwaysRisky = new byte[]
			{
				0, 1, 1, 1, 1, 1, 1, 1, 1, 1,
				1, 1, 1, 1, 1
			};
		}

		private float GetInflationChange()
		{
			return Mathf.Max(0.1f * GrowthPlugin.GrowthSpeed.Value * Time.deltaTime / 1.3f, Mathf.Abs(Time.deltaTime * (this._inflationChange * GrowthPlugin.GrowthSpeed.Value) / 5f));
		}

		private readonly PregnancyBoneEffect _boneEffect;

		private static readonly byte[] _menstruationsRisky = new byte[]
		{
			0, 0, 0, 0, 1, 1, 1, 1, 1, 1,
			1, 1, 1, 0, 0
		};

		private static readonly byte[] _menstruationsAlwaysSafe;

		private static readonly byte[] _menstruationsAlwaysRisky;

		private float _inflationChange;

		private int _inflationAmount;
	}
}
