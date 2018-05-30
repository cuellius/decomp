//#define RELEASE

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Decomp.Core.Operators;
using Decomp.Windows;

namespace Decomp.Core
{
    public static class Decompiler
    {
        private static string GetDirectory(this DirectoryNotFoundException exception)
        {
            var message = exception.Message;
            var beginPos = message.IndexOf('"');
            var endPos = message.IndexOf('"', beginPos + 1);
            return Path.GetDirectoryName(message.Substring(beginPos + 1, endPos - beginPos - 1));
        }

        public static MainWindow Window;
        private static string Status
        {
            set => Window.StatusTextBlock.SetText(value);
        }

        private static Thread _workThread = new Thread(Decompile);

        public static bool Alive => _workThread.IsAlive;

        public static void StopDecompilation()
        {
            _workThread.Abort();
        }

        public static void StartDecompilation()
        {
            _workThread = new Thread(Decompile);
            _workThread.Start();
        }

        public static void Decompile()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(1033);
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(1033);

            var sw = Stopwatch.StartNew();
            Window.Print(Application.GetResource("LocalizationInitialization") + " ");

            //Window.DecompileButton.SetContent("Decompile");
            Status = "";

            InitializePath(out var isSingleFile);

            if (!File.Exists(Common.InputPath) && !Directory.Exists(Common.InputPath))
            {
                Window.Print("\n" + Application.GetResource("LocalizationPleasePath"));
                Window.DecompileButton.SetContent(Application.GetResource("LocalizationDecompile"));
                Status = "";
                return;
            }

            if (!Directory.Exists(Common.OutputPath))
            {
                try
                {
                    Directory.CreateDirectory(Common.OutputPath);
                }
                catch
                {
                    Window.Print("\n" + Application.GetResource("LocalizationPleaseOutput"));
                    Window.DecompileButton.SetContent(Application.GetResource("LocalizationDecompile"));
                    Status = "";
                    return;
                }
            }

            try
            {
                if (!isSingleFile)
                {
                    InitializeOpCodes();
                    InitializeModuleData();
                    Common.NeedId = Window.GenerateIdFilesCheckBox.IsChecked();
                }
                else
                {
                    var f = InitializeTrie[GetSingleFileName()] ?? (() => { });
                    f();
                }
            }
            catch (FileNotFoundException ex)
            {
                //Window.Print("\nFile \"{0}\" not found\nDecompilation Aborted", ex.FileName);
                Window.Print("\n" + Application.GetResource("LocalizationFileNotFound"), ex.FileName);
                Window.DecompileButton.SetContent(Application.GetResource("LocalizationDecompile"));
                Status = "";
                return;
            }
            catch (DirectoryNotFoundException ex)
            {
                Window.Print("\n" + Application.GetResource("LocalizationDirectoryNotFound"), ex.GetDirectory());
                Window.DecompileButton.SetContent(Application.GetResource("LocalizationDecompile"));
                Status = "";
                return;
            }
            catch (ThreadAbortException)
            {
                Window.Print(Application.GetResource("LocalizationDecompilationCanceled") + "\n");
                Window.DecompileButton.SetContent(Application.GetResource("LocalizationDecompile"));
                Status = "";
                return;
            }
            catch (Exception e) 
            {
                Window.Print($"\n{Application.GetResource("LocalizationFatalErrorDecompilationAborted")}\n");
                Window.Print("{0}\n", e.Message);
                Window.Print("{0}\n", e.StackTrace);
                Window.DecompileButton.SetContent(Application.GetResource("LocalizationDecompile"));
                Status = "";
                return;
            }

            Window.Print(Application.GetResource("LocalizationTime") + "\n", sw.ElapsedTicks * 1000.0 / Stopwatch.Frequency);

            var success = false;

#if RELEASE
            try
            {
#endif
                if (isSingleFile)
                    ProcessSingleFile();
                else
                    ProcessFullModule();
                success = true;
#if RELEASE
            }
            catch (ThreadAbortException)
            {
                Window.Print(Application.GetResource("LocalizationDecompilationCanceled") + "\n");
                Window.DecompileButton.SetContent(Application.GetResource("LocalizationDecompile"));
                Status = "";
                return;
            }
            catch (Exception ex)
            {
                Window.Print(Application.GetResource("LocalizationFatalErrorDecompilationAborted") + "\n");
                Window.Print("{0}\n", ex.Message);
                Window.Print("{0}\n", ex.StackTrace);

                var errorWindowThread = new Thread(() =>
                {
                    var errorWindow = new ErrorWindow(ex);
                    errorWindow.ShowDialog();
                    Thread.CurrentThread.Abort();
                    //_workThread.Resume();
                });
                errorWindowThread.SetApartmentState(ApartmentState.STA);
                errorWindowThread.CurrentCulture = CultureInfo.GetCultureInfo(1033);
                errorWindowThread.CurrentUICulture = CultureInfo.GetCultureInfo(1033);
                errorWindowThread.Start();
                //_workThread.Suspend();
            }
#endif

