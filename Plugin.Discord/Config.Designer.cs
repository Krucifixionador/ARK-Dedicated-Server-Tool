﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ArkServerManager.Plugin.Discord {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "15.1.0.0")]
    internal sealed partial class Config : global::System.Configuration.ApplicationSettingsBase {
        
        private static Config defaultInstance = ((Config)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Config())));
        
        public static Config Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("http://arkservermanager.azurewebsites.net/api/plugin/call/{0}/{1}")]
        public string PluginCallUrlFormat {
            get {
                return ((string)(this["PluginCallUrlFormat"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("http://whatismyip.akamai.com/")]
        public string PublicIPCheckUrl {
            get {
                return ((string)(this["PublicIPCheckUrl"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("http://arkservermanager.azurewebsites.net/downloads/release/ArkServerManager.Plug" +
            "in.Discord.zip")]
        public string LatestDownloadUrl {
            get {
                return ((string)(this["LatestDownloadUrl"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("http://arkservermanager.azurewebsites.net/downloads/release/ArkServerManager.Plug" +
            "in.Discord.txt")]
        public string LatestVersionUrl {
            get {
                return ((string)(this["LatestVersionUrl"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("http://arkservermanager.azurewebsites.net/downloads/beta/ArkServerManager.Plugin." +
            "Discord.zip")]
        public string LatestBetaDownloadUrl {
            get {
                return ((string)(this["LatestBetaDownloadUrl"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("http://arkservermanager.azurewebsites.net/downloads/beta/ArkServerManager.Plugin." +
            "Discord.txt")]
        public string LatestBetaVersionUrl {
            get {
                return ((string)(this["LatestBetaVersionUrl"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("ArkServerManager.Plugin.Discord.zip")]
        public string PluginZipFilename {
            get {
                return ((string)(this["PluginZipFilename"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("12")]
        public int CallHomeDelay {
            get {
                return ((int)(this["CallHomeDelay"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("5000")]
        public int RequestTimeout {
            get {
                return ((int)(this["RequestTimeout"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("E0CF2C1F-17B7-45E1-A2C9-2718493D0873")]
        public string PluginCode {
            get {
                return ((string)(this["PluginCode"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Discord Plugin")]
        public string PluginName {
            get {
                return ((string)(this["PluginName"]));
            }
        }
    }
}
