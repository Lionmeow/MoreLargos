using SRML;
using SRML.SR;
using SRML.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using LargoLibrary;
using System.Diagnostics;
using UnityEngine.Assertions.Must;
using MonomiPark.SlimeRancher.DataModel;
using System.Security.Policy;
using System.Runtime.CompilerServices;
using System.IO;
using MonomiPark.SlimeRancher.Regions;

namespace MoreLargos
{
    public class Main : ModEntryPoint
    {
        private static int bottomColorNameId = Shader.PropertyToID("_BottomColor");
        private static int middleColorNameId = Shader.PropertyToID("_MiddleColor");
        private static int topColorNameId = Shader.PropertyToID("_TopColor");

        public static SlimeAppearanceDirector mainMenuDirector;
        public static SlimeAppearanceDirector saveGameDirector;
        public static SlimeDiet.EatMapEntry referenceEatMap;
        public static List<LargoObject> largoObjects = new List<LargoObject>();

        public override void PreLoad()
        {
            HarmonyInstance.PatchAll();
        }

        public override void Load()
        {
            if (!SRModLoader.IsModPresent("largolibrary"))
            {
                throw new Exception("Largo Library is not present, but is required!");
            }

            SRSingleton<GameContext>.Instance.SlimeDefinitions.GetSlimeByIdentifiableId(Identifiable.Id.PUDDLE_SLIME).Diet.Produces = new Identifiable.Id[1]
            {
                Identifiable.Id.PUDDLE_PLORT
            };
            SRSingleton<GameContext>.Instance.SlimeDefinitions.GetSlimeByIdentifiableId(Identifiable.Id.FIRE_SLIME).Diet.Produces = new Identifiable.Id[1]
            {
                Identifiable.Id.FIRE_PLORT
            };
            SRSingleton<GameContext>.Instance.SlimeDefinitions.GetSlimeByIdentifiableId(Identifiable.Id.QUICKSILVER_SLIME).Diet.Produces = new Identifiable.Id[1]
            {
                Identifiable.Id.QUICKSILVER_PLORT
            };
            SRSingleton<GameContext>.Instance.SlimeDefinitions.GetSlimeByIdentifiableId(Identifiable.Id.GLITCH_SLIME).Diet.Produces = new Identifiable.Id[1]
            {
                Identifiable.Id.NONE
            };

            foreach (SlimeDiet.EatMapEntry entry in SRSingleton<GameContext>.Instance.SlimeDefinitions.GetSlimeByIdentifiableId(Identifiable.Id.PINK_SLIME).Diet.EatMap)
            {
                if (entry.becomesId == Identifiable.Id.PINK_TABBY_LARGO)
                {
                    referenceEatMap = entry;
                    break;
                }
            }

            SRSingleton<GameContext>.Instance.SlimeDefinitions.GetSlimeByIdentifiableId(Identifiable.Id.PUDDLE_SLIME).CanLargofy = true;
            SRSingleton<GameContext>.Instance.SlimeDefinitions.GetSlimeByIdentifiableId(Identifiable.Id.FIRE_SLIME).CanLargofy = true;
            SRSingleton<GameContext>.Instance.SlimeDefinitions.GetSlimeByIdentifiableId(Identifiable.Id.QUICKSILVER_SLIME).CanLargofy = true;
            SRSingleton<GameContext>.Instance.SlimeDefinitions.GetSlimeByIdentifiableId(Identifiable.Id.LUCKY_SLIME).CanLargofy = true;
            SRSingleton<GameContext>.Instance.SlimeDefinitions.GetSlimeByIdentifiableId(Identifiable.Id.GLITCH_SLIME).CanLargofy = true;

            Type type = typeof(Id);
            FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            foreach (FieldInfo field in fields)
            {
                if (field.Name.Contains("_PUDDLE_SLIME"))
                {
                    largoObjects.Add(new LargoObject
                    {
                        sharedSlimeId = Identifiable.Id.PUDDLE_SLIME,
                        baseSlimeId = (Identifiable.Id)Enum.Parse(typeof(Identifiable.Id), Enum.Parse(typeof(Identifiable.Id), field.Name, true).ToString().Replace("_PUDDLE", ""), true),
                        largoId = (Identifiable.Id)Enum.Parse(typeof(Identifiable.Id), field.Name, true)
                    });
                }
                else if (field.Name.Contains("_FIRE_SLIME"))
                {
                    largoObjects.Add(new LargoObject
                    {
                        sharedSlimeId = Identifiable.Id.FIRE_SLIME,
                        baseSlimeId = (Identifiable.Id)Enum.Parse(typeof(Identifiable.Id), Enum.Parse(typeof(Identifiable.Id), field.Name, true).ToString().Replace("_FIRE", ""), true),
                        largoId = (Identifiable.Id)Enum.Parse(typeof(Identifiable.Id), field.Name, true)
                    });
                }
                else if (field.Name.Contains("_GLITCH_SLIME"))
                {
                    largoObjects.Add(new LargoObject
                    {
                        sharedSlimeId = Identifiable.Id.GLITCH_SLIME,
                        baseSlimeId = (Identifiable.Id)Enum.Parse(typeof(Identifiable.Id), Enum.Parse(typeof(Identifiable.Id), field.Name, true).ToString().Replace("_GLITCH", ""), true),
                        largoId = (Identifiable.Id)Enum.Parse(typeof(Identifiable.Id), field.Name, true)
                    });
                }
                else if (field.Name.Contains("_LUCKY_SLIME"))
                {
                    largoObjects.Add(new LargoObject
                    {
                        sharedSlimeId = Identifiable.Id.LUCKY_SLIME,
                        baseSlimeId = (Identifiable.Id)Enum.Parse(typeof(Identifiable.Id), Enum.Parse(typeof(Identifiable.Id), field.Name, true).ToString().Replace("_LUCKY", ""), true),
                        largoId = (Identifiable.Id)Enum.Parse(typeof(Identifiable.Id), field.Name, true)
                    });
                }

            }

            foreach (LargoObject largoObject in largoObjects)
            {
                switch (largoObject.sharedSlimeId)
                {
                    case Identifiable.Id.PUDDLE_SLIME:
                        CreatePuddleLargo(largoObject.sharedSlimeId, largoObject.baseSlimeId, largoObject.largoId);
                        break;

                    case Identifiable.Id.FIRE_SLIME:
                        CreateFireLargo(largoObject.sharedSlimeId, largoObject.baseSlimeId, largoObject.largoId);
                        break;

                    case Identifiable.Id.GLITCH_SLIME:
                        CreateGlitchLargo(largoObject.sharedSlimeId, largoObject.baseSlimeId, largoObject.largoId);
                        break;

                    case Identifiable.Id.LUCKY_SLIME:
                        CreateGlitchLargo(largoObject.sharedSlimeId, largoObject.baseSlimeId, largoObject.largoId);
                        break;
                };
            }

            //SlimeEat.foodGroupIds.TryGetValue(SlimeEat.FoodGroup.PLORTS, out Identifiable.Id[] plortArray);
            //List<Identifiable.Id> plortList = plortArray.ToList();
            //plortList.Add(Identifiable.Id.PUDDLE_PLORT);
            //plortList.Add(Identifiable.Id.FIRE_PLORT);
            //SlimeEat.foodGroupIds.Remove(SlimeEat.FoodGroup.PLORTS);
            //SlimeEat.foodGroupIds.Add(SlimeEat.FoodGroup.PLORTS, plortList.ToArray());
        }

