using System;
using System.Collections.Generic;
using System.Linq;

namespace Decomp.Core.Operators
{
#pragma warning disable CA1720 // Identifier contains type name
    public enum Parameter
    {
        None,
        FaceKeyRegister,
        FloatRegister,
        GameKeyCode,
        KeyCode,
        Position,
        String,
        InventorySlot,
        Tooltip,
        ToolTip = Tooltip,
        Color,
        Alpha,
        TextFlags,
        MenuFlags,
        TroopFlags,
        WeaponProficiency,
        CharacterAttribute,
        PartyFlags,
        AiBehavior,
        ItemProperty,
        ItemCapability,
        TroopIdentifier,
        ItemIdentifier,
        PartyIdentifier,
        AnimationIdentifier,
        ScenePropIdentifier,
        SceneIdentifier,
        FactionIdentifier,
        TableauMaterialIdentifier,
        TableauIdentifier = TableauMaterialIdentifier,
        QuestIdentifier,
        PartyTemplateIdentifier,
        InfoPageIdentifier,
        SkillIdentifier,
        MapIconIdentifier,
        MeshIdentifier,
        ItemType,
        SoundIdentifier,
        SoundFlags,
        ScriptIdentifier,
        ParticleSystemIdentifier,
        AttributeIdentifier,
        ItemModifier,
        MenuIdentifier,
        PresentationIdentifier,
        TrackIdentifier,
        MusicFlags,
        EquipmentOverrideFlags,
        MissionTemplateIdentifier,
        SceneFlags,
        SortMode,
        SkinIdentifier,
    }
#pragma warning restore CA1720 // Identifier contains type name

#pragma warning disable CA1716 // Identifiers should not match keywords
    public class Operator
    {
        public string Value { get; set; }
        public int Code { get; set; }
        public IReadOnlyDictionary<int, Parameter> Parameters { get; set; }

        private void Initialize(string value, int code)
        {
            Value = value;
            Code = code;
            Parameters = new Dictionary<int, Parameter>(16);
        }

        public Operator(string value, int code)
        {
            Initialize(value, code);
        }
        
        public Operator(string value, int code, params Parameter[] @params)
        {
            Initialize(value, code);

            var p = new Dictionary<int, Parameter>();
            for (int i = 0; i < @params.Length; i++) p[i] = @params[i];
            Parameters = p;
        }

