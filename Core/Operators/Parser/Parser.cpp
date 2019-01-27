#include <iostream>
#include <fstream>
#include <string>
#include <cstdio>
#include <cctype>
#include <vector>
#include <functional>
#include <algorithm>
#include <map>
#include <sstream>

template<typename T>
void split(const std::string &s, char delim, T result)
{
	std::stringstream ss(s);
	std::string item;
	while (std::getline(ss, item, delim)) *(result++) = item;
}

std::vector<std::string> split(const std::string &s, char delim)
{
	std::vector<std::string> elems;
	split(s, delim, std::back_inserter(elems));
	return elems;
}

template<class T, class F>
void map_vector(std::vector<T> &a, F fun)
{
	for (size_t i = 0; i < a.size(); i++) a[i] = fun(a[i]);
}

std::string ltrim(const std::string &str)
{
	auto s(str);
	s.erase(s.begin(), std::find_if(s.begin(), s.end(), [](int ch) { return !isspace(ch); }));
	return s;
}

std::string rtrim(const std::string &str)
{
	auto s(str);
	s.erase(std::find_if(s.rbegin(), s.rend(), [](int ch) { return !isspace(ch); }).base(), s.end());
	return s;
}

std::string trim(const std::string &s)
{
	return ltrim(rtrim(s));
}

bool is_number(const std::string &test)
{
	for (size_t i = 0; i < test.size(); i++)
	{
		if (!isdigit(test[i])) return false;
	}
	return true;
}

std::string remove_comment(const std::string &s)
{
	return trim(s.substr(0, s.find('#')));
}

std::string get_comment(const std::string &s)
{
	auto i = s.find('#');
	return i == std::string::npos ? "" : trim(s.substr(i + 1));
}

bool is_command(const std::string &cmd, std::string &operand, int &opcode)
{
	auto i = cmd.find('=');
	if (i == std::string::npos) return false;
	operand = trim(cmd.substr(0, i));
	auto opcodestr = trim(cmd.substr(i + 1));
	//std::cerr << "Process: operand = \'" << operand << "\', opcode \'" << opcodestr << "\'"  << std::endl;
	if (!is_number(opcodestr)) return false;
	opcode = atoi(opcodestr.data());
	return true;
}

std::string pre_process_param(const std::string &param)
{
	std::string result;
	result.reserve(param.size());

	for (size_t i = 0; i < param.size(); i++)
	{
		if (isalnum(param[i]) || param[i] == '_') result.push_back(param[i]);
	}

	return result;
}

std::string get_suffix(const std::string &s)
{
	size_t i = s.find('_');
	return i == std::string::npos ? "" : s.substr(i + 1);
}

std::string get_prefix(const std::string &s)
{
	size_t i = s.find('_');
	return i == std::string::npos ? "" : s.substr(0, i);
}

std::string process_param_using_repl_table(const std::string &param, std::map<std::string, std::string> &table)
{
	auto it = table.find(param);

	if (it != table.end()) return it->second;
	if (param.find("position") != std::string::npos) return table["position"];
	if (param.find("string") != std::string::npos) return table["string"];
	if (param.find("fp_register") != std::string::npos) return table["fp_register"];
	
	auto suff = get_suffix(param);
	auto pref = get_prefix(param);

	if (pref == "agent") return "Parameter.None";
	if (pref == "entry") return "Parameter.None";
	if (pref == "team") return "Parameter.None";
	if (pref == "slot") return "Parameter.None";
	if (pref == "profile") return "Parameter.None";
	if (pref == "player") return "Parameter.None";
	if (pref == "player") return "Parameter.None";

	if (suff == "id") fprintf(stderr, "WARNING: %s was not proceed!\n", param.data());
	else if (suff == "no") fprintf(stderr, "WARNING: %s was not proceed!\n", param.data());
	return "Parameter.None";
}

std::string params_vector_to_str(const std::vector<std::string> &vec)
{
	std::string result;
	for (size_t i = 0; i < vec.size(); i++)
	{
		result += ", ";
		result += vec[i];
	}
	return result;
}

