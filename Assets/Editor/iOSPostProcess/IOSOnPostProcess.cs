using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Editor.iOSPostProcess
{
    public class IOSOnPostProcess : IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;
        const string TrackingDescription
            = "Your data will be used to provide you a better and personalized ad experience.";

        public async void OnPostprocessBuild(BuildReport report)
        {
            var outputPath = report.summary.outputPath;
            if (report.summary.platform == BuildTarget.iOS) // Check if the build is for iOS 
            {
                AddPListValues(outputPath);
                ExemptFromEncryptionSet(report);
                RemoveBitcodeSupport(outputPath);
                await ExecutePodInstall();
            }
        }
        
        static void AddPListValues(string pathToXcode) {
            // Retrieve the plist file from the Xcode project directory:
            string plistPath = pathToXcode + "/Info.plist";
            PlistDocument plistObj = new PlistDocument();
 
 
            // Read the values from the plist file:
            plistObj.ReadFromString(File.ReadAllText(plistPath));
 
            // Set values from the root object:
            PlistElementDict plistRoot = plistObj.root;
 
            // Set the description key-value in the plist:
            plistRoot.SetString("NSUserTrackingUsageDescription", TrackingDescription);
 
            // Save changes to the plist:
            File.WriteAllText(plistPath, plistObj.WriteToString());
        }

        /// <summary>
        ///     Removes bitcode support. This bitcode behaviour deprecated depend on APPLE.
        /// </summary>
        /// <param name="outputPath"></param>
        private static void RemoveBitcodeSupport(string outputPath)
        {
            var projPath = PBXProject.GetPBXProjectPath(outputPath);

            var project = new PBXProject();
            project.ReadFromFile(projPath);

            var mainTargetGuid = project.GetUnityMainTargetGuid();

            foreach (var targetGuid in new[] { mainTargetGuid, project.GetUnityFrameworkTargetGuid() })
            {
                project.SetBuildProperty(targetGuid, "ENABLE_BITCODE", "NO");
            }

            project.WriteToFile(projPath);
        }


        /// <summary>
        ///     Insert necessary scripts to pod file
        /// </summary>
        /// <param name="pathToXcode">Xcode Path</param>
        public static Task InsertToPodFile(string pathToXcode)
        {
            var podPath = pathToXcode + "/Podfile";
            var text = File.ReadAllLines(podPath).ToList();

            var alltext = File.ReadAllText(podPath);
            var pattern = @"\b" + Regex.Escape("post_install") + @"\b";
            var match = Regex.Match(alltext, pattern, RegexOptions.IgnoreCase);

            if (!match.Success)
            {
                text.Insert(text.Count, @"
post_install do |installer|
  installer.generated_projects.each do |target|
    target.build_configurations.each do |config|
      config.build_settings['CODE_SIGNING_ALLOWED'] = ""NO""
      config.build_settings['CODE_SIGNING_REQUIRED'] = ""NO""
      config.build_settings['EXPANDED_CODE_SIGN_IDENTITY'] = """"
    end
  end

  bitcode_strip_path = `xcrun --find bitcode_strip`.chomp

  def strip_bitcode_from_framework(bitcode_strip_path, framework_relative_path)
    framework_path = File.join(Dir.pwd, framework_relative_path)
    command = ""#{bitcode_strip_path} #{framework_path} -r -o #{framework_path}""
    puts ""Stripping bitcode: #{command}""
    system(command)
  end

  installer.pods_project.targets.each do |target|
    target.build_configurations.each do |config|
      config.build_settings[""ONLY_ACTIVE_ARCH""] = ""NO""
    end

    if ['FBAEMKit', 'FBSDKCoreKit', 'FBSDKCoreKit_Basics', 'FBSDKGamingServicesKit', 'FBSDKLoginKit', 'FBSDKShareKit', 'OneSignalXCFramework'].include?(target.name)
      if target.name == 'OneSignalXCFramework'
        device_framework_path = ""Pods/OneSignalXCFramework/iOS_SDK/OneSignalSDK/OneSignal_XCFramework/OneSignal.xcframework/ios-arm64_armv7_armv7s/OneSignal.framework/OneSignal""
        simulator_framework_path = ""Pods/OneSignalXCFramework/iOS_SDK/OneSignalSDK/OneSignal_XCFramework/OneSignal.xcframework/ios-arm64_i386_x86_64-simulator/OneSignal.framework/OneSignal""
      else
        device_framework_path = ""Pods/#{target.name}/XCFrameworks/#{target.name}.xcframework/ios-arm64/#{target.name}.framework/#{target.name}""
        simulator_framework_path = ""Pods/#{target.name}/XCFrameworks/#{target.name}.xcframework/ios-arm64_x86_64-simulator/#{target.name}.framework/#{target.name}""
      end

      strip_bitcode_from_framework(bitcode_strip_path, device_framework_path)
      strip_bitcode_from_framework(bitcode_strip_path, simulator_framework_path)
    end
  end
end
");
                File.WriteAllLines(podPath, text.ToArray());
            }
            return Task.CompletedTask;
        }


        private static void ExemptFromEncryptionSet(BuildReport report)
        {
            var plistPath = report.summary.outputPath + "/Info.plist";

            var plist = new PlistDocument(); // Read Info.plist file into memory
            plist.ReadFromString(File.ReadAllText(plistPath));

            var rootDict = plist.root;
            rootDict.SetBoolean("ITSAppUsesNonExemptEncryption", false);

            File.WriteAllText(plistPath, plist.WriteToString()); // Override Info.plist
        }

        public static async Task ExecutePodInstall()
        {
            // Pod install execute needs after build for .xcworkspace requirements
            var iosBuildPath = Path.GetFullPath(Path.Combine(Application.dataPath, @"../Builds/iOS"));
            await InsertToPodFile(iosBuildPath);
            var argument = $"cd {iosBuildPath} && export LANG=en_US.UTF-8 && /opt/homebrew/bin/pod install";
            await RunCommandOSX(argument);
        }

        private static Task<int> RunCommandOSX(string command)
        {
            return Task.Run(() =>
            {
                using (var process = Process.Start(
                           new ProcessStartInfo("/bin/zsh", "-c \"" + command.Replace("\"", "\"\"") + "\"")
                           {
                               UseShellExecute = false,
                               RedirectStandardError = true,
                               CreateNoWindow = true,
                               WindowStyle = ProcessWindowStyle.Hidden
                           }))
                {
                    Debug.Log($"Process Id: {process.Id}");
                    var errorOutput = process.StandardError.ReadToEnd();
                    process.WaitForExit();
                    if (!string.IsNullOrEmpty(errorOutput))
                    {
                        Debug.LogError($"Command '{command}' failed:" + Environment.NewLine + errorOutput);
                    }

                    return process.ExitCode;
                }
            });
        }
    }
}