            Window.Print(Application.GetResource("LocalizationTotalTime") + "\n", sw.ElapsedTicks * 1000.0 / Stopwatch.Frequency);

            if (Window.OpenAfterCompleteCheckBox.IsChecked() && success) Process.Start(Common.OutputPath);

            Window.DecompileButton.SetContent(Application.GetResource("LocalizationDecompile"));
            Status = "";
        }

        private static void InitializePath(out bool isSingleFile)
        {
            var x = false;
            Window.Dispatcher.Invoke(() =>
            {
                if (!File.Exists(Window.SourcePathTextBox.Text))
                {
                    Common.InputPath = Window.SourcePathTextBox.Text;
                    x = false;
                }
                else
                {
                    Common.InputPath = Path.GetDirectoryName(Window.SourcePathTextBox.Text);
                    x = true;
                }
                Common.OutputPath = Window.OutputPathTextBox.Text;

                if (Common.InputPath != null && Common.InputPath[Common.InputPath.Length - 1] == '\\')
                    Common.InputPath = Common.InputPath.Remove(Common.InputPath.Length - 1, 1);
                if (Common.OutputPath[Common.OutputPath.Length - 1] == '\\')
                    Common.OutputPath = Common.OutputPath.Remove(Common.OutputPath.Length - 1, 1);
            });
            isSingleFile = x;
        }

        private static void InitializeOpCodes()
        {
            //Common.Operations = new Dictionary<long, string>();
            Common.Operators = new Dictionary<int, Operator>();

            Window.ModeComboBox.Dispatcher.Invoke(() => Common.SelectedMode = (Mode)Window.ModeComboBox.SelectedIndex);
            var operators = Operator.GetCollection(Common.SelectedMode);
            foreach (var op in operators) Common.Operators[op.Code] = op;
        }

