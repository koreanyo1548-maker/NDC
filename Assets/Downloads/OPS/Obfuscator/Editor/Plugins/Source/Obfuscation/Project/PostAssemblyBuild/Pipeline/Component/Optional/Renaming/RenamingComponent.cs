// System
using System;

// OPS - Settings
using OPS.Editor.Settings.File;

// OPS - Obfuscator - Components
using OPS.Obfuscator.Editor.Component.Gui;

// OPS - Gui
using OPS.Editor.Gui;

// OPS - Project
using OPS.Editor.Project.Step;

// OPS - Obfuscator - Gui
using OPS.Obfuscator.Editor.Gui;

// OPS - Obfuscator - PostAssemblyBuild
using OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Renaming.Charset;

namespace OPS.Obfuscator.Editor.Project.PostAssemblyBuild.Pipeline.Component.Optional
{
    /// <summary>
    /// The renaming component allows to manage the renaming pattern and mapping.
    /// </summary>
    public class RenamingComponent : APostAssemblyBuildComponent, IGuiComponent
    {
        // Name
        #region Name

        /// <summary>
        /// Name of the component.
        /// </summary>
        public override String Name
        {
            get
            {
                return "Renaming - Settings";
            }
        }

        #endregion

        // Description
        #region Description

        /// <summary>
        /// Description, descriping what this component does.
        /// </summary>
        public override string Description
        {
            get
            {
                return "Manage here the renaming settings.";
            }
        }

        #endregion

        // Short Description
        #region Short Description

        /// <summary>
        /// Short description, descriping short what this component does.
        /// </summary>
        public override String ShortDescription
        {
            get
            {
                return "Manage here the renaming settings.";
            }
        }

        #endregion

        // Settings
        #region Settings

        // Constants - Component
        public const String CSettingsKey = "Default_Obfuscation_Component_Renaming";

        /// <summary>
        /// The settings key for this component in the obfuscator settings.
        /// </summary>
        public String SettingsKey { get; } = CSettingsKey;

        // Constants - Component - Elements

        // Constants - Component - Elements - Pattern
        public const String CActive_Renaming_Pattern = "Active_Renaming_Pattern";
        public const String CCustom_Renaming_Pattern = "Custom_Renaming_Pattern";

        //Constants - Component - Elements - Mapping
        public const String CEnable_Load_Mapping = "Enable_Load_Mapping";
        public const String CLoad_Mapping_FilePath = "Load_Mapping_FilePath";
        public const String CLoad_Mapping_Endpoint = "Load_Mapping_Endpoint";
        public const String CEnable_Save_Mapping = "Enable_Save_Mapping";
        public const String CSave_Mapping_FilePath = "Save_Mapping_FilePath";
        public const String CSave_Mapping_Endpoint = "Save_Mapping_Endpoint";

        #endregion

        // Gui
        #region Gui

        /// <summary>
        /// Category this Component belongs too.
        /// </summary>
        public EObfuscatorCategory ObfuscatorCategory
        {
            get
            {
                return EObfuscatorCategory.Optional;
            }
        }

        // Rows
        private Row_DropDown_Enum<ECharset> active_RenamingMapping_Row;
        private Row_TextBox custom_RenamingMapping_Row;