        public string GetParameter(int index, string s)
        {
            var b = ulong.TryParse(s, out var t);
            if (!b) return s;

            //maybe t is common param?
            if (t > 0x00FFFFFFFFFFFFFF) return Common.GetParam(t);

            if (!Parameters.ContainsKey(index)) return s;

            return Parameters[index] switch
            {
                Parameter.None => s,
                Parameter.FaceKeyRegister => Common.GetFaceKey(t),
                Parameter.FloatRegister => "fp" + s,
                Parameter.GameKeyCode => Common.GetGameKey(t),
                Parameter.KeyCode => Common.GetKey(t),
                Parameter.Position => "pos" + s,
                Parameter.String => "s" + s,
                Parameter.InventorySlot => Common.GetInventorySlot(t),
                Parameter.Tooltip => Common.GetTooltip(t),
                Parameter.Color => Common.GetColor(t),
                Parameter.TextFlags => Common.DecompileTextFlags((uint)t),
                Parameter.Alpha => Common.GetAlpha(t),
                Parameter.MenuFlags => Menus.DecompileFlags(t),
                Parameter.TroopFlags => Troops.DecompileFlags((uint)t),
                Parameter.WeaponProficiency => Common.GetWeaponProficiency(t),
                Parameter.CharacterAttribute => Common.GetCharacterAttribute(t),
                Parameter.PartyFlags => Parties.DecompileFlags((uint)t),
                Parameter.AiBehavior => Parties.GetAiBehavior(t),
                Parameter.ItemProperty => Items.DecompileFlags(t),
                Parameter.ItemCapability => Items.DecompileCapabilities(t),
                Parameter.TroopIdentifier => Common.GetCommonIdentifier("trp", Common.Troops, t),
                Parameter.ItemIdentifier => Common.GetCommonIdentifier("itm", Common.Items, t),
                Parameter.PartyIdentifier => Common.GetCommonIdentifier("p", Common.Parties, t),
                Parameter.AnimationIdentifier => Common.GetCommonIdentifier("anim", Common.Animations, t),
                Parameter.ScenePropIdentifier => Common.GetCommonIdentifier("spr", Common.SceneProps, t),
                Parameter.SceneIdentifier => Common.GetCommonIdentifier("scn", Common.Scenes, t),
                Parameter.FactionIdentifier => Common.GetCommonIdentifier("fac", Common.Factions, t),
                Parameter.TableauMaterialIdentifier => Common.GetCommonIdentifier("tableau", Common.Tableaus, t),
                Parameter.QuestIdentifier => Common.GetCommonIdentifier("qst", Common.Factions, t),
                Parameter.PartyTemplateIdentifier => Common.GetCommonIdentifier("pt", Common.Factions, t),
                Parameter.InfoPageIdentifier => Common.GetCommonIdentifier("ip", Common.InfoPages, t),
                Parameter.SkillIdentifier => Common.GetCommonIdentifier("skl", Common.Skills, t),
                Parameter.MapIconIdentifier => Common.GetCommonIdentifier("icon", Common.MapIcons, t),
                Parameter.MeshIdentifier => Common.GetCommonIdentifier("mesh", Common.Meshes, t),
                Parameter.ItemType => Items.DecompileType(t),
                Parameter.SoundIdentifier => Common.GetCommonIdentifier("snd", Common.Sounds, t),
                Parameter.SoundFlags => Sounds.DecompileFlags((uint)t),
                Parameter.ScriptIdentifier => Common.GetCommonIdentifier("script", Common.Procedures, t),
                Parameter.ParticleSystemIdentifier => Common.GetCommonIdentifier("psys", Common.ParticleSystems, t),
                Parameter.AttributeIdentifier => Troops.DecompileCharacterAttribute((uint)t),
                Parameter.ItemModifier => Items.DecompileModifier((uint)t),
                Parameter.MenuIdentifier => Common.GetCommonIdentifier("mnu", Common.Menus, t),
                Parameter.PresentationIdentifier => Common.GetCommonIdentifier("prsnt", Common.Presentations, t),
                Parameter.TrackIdentifier => Common.GetCommonIdentifier("track", Common.Music, t),
                Parameter.MusicFlags => Music.DecompileFlags((uint)t),
                Parameter.EquipmentOverrideFlags => MissionTemplates.DecompileAlterFlags((uint)t),
                Parameter.MissionTemplateIdentifier => Common.GetCommonIdentifier("mt", Common.MissionTemplates, t),
                Parameter.SceneFlags => Scenes.DecompileFlags((uint)t),
                Parameter.SortMode => Common.DecompileSortMode(t),
                Parameter.SkinIdentifier => Common.GetCommonIdentifier("tf_", Common.Skins, t),
                _ => s,
            };
        }

        public static IEnumerable<Operator> GetCollection(IEnumerable<IGameVersion> versions) => versions.SelectMany(x => x.GetOperators());

        public static IEnumerable<Operator> GetCollection(Mode m) => m switch
        {
            Mode.Caribbean => GetCollection(new List<IGameVersion> { new CaribbeanVersion() }),
            Mode.WarbandScriptEnhancer450 => GetCollection(new List<IGameVersion> { new Warband1171Version(), new WarbandScriptEnhancer450Version() }),
            Mode.WarbandScriptEnhancer320 => GetCollection(new List<IGameVersion> { new Warband1153Version(), new WarbandScriptEnhancer320Version() }),
            Mode.Vanilla => GetCollection(new List<IGameVersion> { new VanillaVersion() }),
            _ => throw new ArgumentOutOfRangeException(nameof(m), m, null),  
        };
    }
#pragma warning restore CA1716 // Identifiers should not match keywords

    public interface IGameVersion
    {
        IEnumerable<Operator> GetOperators();
    }
}