        private static void InitializeModuleData()
        {
            Status = $"{Application.GetResource("LocalizationDecompilation")} -- {Application.GetResource("LocalizationInitialization")} scripts.txt";
            Common.Procedures = Scripts.Initialize();
            Status = $"{Application.GetResource("LocalizationDecompilation")} -- {Application.GetResource("LocalizationInitialization")} quick_strings.txt";
            Common.QuickStrings = QuickStrings.Initialize();
            Status = $"{Application.GetResource("LocalizationDecompilation")} -- {Application.GetResource("LocalizationInitialization")} strings.txt";
            Common.Strings = Strings.Initialize();
            Status = $"{Application.GetResource("LocalizationDecompilation")} -- {Application.GetResource("LocalizationInitialization")} item_kinds1.txt";
            Common.Items = Text.GetFirstStringFromFile(Common.InputPath + @"\item_kinds1.txt") == "itemsfile version 2"
                ? Vanilla.Items.GetIdFromFile(Common.InputPath + @"\item_kinds1.txt") : Items.Initialize();
            Status = $"{Application.GetResource("LocalizationDecompilation")} -- {Application.GetResource("LocalizationInitialization")} troops.txt";
            Common.Troops = Text.GetFirstStringFromFile(Common.InputPath + @"\troops.txt") == "troopsfile version 1"
                ? Vanilla.Troops.GetIdFromFile(Common.InputPath + @"\troops.txt") : Troops.Initialize();
            Status = $"{Application.GetResource("LocalizationDecompilation")} -- {Application.GetResource("LocalizationInitialization")} factions.txt";
            Common.Factions = Factions.Initialize();
            Status = $"{Application.GetResource("LocalizationDecompilation")} -- {Application.GetResource("LocalizationInitialization")} quests.txt";
            Common.Quests = Quests.Initialize();
            Status = $"{Application.GetResource("LocalizationDecompilation")} -- {Application.GetResource("LocalizationInitialization")} party_templates.txt";
            Common.PTemps = PartyTemplates.Initialize();
            Status = $"{Application.GetResource("LocalizationDecompilation")} -- {Application.GetResource("LocalizationInitialization")} parties.txt";
            Common.Parties = Parties.Initialize();
            Status = $"{Application.GetResource("LocalizationDecompilation")} -- {Application.GetResource("LocalizationInitialization")} menus.txt";
            Common.Menus = Menus.Initialize();
            Status = $"{Application.GetResource("LocalizationDecompilation")} -- {Application.GetResource("LocalizationInitialization")} sounds.txt";
            Common.Sounds = Sounds.Initialize();
            Status = $"{Application.GetResource("LocalizationDecompilation")} -- {Application.GetResource("LocalizationInitialization")} skills.txt";
            Common.Skills = Skills.Initialize();
            Status = $"{Application.GetResource("LocalizationDecompilation")} -- {Application.GetResource("LocalizationInitialization")} meshes.txt";
            Common.Meshes = Meshes.Initialize();
            Status = $"{Application.GetResource("LocalizationDecompilation")} -- {Application.GetResource("LocalizationInitialization")} variables.txt";
            Common.Variables = Scripts.InitializeVariables();
            Status = $"{Application.GetResource("LocalizationDecompilation")} -- {Application.GetResource("LocalizationInitialization")} dialog_states.txt";
            Common.DialogStates = Dialogs.Initialize();
            Status = $"{Application.GetResource("LocalizationDecompilation")} -- {Application.GetResource("LocalizationInitialization")} scenes.txt";
            Common.Scenes = Scenes.Initialize();
            Status = $"{Application.GetResource("LocalizationDecompilation")} -- {Application.GetResource("LocalizationInitialization")} mission_templates.txt";
            Common.MissionTemplates = MissionTemplates.Initialize();
            Status = $"{Application.GetResource("LocalizationDecompilation")} -- {Application.GetResource("LocalizationInitialization")} particle_systems.txt";
            Common.ParticleSystems = ParticleSystems.Initialize();
            Status = $"{Application.GetResource("LocalizationDecompilation")} -- {Application.GetResource("LocalizationInitialization")} scene_props.txt";
            Common.SceneProps = SceneProps.Initialize();
            Status = $"{Application.GetResource("LocalizationDecompilation")} -- {Application.GetResource("LocalizationInitialization")} map_icons.txt";
            Common.MapIcons = MapIcons.Initialize();
            Status = $"{Application.GetResource("LocalizationDecompilation")} -- {Application.GetResource("LocalizationInitialization")} presentations.txt";
            Common.Presentations = Presentations.Initialize();
            Status = $"{Application.GetResource("LocalizationDecompilation")} -- {Application.GetResource("LocalizationInitialization")} tableau_materials.txt";
            Common.Tableaus = TableauMaterials.Initialize();
            Status = $"{Application.GetResource("LocalizationDecompilation")} -- {Application.GetResource("LocalizationInitialization")} actions.txt";
            Common.Animations = Common.IsVanillaMode ? Vanilla.Animations.GetIdFromFile(Common.InputPath + @"\actions.txt") : Animations.Initialize();
            Status = $"{Application.GetResource("LocalizationDecompilation")} -- {Application.GetResource("LocalizationInitialization")} music.txt";
            Common.Music = Music.Initialize();
            Status = $"{Application.GetResource("LocalizationDecompilation")} -- {Application.GetResource("LocalizationInitialization")} skins.txt";
            Common.Skins = Skins.Initialize();
            Status = Application.GetResource("LocalizationDecompilation");
        }