        public ObfuscatorContainer GetGuiContainer(ComponentSettings _ComponentSettings)
        {
            // Header
            ObfuscatorHeader var_Header = new ObfuscatorHeader(this.Name);

            // Description
            ObfuscatorDescription var_Description = new ObfuscatorDescription(this.ShortDescription);

            // Content
            ObfuscatorContent var_Content = new ObfuscatorContent();

            // Renaming Pattern
            this.active_RenamingMapping_Row = new Row_DropDown_Enum<ECharset>("Active renaming pattern:", _ComponentSettings, CActive_Renaming_Pattern, this.OnActiveRenamingMappingChanged);
            this.active_RenamingMapping_Row.Notification_Info = "Select one of the predefined renaming pattern, or use a custom one.";
            this.active_RenamingMapping_Row.Notification_Warning = "Some characters may not be valid for some build targets. For example, when building for IOS, an XCode project is created with file names based on class files. If some of the characters are not valid characters on the Mac platform, you will get errors. The same applies if you are building to IL2CPP. The default renaming pattern will always work.";
            var_Content.AddRow(this.active_RenamingMapping_Row, true);

            this.custom_RenamingMapping_Row = new Row_TextBox("Custom renaming pattern: ", _ComponentSettings, CCustom_Renaming_Pattern);
            this.custom_RenamingMapping_Row.Notification_Info = "Enter the characters you want to use as a user-defined renaming pattern here. Do not insert any delimiters between the characters! The standard renaming pattern is, for example: abcdefghijklmnopqrstuvwxyz. You can change the pattern as often as you like, it is also independent of the renaming assignment.";
            var_Content.AddRow(this.custom_RenamingMapping_Row, true);

            // Use Load Mapping
            Row_Boolean var_UseLoadMapping_Row = new Row_Boolean("Load an obfuscation mapping file: ", _ComponentSettings, CEnable_Load_Mapping);
            var_UseLoadMapping_Row.Notification_Info = "Activate this setting to load an obfuscation mapping from a file. Define a file path below. Recommended if you want to obfuscate serializable classes/fields/... .";
            var_Content.AddRow(var_UseLoadMapping_Row);

            Row_OpenFileSelect var_LoadMapping_File_Row = new Row_OpenFileSelect("Load mapping file path: ", _ComponentSettings, CLoad_Mapping_FilePath);
            var_LoadMapping_File_Row.Notification_Info = "Enter here a file path you want to load the mapping from.";
            var_Content.AddRow(var_LoadMapping_File_Row);

            Row_TextBox var_LoadMapping_Endpoint_Row = new Row_TextBox("Load mapping from endpoint: ", _ComponentSettings, CLoad_Mapping_Endpoint);
            var_LoadMapping_Endpoint_Row.Notification_Info = "Optional, you can load a mapping from an endpoint. Enter here the endpoint you want to load the mapping from. The endpoint has to return a mapping file as json. The endpoint may contain a {version} placeholder, which will be replaced by the current version (Application.version) of the application. Only applies if the 'mapping file path' setting is empty.";
            var_Content.AddRow(var_LoadMapping_Endpoint_Row);

            // Use Save Mapping
            Row_Boolean var_UseSaveMapping_Row = new Row_Boolean("Save an obfuscation mapping file: ", _ComponentSettings, CEnable_Save_Mapping);
            var_UseSaveMapping_Row.Notification_Info = "Activate this setting to save an obfuscation mapping to a file. Define a file path below. Recommended if you want to obfuscate serializable classes/fields/... .";
            var_Content.AddRow(var_UseSaveMapping_Row);

            Row_SaveFileSelect var_SaveMapping_File_Row = new Row_SaveFileSelect("Save mapping file path: ", _ComponentSettings, CSave_Mapping_FilePath);
            var_SaveMapping_File_Row.Notification_Info = "Enter here a file path you want to save the mapping to.";
            var_Content.AddRow(var_SaveMapping_File_Row);

            Row_TextBox var_SaveMapping_Endpoint_Row = new Row_TextBox("Save mapping to endpoint: ", _ComponentSettings, CSave_Mapping_Endpoint);
            var_SaveMapping_Endpoint_Row.Notification_Info = "Additionally, you can save a mapping to an endpoint. Enter here the endpoint you want to save the mapping to. The endpoint has to accept a mapping file as json. The endpoint may contain a {version} placeholder, which will be replaced by the current version (Application.version) of the application. Applies in addition to the 'mapping file path' setting.";
            var_Content.AddRow(var_SaveMapping_Endpoint_Row);

            // Container
            ObfuscatorContainer var_ObfuscatorContainer = new ObfuscatorContainer(var_Header, var_Description, var_Content, false);

            return var_ObfuscatorContainer;
        }

        private void OnActiveRenamingMappingChanged()
        {
            this.custom_RenamingMapping_Row.Enabled = (ECharset)this.active_RenamingMapping_Row.RowContent == ECharset.Custom;
        }

        #endregion

        // On Pre Pipeline Process
        #region On Pre Pipeline Process

