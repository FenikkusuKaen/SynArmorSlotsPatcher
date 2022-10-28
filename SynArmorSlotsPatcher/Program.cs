using System;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Noggog;
using SynArmorSlotsPatchers.Settings;
using Microsoft.Extensions.Logging;

namespace SynArmorSlotsPatchers
{
    public class Program
    {

        static Lazy<PatcherSettings> Settings = null!;
        public static async Task<int> Main(string[] args)
        {
            return await SynthesisPipeline.Instance
                .SetAutogeneratedSettings(
                    nickname: "Settings",
                    path: "settings.json",
                    out Settings)
                .AddPatch<ISkyrimMod, ISkyrimModGetter>(RunPatch)
                .SetTypicalOpen(GameRelease.SkyrimSE, "SynArmorSlotsPatch.esp")
                .Run(args);
        }

        public static void RunPatch(IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            var settings = Settings.Value;
            if (settings != null)
            {
                var raceFormKeys = settings.ArmorRaces.Select(x => x.FormKey).ToList();
                if (raceFormKeys.Count == 0)
                {
                    raceFormKeys = state.LoadOrder.PriorityOrder.WinningOverrides<IRaceGetter>()
                        .Where(x => x.EditorID == "DefaultRace")
                        .Select(x => x.FormKey)
                        .ToList();
                }

                foreach (var perSlotSetting in settings.SlotSettings)
                {
                    var rootSlot = IntToSlot(perSlotSetting.SlotToModify);
                    var addedSlots = perSlotSetting.AddedSlots.Select(x => IntToSlot(x));
                    var removedSlots = perSlotSetting.RemovedSlots.Select(x => IntToSlot(x));
                    
                    if (settings.ArmorAddonSettings.EnableModule)
                    {
                        
                        foreach (var record in state.LoadOrder.PriorityOrder.WinningOverrides<IArmorAddonGetter>())
                        {
                            System.Console.WriteLine($"Processing {record.EditorID}");
                            if (!raceFormKeys.Any(x => x == record.Race.FormKey))
                            {
                                System.Console.WriteLine($"Skipped race {record.Race.FormKey}");
                                continue;
                            }

                            if (settings.ArmorAddonSettings.ExcludedArmorAddons.Contains(record) ||
                                settings.ExcludedMods_Source.Contains(record.FormKey.ModKey) ||
                                (record.EditorID is not null &&
                                    (settings.ExcludedEditorIDs.Contains(record.EditorID) ||
                                        settings.ExcludedEditorIDs_Partial.Any(x => record.EditorID.Contains(x)))))
                            {
                                continue;
                            }

                            var overrideMods = state.LinkCache.ResolveAllContexts<IArmorAddon, IArmorAddonGetter>(record.FormKey).Where(x => !x.ModKey.Equals(record.FormKey.ModKey)).Select(x => x.ModKey);
                            if (overrideMods.Any(x => settings.ExcludedMods_Override.Contains(x))) { continue; }

                            if (record.BodyTemplate is not null && record.BodyTemplate.FirstPersonFlags.HasFlag(rootSlot))
                            {
                                // add needed flags
                                foreach (var slotToAdd in addedSlots)
                                {
                                    if (!record.BodyTemplate.FirstPersonFlags.HasFlag(slotToAdd))
                                    {
                                        var moddedArmature = state.PatchMod.ArmorAddons.GetOrAddAsOverride(record);
                                        if (moddedArmature != null && moddedArmature.BodyTemplate != null)
                                        {
                                            moddedArmature.BodyTemplate.FirstPersonFlags |= slotToAdd;
                                        }
                                    }
                                }

                                // remove needed flags
                                foreach (var slotToRemove in removedSlots)
                                {
                                    if (record.BodyTemplate.FirstPersonFlags.HasFlag(slotToRemove))
                                    {
                                        var moddedArmature = state.PatchMod.ArmorAddons.GetOrAddAsOverride(record);
                                        if (moddedArmature != null && moddedArmature.BodyTemplate != null)
                                        {
                                            moddedArmature.BodyTemplate.FirstPersonFlags &= ~slotToRemove;
                                        }
                                    }
                                }
                            }
                        }
                    }



                    foreach (var record in state.LoadOrder.PriorityOrder.WinningOverrides<IArmorGetter>())
                    {
                        System.Console.WriteLine($"Processing {record.EditorID}");
                        if (!raceFormKeys.Any(x => x == record.Race.FormKey))
                        {
                            System.Console.WriteLine($"Skipped race {record.Race.FormKey}");
                            continue;
                        }

                        if (settings.ArmorSettings.ExcludedArmors.Contains(record) ||
                            settings.ExcludedMods_Source.Contains(record.FormKey.ModKey) ||
                            (record.EditorID is not null &&
                                (settings.ExcludedEditorIDs.Contains(record.EditorID) ||
                                    settings.ExcludedEditorIDs_Partial.Where(x => record.EditorID.Contains(x)).Any()))) { continue; }

                        var overrideMods = state.LinkCache.ResolveAllContexts<IArmorAddon, IArmorAddonGetter>(record.FormKey).Where(x => !x.ModKey.Equals(record.FormKey.ModKey)).Select(x => x.ModKey);
                        if (overrideMods.Where(x => settings.ExcludedMods_Override.Contains(x)).Any()) { continue; }

                        if (record.BodyTemplate is not null && record.BodyTemplate.FirstPersonFlags.HasFlag(rootSlot))
                        {
                            foreach (var slotToAdd in addedSlots)
                            {
                                if (!record.BodyTemplate.FirstPersonFlags.HasFlag(slotToAdd))
                                {
                                    var moddedArmature = state.PatchMod.Armors.GetOrAddAsOverride(record);
                                    if (moddedArmature != null && moddedArmature.BodyTemplate != null)
                                    {
                                        moddedArmature.BodyTemplate.FirstPersonFlags |= slotToAdd;
                                    }
                                }
                            }

                            foreach (var slotToRemove in removedSlots)
                            {
                                if (record.BodyTemplate.FirstPersonFlags.HasFlag(slotToRemove))
                                {
                                    var moddedArmature = state.PatchMod.Armors.GetOrAddAsOverride(record);
                                    if (moddedArmature != null && moddedArmature.BodyTemplate != null)
                                    {
                                        moddedArmature.BodyTemplate.FirstPersonFlags &= ~slotToRemove;
                                    }
                                }
                            }
                        }

                        System.Console.WriteLine($"Processed race {record.Race.FormKey}");
                    }
                }

            }
        }

