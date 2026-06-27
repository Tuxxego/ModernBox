//========= MODERNBOX 10.0 ============//
//
// Made by Tuxxego
//
//=============================================================================//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReflectionUtility;
namespace ModernBox
{
    class MBTraitGroup
    {
 
        public static string ModernBox = "ModernBox";
        public static string IdeologyBox = "IdeologyBox";
        public static void init()
        {
 
            ActorTraitGroupAsset ModernBox = new ActorTraitGroupAsset();
            ModernBox.id = "ModernBox";
            ModernBox.name = "trait_group_ModernBox";
            ModernBox.color = Toolbox.makeColor("#FFFF00", -1f);
            AssetManager.trait_groups.add(ModernBox);
            addTraitGroupToLocalizedLibrary(ModernBox.id, "ModernBox");
 
             ActorTraitGroupAsset IdeologyBox = new ActorTraitGroupAsset();
            IdeologyBox.id = "IdeologyBox";
            IdeologyBox.name = "trait_group_IdeologyBox";
            IdeologyBox.color = Toolbox.makeColor("#FFFF00", -1f);
            AssetManager.trait_groups.add(IdeologyBox);
            addTraitGroupToLocalizedLibrary(IdeologyBox.id, "ModernBox - Ideologies");
        }
        private static void addTraitGroupToLocalizedLibrary(string id, string name)
        {
            string language = Reflection.GetField(LocalizedTextManager.instance.GetType(), LocalizedTextManager.instance, "language") as string;
            Dictionary<string, string> localizedText = Reflection.GetField(LocalizedTextManager.instance.GetType(), LocalizedTextManager.instance, "localizedText") as Dictionary<string, string>;
            localizedText.Add("trait_group_" + id, name);
        }
    }
}
