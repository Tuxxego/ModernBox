using NCMS;
using NeoModLoader.api;
using UnityEngine;

namespace ModernBox
{
    [ModEntry]
    internal sealed class ModernBoxBootstrap : BasicMod<ModernBoxBootstrap>
    {
        private const string HostObjectName = "ModernBoxHost";

        protected override void OnModLoad()
        {
            EnsureMainHost();
        }

        private static void EnsureMainHost()
        {
            Main existingMain = Object.FindObjectOfType<Main>();
            if (existingMain != null)
            {
                return;
            }

            GameObject hostObject = GameObject.Find(HostObjectName);
            if (hostObject == null)
            {
                hostObject = new GameObject(HostObjectName);
                Object.DontDestroyOnLoad(hostObject);
            }

            if (hostObject.GetComponent<Main>() == null)
            {
                hostObject.AddComponent<Main>();
            }
        }
    }
}
