using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ActionGame;
using HarmonyLib;
using Illusion.Extensions;
using KKAPI.Chara;
using KKAPI.Maker;
using KKAPI.Maker.UI;
using KKAPI.Studio;
using KKAPI.Studio.UI;
using KKAPI.Utilities;
using Studio;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace KK_Growth
{
	// Token: 0x02000002 RID: 2
	public static class GrowthGui
	{
		// Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
		internal static void Init(Harmony hi, GrowthPlugin instance)
		{
			GrowthGui._pluginInstance = instance;
			bool insideStudio = StudioAPI.InsideStudio;
			if (insideStudio)
			{
				GrowthGui.RegisterStudioControls();
			}
			else
			{
				MakerAPI.RegisterCustomSubCategories += GrowthGui.MakerAPI_MakerBaseLoaded;
				Sprite pregSprite = GrowthGui.LoadIcon("pregnant.png");
				Sprite riskySprite = GrowthGui.LoadIcon("risky.png");
				Sprite safeSprite = GrowthGui.LoadIcon("safe.png");
				Sprite unknownSprite = GrowthGui.LoadIcon("unknown.png");
				Sprite leaveSprite = GrowthGui.LoadIcon("leave.png");
				GrowthGui.StatusIcons.Init(hi, unknownSprite, pregSprite, safeSprite, riskySprite, leaveSprite);
				GrowthGui.HSceneMenstrIconOverride.Init(hi, unknownSprite, pregSprite, safeSprite, riskySprite, leaveSprite);
			}
		}

		// Token: 0x06000002 RID: 2 RVA: 0x000020E4 File Offset: 0x000002E4
		private static void RegisterStudioControls()
		{
			CurrentStateCategory cat = StudioAPI.GetOrCreateCurrentStateCategory(null);
			UniRx.ObservableExtensions.Subscribe<float>(cat.AddControl<CurrentStateCategorySlider>(new CurrentStateCategorySlider("Growth", delegate(OCIChar c)
			{
				bool flag = c.charInfo == null;
				float num;
				if (flag)
				{
					num = 0f;
				}
				else
				{
					PregnancyCharaController controller = c.charInfo.GetComponent<PregnancyCharaController>();
					bool flag2 = controller == null;
					if (flag2)
					{
						num = 0f;
					}
					else
					{
						num = (float)controller.Data.Week;
					}
				}
				return num;
			}, 0f, 40f)).Value, delegate(float f)
			{
				foreach (PregnancyCharaController ctrl in StudioAPI.GetSelectedControllers<PregnancyCharaController>())
				{
					ctrl.Data.Week = Mathf.RoundToInt(f);
				}
			});
		}

		// Token: 0x06000003 RID: 3 RVA: 0x0000215C File Offset: 0x0000035C
		private static void MakerAPI_MakerBaseLoaded(object sender, RegisterSubCategoriesEvent e)
		{
			bool female = MakerAPI.GetMakerSex() != 0;
			MakerCategory cat = new MakerCategory(MakerConstants.Parameter.Character.CategoryName, "Growth", int.MaxValue, null);
			e.AddSubCategory(cat);
			Color hintColor = new Color(0.7f, 0.7f, 0.7f);
			MakerToggle gameplayToggle = e.AddControl<MakerToggle>(new MakerToggle(cat, "Enable growth progression", true, GrowthGui._pluginInstance));
			CharacterExtensions.BindToFunctionController<PregnancyCharaController, bool>(gameplayToggle, (PregnancyCharaController controller) => controller.Data.GameplayEnabled, delegate(PregnancyCharaController controller, bool value)
			{
				controller.Data.GameplayEnabled = value;
			});
			e.AddControl<MakerText>(new MakerText(female ? "If off, the character will not grow." : "If on, the character will grow with each cum absorbed.", cat, GrowthGui._pluginInstance)
			{
				TextColor = hintColor
			});
			MakerSlider weeksSlider = e.AddControl<MakerSlider>(new MakerSlider(cat, "Orgasms Absorbed", 0f, (float)PregnancyData.LeaveSchoolWeek - 1f, 0f, GrowthGui._pluginInstance));
			weeksSlider.ValueToString = (float f) => Mathf.RoundToInt(f).ToString();
			weeksSlider.StringToValue = (string s) => (float)int.Parse(s);
			CharacterExtensions.BindToFunctionController<PregnancyCharaController, float>(weeksSlider, (PregnancyCharaController controller) => (float)controller.Data.Week, delegate(PregnancyCharaController controller, float value)
			{
				controller.Data.Week = Mathf.RoundToInt(value);
			});
			e.AddControl<MakerText>(new MakerText(female ? "The total number of male orgasms absorbed, increasing this value increasing scaling." : "The only way for male characters to get pregnant is to manually set this slider above 0.", cat, GrowthGui._pluginInstance)
			{
				TextColor = hintColor
			});
		}

		private static Sprite LoadIcon(string resourceFileName)
		{
			Texture2D iconTex = new Texture2D(2, 2, (TextureFormat)12, false);
			UnityEngine.Object.DontDestroyOnLoad(iconTex);
			iconTex.LoadImage(ResourceUtils.GetEmbeddedResource(resourceFileName, null));
			Sprite sprite = Sprite.Create(iconTex, new Rect(0f, 0f, (float)iconTex.width, (float)iconTex.height), new Vector2(0.5f, 0.5f), 100f, 0U, 0);
			UnityEngine.Object.DontDestroyOnLoad(sprite);
			return sprite;
		}

		// Token: 0x04000001 RID: 1
		private static GrowthPlugin _pluginInstance;

		// Token: 0x0200000D RID: 13
		private static class HSceneMenstrIconOverride
		{
			// Token: 0x0600005D RID: 93 RVA: 0x00003AAC File Offset: 0x00001CAC
			public static void Init(Harmony hi, Sprite unknownSprite, Sprite pregSprite, Sprite safeSprite, Sprite riskySprite, Sprite leaveSprite)
			{
				bool value = GrowthPlugin.HSceneMenstrIconOverride.Value;
				if (value)
				{
					if (!unknownSprite)
					{
						throw new ArgumentNullException("unknownSprite");
					}
					GrowthGui.HSceneMenstrIconOverride._unknownSprite = unknownSprite;
					if (!pregSprite)
					{
						throw new ArgumentNullException("pregSprite");
					}
					GrowthGui.HSceneMenstrIconOverride._pregSprite = pregSprite;
					if (!riskySprite)
					{
						throw new ArgumentNullException("riskySprite");
					}
					GrowthGui.HSceneMenstrIconOverride._riskySprite = riskySprite;
					if (!safeSprite)
					{
						throw new ArgumentNullException("safeSprite");
					}
					GrowthGui.HSceneMenstrIconOverride._safeSprite = safeSprite;
					if (!leaveSprite)
					{
						throw new ArgumentNullException("leaveSprite");
					}
					GrowthGui.HSceneMenstrIconOverride._leaveSprite = leaveSprite;
					hi.PatchAll(typeof(GrowthGui.HSceneMenstrIconOverride));
				}
			}

			// Token: 0x0600005E RID: 94 RVA: 0x00003B60 File Offset: 0x00001D60
			[HarmonyPostfix]
			[HarmonyPatch(typeof(HSprite), "Init")]
			[HarmonyPatch(typeof(HSprite), "InitHeroine")]
			private static void HideMenstrIconIfNeeded(HSprite __instance)
			{
				try
				{
					if (__instance.categoryMenstruation.lstObj.Count == 2)
					{
						GameObject original = __instance.categoryMenstruation.lstObj[0];
						AddNewState(_unknownSprite, __instance, original);
						AddNewState(_pregSprite, __instance, original);
						AddNewState(_safeSprite, __instance, original);
						AddNewState(_riskySprite, __instance, original);
						AddNewState(_leaveSprite, __instance, original);
					}
					switch (HSceneUtils.GetLeadingHeroine(__instance).GetCharaStatus(null))
					{
					case HeroineStatus.Unknown:
						__instance.categoryMenstruation.SetActiveToggle(2);
						break;
					case HeroineStatus.Safe:
						__instance.categoryMenstruation.SetActiveToggle(4);
						break;
					case HeroineStatus.Risky:
						__instance.categoryMenstruation.SetActiveToggle(5);
						break;
					case HeroineStatus.Pregnant:
						__instance.categoryMenstruation.SetActiveToggle(3);
						break;
					case HeroineStatus.OnLeave:
						__instance.categoryMenstruation.SetActiveToggle(6);
						break;
					}
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
				}
			}

			private static void AddNewState(Sprite sprite, HSprite hsprite, GameObject original)
			{
				GameObject copy = UnityEngine.Object.Instantiate<GameObject>(original, original.transform.parent, false);
				copy.GetComponent<Image>().sprite = sprite;
				hsprite.categoryMenstruation.lstObj.Add(copy);
			}

			// Token: 0x0400003E RID: 62
			private static Sprite _unknownSprite;

			// Token: 0x0400003F RID: 63
			private static Sprite _pregSprite;

			// Token: 0x04000040 RID: 64
			private static Sprite _safeSprite;

			// Token: 0x04000041 RID: 65
			private static Sprite _riskySprite;

			// Token: 0x04000042 RID: 66
			private static Sprite _leaveSprite;
		}

		// Token: 0x0200000E RID: 14
		private class StatusIcons : MonoBehaviour
		{
			// Token: 0x06000060 RID: 96 RVA: 0x00003D18 File Offset: 0x00001F18
			internal static void Init(Harmony hi, Sprite unknownSprite, Sprite pregSprite, Sprite safeSprite, Sprite riskySprite, Sprite leaveSprite)
			{
				if (!unknownSprite)
				{
					throw new ArgumentNullException("unknownSprite");
				}
				GrowthGui.StatusIcons._unknownSprite = unknownSprite;
				if (!pregSprite)
				{
					throw new ArgumentNullException("pregSprite");
				}
				GrowthGui.StatusIcons._pregSprite = pregSprite;
				if (!riskySprite)
				{
					throw new ArgumentNullException("riskySprite");
				}
				GrowthGui.StatusIcons._riskySprite = riskySprite;
				if (!safeSprite)
				{
					throw new ArgumentNullException("safeSprite");
				}
				GrowthGui.StatusIcons._safeSprite = safeSprite;
				if (!leaveSprite)
				{
					throw new ArgumentNullException("leaveSprite");
				}
				GrowthGui.StatusIcons._leaveSprite = leaveSprite;
				SceneManager.sceneLoaded += new UnityAction<Scene, LoadSceneMode>(GrowthGui.StatusIcons.SceneManager_sceneLoaded);
				SceneManager.sceneUnloaded += delegate(Scene s)
				{
					bool flag = GrowthGui.StatusIcons._currentHeroine.Count > 0;
					if (flag)
					{
						GrowthGui.StatusIcons.SceneManager_sceneLoaded(s, (LoadSceneMode)0);
					}
				};
				hi.PatchAll(typeof(GrowthGui.StatusIcons));
			}

			// Token: 0x06000061 RID: 97 RVA: 0x00003DF0 File Offset: 0x00001FF0
			[HarmonyPostfix]
			[HarmonyPatch(typeof(ClassRoomList), "PreviewUpdate")]
			public static void ClassroomPreviewUpdateHook(ClassRoomList __instance)
			{
				GrowthGui._pluginInstance.StartCoroutine(ClassroomPreviewUpdateCo(__instance));
			}

			// Token: 0x06000062 RID: 98 RVA: 0x00003E20 File Offset: 0x00002020
			private static void SceneManager_sceneLoaded(Scene scene, LoadSceneMode mode)
			{
				if (GrowthGui._pluginInstance == null) return;
				_currentHeroine.Clear();
				ChaStatusScene chaStatusScene = UnityEngine.Object.FindObjectOfType<ChaStatusScene>();
				if (chaStatusScene != null)
				{
					SpawnGUI();
					GrowthGui._pluginInstance.StartCoroutine(CreatePregnancyIconCo(chaStatusScene));
				}
			}

			// Token: 0x06000063 RID: 99 RVA: 0x00003E8C File Offset: 0x0000208C
			[HarmonyPostfix]
			[HarmonyPatch(typeof(ParamUI), "SetHeroine", new Type[] { typeof(SaveData.Heroine) })]
			private static void ParamUI_SetHeroine(ParamUI __instance, SaveData.Heroine _heroine)
			{
				System.Reflection.FieldInfo _fi = typeof(ParamUI).GetField("objFemaleRoot");
				GameObject objFemaleRoot = _fi != null ? _fi.GetValue(__instance) as GameObject : null;
				if (objFemaleRoot == null) return;
				SpawnGUI();
				GrowthGui._pluginInstance.StartCoroutine(HeroineCanvasPreviewUpdateCo(_heroine, objFemaleRoot));
			}

			private static System.Collections.IEnumerator ClassroomPreviewUpdateCo(ClassRoomList instance)
			{
				yield return null;
				_currentHeroine.Clear();
			}

			private static System.Collections.IEnumerator CreatePregnancyIconCo(ChaStatusScene chaStatusScene)
			{
				yield return null;
			}

			private static System.Collections.IEnumerator HeroineCanvasPreviewUpdateCo(SaveData.Heroine heroine, GameObject objFemaleRoot)
			{
				yield return null;
				SetHeart(objFemaleRoot, heroine, false);
			}

			// Token: 0x06000064 RID: 100 RVA: 0x00003EE0 File Offset: 0x000020E0
			private static void SpawnGUI()
			{
				bool flag = !GameObject.Find("PregnancyGUI");
				if (flag)
				{
					new GameObject("PregnancyGUI").AddComponent<GrowthGui.StatusIcons>();
				}
			}

			// Token: 0x06000065 RID: 101 RVA: 0x00003F14 File Offset: 0x00002114
			private void OnGUI()
			{
				bool flag = GrowthGui.StatusIcons._currentHeroine.Count == 0;
				if (!flag)
				{
					Vector2 pos = new Vector2(Input.mousePosition.x, -(Input.mousePosition.y - (float)Screen.height));
					SaveData.CharaData chara = GrowthGui.StatusIcons._currentHeroine.FirstOrDefault(delegate(KeyValuePair<SaveData.CharaData, RectTransform> x)
					{
						bool flag6 = x.Value == null;
						return !flag6 && GrowthGui.StatusIcons.GetOccupiedScreenRect(x).Contains(pos);
					}).Key;
					bool flag2 = chara == null;
					if (!flag2)
					{
						PregnancyData pregData = chara.GetPregnancyData();
						HeroineStatus status = chara.GetCharaStatus(pregData);
						SaveData.Heroine heroine = chara as SaveData.Heroine;
						int windowHeight = ((status == HeroineStatus.Unknown) ? 100 : ((status == HeroineStatus.Pregnant || status == HeroineStatus.OnLeave) ? 180 : 370));
						Rect screenRect = new Rect((float)((int)pos.x + 30), (float)((int)pos.y - windowHeight / 2), 180f, (float)windowHeight);
						IMGUIUtils.DrawSolidBox(screenRect);
						GUILayout.BeginArea(screenRect, GUI.skin.box);
						GUILayout.BeginVertical(new GUILayoutOption[0]);
						GUILayout.FlexibleSpace();
						switch (status)
						{
						case HeroineStatus.Unknown:
							GUILayout.Label("This character didn't tell you their risky day schedule yet.", new GUILayoutOption[0]);
							GUILayout.FlexibleSpace();
							GUILayout.Label("Become closer to learn it!", new GUILayoutOption[0]);
							break;
						case HeroineStatus.Safe:
						case HeroineStatus.Risky:
						{
							bool flag3 = heroine == null;
							if (!flag3)
							{
								GUILayout.Label((status == HeroineStatus.Safe) ? "This character is on a safe day, have fun!" : "This character is on a risky day, be careful!", new GUILayoutOption[0]);
								GUILayout.FlexibleSpace();
								Cycle.Week day = Singleton<Cycle>.Instance.nowWeek;
								GUILayout.Label("Forecast for this week:", new GUILayoutOption[0]);
								MenstruationSchedule menstruationSchedule = pregData.MenstruationSchedule;
								MenstruationSchedule menstruationSchedule2 = menstruationSchedule;
								if (menstruationSchedule2 != MenstruationSchedule.AlwaysSafe)
								{
									if (menstruationSchedule2 != MenstruationSchedule.AlwaysRisky)
									{
										GUILayout.Label(string.Format("Today ({0}): {1}", day, status), new GUILayoutOption[0]);
										for (int dayOffset = 1; dayOffset < 7; dayOffset++)
										{
											Cycle.Week adjustedDay = (Cycle.Week)(((int)day + dayOffset) % Enum.GetValues(typeof(Cycle.Week)).Length);
											bool adjustedSafe = HFlag.GetMenstruation((byte)(((int)heroine.MenstruationDay + dayOffset) % HFlag.menstruations.Length)) == 0;
											GUILayout.Label(string.Format("{0}: {1}", adjustedDay, adjustedSafe ? "Safe" : "Risky"), new GUILayoutOption[0]);
										}
									}
									else
									{
										GUILayout.Label("It's always risky!", new GUILayoutOption[0]);
									}
								}
								else
								{
									GUILayout.Label("It's always safe!", new GUILayoutOption[0]);
								}
								int pregnancyCount = (pregData.IsPregnant ? (pregData.PregnancyCount - 1) : pregData.PregnancyCount);
								bool flag4 = pregnancyCount > 0;
								if (flag4)
								{
									GUILayout.FlexibleSpace();
									GUILayout.Label(string.Format("This character was pregnant {0} times.", pregnancyCount), new GUILayoutOption[0]);
								}
								bool flag5 = pregData.WeeksSinceLastPregnancy > 0;
								if (flag5)
								{
									GUILayout.FlexibleSpace();
									GUILayout.Label(string.Format("Last pregnancy was {0} weeks ago.", pregData.WeeksSinceLastPregnancy), new GUILayoutOption[0]);
								}
							}
							break;
						}
						case HeroineStatus.Pregnant:
						{
							GUILayout.Label(string.Format("This character is pregnant (on week {0} / 40).", pregData.Week), new GUILayoutOption[0]);
							GUILayout.FlexibleSpace();
							bool gameplayEnabled = pregData.GameplayEnabled;
							if (gameplayEnabled)
							{
								GUILayout.Label((heroine != null) ? "The character's body will slowly change, and at the end they will temporarily leave." : "The character's body will slowly change.", new GUILayoutOption[0]);
							}
							GUILayout.FlexibleSpace();
							int previousPregcount = Mathf.Max(0, pregData.PregnancyCount - 1);
							GUILayout.Label(string.Format("This character was pregnant {0} times before.", previousPregcount), new GUILayoutOption[0]);
							break;
						}
						case HeroineStatus.OnLeave:
							GUILayout.Label("This character is on a maternal leave and will not appear until it is over.", new GUILayoutOption[0]);
							GUILayout.FlexibleSpace();
							GUILayout.Label("Consider using a rubber next time!", new GUILayoutOption[0]);
							break;
						default:
							throw new ArgumentOutOfRangeException();
						}
						GUILayout.FlexibleSpace();
						GUILayout.EndVertical();
						GUILayout.EndArea();
					}
				}
			}

			// Token: 0x06000066 RID: 102 RVA: 0x00004310 File Offset: 0x00002510
			private static void SetHeart(GameObject rootObj, SaveData.CharaData chara, bool classRoster)
			{
				Transform pregIconTr = rootObj.transform.Find("Pregnancy_Icon");
				bool flag = chara == null;
				if (flag)
				{
					bool flag2 = pregIconTr != null;
					if (flag2)
					{
						UnityEngine.Object.Destroy(pregIconTr.gameObject);
					}
				}
				else
				{
					bool flag3 = pregIconTr == null;
					if (flag3)
					{
						pregIconTr = new GameObject("Pregnancy_Icon", new Type[]
						{
							typeof(RectTransform),
							typeof(CanvasRenderer),
							typeof(Image)
						}).transform;
						pregIconTr.SetParent(rootObj.transform, false);
						RectTransform rt = pregIconTr.GetComponent<RectTransform>();
						if (classRoster)
						{
							RectTransform rectTransform = rt;
							RectTransform rectTransform2 = rt;
							RectTransform rectTransform3 = rt;
							Vector2 vector = new Vector2(0f, 1f);
							rectTransform3.pivot = vector;
							rectTransform.anchorMax = (rectTransform2.anchorMin = vector);
							rt.offsetMin = Vector2.zero;
							rt.offsetMax = new Vector2(48f, 48f);
							rt.localScale = Vector3.one;
							rt.localPosition = new Vector3(4f, -115f, 0f);
						}
						else
						{
							RectTransform rectTransform4 = rt;
							RectTransform rectTransform5 = rt;
							RectTransform rectTransform6 = rt;
							Vector2 vector = new Vector2(0.5f, 0.5f);
							rectTransform6.pivot = vector;
							rectTransform4.anchorMax = (rectTransform5.anchorMin = vector);
							rt.offsetMin = Vector2.zero;
							rt.offsetMax = new Vector2(48f, 48f);
							rt.localScale = Vector3.one;
							rt.localPosition = new Vector3(-273f, -85f, 0f);
						}
					}
					GrowthGui.StatusIcons.AddPregIcon(pregIconTr, chara);
				}
			}

			// Token: 0x06000067 RID: 103 RVA: 0x000044D4 File Offset: 0x000026D4
			private static void SetQuickStatusIcon(GameObject characterImageObj, SaveData.Heroine heroine, float xOffset, float yOffset)
			{
				Transform existing = characterImageObj.transform.Find("Pregnancy_Icon");
				bool flag = heroine == null;
				if (flag)
				{
					bool flag2 = existing != null;
					if (flag2)
					{
						UnityEngine.Object.Destroy(existing.gameObject);
					}
				}
				else
				{
					bool flag3 = existing == null;
					if (flag3)
					{
						GameObject newChildIcon = new GameObject();
						newChildIcon.AddComponent<RectTransform>();
						newChildIcon.AddComponent<Image>();
						GameObject copy = UnityEngine.Object.Instantiate<GameObject>(newChildIcon, characterImageObj.transform);
						copy.name = "Pregnancy_Icon";
						copy.SetActive(true);
						RectTransform charRt = characterImageObj.GetComponent<RectTransform>();
						RectTransform rt = copy.GetComponent<RectTransform>();
						rt.anchoredPosition = new Vector2(charRt.anchoredPosition.x + xOffset, charRt.anchoredPosition.y + yOffset);
						rt.sizeDelta = new Vector2(48f, 48f);
						existing = copy.transform;
					}
					GrowthGui.StatusIcons.AddPregIcon(existing, heroine);
				}
			}

			// Token: 0x06000068 RID: 104 RVA: 0x000045C8 File Offset: 0x000027C8
			private static void AddPregIcon(Transform pregIconTransform, SaveData.CharaData chara)
			{
				Image image = pregIconTransform.GetComponent<Image>();
				GrowthGui.StatusIcons._currentHeroine.Add(new KeyValuePair<SaveData.CharaData, RectTransform>(chara, image.GetComponent<RectTransform>()));
				HeroineStatus status = chara.GetCharaStatus(chara.GetPregnancyData());
				switch (status)
				{
				case HeroineStatus.Unknown:
					image.sprite = GrowthGui.StatusIcons._unknownSprite;
					break;
				case HeroineStatus.Safe:
					image.sprite = GrowthGui.StatusIcons._safeSprite;
					break;
				case HeroineStatus.Risky:
					image.sprite = GrowthGui.StatusIcons._riskySprite;
					break;
				case HeroineStatus.Pregnant:
					image.sprite = GrowthGui.StatusIcons._pregSprite;
					break;
				case HeroineStatus.OnLeave:
					image.sprite = GrowthGui.StatusIcons._leaveSprite;
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
				GameObjectExtensions.SetActiveIfDifferent(pregIconTransform.gameObject, chara is SaveData.Heroine || status == HeroineStatus.Pregnant);
			}

			// Token: 0x06000069 RID: 105 RVA: 0x00004688 File Offset: 0x00002888
			private static Rect GetOccupiedScreenRect(KeyValuePair<SaveData.CharaData, RectTransform> x)
			{
				x.Value.GetWorldCorners(GrowthGui.StatusIcons._worldCornersBuffer);
				Rect screenPos = new Rect(GrowthGui.StatusIcons._worldCornersBuffer[0].x, (float)Screen.height - GrowthGui.StatusIcons._worldCornersBuffer[2].y, GrowthGui.StatusIcons._worldCornersBuffer[2].x - GrowthGui.StatusIcons._worldCornersBuffer[0].x, GrowthGui.StatusIcons._worldCornersBuffer[2].y - GrowthGui.StatusIcons._worldCornersBuffer[0].y);
				return screenPos;
			}

			// Token: 0x04000043 RID: 67
			private static Sprite _pregSprite;

			// Token: 0x04000044 RID: 68
			private static Sprite _riskySprite;

			// Token: 0x04000045 RID: 69
			private static Sprite _safeSprite;

			// Token: 0x04000046 RID: 70
			private static Sprite _unknownSprite;

			// Token: 0x04000047 RID: 71
			private static Sprite _leaveSprite;

			// Token: 0x04000048 RID: 72
			private const string ICON_NAME = "Pregnancy_Icon";

			// Token: 0x04000049 RID: 73
			private static readonly List<KeyValuePair<SaveData.CharaData, RectTransform>> _currentHeroine = new List<KeyValuePair<SaveData.CharaData, RectTransform>>();

			// Token: 0x0400004A RID: 74
			private static readonly Vector3[] _worldCornersBuffer = new Vector3[4];
		}
	}
}