        public override void PostLoad()
        {
            Identifiable.Id id = Identifiable.Id.NONE;
            foreach (LargoObject largoObject in largoObjects)
            {
                if (largoObject.largoId.ToString() != "PUDDLE_PUDDLE_SLIME")
                {
                    if (SRSingleton<GameContext>.Instance.SlimeDefinitions.GetSlimeByIdentifiableId(largoObject.baseSlimeId).Diet.Produces.Length != 0)
                    {
                        SRSingleton<GameContext>.Instance.SlimeDefinitions.GetSlimeByIdentifiableId(largoObject.sharedSlimeId).Diet.EatMap.Add(new SlimeDiet.EatMapEntry
                        {
                            becomesId = largoObject.largoId,
                            eats = SRSingleton<GameContext>.Instance.SlimeDefinitions.GetSlimeByIdentifiableId(largoObject.baseSlimeId).Diet.Produces[0],
                            driver = referenceEatMap.driver,
                            minDrive = referenceEatMap.minDrive,
                            extraDrive = referenceEatMap.extraDrive
                        });
                    }
                    if (SRSingleton<GameContext>.Instance.SlimeDefinitions.GetSlimeByIdentifiableId(largoObject.sharedSlimeId).Diet.Produces.Length != 0)
                    {
                        SRSingleton<GameContext>.Instance.SlimeDefinitions.GetSlimeByIdentifiableId(largoObject.baseSlimeId).Diet.EatMap.Add(new SlimeDiet.EatMapEntry
                        {
                            becomesId = largoObject.largoId,
                            eats = SRSingleton<GameContext>.Instance.SlimeDefinitions.GetSlimeByIdentifiableId(largoObject.sharedSlimeId).Diet.Produces[0],
                            driver = referenceEatMap.driver,
                            minDrive = referenceEatMap.minDrive,
                            extraDrive = referenceEatMap.extraDrive
                        });
                    }
                }
            }
        }

