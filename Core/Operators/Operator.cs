using System;
using System.Collections.Generic;
using System.Linq;

namespace Decomp.Core.Operators
{
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
        ItemType
    }

    public class Operator
    {
        public string Value;
        public int Code;
        public Dictionary<int, Parameter> Parameters;

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
            for (int i = 0; i < @params.Length; i++) 
                Parameters[i] = @params[i]; 
        }

        public string GetParameter(int index, string s)
        {
            ulong t;
            var b = ulong.TryParse(s, out t);
            if (!b) return s;

            //maybe t is common param?
            if (t > 0x00FFFFFFFFFFFFFF) return Common.GetParam(t);

            if (!Parameters.ContainsKey(index)) return s;

            switch (Parameters[index])
            {
                case Parameter.None:
                    return s;
                case Parameter.FaceKeyRegister:
                    return Common.GetFaceKey(t);
                case Parameter.FloatRegister:
                    return "fp" + s;
                case Parameter.GameKeyCode:
                    return Common.GetGameKey(t);
                case Parameter.KeyCode:
                    return Common.GetKey(t);
                case Parameter.Position:
                    return "pos" + s;
                case Parameter.String:
                    return "s" + s;
                case Parameter.InventorySlot:
                    return Common.GetInventorySlot(t);
                case Parameter.Tooltip:
                    return Common.GetTooltip(t);
                case Parameter.Color:
                    return Common.GetColor(t);
                case Parameter.TextFlags:
                    return Common.DecompileTextFlags((uint)t);
                case Parameter.Alpha:
                    return Common.GetAlpha(t);
                case Parameter.MenuFlags:
                    return Menus.DecompileFlags(t);
                case Parameter.TroopFlags:
                    return Troops.DecompileFlags((uint)t);
                case Parameter.WeaponProficiency:
                    return Common.GetWeaponProficiency(t);
                case Parameter.CharacterAttribute:
                    return Common.GetCharacterAttribute(t);
                case Parameter.PartyFlags:
                    return Parties.DecompileFlags((uint)t);
                case Parameter.AiBehavior:
                    return Parties.GetAiBehavior(t);
                case Parameter.ItemProperty:
                    return Items.DecompileFlags(t);
                case Parameter.ItemCapability:
                    return Items.DecompileCapabilities(t);
                case Parameter.TroopIdentifier:
                    return Common.GetCommonIdentifier("trp", Common.Troops, t);
                case Parameter.ItemIdentifier:
                    return Common.GetCommonIdentifier("itm", Common.Items, t);
                case Parameter.PartyIdentifier:
                    return Common.GetCommonIdentifier("p", Common.Parties, t);
                case Parameter.AnimationIdentifier:
                    return Common.GetCommonIdentifier("anim", Common.Animations, t);
                case Parameter.ScenePropIdentifier:
                    return Common.GetCommonIdentifier("spr", Common.SceneProps, t);
                case Parameter.SceneIdentifier:
                    return Common.GetCommonIdentifier("scn", Common.Scenes, t);
                case Parameter.FactionIdentifier:
                    return Common.GetCommonIdentifier("fac", Common.Factions, t);
                case Parameter.TableauMaterialIdentifier:
                    return Common.GetCommonIdentifier("tableau", Common.Tableaus, t);
                case Parameter.QuestIdentifier:
                    return Common.GetCommonIdentifier("qst", Common.Factions, t);
                case Parameter.PartyTemplateIdentifier:
                    return Common.GetCommonIdentifier("pt", Common.Factions, t);
                case Parameter.InfoPageIdentifier:
                    return s;
                case Parameter.SkillIdentifier:
                    return Common.GetCommonIdentifier("skl", Common.Skills, t);
                case Parameter.MapIconIdentifier:
                    return Common.GetCommonIdentifier("icon", Common.MapIcons, t);
                case Parameter.MeshIdentifier:
                    return Common.GetCommonIdentifier("mesh", Common.Meshes, t);
                case Parameter.ItemType:
                    return Items.DecompileType(t); 
                default:
                    return s;
            }
        }

        public static IEnumerable<Operator> GetCollection(IEnumerable<IGameVersion> versions)
        {
            return versions.SelectMany(x => x.GetOperators());
        }

        public static IEnumerable<Operator> GetCollection(Mode m)
        {
            switch (m)
            {
                case Mode.Caribbean:
                    return GetCollection(new List<IGameVersion> { new Warband1153Version() });
                case Mode.WarbandScriptEnhancer450:
                    return GetCollection(new List<IGameVersion> { new Warband1171Version(), new WarbandScriptEnhancer450Version() });
                case Mode.WarbandScriptEnhancer320:
                    return GetCollection(new List<IGameVersion> { new Warband1153Version(), new WarbandScriptEnhancer320Version() });
                case Mode.Vanilla:
                    return GetCollection(new List<IGameVersion> { new Warband1153Version() });
                default:
                    throw new ArgumentOutOfRangeException(nameof(m), m, null);
            }
        }
    }

    public interface IGameVersion
    {
        IEnumerable<Operator> GetOperators();
    }
}
