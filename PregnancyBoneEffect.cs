using System;
using System.Collections.Generic;
using System.Linq;
using KKABMX.Core;
using KKAPI.Maker;
using KKAPI.Studio;
using UnityEngine;

namespace KK_Growth
{
	// Token: 0x02000009 RID: 9
	public class PregnancyBoneEffect : BoneEffect
	{
		// Token: 0x06000026 RID: 38 RVA: 0x00002CA3 File Offset: 0x00000EA3
		public PregnancyBoneEffect(PregnancyCharaController controller)
		{
			this._controller = controller;
		}

		// Token: 0x06000027 RID: 39 RVA: 0x00002CB4 File Offset: 0x00000EB4
		public override IEnumerable<string> GetAffectedBones(BoneController origin)
		{
			bool flag = this._controller.Data.IsPregnant || MakerAPI.InsideMaker || StudioAPI.InsideStudio || PregnancyGameController.InsideHScene;
			IEnumerable<string> enumerable;
			if (flag)
			{
				enumerable = PregnancyBoneEffect._affectedBoneNames;
			}
			else
			{
				enumerable = Enumerable.Empty<string>();
			}
			return enumerable;
		}

		// Token: 0x06000028 RID: 40 RVA: 0x00002D00 File Offset: 0x00000F00
		public override BoneModifierData GetEffect(string bone, BoneController origin, ChaFileDefine.CoordinateType coordinate)
		{
			bool isPregnant = this._controller.Data.IsPregnant;
			bool flag = isPregnant;
			if (flag)
			{
				BoneModifierData mod;
				bool flag2 = PregnancyBoneEffect._pregnancyFullValues.TryGetValue(bone, out mod);
				if (flag2)
				{
					float prEffect = this.GetPregnancyEffectPercent();
					return PregnancyBoneEffect.LerpModifier(mod, prEffect);
				}
			}
			bool flag3 = isPregnant || this._controller.IsInflated;
			if (flag3)
			{
				BoneModifierData mod2;
				bool flag4 = PregnancyBoneEffect._bellyFullValues.TryGetValue(bone, out mod2);
				if (flag4)
				{
					float prEffect2 = this.GetPregnancyEffectPercent();
					float infEffect = this._controller.GetInflationEffectPercent() + prEffect2;
					float bellySize = infEffect;
					return PregnancyBoneEffect.LerpModifier(mod2, bellySize);
				}
			}
			return null;
		}

		// Token: 0x06000029 RID: 41 RVA: 0x00002DAC File Offset: 0x00000FAC
		private static BoneModifierData LerpModifier(BoneModifierData mod, float bellySize)
		{
			return new BoneModifierData(new Vector3(Mathf.Lerp(1f, mod.ScaleModifier.x, bellySize), Mathf.Lerp(1f, mod.ScaleModifier.y, bellySize), Mathf.Lerp(1f, mod.ScaleModifier.z, bellySize)), Mathf.Lerp(1f, mod.LengthModifier, bellySize), new Vector3(Mathf.Lerp(0f, mod.PositionModifier.x, bellySize), Mathf.Lerp(0f, mod.PositionModifier.y, bellySize), Mathf.Lerp(0f, mod.PositionModifier.z, bellySize)), new Vector3(Mathf.Lerp(0f, mod.RotationModifier.x, bellySize), Mathf.Lerp(0f, mod.RotationModifier.y, bellySize), Mathf.Lerp(0f, mod.RotationModifier.z, bellySize)));
		}

		// Token: 0x0600002A RID: 42 RVA: 0x00002EAC File Offset: 0x000010AC
		public float GetPregnancyEffectPercent()
		{
			float progressionSpeed = Mathf.Ceil(0.5f);
			return Mathf.Clamp01((float)this._controller.Data.Week / ((float)GrowthPlugin.NutTotal.Value - progressionSpeed));
		}

		// Token: 0x04000020 RID: 32
		private readonly PregnancyCharaController _controller;

		// Token: 0x04000021 RID: 33
		private static Dictionary<string, BoneModifierData> _bellyFullValues = new Dictionary<string, BoneModifierData>
		{
			{
				"cf_n_height",
				new BoneModifierData(new Vector3(GrowthPlugin.HeightScale.Value, GrowthPlugin.HeightScale.Value, GrowthPlugin.HeightScale.Value), 1f, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f))
			},
			{
				"cf_d_bust01_R",
				new BoneModifierData(new Vector3(GrowthPlugin.BreastExpansion.Value, GrowthPlugin.BreastExpansion.Value, GrowthPlugin.BreastExpansion.Value), 1f, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f))
			},
			{
				"cf_d_bust01_L",
				new BoneModifierData(new Vector3(GrowthPlugin.BreastExpansion.Value, GrowthPlugin.BreastExpansion.Value, GrowthPlugin.BreastExpansion.Value), 1f, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f))
			},
			{
				"cf_s_siri_R",
				new BoneModifierData(new Vector3(GrowthPlugin.AssExpansion.Value, GrowthPlugin.AssExpansion.Value, GrowthPlugin.AssExpansion.Value), 1f, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f))
			},
			{
				"cf_s_siri_L",
				new BoneModifierData(new Vector3(GrowthPlugin.AssExpansion.Value, GrowthPlugin.AssExpansion.Value, GrowthPlugin.AssExpansion.Value), 1f, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f))
			},
			{
				"cf_j_waist01",
				new BoneModifierData(new Vector3(Mathf.Max(1f, GrowthPlugin.AssExpansion.Value / 2f), 1f, Mathf.Max(1f, GrowthPlugin.AssExpansion.Value / 2f)), 1f, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f))
			}
		};

		// Token: 0x04000022 RID: 34
		private static readonly Dictionary<string, BoneModifierData> _pregnancyFullValues = new Dictionary<string, BoneModifierData>();

		// Token: 0x04000023 RID: 35
		private static readonly IEnumerable<string> _affectedBoneNames = PregnancyBoneEffect._bellyFullValues.Keys.Concat(PregnancyBoneEffect._pregnancyFullValues.Keys).ToArray<string>();
	}
}