        private static void ProcessFile(string strFileName)
        {
            if (!File.Exists(Path.Combine(Common.InputPath, strFileName)))
            {
                Window.Print(Application.GetResource("LocalizationFileNotFound2") + "\n", Common.InputPath, strFileName);
                return;
            }

            var sw = Stopwatch.StartNew();
            var dblTime = sw.ElapsedTicks * 1000.0 / Stopwatch.Frequency;
            
            var fInput = new Text(Path.Combine(Common.InputPath, strFileName));
            var strFirstString = fInput.GetString();
            if (strFirstString == null)
            {
                Window.Print(Application.GetResource("LocalizationUnknownFormat") + "\n");
                return;
            }
            var bFirstNumber = Int32.TryParse(strFirstString, out var _);
            fInput.Close();

            if (strFirstString == "scriptsfile version 1")
                Scripts.Decompile();
            else if (strFirstString == "triggersfile version 1")
                Triggers.Decompile();
            else if (strFirstString == "simple_triggers_file version 1")
                SimpleTriggers.Decompile();
            else if (strFirstString == "dialogsfile version 2") //Warband dialogs file
                Dialogs.Decompile();
            else if (strFirstString == "dialogsfile version 1") //M&B v1.011/v1.010 dialogs file
                Vanilla.Dialogs.Decompile();
            else if (strFirstString == "menusfile version 1")
                Menus.Decompile();
            else if (strFirstString == "factionsfile version 1")
                Factions.Decompile();
            else if (strFirstString == "infopagesfile version 1")
                InfoPages.Decompile();
            else if (strFirstString == "itemsfile version 3") //Warband items file
                Items.Decompile();
            else if (strFirstString == "itemsfile version 2") //M&B v1.011/v1.010 items file
                Vanilla.Items.Decompile();
            else if (strFirstString == "map_icons_file version 1")
                MapIcons.Decompile();
            else if (strFirstString == "missionsfile version 1")
                MissionTemplates.Decompile();
            else if (strFirstString == "particle_systemsfile version 1")
                ParticleSystems.Decompile();
            else if (strFirstString == "partiesfile version 1")
                Parties.Decompile();
            else if (strFirstString == "partytemplatesfile version 1")
                PartyTemplates.Decompile();
            else if (strFirstString == "postfx_paramsfile version 1")
                Postfx.Decompile();
            else if (strFirstString == "presentationsfile version 1")
                Presentations.Decompile();
            else if (strFirstString == "questsfile version 1")
                Quests.Decompile();
            else if (strFirstString == "scene_propsfile version 1")
                SceneProps.Decompile();
            else if (strFirstString == "scenesfile version 1")
                Scenes.Decompile();
            else if (strFirstString == "skins_file version 1" && Common.SelectedMode == Mode.Caribbean) //Caribbean skins file
                Caribbean.Skins.Decompile();
            else if (strFirstString == "skins_file version 1") //Warband skins file
                Skins.Decompile();
            else if (strFirstString == "soundsfile version 3") //Warband sounds file
                Sounds.Decompile();
            else if (strFirstString == "soundsfile version 2") //M&B v1.011/v1.010 sounds file
                Vanilla.Sounds.Decompile();
            else if (strFirstString == "stringsfile version 1")
                Strings.Decompile();
            else if (strFirstString == "troopsfile version 2") //Warband troops file
                Troops.Decompile();
            else if (strFirstString == "troopsfile version 1") //M&B v1.011/v1.010 troops file
                Vanilla.Troops.Decompile();
            else if (bFirstNumber && strFileName == "tableau_materials.txt")
                TableauMaterials.Decompile();
            else if (bFirstNumber && strFileName == "skills.txt")
                Skills.Decompile();
            else if (bFirstNumber && strFileName == "music.txt")
                Music.Decompile();
            else if (bFirstNumber && strFileName == "actions.txt")
            {
                if (Common.IsVanillaMode)
                    Vanilla.Animations.Decompile();
                else
                    Animations.Decompile();
            }
            else if (bFirstNumber && strFileName == "meshes.txt")
                Meshes.Decompile();
            else if (bFirstNumber && strFileName == "flora_kinds.txt")
                Flora.Decompile();
            else if (strFileName == "ground_specs.txt")
                GroundSpecs.Decompile();
            else if (bFirstNumber && strFileName == "skyboxes.txt")
                Skyboxes.Decompile();
            else 
                Window.Print(Application.GetResource("LocalizationUnknownFormat") + "\n"); 

            Window.Print(Application.GetResource("LocalizationFileTime") + "\n", strFileName, sw.ElapsedTicks * 1000.0 / Stopwatch.Frequency - dblTime);
        }

        private static string GetSingleFileName()
        {
            string result = null;
            Window.SourcePathTextBox.Dispatcher.Invoke(() => result = Path.GetFileName(Window.SourcePathTextBox.Text));
            return result;
        }

        public static string GetShadersFullFileName(out bool founded)
        {
            founded = true;
            if (File.Exists(Path.Combine(Common.InputPath, "mb_2a.fxo"))) return Path.Combine(Common.InputPath, "mb_2a.fxo");
            if (File.Exists(Path.Combine(Common.InputPath, "mb_2b.fxo"))) return Path.Combine(Common.InputPath, "mb_2b.fxo");
            if (File.Exists(Path.Combine(Common.InputPath, "mb.fx"))) return Path.Combine(Common.InputPath, "mb.fx");
            founded = false;
            return "";
        }

