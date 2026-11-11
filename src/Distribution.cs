using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Win32;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;

namespace StickyNotes;
internal static class Distribution {
 public const string StartupTaskId="StickyNotesStartup";
 public static string Version=>typeof(NotesApp).Assembly.GetName().Version?.ToString(3)??"1.2.0";
 public static bool IsPackaged {get{uint n=0;return GetCurrentPackageFullName(ref n,IntPtr.Zero)!=15700;}}
 public static bool IsStartupActivation(string[] args){if(Array.IndexOf(args,"--startup")>=0)return true;if(!IsPackaged)return false;try{return AppInstance.GetActivatedEventArgs()?.Kind==ActivationKind.StartupTask;}catch{return false;}}
 public static async Task<bool> StartupEnabled(){if(IsPackaged){var task=await StartupTask.GetAsync(StartupTaskId);return task.State is StartupTaskState.Enabled or StartupTaskState.EnabledByPolicy;}using var k=Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");return k?.GetValue("WindowsStickyNotes") is string value&&value.Trim()=="\""+Environment.ProcessPath+"\" --startup";}
 public static async Task SetStartup(bool enabled){if(IsPackaged){var task=await StartupTask.GetAsync(StartupTaskId);if(!enabled){task.Disable();return;}var state=await task.RequestEnableAsync();if(state is not StartupTaskState.Enabled and not StartupTaskState.EnabledByPolicy)throw new InvalidOperationException("O Windows controla a inicialização. Ative o aplicativo em Configurações > Aplicativos > Inicialização.");return;}using var k=Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");if(enabled)k.SetValue("WindowsStickyNotes","\""+Environment.ProcessPath+"\" --startup");else k.DeleteValue("WindowsStickyNotes",false);}
 [DllImport("kernel32.dll",CharSet=CharSet.Unicode)]static extern int GetCurrentPackageFullName(ref uint length,IntPtr name);
}