        public static bool CreatePuddleLargo(Identifiable.Id slime1, Identifiable.Id slime2, Identifiable.Id largoId)
        {
            bool result = LargoGenerator.CreateLargo(slime1, slime2, largoId);

            GameObject largoObject = SRSingleton<GameContext>.Instance.LookupDirector.GetPrefab(largoId);
            GameObject puddleObject = SRSingleton<GameContext>.Instance.LookupDirector.GetPrefab(Identifiable.Id.PUDDLE_SLIME);

            if (largoObject.GetComponent<KeepUpright>() != null)
            {
                UnityEngine.Object.Destroy(largoObject.GetComponent<KeepUpright>());
            }

            if (largoObject.GetComponents<SphereCollider>().Length >= 1)
            {
                foreach (SphereCollider sphereCollider in largoObject.GetComponents<SphereCollider>())
                {
                    if (!sphereCollider.isTrigger)
                    {
                        UnityEngine.Object.DestroyImmediate(sphereCollider);
                    }
                }
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(largoObject.GetComponent<SphereCollider>());
            }
            largoObject.AddComponent<SphereCollider>().GetCopyOf(puddleObject.GetComponent<SphereCollider>());

            return result;
        }

        public static bool CreateFireLargo(Identifiable.Id slime1, Identifiable.Id slime2, Identifiable.Id largoId)
        {
            bool result = LargoGenerator.CreateLargo(slime1, slime2, largoId);

            GameObject largoObject = SRSingleton<GameContext>.Instance.LookupDirector.GetPrefab(largoId);
            GameObject fireObject = SRSingleton<GameContext>.Instance.LookupDirector.GetPrefab(Identifiable.Id.FIRE_SLIME);

            if (largoObject.GetComponent<KeepUpright>() != null)
            {
                UnityEngine.Object.Destroy(largoObject.GetComponent<KeepUpright>());
            }

            if (largoObject.GetComponents<SphereCollider>().Length > 1)
            {
                foreach (SphereCollider sphereCollider in largoObject.GetComponents<SphereCollider>())
                {
                    if (!sphereCollider.isTrigger)
                    {
                        UnityEngine.Object.DestroyImmediate(sphereCollider);
                    }
                }
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(largoObject.GetComponent<SphereCollider>());
            }
            largoObject.AddComponent<SphereCollider>().GetCopyOf(fireObject.GetComponent<SphereCollider>());

            return result;
        }

        public static bool CreateGlitchLargo(Identifiable.Id slime1, Identifiable.Id slime2, Identifiable.Id largoId)
        {
            bool result = LargoGenerator.CreateLargo(slime1, slime2, largoId);

            GameObject largoObject = SRSingleton<GameContext>.Instance.LookupDirector.GetPrefab(largoId);
            GameObject glitchObject = SRSingleton<GameContext>.Instance.LookupDirector.GetPrefab(Identifiable.Id.GLITCH_SLIME);

            if (largoObject.GetComponent<KeepUpright>() != null)
            {
                UnityEngine.Object.Destroy(largoObject.GetComponent<KeepUpright>());
            }
            if (largoObject.GetComponent<KeepPuddleUpright>() != null)
            {
                UnityEngine.Object.Destroy(largoObject.GetComponent<KeepPuddleUpright>());
            }
            if (largoObject.GetComponents<SphereCollider>().Length > 1)
            {
                foreach (SphereCollider sphereCollider in largoObject.GetComponents<SphereCollider>())
                {
                    if (!sphereCollider.isTrigger)
                    {
                        UnityEngine.Object.DestroyImmediate(sphereCollider);
                    }
                }
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(largoObject.GetComponent<SphereCollider>());
            }
            largoObject.AddComponent<SphereCollider>().GetCopyOf(glitchObject.GetComponent<SphereCollider>());

            return result;
        }

        public static bool CreateLuckyLargo(Identifiable.Id slime1, Identifiable.Id slime2, Identifiable.Id largoId)
        {
            bool result = LargoGenerator.CreateLargo(slime1, slime2, largoId);

            GameObject largoObject = SRSingleton<GameContext>.Instance.LookupDirector.GetPrefab(largoId);
            GameObject luckyObject = SRSingleton<GameContext>.Instance.LookupDirector.GetPrefab(Identifiable.Id.LUCKY_SLIME);

            return result;
        }
    }
}