        private static void ProcessSingleFile()
        {
            var strFileName = GetSingleFileName();
            Common.NeedId = false;

            var ext = Path.GetExtension(strFileName);
            if (ext == ".fx" || ext == ".fxo")
            {
                var sw = Stopwatch.StartNew();
                var dblTime = sw.ElapsedTicks * 1000.0 / Stopwatch.Frequency;
                Shaders.Shaders.Decompile(Common.InputPath + @"\" + strFileName);
                Window.Print(Application.GetResource("LocalizationFileTime") + "\n", strFileName, sw.ElapsedTicks * 1000.0 / Stopwatch.Frequency - dblTime);
                return;
            }

            string[] strModFiles = { "actions.txt", "conversation.txt", "factions.txt", "info_pages.txt", "item_kinds1.txt", "map_icons.txt",
            "menus.txt", "meshes.txt", "mission_templates.txt", "music.txt", "particle_systems.txt", "parties.txt", "party_templates.txt",
            "postfx.txt", "presentations.txt", "quests.txt", "scene_props.txt", "scenes.txt", "scripts.txt", "simple_triggers.txt",
            "skills.txt", "skins.txt", "sounds.txt", "strings.txt", "tableau_materials.txt", "triggers.txt", "troops.txt",
            "flora_kinds.txt", "ground_specs.txt", "skyboxes.txt" };

            string strFileToProcess = strModFiles.FirstOrDefault(t => t == strFileName);
            ProcessFile(strFileToProcess);
        }

        private static void ProcessFullModule()
        {
            File.Copy(Path.Combine(Common.InputPath, "variables.txt"), Path.Combine(Common.OutputPath, "variables.txt"), true);

            var decompileShaders = Window.DecompileShadersCheckBox.IsChecked();

            if (!Common.IsVanillaMode)
                Win32FileWriter.WriteAllText(Path.Combine(Common.OutputPath, "module_constants.py"), Header.Standard + Common.ModuleConstantsText);
            else
                Win32FileWriter.WriteAllText(Path.Combine(Common.OutputPath, "module_constants.py"), Header.Standard + Common.ModuleConstantsVanillaText);

            string[] strModFiles = { "actions.txt", "conversation.txt", "factions.txt", "info_pages.txt", "item_kinds1.txt", "map_icons.txt",
            "menus.txt", "meshes.txt", "mission_templates.txt", "music.txt", "particle_systems.txt", "parties.txt", "party_templates.txt",
            "postfx.txt", "presentations.txt", "quests.txt", "scene_props.txt", "scenes.txt", "scripts.txt", "simple_triggers.txt",
            "skills.txt", "skins.txt", "sounds.txt", "strings.txt", "tableau_materials.txt", "triggers.txt", "troops.txt" };
            string[] strModDataFiles = { "flora_kinds.txt", "ground_specs.txt", "skyboxes.txt" };

            int iNumFiles = strModFiles.Length;
            if (Common.IsVanillaMode) iNumFiles -= 2;

            iNumFiles += strModDataFiles.Count(strModDataFile => File.Exists(Path.Combine(Common.InputPath, "Data", strModDataFile)));

            var sShadersFile = GetShadersFullFileName(out bool b);
            if (b && decompileShaders) iNumFiles++;
            
            double dblProgressForOneFile = 100.0 / iNumFiles, dblProgress = 0;
            
            foreach (var strModFile in strModFiles.Where(strModFile => !(Common.IsVanillaMode && (strModFile == "info_pages.txt" || strModFile == "postfx.txt"))))
            {
                ProcessFile(strModFile);
                dblProgress += dblProgressForOneFile;
                Status = $"{Application.GetResource("LocalizationDecompilation")} {dblProgress:F2}%";
            }

            if (b && decompileShaders)
            {
                ProcessShaders(sShadersFile);
                dblProgress += dblProgressForOneFile;
                Status = $"{Application.GetResource("LocalizationDecompilation")} {dblProgress:F2}%";
            }

            Common.InputPath = Path.Combine(Common.InputPath, "Data");

            foreach (var strModDataFile in strModDataFiles.Where(strModDataFile => File.Exists(Common.InputPath + @"\" + strModDataFile)))
            {
                ProcessFile(strModDataFile);
                dblProgress += dblProgressForOneFile;
                Status = $"{Application.GetResource("LocalizationDecompilation")} {dblProgress:F2}%";
            }
        }

        private static void ProcessShaders(string sShadersFile)
        {
            var sw = Stopwatch.StartNew();
            Shaders.Shaders.Decompile(sShadersFile);
            Window.Print(Application.GetResource("LocalizationFileTime") + "\n", Path.GetFileName(sShadersFile), sw.ElapsedTicks * 1000.0 / Stopwatch.Frequency);
        }
        
        private static readonly SimpleTrie<Action> InitializeTrie = new SimpleTrie<Action>
        {
            ["actions.txt"] = () => { },
            ["conversation.txt"] = () => { InitializeOpCodes(); InitializeModuleData(); },
            ["factions.txt"] = () => { },
            ["info_pages.txt"] = () => { },
            ["item_kinds1.txt"] = () => { InitializeOpCodes(); InitializeModuleData(); },
            ["map_icons.txt"] = () => { },
            ["menus.txt"] = () => { InitializeOpCodes(); InitializeModuleData(); },
            ["meshes.txt"] = () => { },
            ["mission_templates.txt"] = () => { InitializeOpCodes(); InitializeModuleData(); },
            ["music.txt"] = () => { },
            ["particle_systems.txt"] = () => { },
            ["parties.txt"] = () => {
                Status = $"{Application.GetResource("LocalizationDecompilation")} -- {Application.GetResource("LocalizationInitialization")} troops.txt";
                Common.Troops = Text.GetFirstStringFromFile(Common.InputPath + @"\troops.txt") == "troopsfile version 1"
                    ? Vanilla.Troops.GetIdFromFile(Common.InputPath + @"\troops.txt") : Troops.Initialize();
                Status = $"{Application.GetResource("LocalizationDecompilation")} -- {Application.GetResource("LocalizationInitialization")} factions.txt";
                Common.Factions = Factions.Initialize();
                Status = $"{Application.GetResource("LocalizationDecompilation")} -- {Application.GetResource("LocalizationInitialization")} map_icons.txt";
                Common.MapIcons = MapIcons.Initialize();
            },
            ["party_templates.txt"] = () => {
                Status = $"{Application.GetResource("LocalizationDecompilation")} -- {Application.GetResource("LocalizationInitialization")} troops.txt";
                Common.Troops = Text.GetFirstStringFromFile(Common.InputPath + @"\troops.txt") == "troopsfile version 1"
                    ? Vanilla.Troops.GetIdFromFile(Common.InputPath + @"\troops.txt") : Troops.Initialize();
                Status = $"{Application.GetResource("LocalizationDecompilation")} -- {Application.GetResource("LocalizationInitialization")} factions.txt";
                Common.Factions = Factions.Initialize();
                Status = $"{Application.GetResource("LocalizationDecompilation")} -- {Application.GetResource("LocalizationInitialization")} map_icons.txt";
                Common.MapIcons = MapIcons.Initialize();
            },
            ["postfx.txt"] = () => { },
            ["presentations.txt"] = () => { InitializeOpCodes(); InitializeModuleData(); },
            ["quests.txt"] = () => { },
            ["scene_props.txt"] = () => { InitializeOpCodes(); InitializeModuleData(); },
            ["scenes.txt"] = () => { },
            ["scripts.txt"] = () => { InitializeOpCodes(); InitializeModuleData(); },
            ["simple_triggers.txt"] = () => { InitializeOpCodes(); InitializeModuleData(); },
            ["skills.txt"] = () => { },
            ["skins.txt"] = () => { },
            ["sounds.txt"] = () => { },
            ["strings.txt"] = () => { },
            ["tableau_materials.txt"] = () => { InitializeOpCodes(); InitializeModuleData(); },
            ["triggers.txt"] = () => { InitializeOpCodes(); InitializeModuleData(); },
            ["troops.txt"] = () => {
                Status = $"{Application.GetResource("LocalizationDecompilation")} -- {Application.GetResource("LocalizationInitialization")} item_kinds1.txt";
                Common.Items = Text.GetFirstStringFromFile(Common.InputPath + @"\item_kinds1.txt") == "itemsfile version 2"
                    ? Vanilla.Items.GetIdFromFile(Common.InputPath + @"\item_kinds1.txt") : Items.Initialize();
                Status = $"{Application.GetResource("LocalizationDecompilation")} -- {Application.GetResource("LocalizationInitialization")} scenes.txt";
                Common.Scenes = Scenes.Initialize();
            }
        };
    }
}
