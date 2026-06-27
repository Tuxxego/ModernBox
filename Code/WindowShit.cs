using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ModernBox
{
    public static class WindowPreloaderUtils
    {
        public static void ForceClearPreloader()
        {
            return;
            Type type = typeof(WindowPreloader);
            BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic;

            FieldInfo preloadedField = type.GetField("_windows_preloaded", flags);
            preloadedField?.SetValue(null, false);

            FieldInfo preloadedDictField = type.GetField("_preloaded_windows", flags);
            var dict = preloadedDictField?.GetValue(null) as Dictionary<string, ScrollWindow>;
            if (dict != null)
            {
                foreach (var window in dict.Values)
                {
                    if (window != null && window.gameObject != null)
                    {
                        UnityEngine.Object.Destroy(window.gameObject);
                    }
                }
                dict.Clear();
            }

            string[] collectionsToClear = { 
                "_windows_resources_requests", 
                "_windows_preloading_operations", 
                "_windows_tabs_objects", 
                "_windows_preload_list" 
            };

            foreach (string fieldName in collectionsToClear)
            {
                FieldInfo field = type.GetField(fieldName, flags);
                object collection = field?.GetValue(null);

                collection?.GetType().GetMethod("Clear")?.Invoke(collection, null);
            }

            Debug.Log("ModernBox: WindowPreloader has been forcefully cleared.");
        }
    }
}