int read_version(const std::string &s)
{
	if (s.substr(0, 7) == "Version")
	{
		auto str = s.substr(8, 5);
		int result = 0;
		for (size_t i = 0; i < str.size(); ++i)
		{
			if (isdigit(str[i])) result = result * 10 + (str[i] - '0');
		}
		return result;
	}
	return -1;
}

class Operator
{
public:
	std::string Name;
	int OpCode;
	std::vector<std::string> Params;

	Operator(std::string s, int o, const std::vector<std::string> &p) : Name(std::move(s)), OpCode(o), Params(p)
	{
	}
};

int main(int argc, char* argv[])
{
	int needed_version = 0;

	if (argc < 3)
	{
		printf("'%s' <input file> <output file>\n", argv[0]);
		return 0;
	}

	if (argc == 4) needed_version = atoi(argv[3]);

	printf("Mount & Blade: Warband Decompiler -- Parser\n");
	printf("File To Process: '%s'\n", argv[1]);
	if(needed_version > 0) printf("Version: %.3f\n", needed_version / 1000.0);

	std::ifstream fin(argv[1]);
	std::ofstream fout(argv[2]);

	std::map<std::string, std::string> repl_table;
	repl_table.insert({ "scene_prop_id", "Parameter.ScenePropIdentifier" });
	repl_table.insert({ "string", "Parameter.String" });
	repl_table.insert({ "material_name", "Parameter.String" });
	repl_table.insert({ "new_material_name", "Parameter.String" });
	repl_table.insert({ "mesh_name", "Parameter.String" });
	repl_table.insert({ "string_id", "Parameter.String" });
	repl_table.insert({ "string_no", "Parameter.String" });
	repl_table.insert({ "position", "Parameter.Position" });
	repl_table.insert({ "dest_position", "Parameter.Position" });
	repl_table.insert({ "key_code", "Parameter.KeyCode" });
	repl_table.insert({ "key_no", "Parameter.KeyCode" });
	repl_table.insert({ "game_key_code", "Parameter.GameKeyCode" });
	repl_table.insert({ "game_key_no", "Parameter.GameKeyCode" });
	repl_table.insert({ "key", "Parameter.KeyCode" });
	repl_table.insert({ "key_id", "Parameter.KeyCode" });
	repl_table.insert({ "game_key", "Parameter.GameKeyCode" });
	repl_table.insert({ "troop_id", "Parameter.TroopIdentifier" });
	repl_table.insert({ "faction_no", "Parameter.FactionIdentifier" });
	repl_table.insert({ "faction_id_1", "Parameter.FactionIdentifier" });
	repl_table.insert({ "faction_id_2", "Parameter.FactionIdentifier" });
	repl_table.insert({ "faction_id", "Parameter.FactionIdentifier" });
	repl_table.insert({ "color_code", "Parameter.Color" });
	repl_table.insert({ "party_id", "Parameter.PartyIdentifier" });
	repl_table.insert({ "destinationparty_id", "Parameter.PartyIdentifier" });
	repl_table.insert({ "host_party_id", "Parameter.PartyIdentifier" });
	repl_table.insert({ "town_party_id", "Parameter.PartyIdentifier" });
	repl_table.insert({ "party_template_id", "Parameter.PartyTemplateIdentifier" });
	repl_table.insert({ "relocated_party_id", "Parameter.PartyIdentifier" });
	repl_table.insert({ "target_party_id", "Parameter.PartyIdentifier" });
	repl_table.insert({ "party_id_to_attach_to", "Parameter.PartyIdentifier" });
	repl_table.insert({ "source_party_id", "Parameter.PartyIdentifier" });
	repl_table.insert({ "collected_party_id", "Parameter.PartyIdentifier" });
	repl_table.insert({ "map_icon_id", "Parameter.MapIconIdentifier" });
	repl_table.insert({ "particle_system_id", "Parameter.ParticleSystemIdentifier" });
	repl_table.insert({ "party_to_be_distributed", "Parameter.PartyIdentifier" });
	repl_table.insert({ "group_root_party", "Parameter.PartyIdentifier" });
	repl_table.insert({ "parent_party_id", "Parameter.PartyIdentifier" });
	repl_table.insert({ "party_id_to_add_causalties_to", "Parameter.PartyIdentifier" });
	repl_table.insert({ "party_no", "Parameter.PartyIdentifier" });
	repl_table.insert({ "object_party_id", "Parameter.PartyIdentifier" });
	repl_table.insert({ "ai_bhvr", "Parameter.AiBehavior" });
	repl_table.insert({ "item_id", "Parameter.ItemIdentifier" });
	repl_table.insert({ "item_kind_no", "Parameter.ItemIdentifier" });
	repl_table.insert({ "property", "Parameter.ItemProperty" });
	repl_table.insert({ "capability", "Parameter.ItemCapability" });
	repl_table.insert({ "attribute_id", "Parameter.AttributeIdentifier" });
	repl_table.insert({ "skill_id", "Parameter.SkillIdentifier" });
	repl_table.insert({ "proficiency_no", "Parameter.WeaponProficiency" });
	repl_table.insert({ "modifier", "Parameter.ItemModifier" });
	repl_table.insert({ "item_modifier_no", "Parameter.ItemModifier" });
	repl_table.insert({ "inventory_slot_no", "Parameter.InventorySlot" });
	repl_table.insert({ "item_slot_no", "Parameter.InventorySlot" });
	repl_table.insert({ "source_troop_id", "Parameter.TroopIdentifier" });
	repl_table.insert({ "target_troop_id", "Parameter.TroopIdentifier" });
	repl_table.insert({ "imod_value", "Parameter.ItemModifier" });
	repl_table.insert({ "item_type_id", "Parameter.ItemType" });
	repl_table.insert({ "target_troop_id", "Parameter.TroopIdentifier" });
	repl_table.insert({ "target_troop_id", "Parameter.TroopIdentifier" });
	repl_table.insert({ "troop_no", "Parameter.TroopIdentifier" });
	repl_table.insert({ "quest_id", "Parameter.QuestIdentifier" });
	repl_table.insert({ "giver_troop_id", "Parameter.TroopIdentifier" });
	repl_table.insert({ "mesh_name_string", "Parameter.String" });
	repl_table.insert({ "sound_id", "Parameter.SoundIdentifier" });
	repl_table.insert({ "sound_flags", "Parameter.SoundFlags" });
	repl_table.insert({ "track_id", "Parameter.TrackIdentifier" });
	repl_table.insert({ "situation_type", "Parameter.MusicFlags" });
	repl_table.insert({ "culture_type", "Parameter.MusicFlags" });
	repl_table.insert({ "tableau_material_id", "Parameter.TableauMaterialIdentifier" });
	repl_table.insert({ "info_page_id", "Parameter.InfoPageIdentifier" });
	repl_table.insert({ "mesh_id", "Parameter.MeshIdentifier" });
	repl_table.insert({ "mesh_no", "Parameter.MeshIdentifier" });
	repl_table.insert({ "animation_id", "Parameter.AnimationIdentifier" });
	repl_table.insert({ "item_kind_id", "Parameter.ItemIdentifier" });
	repl_table.insert({ "string_register", "Parameter.String" });
	repl_table.insert({ "item_kind_id", "Parameter.ItemIdentifier" });
	repl_table.insert({ "hex_colour_code", "Parameter.Color" });
	repl_table.insert({ "code", "Parameter.Color" });
	repl_table.insert({ "item_kind_id", "Parameter.ItemIdentifier" });
	repl_table.insert({ "town_id", "Parameter.PartyIdentifier" });
	repl_table.insert({ "scene_id", "Parameter.SceneIdentifier" });
	repl_table.insert({ "mission_template_id", "Parameter.MissionTemplateIdentifier" });
	repl_table.insert({ "af_flags", "Parameter.EquipmentOverrideFlags" });
	repl_table.insert({ "fog_color", "Parameter.Color" });
	repl_table.insert({ "scene_prop_instance_id", "Parameter.ScenePropIdentifier" });
	repl_table.insert({ "new_scene_prop_id", "Parameter.ScenePropIdentifier" });
	repl_table.insert({ "old_scene_prop_id", "Parameter.ScenePropIdentifier" });
	repl_table.insert({ "pos", "Parameter.Position" });
	repl_table.insert({ "prop_instance_no", "Parameter.ScenePropIdentifier" });
	repl_table.insert({ "old_item_id", "Parameter.ItemIdentifier" });
	repl_table.insert({ "item_modifier", "Parameter.ItemModifier" });
	repl_table.insert({ "par_sys_id", "Parameter.ParticleSystemIdentifier" });
	repl_table.insert({ "weapon_item_modifier", "Parameter.ItemModifier" });
	repl_table.insert({ "missile_item_modifier", "Parameter.ItemModifier" });
	repl_table.insert({ "weapon_item_id", "Parameter.ItemIdentifier" });
	repl_table.insert({ "missile_item_id", "Parameter.ItemIdentifier" });
	repl_table.insert({ "presentation_id", "Parameter.PresentationIdentifier" });
	repl_table.insert({ "mesh_id", "Parameter.MeshIdentifier" });
	repl_table.insert({ "alpha", "Parameter.Alpha" });
	repl_table.insert({ "menu_id", "Parameter.MenuIdentifier" });
	repl_table.insert({ "text_flags", "Parameter.TextFlags" });
	repl_table.insert({ "mesh_name_string", "Parameter.String" });
	repl_table.insert({ "party_flags", "Parameter.PartyFlags" });
	repl_table.insert({ "menu_flags", "Parameter.MenuFlags" });
	repl_table.insert({ "tooltip_type", "Parameter.ToolTip" });
	repl_table.insert({ "skill_no", "Parameter.SkillIdentifier" });
	repl_table.insert({ "func_name", "Parameter.String" });
	repl_table.insert({ "script_id", "Parameter.ScriptIdentifier" });
	repl_table.insert({ "script_no", "Parameter.ScriptIdentifier" });
	repl_table.insert({ "item_no", "Parameter.ItemIdentifier" });
	repl_table.insert({ "itm_id", "Parameter.ItemIdentifier" });
	repl_table.insert({ "anim_id", "Parameter.AnimationIdentifier" });
	repl_table.insert({ "anim_no", "Parameter.AnimationIdentifier" });
	repl_table.insert({ "quest_no", "Parameter.QuestIdentifier" });
	repl_table.insert({ "scene_no", "Parameter.SceneIdentifier" });
	repl_table.insert({ "presentation_no", "Parameter.PresentationIdentifier" });
	repl_table.insert({ "menu_no", "Parameter.MenuIdentifier" });
	repl_table.insert({ "fp_register", "Parameter.FloatRegister" });
	repl_table.insert({ "scene_flags", "Parameter.SceneFlags" });
	repl_table.insert({ "outer_terrain_mesh_name", "Parameter.String" });
	repl_table.insert({ "troop_flags", "Parameter.TroopFlags" });

	std::vector<Operator> out;

	while (!fin.eof())
	{
		std::string line;
		std::getline(fin, line);
		line = trim(line);
		if (!line.size()) continue;

		std::string a = remove_comment(line), b = get_comment(line);

		std::string operand; int opcode;
		if (is_command(a, operand, opcode))
		{
			size_t l = b.find('(');
			size_t r = b.rfind(')');
			std::vector<std::string> params;
			if (r == std::string::npos || l == std::string::npos) goto noparams;
			
			{
				l++; r--;
				auto c = trim(b.substr(l, r - l + 1));

				params = split(c, ',');
				params.erase(params.begin());
				map_vector(params, pre_process_param);
				map_vector(params, [&](const std::string &s) { return process_param_using_repl_table(s, repl_table); });

				while (params.size() > 0 && params.back() == "Parameter.None") params.pop_back();
			}
			
		noparams:
			out.push_back(Operator(operand, opcode, params));
			//fout << "                " << "new Operator(\"" << operand << "\", " << opcode << params_vector_to_str(params) << ")," << std::endl;
		}
		else
		{
			int v = read_version(b);
			if (v > needed_version) out.pop_back();
		}
	}

	for (auto &t : out)
	{
		fout << "                " << "new Operator(\"" << t.Name << "\", " << t.OpCode << params_vector_to_str(t.Params) << ")," << std::endl;
	}

	fin.close();
	fout.close();

	return 0;
}