        public static BipedObjectFlag IntToSlot(int iFlag)
        {
            switch (iFlag)
            {
                case 30: return (BipedObjectFlag)0x00000001;
                case 31: return (BipedObjectFlag)0x00000002;
                case 32: return (BipedObjectFlag)0x00000004;
                case 33: return (BipedObjectFlag)0x00000008;
                case 34: return (BipedObjectFlag)0x00000010;
                case 35: return (BipedObjectFlag)0x00000020;
                case 36: return (BipedObjectFlag)0x00000040;
                case 37: return (BipedObjectFlag)0x00000080;
                case 38: return (BipedObjectFlag)0x00000100;
                case 39: return (BipedObjectFlag)0x00000200;
                case 40: return (BipedObjectFlag)0x00000400;
                case 41: return (BipedObjectFlag)0x00000800;
                case 42: return (BipedObjectFlag)0x00001000;
                case 43: return (BipedObjectFlag)0x00002000;
                case 44: return (BipedObjectFlag)0x00004000;
                case 45: return (BipedObjectFlag)0x00008000;
                case 46: return (BipedObjectFlag)0x00010000;
                case 47: return (BipedObjectFlag)0x00020000;
                case 48: return (BipedObjectFlag)0x00040000;
                case 49: return (BipedObjectFlag)0x00080000;
                case 50: return (BipedObjectFlag)0x00100000;
                case 51: return (BipedObjectFlag)0x00200000;
                case 52: return (BipedObjectFlag)0x00400000;
                case 53: return (BipedObjectFlag)0x00800000;
                case 54: return (BipedObjectFlag)0x01000000;
                case 55: return (BipedObjectFlag)0x02000000;
                case 56: return (BipedObjectFlag)0x04000000;
                case 57: return (BipedObjectFlag)0x08000000;
                case 58: return (BipedObjectFlag)0x10000000;
                case 59: return (BipedObjectFlag)0x20000000;
                case 60: return (BipedObjectFlag)0x40000000;
                case 61: return (BipedObjectFlag)0x80000000;
                default: throw new Exception(iFlag + " is not a valid armor slot.");
            }
        }

        public static int SlotToInt(BipedObjectFlag oFlag)
        {
            switch (oFlag)
            {
                case (BipedObjectFlag)0x00000001: return 30;
                case (BipedObjectFlag)0x00000002: return 31;
                case (BipedObjectFlag)0x00000004: return 32;
                case (BipedObjectFlag)0x00000008: return 33;
                case (BipedObjectFlag)0x00000010: return 34;
                case (BipedObjectFlag)0x00000020: return 35;
                case (BipedObjectFlag)0x00000040: return 36;
                case (BipedObjectFlag)0x00000080: return 37;
                case (BipedObjectFlag)0x00000100: return 38;
                case (BipedObjectFlag)0x00000200: return 39;
                case (BipedObjectFlag)0x00000400: return 40;
                case (BipedObjectFlag)0x00000800: return 41;
                case (BipedObjectFlag)0x00001000: return 42;
                case (BipedObjectFlag)0x00002000: return 43;
                case (BipedObjectFlag)0x00004000: return 44;
                case (BipedObjectFlag)0x00008000: return 45;
                case (BipedObjectFlag)0x00010000: return 46;
                case (BipedObjectFlag)0x00020000: return 47;
                case (BipedObjectFlag)0x00040000: return 48;
                case (BipedObjectFlag)0x00080000: return 49;
                case (BipedObjectFlag)0x00100000: return 50;
                case (BipedObjectFlag)0x00200000: return 51;
                case (BipedObjectFlag)0x00400000: return 52;
                case (BipedObjectFlag)0x00800000: return 53;
                case (BipedObjectFlag)0x01000000: return 54;
                case (BipedObjectFlag)0x02000000: return 55;
                case (BipedObjectFlag)0x04000000: return 56;
                case (BipedObjectFlag)0x08000000: return 57;
                case (BipedObjectFlag)0x10000000: return 58;
                case (BipedObjectFlag)0x20000000: return 59;
                case (BipedObjectFlag)0x40000000: return 60;
                case (BipedObjectFlag)0x80000000: return 61;
                default: return 0;
            }
        }
    }
}