        /// <summary>
        /// Before the pipeline process, load the renaming pattern and mapping.
        /// </summary>
        /// <param name="_StepInput">The step input.</param>
        /// <returns>The success of the operation.</returns>
        public override bool OnPrePipelineProcess(IStepInput _StepInput)
        {
            // Load renaming pattern!
            ACharset var_Charset = new DefaultCharset();

#if Obfuscator_Free
#else
            String var_Enum_Charset_String_Value = this.Step.Settings.Get_ComponentSettings_As_String(this.SettingsKey, CActive_Renaming_Pattern);

            try
            {
                ECharset var_Enum_Charset_Value = (ECharset)Enum.Parse(typeof(ECharset), var_Enum_Charset_String_Value);

                switch (var_Enum_Charset_Value)
                {
                    case ECharset.Default:
                        {
                            break;
                        }
                    case ECharset.Unicode:
                        {
                            var_Charset = new UnicodeCharset();
                            break;
                        }
                    case ECharset.Common_Chinese:
                        {
                            var_Charset = new ChineseCharset();
                            break;
                        }
                    case ECharset.Common_Korean:
                        {
                            var_Charset = new KoreanCharset();
                            break;
                        }
                    case ECharset.Custom:
                        {
                            String var_Custom_Pattern_String_Value = this.Step.Settings.Get_ComponentSettings_As_String(this.SettingsKey, CCustom_Renaming_Pattern);

                            var_Charset = new CustomCharset(var_Custom_Pattern_String_Value);
                            break;
                        }
                }
            }
            catch(Exception e)
            {
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, "Failed to load Charset: " + var_Enum_Charset_String_Value + ". Using default! Exception: " + e.ToString());

                // Failed and so use default.
                var_Charset = new DefaultCharset();
            }
#endif

            // Set Charset.
            this.DataContainer.RenameManager.NameGenerator.UseCharSet(var_Charset);

            // Check if should load a mapping.
            if (!this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CEnable_Load_Mapping))
            {
                return true;
            }

            // Get the mapping file path.
            String var_Mapping_FilePath = this.Step.Settings.Get_ComponentSettings_As_String(this.SettingsKey, CLoad_Mapping_FilePath);

            // Check if the mapping file path is empty.
            if (String.IsNullOrEmpty(var_Mapping_FilePath))
            {
                // Get the mapping endpoint.
                String var_Mapping_Endpoint = this.Step.Settings.Get_ComponentSettings_As_String(this.SettingsKey, CLoad_Mapping_Endpoint);

                // Check if the mapping endpoint is empty.
                if (String.IsNullOrEmpty(var_Mapping_Endpoint))
                {
                    return true;
                }

                // Load mapping from endpoint.
                this.DataContainer.RenameManager.LoadMappingFromUrl(var_Mapping_Endpoint);
            }
            else
            {
                // Load mapping from file.
                this.DataContainer.RenameManager.LoadMappingFromFile(var_Mapping_FilePath);
            }

            return true;
        }

        #endregion

        // On Post Pipeline Proces
        #region On Post Pipeline Proces

        /// <summary>
        /// After the pipeline process, save the mapping.
        /// </summary>
        /// <param name="_StepOutput">The step output.</param>
        /// <returns>The success of the operation.</returns>
        public override bool OnPostPipelineProcess(IStepOutput _StepOutput)
        {
            // Check if should save a mapping.
            if (!this.Step.Settings.Get_ComponentSettings_As_Bool(this.SettingsKey, CEnable_Save_Mapping))
            {
                return true;
            }

            // Try to save the mapping. If this fails, store it in the obfuscator temp directory.
            try
            {

                // Get the mapping file path.
                String var_Mapping_FilePath = this.Step.Settings.Get_ComponentSettings_As_String(this.SettingsKey, CSave_Mapping_FilePath);

                // Check if the mapping file path is not empty.
                if (!String.IsNullOrEmpty(var_Mapping_FilePath))
                {
                    // Save the mapping to the file.
                    this.DataContainer.RenameManager.SaveMappingToFile(var_Mapping_FilePath);
                }

                // Get the mapping endpoint.
                String var_Mapping_Endpoint = this.Step.Settings.Get_ComponentSettings_As_String(this.SettingsKey, CSave_Mapping_Endpoint);

                // Check if the mapping endpoint is not empty.
                if (!String.IsNullOrEmpty(var_Mapping_Endpoint))
                {
                    // Save the mapping to the endpoint.
                    this.DataContainer.RenameManager.SaveMappingToUrl(var_Mapping_Endpoint);
                }
            }
            catch (Exception e)
            {
                // Store the mapping in the obfuscator temp directory.
                this.DataContainer.RenameManager.SaveMappingToTempPath();

                // Log - Failed to save the mapping.
                Obfuscator.Report.Append(OPS.Editor.Report.EReportLevel.Warning, "Failed to save the mapping at your entered location! Storing it in the obfuscator temp directory. Exception: " + e.ToString(), true);
            }

            return true;
        }

        #endregion
    }
}
