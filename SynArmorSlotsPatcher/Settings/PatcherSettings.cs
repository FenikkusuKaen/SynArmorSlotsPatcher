using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SynArmorSlotsPatchers.Settings
{
    public class PatcherSettings
    {
        [SynthesisOrder]
        [SynthesisSettingName("Slot Settings")]
        [SynthesisTooltip("(Required) Slot settings.")]
        public List<SlotSettings> SlotSettings { get; set; } = new();

        [SynthesisOrder]
        [SynthesisSettingName("Races")]
        [SynthesisTooltip("List of armor races to be patched. 'DefaultRace' will be used if left blank.")]
        public HashSet<IFormLinkGetter<IRaceGetter>> ArmorRaces { get; set; } = new();

        [SynthesisOrder]
        public ArmorSettings ArmorSettings { get; set; } = new ();

        [SynthesisOrder]
        public ArmorAddonSettings ArmorAddonSettings { get; set; } = new();

        [SynthesisOrder]
        [SynthesisSettingName("Excluded EditorIDs")]
        [SynthesisTooltip("List of Editor IDs to be excluded.")]        
        public HashSet<string> ExcludedEditorIDs { get; set; } = new();

        [SynthesisOrder]
        [SynthesisSettingName("Excluded EditorIDs (Partial Matching)")]
        [SynthesisTooltip("List of Editor IDs (partial matching) to be excluded.")]
        public HashSet<string> ExcludedEditorIDs_Partial { get; set; } = new();

        [SynthesisOrder]
        [SynthesisSettingName("Excluded Mods (Source)")]
        [SynthesisTooltip("All items originated from those mods will be excluded.")]
        public HashSet<ModKey> ExcludedMods_Source { get; set; } = new();

        [SynthesisOrder]
        [SynthesisSettingName("Excluded Mods (Override)")]
        [SynthesisTooltip("All items overridden by those mods will be excluded.")]
        public HashSet<ModKey> ExcludedMods_Override { get; set; } = new();
    }
    public class SlotSettings
    {
        public int SlotToModify { get; set; }
        public HashSet<int> AddedSlots { get; set; } = new();
        public HashSet<int> RemovedSlots { get; set; } = new();

    }
    public class ArmorSettings
    {
        [SynthesisOrder]
        [SynthesisSettingName("Enable Armor Patching")]
        [SynthesisTooltip("Enable patching armor records.")]
        public bool EnableModule = true;

        [SynthesisOrder]
        [SynthesisSettingName("Excluded Armors")]
        [SynthesisTooltip("List of armor records to be excluded.")]
        public HashSet<IFormLinkGetter<IArmorGetter>> ExcludedArmors { get; set; } = new();

    }

    public class ArmorAddonSettings
    {
        [SynthesisOrder]
        [SynthesisSettingName("Enable Armor Addons Patching")]
        [SynthesisTooltip("Enable patching armor addon records.")]
        public bool EnableModule = true;

        [SynthesisOrder]
        [SynthesisSettingName("Excluded Armor Addons")]
        [SynthesisTooltip("List of armor addon records to be excluded.")]
        public HashSet<IFormLinkGetter<IArmorAddonGetter>> ExcludedArmorAddons { get; set; } = new();

    }
}
