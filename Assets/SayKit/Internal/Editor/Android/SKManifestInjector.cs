#if UNITY_EDITOR && UNITY_ANDROID

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;

#region ReSharper

// ReSharper disable CheckNamespace
// ReSharper disable UnusedParameter.Local
// ReSharper disable InconsistentNaming

#endregion

namespace SayKitInternal
{
    public class SKManifestInjector
    {
        public static SKManifestInjector Instance { get; } = new SKManifestInjector();

        private class ManifestLine
        {
            public string Tag;
            public List<ManifestAttribute> Attributes;
            public string ParentTag;
        }

        private class ManifestAttribute
        {
            public string Name;
            public string Value;
        }

        #region Permissions

        private readonly string[] userPermissions =
        {
            "com.google.android.gms.permission.AD_ID",
            "android.permission.INTERNET",
            "android.permission.ACCESS_NETWORK_STATE",
            "android.permission.ACCESS_WIFI_STATE",
            "android.permission.VIBRATE",
            "android.permission.WAKE_LOCK"
        };

        private readonly string[] removeUserPermissions =
        {
            "android.permission.WRITE_EXTERNAL_STORAGE",
            "android.permission.READ_EXTERNAL_STORAGE",
            "android.permission.ACCESS_BACKGROUND_LOCATION",
            "android.permission.ACCESS_COARSE_LOCATION",
            "android.permission.ACCESS_FINE_LOCATION"
        };

        #endregion

        private const string AndroidSpace = "http://schemas.android.com/apk/res/android";
        private const string ToolsSpace = "http://schemas.android.com/tools";

        public void AddDependencyLinesToManifestFile(string path)
        {
            var unityLibraryManifestFile = path + "/src/main/AndroidManifest.xml";

            var manifest = XElement.Load(unityLibraryManifestFile);
            var application = manifest.Element("application");
            var parentLines = new List<ManifestLine>();

            SetApplicationAttributes(application);
            SetActivityAttributes(application);
            AddSKVersionMetaData(application);
            AddAppKeyMetaData(application);
            AddFacebookAutoLogAppEventMetaData(application);
            AddUserPermissions(parentLines);
            AddPlayGamesServicesQueries(parentLines);
            RemoveUserPermissions(manifest);
            InjectManifestLines(manifest, parentLines);
            UpdateAndroidManifestFile(manifest);
            PatchResizeableActivityAttribute(manifest);

#if SAYKIT_DEEP_LINK
            AddDeepLinkIntentFilter(application);
#endif
            manifest.Save(unityLibraryManifestFile);

            RemovePackageAttribute(path);
        }

        private static void RemovePackageAttribute(string path)
        {
            var manifestFiles =
                Directory.GetFiles(path, "AndroidManifest.xml", SearchOption.AllDirectories);

            if (manifestFiles.Length > 0)
            {
                foreach (var manifestPath in manifestFiles)
                {
                    try
                    {
                        var document = XDocument.Load(manifestPath);
                        var manifestElement = document.Element("manifest");
                        if (manifestElement != null)
                        {
                            var packageAttribute = manifestElement.Attribute("package");
                            if (packageAttribute != null)
                            {
                                packageAttribute.Remove();
                                document.Save(manifestPath);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        SayKitDebug.LogError(
                            $"[SKManifestInjector][RemovePackageAttribute] {manifestPath}: {ex.Message}");
                    }
                }
            }
        }

        private void SetActivityAttributes(XContainer application)
        {
            var activity = application?.Element("activity");
            activity?.SetAttributeValue(XName.Get("name", AndroidSpace), "by.saygames.SayKitActivity");
            activity?.SetAttributeValue(XName.Get("hardwareAccelerated", AndroidSpace), "true");
            activity?.SetAttributeValue(XName.Get("exported", AndroidSpace), "true");
        }

        private void SetApplicationAttributes(XElement application)
        {
            application?.SetAttributeValue(XName.Get("networkSecurityConfig", AndroidSpace),
                "@xml/network_security_config");

#if SAYKIT_BACKUP_DISABLE
            application?.SetAttributeValue(XName.Get("replace", ToolsSpace), "android:networkSecurityConfig, android:allowBackup");
            application?.SetAttributeValue(XName.Get("allowBackup", AndroidSpace), "false");
#else
            application?.SetAttributeValue(XName.Get("replace", ToolsSpace), "android:networkSecurityConfig");
#endif
            application?.SetAttributeValue(XName.Get("largeHeap", AndroidSpace), "true");
        }

        private void AddSKVersionMetaData(XContainer application)
        {
            application?.Add(new XElement("meta-data",
                new XAttribute(XName.Get("name", AndroidSpace), "saygames.saykit.version"),
                new XAttribute(XName.Get("value", AndroidSpace), SKManager.Instance.Version.ToString())
            ));
        }

        private void AddAppKeyMetaData(XContainer application)
        {
            application?.Add(new XElement("meta-data",
                new XAttribute(XName.Get("name", AndroidSpace), "saygames.saykit.appKey"),
                new XAttribute(XName.Get("value", AndroidSpace), SayKitApp.APP_KEY_ANDROID)
            ));
        }

        private void AddFacebookAutoLogAppEventMetaData(XContainer application)
        {
            var facebookAutoLogAppEventEventEnabled = true;

#if SAYKIT_DISABLE_FACEBOOK_AUTOLOG
            facebookAutoLogAppEventEventEnabled = false;
#endif

            application?.Add(new XElement("meta-data",
                new XAttribute(XName.Get("name", AndroidSpace), "com.facebook.sdk.AutoInitEnabled"),
                new XAttribute(XName.Get("value", AndroidSpace), facebookAutoLogAppEventEventEnabled)
            ));

            application?.Add(new XElement("meta-data",
                new XAttribute(XName.Get("name", AndroidSpace), "com.facebook.sdk.AutoLogAppEventsEnabled"),
                new XAttribute(XName.Get("value", AndroidSpace), facebookAutoLogAppEventEventEnabled)
            ));
        }

        private void AddPlayGamesServicesQueries(ICollection<ManifestLine> manifestLines)
        {
#if SAYKIT_PLAY_GAMES_SERVICES
            manifestLines?.Add(new ManifestLine
            {
                Tag = "queries",
                Attributes = new List<ManifestAttribute>(),
            });

            manifestLines?.Add(new ManifestLine
            {
                Tag = "package",
                Attributes = new List<ManifestAttribute>
                {
                    new ManifestAttribute
                    {
                        Name = "android:name",
                        Value = "com.google.android.play.games"
                    }
                },
                ParentTag = "queries"
            });
#endif
        }

        private void AddUserPermissions(ICollection<ManifestLine> manifestLines)
        {
            foreach (var permission in userPermissions)
            {
                manifestLines?.Add(new ManifestLine
                {
                    Tag = "uses-permission",
                    Attributes = new List<ManifestAttribute>
                    {
                        new ManifestAttribute()
                        {
                            Name = "android:name",
                            Value = permission
                        }
                    }
                });
            }
        }

        private void RemoveUserPermissions(XContainer parentElement)
        {
            foreach (var permission in removeUserPermissions)
            {
                var permissionElement = parentElement?.Elements("uses-permission")
                    .FirstOrDefault(e =>
                        e.Attribute(XName.Get("name", "http://schemas.android.com/apk/res/android"))?.Value ==
                        permission);

                permissionElement?.Remove();
            }
        }

        private void InjectManifestLines(XElement parentElement, List<ManifestLine> manifestLines)
        {
            XNamespace AndroidNameSpace = "http://schemas.android.com/apk/res/android";
            XNamespace ToolsNameSpace = "http://schemas.android.com/tools";

            foreach (var line in manifestLines)
            {
                var targetParentElement = parentElement;

                if (!string.IsNullOrEmpty(line.ParentTag))
                {
                    targetParentElement = parentElement?.Elements().FirstOrDefault(e => e.Name == line.ParentTag);

                    if (targetParentElement == null)
                    {
                        targetParentElement = new XElement(line.ParentTag);
                        parentElement?.Add(targetParentElement);
                    }
                }

                var existingElement = targetParentElement?.Elements()
                    .FirstOrDefault(t => t.Name == line.Tag &&
                                         t.Attributes().Any(a => a.Value == line.Attributes[0].Value));

                if (existingElement == null)
                {
                    var newElement = new XElement(line.Tag);

                    foreach (var attribute in line.Attributes)
                    {
                        if (attribute.Name.StartsWith("android:"))
                        {
                            var localName = attribute.Name.Split(':')[1];
                            newElement.SetAttributeValue(AndroidNameSpace + localName, attribute.Value);
                        }
                        else if (attribute.Name.StartsWith("tools:"))
                        {
                            var localName = attribute.Name.Split(':')[1];
                            newElement.SetAttributeValue(ToolsNameSpace + localName, attribute.Value);
                        }
                        else
                        {
                            newElement.SetAttributeValue(attribute.Name, attribute.Value);
                        }
                    }

                    targetParentElement?.Add(newElement);
                }
            }
        }

        private void UpdateAndroidManifestFile(XContainer manifest)
        {
#if SAYKIT_DISABLE_AD_SKIP_BUG
            foreach (var activity in manifest.Descendants("activity"))
            {
                var launchModeAttribute =
                    activity.Attribute(XName.Get("launchMode", "http://schemas.android.com/apk/res/android"));

                if (launchModeAttribute != null && launchModeAttribute.Value == "singleTask")
                {
                    launchModeAttribute.Value = "standard";
                    break;
                }
            }
#endif
        }

#if SAYKIT_DEEP_LINK
        private void AddDeepLinkIntentFilter(XElement application)
        {
            var appKey = SKUtils.GetAppKey();
            XNamespace androidNs = AndroidSpace;

            var activity = application.Element("activity");
            if (activity != null)
            {
                var exists = activity.Elements("intent-filter")
                    .Any(ifElem =>
                        ifElem.Attribute(androidNs + "autoVerify")?.Value == "true" &&
                        ifElem.Elements("data").Any(d =>
                            d.Attribute(androidNs + "host")?.Value == $"{appKey}.go.link"));

                if (!exists)
                {
                    var intentFilter = new XElement("intent-filter",
                        new XAttribute(androidNs + "autoVerify", "true"),
                        new XElement("action", new XAttribute(androidNs + "name", "android.intent.action.VIEW")),
                        new XElement("category", new XAttribute(androidNs + "name", "android.intent.category.DEFAULT")),
                        new XElement("category", new XAttribute(androidNs + "name", "android.intent.category.BROWSABLE")),
                        new XElement("data", new XAttribute(androidNs + "scheme", "http"),
                            new XAttribute(androidNs + "host", $"{appKey}.go.link")),
                        new XElement("data", new XAttribute(androidNs + "scheme", "https"),
                            new XAttribute(androidNs + "host", $"{appKey}.go.link"))
                    );

                    activity.Add(intentFilter);
                }
                
                var schemeExist = activity.Elements("intent-filter")
                    .Any(ifElem =>
                        ifElem.Attribute(androidNs + "autoVerify") == null &&
                        ifElem.Elements("data").Any(d =>
                            d.Attribute(androidNs + "scheme")?.Value == "ourCustomScheme"));

                if (!schemeExist)
                {
                    var customSchemeFilter = new XElement("intent-filter",
                        new XElement("action", new XAttribute(androidNs + "name", "android.intent.action.VIEW")),
                        new XElement("category", new XAttribute(androidNs + "name", "android.intent.category.DEFAULT")),
                        new XElement("category", new XAttribute(androidNs + "name", "android.intent.category.BROWSABLE")),
                        new XElement("data", new XAttribute(androidNs + "scheme", $"{appKey}"))
                    );
                    activity.Add(customSchemeFilter);
                }
            }
        }
#endif

        private static void PatchResizeableActivityAttribute(XContainer manifest)
        {
            try
            {
                if (manifest != null)
                {
                    XNamespace android = "http://schemas.android.com/apk/res/android";

                    var app = manifest.Element("application");

                    var act = app?.Elements("activity")
                        .FirstOrDefault(e => e.Attribute(android + "name")?.Value == "by.saygames.SayKitActivity");

                    if (act != null)
                    {
                        var resAttr = act.Attribute(android + "resizeableActivity");
                        if (resAttr == null)
                        {
                            act.SetAttributeValue(android + "resizeableActivity", "true");
                        }
                        else if (!string.Equals(resAttr.Value, "true", StringComparison.OrdinalIgnoreCase))
                        {
                            resAttr.Value = "true";
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SKManifestInjector][PatchResizeableActivityAttribute] {e}");
            }
        }
    }
}
#endif