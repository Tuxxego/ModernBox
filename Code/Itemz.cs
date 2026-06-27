//========= MODERNBOX 2.1.0.1 ============//
//
// Made by Tuxxego
//
//=============================================================================//
using System;
using tools;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NCMS;
using NCMS.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ReflectionUtility;
using HarmonyLib;
using System.Text.RegularExpressions;
using Beebyte.Obfuscator;
using ai;
using ai.behaviours;

namespace ModernBox
{
    class Itemz : MonoBehaviour
    {

        public static void init()
        {
            LoadItems();
        }

        private static void LoadItems()
        {
            // Culture Trait
            //
            //=============================================================================//

            // Guns
            //
            //=============================================================================//

            EquipmentAsset Glock17 = AssetManager.items.clone("Glock17", "$range");
            Glock17.equipment_type = EquipmentType.Weapon;
            Glock17.equipment_subtype = "stick";
            Glock17.translation_key = "Glock17, the bestest gun";
            Glock17.material = "basic";
            Glock17.group_id = "firearm";
            Glock17.metallic = true;
            Glock17.setCost(0, "wood", 1);
            Glock17.minimum_city_storage_resource_1 = 1;
            Glock17.rigidity_rating = 4;
            Glock17.name_templates = AssetLibrary<ItemAsset>.l<string>("shotgun_name");
            Glock17.is_pool_weapon = true;
            Glock17.pool_rate = 15;
            Glock17.path_icon = "ui/icons/items/icon_Glock17";
            Glock17.path_gameplay_sprite = "items/weapons/w_Glock17";
            Glock17.projectile = "shotgun_bullet";
            Glock17.path_slash_animation = "effects/slashes/slash_punch";
            Glock17.base_stats["projectiles"] = 1f;
            Glock17.base_stats["attack_speed"] = 0f;
            Glock17.base_stats["accuracy"] = 0.5f;
            Glock17.base_stats["damage"] = 30f;
            Glock17.base_stats["critical_chance"] = 0.3f;
            Glock17.base_stats["critical_damage_multiplier"] = 0.5f;
            Glock17.base_stats["recoil"] = 1f;
            Glock17.base_stats["range"] = 8f;
            Glock17.base_stats["targets"] = 1f;
            Glock17.base_stats["damage_range"] = 0.9f;
            Glock17.base_stats["mana"] = 10f;
            Glock17.base_stats["stamina"] = 10f;
            Glock17.equipment_value = 700;
Glock17.gameplay_sprites = FetchSprites("Glock17");
            AssetManager.items.add(Glock17);
            AssetManager.items.pot_weapon_assets_all.Add(Glock17);
            AssetManager.items.pot_weapon_assets_unlocked.Add(Glock17);
            addWeaponsSprite(Glock17.id);


            EquipmentAsset Uzi = AssetManager.items.clone("Uzi", "$range");
Uzi.equipment_type = EquipmentType.Weapon;
Uzi.translation_key = "Uzi SMG";
Uzi.equipment_subtype = "stick";
Uzi.material = "basic";
Uzi.group_id = "firearm";
Uzi.metallic = true;
Uzi.setCost(0, "wood", 2);
Uzi.minimum_city_storage_resource_1 = 1;
Uzi.rigidity_rating = 4;
Uzi.is_pool_weapon = true;
Uzi.pool_rate = 15;
Uzi.path_icon = "ui/icons/items/icon_Uzi";
Uzi.path_gameplay_sprite = "items/weapons/w_Uzi";
Uzi.projectile = "shotgun_bullet";
Uzi.path_slash_animation = "effects/slashes/slash_punch";
Uzi.base_stats["projectiles"] = 1f;
Uzi.base_stats["attack_speed"] = 10f;
Uzi.base_stats["accuracy"] = 0.45f;
Uzi.base_stats["damage"] = 25f;
Uzi.base_stats["critical_chance"] = 0.15f;
Uzi.base_stats["critical_damage_multiplier"] = 0.25f;
Uzi.base_stats["recoil"] = 0.9f;
Uzi.base_stats["range"] = 8f;
Uzi.name_templates = AssetLibrary<ItemAsset>.l<string>("shotgun_name");
Uzi.base_stats["targets"] = 1f;
Uzi.base_stats["damage_range"] = 0.7f;
Uzi.base_stats["mana"] = 8f;
Uzi.base_stats["stamina"] = 10f;
Uzi.equipment_value = 725;
Uzi.gameplay_sprites = FetchSprites("Uzi");
AssetManager.items.add(Uzi);
AssetManager.items.pot_weapon_assets_all.Add(Uzi);
AssetManager.items.pot_weapon_assets_unlocked.Add(Uzi);
addWeaponsSprite(Uzi.id);


EquipmentAsset Malorian = AssetManager.items.clone("Malorian", "$range");
Malorian.equipment_type = EquipmentType.Weapon;
Malorian.translation_key = "Malorian Arms 3516";
Malorian.equipment_subtype = "stick";
Malorian.material = "basic";
Malorian.group_id = "firearm";
Malorian.metallic = true;
Malorian.setCost(0, "wood", 1, "gems", 1);
Malorian.minimum_city_storage_resource_1 = 1;
Malorian.rigidity_rating = 5;
Malorian.is_pool_weapon = true;
Malorian.pool_rate = 15;
Malorian.path_icon = "ui/icons/items/icon_Malorian";
Malorian.path_gameplay_sprite = "items/weapons/w_malorian";
Malorian.projectile = "shotgun_bullet";
Malorian.path_slash_animation = "effects/slashes/slash_punch";
Malorian.base_stats["projectiles"] = 1f;
Malorian.base_stats["attack_speed"] = 2f;
Malorian.base_stats["accuracy"] = 0.75f;
Malorian.base_stats["damage"] = 75f;
Malorian.base_stats["critical_chance"] = 0.4f;
Malorian.base_stats["critical_damage_multiplier"] = 0.6f;
Malorian.base_stats["recoil"] = 0.5f;
Malorian.base_stats["range"] = 12f;
Malorian.name_templates = AssetLibrary<ItemAsset>.l<string>("shotgun_name");
Malorian.base_stats["targets"] = 1f;
Malorian.base_stats["damage_range"] = 0.9f;
Malorian.base_stats["mana"] = 25f;
Malorian.base_stats["stamina"] = 15f;
Malorian.equipment_value = 750;
Malorian.gameplay_sprites = FetchSprites("Malorian");
AssetManager.items.add(Malorian);
AssetManager.items.pot_weapon_assets_all.Add(Malorian);
AssetManager.items.pot_weapon_assets_unlocked.Add(Malorian);
addWeaponsSprite(Malorian.id);

EquipmentAsset DesertEagle = AssetManager.items.clone("DesertEagle", "$range");
DesertEagle.equipment_type = EquipmentType.Weapon;
DesertEagle.translation_key = "Desert Eagle Mark XIX";
DesertEagle.equipment_subtype = "stick";
DesertEagle.material = "basic";
DesertEagle.group_id = "firearm";
DesertEagle.metallic = true;
DesertEagle.setCost(0, "common_metals", 1);
DesertEagle.minimum_city_storage_resource_1 = 1;
DesertEagle.rigidity_rating = 5;
DesertEagle.is_pool_weapon = true;
DesertEagle.pool_rate = 15;
DesertEagle.path_icon = "ui/icons/items/icon_DesertEagle";
DesertEagle.path_gameplay_sprite = "items/weapons/w_DesertEagle";
DesertEagle.projectile = "shotgun_bullet";
DesertEagle.path_slash_animation = "effects/slashes/slash_punch";
DesertEagle.base_stats["projectiles"] = 1f;
DesertEagle.base_stats["attack_speed"] = -2f;
DesertEagle.base_stats["accuracy"] = 0.7f;
DesertEagle.base_stats["damage"] = 50f;
DesertEagle.base_stats["critical_chance"] = 0.35f;
DesertEagle.base_stats["critical_damage_multiplier"] = 0.5f;
DesertEagle.base_stats["recoil"] = 1f;
DesertEagle.base_stats["range"] = 10f;
DesertEagle.name_templates = AssetLibrary<ItemAsset>.l<string>("shotgun_name");
DesertEagle.base_stats["targets"] = 1f;
DesertEagle.base_stats["damage_range"] = 0.8f;
DesertEagle.base_stats["mana"] = 12f;
DesertEagle.base_stats["stamina"] = 12f;
DesertEagle.equipment_value = 775;
DesertEagle.gameplay_sprites = FetchSprites("DesertEagle");
AssetManager.items.add(DesertEagle);
AssetManager.items.pot_weapon_assets_all.Add(DesertEagle);
AssetManager.items.pot_weapon_assets_unlocked.Add(DesertEagle);
addWeaponsSprite(DesertEagle.id);


 EquipmentAsset Americanshotgun = AssetManager.items.clone("Americanshotgun", "$range");
            Americanshotgun.equipment_type = EquipmentType.Weapon;
            Americanshotgun.translation_key = "Bestest Shotgun";
            Americanshotgun.equipment_subtype = "stick";
            Americanshotgun.material = "basic";
            Americanshotgun.group_id = "firearm";
            Americanshotgun.metallic = true;
            Americanshotgun.setCost(0, "common_metals", 2);
            Americanshotgun.minimum_city_storage_resource_1 = 1;
            Americanshotgun.rigidity_rating = 3;
            Americanshotgun.is_pool_weapon = true;
            Americanshotgun.pool_rate = 15;
            Americanshotgun.path_icon = "ui/Icons/items/icon_shotgun";
            Americanshotgun.path_gameplay_sprite = "items/weapons/w_PipeShotgun";
            Americanshotgun.projectile = "shotgun_bullet";
            Americanshotgun.path_slash_animation = "effects/slashes/slash_punch";
            Americanshotgun.base_stats["projectiles"] = 6f;
            Americanshotgun.base_stats["attack_speed"] = -3f;
            Americanshotgun.base_stats["accuracy"] = 0.2f;
            Americanshotgun.base_stats["damage"] = 10f;
            Americanshotgun.base_stats["critical_chance"] = 0.2f;
            Americanshotgun.base_stats["critical_damage_multiplier"] = 0.3f;
            Americanshotgun.base_stats["recoil"] = 1f;
            Americanshotgun.base_stats["range"] = 10f;
            Americanshotgun.name_templates = AssetLibrary<ItemAsset>.l<string>("shotgun_name");
            Americanshotgun.base_stats["targets"] = 1f;
            Americanshotgun.base_stats["damage_range"] = 0.7f;
            Americanshotgun.base_stats["mana"] = 10f;
            Americanshotgun.base_stats["stamina"] = 15f;
            Americanshotgun.equipment_value = 800;
Americanshotgun.gameplay_sprites = FetchSprites("Americanshotgun");
            AssetManager.items.add(Americanshotgun);
            AssetManager.items.pot_weapon_assets_all.Add(Americanshotgun);
            AssetManager.items.pot_weapon_assets_unlocked.Add(Americanshotgun);
            addWeaponsSprite(Americanshotgun.id);

            EquipmentAsset Sluggershotgun = AssetManager.items.clone("Sluggershotgun", "$range");
            Sluggershotgun.equipment_type = EquipmentType.Weapon;
            Sluggershotgun.translation_key = "Greatest SHotgun";
            Sluggershotgun.equipment_subtype = "stick";
            Sluggershotgun.material = "basic";
            Sluggershotgun.group_id = "firearm";
            Sluggershotgun.metallic = true;
            Sluggershotgun.setCost(0, "common_metals", 3);
            Sluggershotgun.minimum_city_storage_resource_1 = 1;
            Sluggershotgun.rigidity_rating = 1;
            Sluggershotgun.is_pool_weapon = true;
            Sluggershotgun.pool_rate = 15;
            Sluggershotgun.path_icon = "ui/Icons/items/icon_shotgun";
            Sluggershotgun.path_gameplay_sprite = "items/weapons/w_PipeShotgun";
            Sluggershotgun.projectile = "shotgun_bullet";
            Sluggershotgun.path_slash_animation = "effects/slashes/slash_punch";
            Sluggershotgun.base_stats["projectiles"] = 1f;
            Sluggershotgun.base_stats["attack_speed"] = -3f;
            Sluggershotgun.base_stats["accuracy"] = 0.7f;
            Sluggershotgun.base_stats["damage"] = 80f;
            Sluggershotgun.base_stats["critical_chance"] = 0.2f;
            Sluggershotgun.base_stats["critical_damage_multiplier"] = 4f;
            Sluggershotgun.base_stats["recoil"] = 2f;
            Sluggershotgun.base_stats["range"] = 10f;
            Sluggershotgun.name_templates = AssetLibrary<ItemAsset>.l<string>("shotgun_name");
            Sluggershotgun.base_stats["targets"] = 1f;
            Sluggershotgun.base_stats["damage_range"] = 0.7f;
            Sluggershotgun.base_stats["mana"] = 10f;
            Sluggershotgun.base_stats["stamina"] = 15f;
            Sluggershotgun.equipment_value = 825;
Sluggershotgun.gameplay_sprites = FetchSprites("Sluggershotgun");
            AssetManager.items.add(Sluggershotgun);
            AssetManager.items.pot_weapon_assets_all.Add(Sluggershotgun);
            AssetManager.items.pot_weapon_assets_unlocked.Add(Sluggershotgun);
            addWeaponsSprite(Sluggershotgun.id);

EquipmentAsset MP7 = AssetManager.items.clone("MP7", "$range");
MP7.equipment_type = EquipmentType.Weapon;
MP7.translation_key = "HK MP7";
MP7.equipment_subtype = "stick";
MP7.material = "basic";
MP7.group_id = "firearm";
MP7.metallic = true;
MP7.setCost(0, "common_metals", 4);
MP7.minimum_city_storage_resource_1 = 1;
MP7.rigidity_rating = 5;
MP7.is_pool_weapon = true;
MP7.pool_rate = 15;
MP7.path_icon = "ui/icons/items/icon_MP7";
MP7.path_gameplay_sprite = "items/weapons/w_MP7";
MP7.projectile = "shotgun_bullet";
MP7.path_slash_animation = "effects/slashes/slash_punch";
MP7.base_stats["projectiles"] = 1f;
MP7.base_stats["attack_speed"] = 9f;
MP7.base_stats["accuracy"] = 0.6f;
MP7.base_stats["damage"] = 28f;
MP7.base_stats["critical_chance"] = 0.2f;
MP7.base_stats["critical_damage_multiplier"] = 0.3f;
MP7.base_stats["recoil"] = 0.4f;
MP7.base_stats["range"] = 9f;
MP7.name_templates = AssetLibrary<ItemAsset>.l<string>("shotgun_name");
MP7.base_stats["targets"] = 1f;
MP7.base_stats["damage_range"] = 0.75f;
MP7.base_stats["mana"] = 8f;
MP7.base_stats["stamina"] = 10f;
MP7.equipment_value = 850;
MP7.gameplay_sprites = FetchSprites("MP7");
AssetManager.items.add(MP7);
AssetManager.items.pot_weapon_assets_all.Add(MP7);
AssetManager.items.pot_weapon_assets_unlocked.Add(MP7);
addWeaponsSprite(MP7.id);

EquipmentAsset ThompsonM1A1 = AssetManager.items.clone("ThompsonM1A1", "$range");
ThompsonM1A1.equipment_type = EquipmentType.Weapon;
ThompsonM1A1.translation_key = "Thompson M1A1";
ThompsonM1A1.equipment_subtype = "stick";
ThompsonM1A1.material = "basic";
ThompsonM1A1.group_id = "firearm";
ThompsonM1A1.metallic = true;
ThompsonM1A1.setCost(0, "wood", 7);
ThompsonM1A1.minimum_city_storage_resource_1 = 1;
ThompsonM1A1.rigidity_rating = 4;
ThompsonM1A1.is_pool_weapon = true;
ThompsonM1A1.pool_rate = 15;
ThompsonM1A1.path_icon = "ui/icons/items/icon_ThompsonM1A1";
ThompsonM1A1.path_gameplay_sprite = "items/weapons/w_ThompsonM1A1";
ThompsonM1A1.projectile = "shotgun_bullet";
ThompsonM1A1.path_slash_animation = "effects/slashes/slash_punch";
ThompsonM1A1.base_stats["projectiles"] = 1f;
ThompsonM1A1.base_stats["attack_speed"] = 7f;
ThompsonM1A1.base_stats["accuracy"] = 0.55f;
ThompsonM1A1.base_stats["damage"] = 32f;
ThompsonM1A1.base_stats["critical_chance"] = 0.2f;
ThompsonM1A1.base_stats["critical_damage_multiplier"] = 0.4f;
ThompsonM1A1.base_stats["recoil"] = 0.8f;
ThompsonM1A1.base_stats["range"] = 10f;
ThompsonM1A1.name_templates = AssetLibrary<ItemAsset>.l<string>("shotgun_name");
ThompsonM1A1.base_stats["targets"] = 1f;
ThompsonM1A1.base_stats["damage_range"] = 0.7f;
ThompsonM1A1.base_stats["mana"] = 10f;
ThompsonM1A1.base_stats["stamina"] = 12f;
ThompsonM1A1.equipment_value = 875;
ThompsonM1A1.gameplay_sprites = FetchSprites("ThompsonM1A1");
AssetManager.items.add(ThompsonM1A1);
AssetManager.items.pot_weapon_assets_all.Add(ThompsonM1A1);
AssetManager.items.pot_weapon_assets_unlocked.Add(ThompsonM1A1);
addWeaponsSprite(ThompsonM1A1.id);


EquipmentAsset SGT44 = AssetManager.items.clone("SGT44", "$range");
SGT44.equipment_type = EquipmentType.Weapon;
SGT44.translation_key = "Sturmgewehr 44";
SGT44.equipment_subtype = "stick";
SGT44.material = "basic";
SGT44.group_id = "firearm";
SGT44.metallic = true;
SGT44.setCost(0, "common_metals", 1, "stone", 1);
SGT44.minimum_city_storage_resource_1 = 1;
SGT44.rigidity_rating = 4;
SGT44.is_pool_weapon = true;
SGT44.pool_rate = 15;
SGT44.path_icon = "ui/icons/items/icon_SGT44";
SGT44.path_gameplay_sprite = "items/weapons/w_SGT44";
SGT44.projectile = "shotgun_bullet";
SGT44.path_slash_animation = "effects/slashes/slash_punch";
SGT44.base_stats["projectiles"] = 1f;
SGT44.base_stats["attack_speed"] = 4f;
SGT44.base_stats["accuracy"] = 0.65f;
SGT44.base_stats["damage"] = 45f;
SGT44.base_stats["critical_chance"] = 0.25f;
SGT44.base_stats["critical_damage_multiplier"] = 0.35f;
SGT44.base_stats["recoil"] = 0.6f;
SGT44.base_stats["range"] = 13f;
SGT44.name_templates = AssetLibrary<ItemAsset>.l<string>("shotgun_name");
SGT44.base_stats["targets"] = 1f;
SGT44.base_stats["damage_range"] = 0.75f;
SGT44.base_stats["mana"] = 15f;
SGT44.base_stats["stamina"] = 18f;
SGT44.equipment_value = 900;
SGT44.gameplay_sprites = FetchSprites("SGT44");
AssetManager.items.add(SGT44);
AssetManager.items.pot_weapon_assets_all.Add(SGT44);
AssetManager.items.pot_weapon_assets_unlocked.Add(SGT44);
addWeaponsSprite(SGT44.id);

            EquipmentAsset AK = AssetManager.items.clone("AK", "$range");
            AK.equipment_type = EquipmentType.Weapon;
            AK.translation_key = "Avtomat Kalashnikov 47";
            AK.equipment_subtype = "stick";
            AK.material = "basic";
            AK.group_id = "firearm";
            AK.metallic = true;
            AK.setCost(0, "common_metals", 1, "wood", 2);
            AK.minimum_city_storage_resource_1 = 1;
            AK.rigidity_rating = 7;
            AK.is_pool_weapon = true;
            AK.pool_rate = 15;
            AK.path_icon = "ui/icons/items/icon_AK47";
            AK.path_gameplay_sprite = "items/weapons/w_AK";
            AK.projectile = "shotgun_bullet";
            AK.path_slash_animation = "effects/slashes/slash_punch";
            AK.base_stats["projectiles"] = 1f;
            AK.base_stats["attack_speed"] = 5f;
            AK.base_stats["accuracy"] = 0.6f;
            AK.base_stats["damage"] = 40f;
            AK.base_stats["critical_chance"] = 0.2f;
            AK.base_stats["critical_damage_multiplier"] = 0.3f;
            AK.base_stats["recoil"] = 0.5f;
            AK.base_stats["range"] = 15f;
             AK.name_templates = AssetLibrary<ItemAsset>.l<string>("shotgun_name");
            AK.base_stats["targets"] = 1f;
            AK.base_stats["damage_range"] = 0.8f;
            AK.base_stats["mana"] = 10f;
            AK.base_stats["stamina"] = 15f;
            AK.equipment_value = 925;
AK.gameplay_sprites = FetchSprites("AK");
            AssetManager.items.add(AK);
            AssetManager.items.pot_weapon_assets_all.Add(AK);
            AssetManager.items.pot_weapon_assets_unlocked.Add(AK);
            addWeaponsSprite(AK.id);

EquipmentAsset AK103 = AssetManager.items.clone("AK103", "$range");
AK103.equipment_type = EquipmentType.Weapon;
AK103.translation_key = "AK-103";
AK103.equipment_subtype = "stick";
AK103.material = "basic";
AK103.group_id = "firearm";
AK103.metallic = true;
AK103.setCost(0, "common_metals", 5);
AK103.minimum_city_storage_resource_1 = 1;
AK103.rigidity_rating = 4;
AK103.is_pool_weapon = true;
AK103.gameplay_sprites = FetchSprites("AK103");
AK103.pool_rate = 15;
AK103.path_icon = "ui/icons/items/icon_AK103";
AK103.path_gameplay_sprite = "items/weapons/w_AK103";
AK103.projectile = "shotgun_bullet";
AK103.path_slash_animation = "effects/slashes/slash_punch";
AK103.base_stats["projectiles"] = 1f;
AK103.base_stats["attack_speed"] = 5f;
AK103.base_stats["accuracy"] = 0.65f;
AK103.base_stats["damage"] = 62f;
AK103.base_stats["critical_chance"] = 0.25f;
AK103.base_stats["critical_damage_multiplier"] = 0.35f;
AK103.base_stats["recoil"] = 0.6f;
AK103.base_stats["range"] = 15f;
AK103.name_templates = AssetLibrary<ItemAsset>.l<string>("shotgun_name");
AK103.base_stats["targets"] = 1f;
AK103.base_stats["damage_range"] = 0.8f;
AK103.base_stats["mana"] = 10f;
AK103.base_stats["stamina"] = 15f;
AK103.equipment_value = 950;
AK103.gameplay_sprites = FetchSprites("AK103");
AssetManager.items.add(AK103);
AssetManager.items.pot_weapon_assets_all.Add(AK103);
AssetManager.items.pot_weapon_assets_unlocked.Add(AK103);
addWeaponsSprite(AK103.id);

EquipmentAsset M16 = AssetManager.items.clone("M16", "$range");
M16.equipment_type = EquipmentType.Weapon;
M16.translation_key = "M16A1";
M16.equipment_subtype = "stick";
M16.material = "basic";
M16.group_id = "firearm";
M16.metallic = true;
M16.setCost(0, "common_metals", 2);
M16.minimum_city_storage_resource_1 = 1;
M16.rigidity_rating = 4;
M16.is_pool_weapon = true;
M16.pool_rate = 15;
M16.path_icon = "ui/icons/items/icon_M16";
M16.path_gameplay_sprite = "items/weapons/w_M16";
M16.projectile = "shotgun_bullet";
M16.path_slash_animation = "effects/slashes/slash_punch";
M16.base_stats["projectiles"] = 1f;
M16.base_stats["attack_speed"] = 4f;
M16.base_stats["accuracy"] = 0.7f;
M16.base_stats["damage"] = 36f;
M16.base_stats["critical_chance"] = 0.2f;
M16.base_stats["critical_damage_multiplier"] = 0.3f;
M16.base_stats["recoil"] = 0.5f;
M16.base_stats["range"] = 18f;
M16.name_templates = AssetLibrary<ItemAsset>.l<string>("shotgun_name");
M16.base_stats["targets"] = 1f;
M16.base_stats["damage_range"] = 0.85f;
M16.base_stats["mana"] = 10f;
M16.base_stats["stamina"] = 15f;
M16.equipment_value = 975;
M16.gameplay_sprites = FetchSprites("M16");
AssetManager.items.add(M16);
AssetManager.items.pot_weapon_assets_all.Add(M16);
AssetManager.items.pot_weapon_assets_unlocked.Add(M16);
addWeaponsSprite(M16.id);





EquipmentAsset FAMAS = AssetManager.items.clone("FAMAS", "$range");
FAMAS.equipment_type = EquipmentType.Weapon;
FAMAS.translation_key = "FAMAS F1";
FAMAS.equipment_subtype = "stick";
FAMAS.material = "basic";
FAMAS.group_id = "firearm";
FAMAS.metallic = true;
FAMAS.setCost(0, "wood", 1, "stone", 1);
FAMAS.minimum_city_storage_resource_1 = 1;
FAMAS.rigidity_rating = 4;
FAMAS.is_pool_weapon = true;
FAMAS.pool_rate = 15;
FAMAS.path_icon = "ui/icons/items/icon_FAMAS";
FAMAS.path_gameplay_sprite = "items/weapons/w_FAMAS";
FAMAS.projectile = "shotgun_bullet";
FAMAS.path_slash_animation = "effects/slashes/slash_punch";
FAMAS.base_stats["projectiles"] = 1f;
FAMAS.base_stats["attack_speed"] = 8f;
FAMAS.base_stats["accuracy"] = 0.65f;
FAMAS.base_stats["damage"] = 35f;
FAMAS.base_stats["critical_chance"] = 0.2f;
FAMAS.base_stats["critical_damage_multiplier"] = 0.3f;
FAMAS.base_stats["recoil"] = 0.7f;
FAMAS.base_stats["range"] = 14f;
FAMAS.name_templates = AssetLibrary<ItemAsset>.l<string>("shotgun_name");
FAMAS.base_stats["targets"] = 1f;
FAMAS.base_stats["damage_range"] = 0.75f;
FAMAS.base_stats["mana"] = 10f;
FAMAS.base_stats["stamina"] = 15f;
FAMAS.equipment_value = 1000;
FAMAS.gameplay_sprites = FetchSprites("FAMAS");
AssetManager.items.add(FAMAS);
AssetManager.items.pot_weapon_assets_all.Add(FAMAS);
AssetManager.items.pot_weapon_assets_unlocked.Add(FAMAS);
addWeaponsSprite(FAMAS.id);

EquipmentAsset M4A1 = AssetManager.items.clone("M4A1", "$range");
M4A1.equipment_type = EquipmentType.Weapon;
M4A1.translation_key = "M4A1 Carbine";
M4A1.equipment_subtype = "stick";
M4A1.material = "basic";
M4A1.group_id = "firearm";
M4A1.metallic = true;
M4A1.setCost(0, "common_metals", 2, "stone", 1);
M4A1.minimum_city_storage_resource_1 = 1;
M4A1.rigidity_rating = 4;
M4A1.is_pool_weapon = true;
M4A1.pool_rate = 15;
M4A1.path_icon = "ui/icons/items/icon_M4A1";
M4A1.path_gameplay_sprite = "items/weapons/w_M4A1";
M4A1.projectile = "thunderplasma";
M4A1.path_slash_animation = "effects/slashes/slash_punch";
M4A1.base_stats["projectiles"] = 1f;
M4A1.base_stats["attack_speed"] = 6f;
M4A1.base_stats["accuracy"] = 0.7f;
M4A1.base_stats["damage"] = 38f;
M4A1.base_stats["critical_chance"] = 0.2f;
M4A1.base_stats["critical_damage_multiplier"] = 0.3f;
M4A1.base_stats["recoil"] = 0.4f;
M4A1.base_stats["range"] = 14f;
M4A1.name_templates = AssetLibrary<ItemAsset>.l<string>("shotgun_name");
M4A1.base_stats["targets"] = 1f;
M4A1.base_stats["damage_range"] = 0.8f;
M4A1.base_stats["mana"] = 10f;
M4A1.base_stats["stamina"] = 12f;
M4A1.equipment_value = 1025;
M4A1.gameplay_sprites = FetchSprites("M4A1");
AssetManager.items.add(M4A1);
AssetManager.items.pot_weapon_assets_all.Add(M4A1);
AssetManager.items.pot_weapon_assets_unlocked.Add(M4A1);
addWeaponsSprite(M4A1.id);

/*

EquipmentAsset greenblaster = AssetManager.items.clone("greenblaster", "$range");
greenblaster.equipment_type = EquipmentType.Weapon;
greenblaster.translation_key = "Green Laserarm";
greenblaster.equipment_subtype = "stick";
greenblaster.material = "basic";
greenblaster.group_id = "firearm";
greenblaster.metallic = true;
greenblaster.setCost(0, "common_metals", 2, "stone", 1);
greenblaster.minimum_city_storage_resource_1 = 1;
greenblaster.rigidity_rating = 4;
greenblaster.is_pool_weapon = true;
greenblaster.pool_rate = 15;
greenblaster.path_icon = "ui/icons/items/icon_greenblaster";
greenblaster.path_gameplay_sprite = "weapons/greenblaster";
greenblaster.projectile = "shotgun_bullet";
greenblaster.path_slash_animation = "effects/slashes/slash_punch";
greenblaster.base_stats["projectiles"] = 1f;
greenblaster.base_stats["attack_speed"] = 2f;
greenblaster.base_stats["accuracy"] = 0.75f;
greenblaster.base_stats["damage"] = 75f;
greenblaster.base_stats["critical_chance"] = 0.4f;
greenblaster.base_stats["critical_damage_multiplier"] = 0.6f;
greenblaster.base_stats["recoil"] = 0.5f;
greenblaster.base_stats["range"] = 12f;
greenblaster.name_templates = AssetLibrary<ItemAsset>.l<string>("shotgun_name");
greenblaster.base_stats["targets"] = 1f;
greenblaster.base_stats["damage_range"] = 0.9f;
greenblaster.base_stats["mana"] = 25f;
greenblaster.base_stats["stamina"] = 15f;
greenblaster.equipment_value = 1025;
greenblaster.gameplay_sprites = FetchSprites("greenblaster");
AssetManager.items.add(greenblaster);
AssetManager.items.pot_weapon_assets_all.Add(greenblaster);
AssetManager.items.pot_weapon_assets_unlocked.Add(greenblaster);
addWeaponsSprite(greenblaster.id);
*/

EquipmentAsset greenheavyblaster = AssetManager.items.clone("greenheavyblaster", "$range");
greenheavyblaster.equipment_type = EquipmentType.Weapon;
greenheavyblaster.translation_key = "Green Laserarm";
greenheavyblaster.equipment_subtype = "stick";
greenheavyblaster.material = "basic";
greenheavyblaster.group_id = "firearm";
greenheavyblaster.metallic = true;
greenheavyblaster.setCost(0, "common_metals", 2, "stone", 1);
greenheavyblaster.minimum_city_storage_resource_1 = 1;
greenheavyblaster.rigidity_rating = 4;
greenheavyblaster.is_pool_weapon = true;
greenheavyblaster.pool_rate = 15;
greenheavyblaster.path_icon = "ui/icons/items/icon_greenheavyblaster";
greenheavyblaster.path_gameplay_sprite = "items/weapons/w_greenheavyblaster";
greenheavyblaster.projectile = "thunderplasma";
greenheavyblaster.path_slash_animation = "effects/slashes/slash_punch";
greenheavyblaster.base_stats["projectiles"] = 1f;
greenheavyblaster.base_stats["attack_speed"] = 6f;
greenheavyblaster.base_stats["accuracy"] = 0.7f;
greenheavyblaster.base_stats["damage"] = 38f;
greenheavyblaster.base_stats["critical_chance"] = 0.2f;
greenheavyblaster.base_stats["critical_damage_multiplier"] = 0.3f;
greenheavyblaster.base_stats["recoil"] = 0.4f;
greenheavyblaster.base_stats["range"] = 14f;
greenheavyblaster.name_templates = AssetLibrary<ItemAsset>.l<string>("shotgun_name");
greenheavyblaster.base_stats["targets"] = 1f;
greenheavyblaster.base_stats["damage_range"] = 0.8f;
greenheavyblaster.base_stats["mana"] = 10f;
greenheavyblaster.base_stats["stamina"] = 12f;
greenheavyblaster.equipment_value = 1025;
greenheavyblaster.gameplay_sprites = FetchSprites("greenheavyblaster");
AssetManager.items.add(greenheavyblaster);
AssetManager.items.pot_weapon_assets_all.Add(greenheavyblaster);
AssetManager.items.pot_weapon_assets_unlocked.Add(greenheavyblaster);
addWeaponsSprite(greenheavyblaster.id);

EquipmentAsset XM8 = AssetManager.items.clone("XM8", "$range");
XM8.equipment_type = EquipmentType.Weapon;
XM8.translation_key = "XM8 Prototype";
XM8.equipment_subtype = "stick";
XM8.material = "basic";
XM8.group_id = "firearm";
XM8.metallic = true;
XM8.setCost(0, "common_metals", 3, "stone", 2);
XM8.minimum_city_storage_resource_1 = 1;
XM8.rigidity_rating = 4;
XM8.is_pool_weapon = true;
XM8.pool_rate = 15;
XM8.path_icon = "ui/icons/items/icon_XM8";
XM8.path_gameplay_sprite = "items/weapons/w_XM8";
XM8.projectile = "shotgun_bullet";
XM8.path_slash_animation = "effects/slashes/slash_punch";
XM8.base_stats["projectiles"] = 1f;
XM8.base_stats["attack_speed"] = 5f;
XM8.base_stats["accuracy"] = 0.8f;
XM8.base_stats["damage"] = 37f;
XM8.base_stats["critical_chance"] = 0.3f;
XM8.base_stats["critical_damage_multiplier"] = 0.4f;
XM8.base_stats["recoil"] = 0.3f;
XM8.base_stats["range"] = 16f;
XM8.name_templates = AssetLibrary<ItemAsset>.l<string>("shotgun_name");
XM8.base_stats["targets"] = 1f;
XM8.base_stats["damage_range"] = 0.85f;
XM8.base_stats["mana"] = 10f;
XM8.base_stats["stamina"] = 14f;
XM8.equipment_value = 1050;
XM8.gameplay_sprites = FetchSprites("XM8");
AssetManager.items.add(XM8);
AssetManager.items.pot_weapon_assets_all.Add(XM8);
AssetManager.items.pot_weapon_assets_unlocked.Add(XM8);
addWeaponsSprite(XM8.id);



EquipmentAsset HK416 = AssetManager.items.clone("HK416", "$range");
HK416.equipment_type = EquipmentType.Weapon;
HK416.translation_key = "HK416";
HK416.equipment_subtype = "stick";
HK416.material = "basic";
HK416.group_id = "firearm";
HK416.metallic = true;
HK416.setCost(0, "common_metals", 1, "wood", 5);
HK416.minimum_city_storage_resource_1 = 1;
HK416.rigidity_rating = 4;
HK416.is_pool_weapon = true;
HK416.pool_rate = 15;
HK416.path_icon = "ui/icons/items/icon_HK416";
HK416.path_gameplay_sprite = "items/weapons/w_HK416";
HK416.projectile = "shotgun_bullet";
HK416.path_slash_animation = "effects/slashes/slash_punch";
HK416.base_stats["projectiles"] = 1f;
HK416.base_stats["attack_speed"] = 6f;
HK416.base_stats["accuracy"] = 0.75f;
HK416.base_stats["damage"] = 39f;
HK416.base_stats["critical_chance"] = 0.25f;
HK416.base_stats["critical_damage_multiplier"] = 0.4f;
HK416.base_stats["recoil"] = 0.35f;
HK416.base_stats["range"] = 16f;
HK416.name_templates = AssetLibrary<ItemAsset>.l<string>("shotgun_name");
HK416.base_stats["targets"] = 1f;
HK416.base_stats["damage_range"] = 0.8f;
HK416.base_stats["mana"] = 10f;
HK416.base_stats["stamina"] = 15f;
HK416.equipment_value = 1075;
HK416.gameplay_sprites = FetchSprites("HK416");
AssetManager.items.add(HK416);
AssetManager.items.pot_weapon_assets_all.Add(HK416);
AssetManager.items.pot_weapon_assets_unlocked.Add(HK416);
addWeaponsSprite(HK416.id);

EquipmentAsset vrifle = AssetManager.items.clone("vrifle", "$range");
vrifle.equipment_type = EquipmentType.Weapon;
vrifle.translation_key = "FNV varmit rifle";
vrifle.equipment_subtype = "stick";
vrifle.material = "basic";
vrifle.group_id = "firearm";
vrifle.metallic = true;
vrifle.setCost(0, "common_metals", 4, "wood", 5);
vrifle.minimum_city_storage_resource_1 = 1;
vrifle.rigidity_rating = 7;
vrifle.is_pool_weapon = true;
vrifle.pool_rate = 15;
vrifle.path_icon = "ui/icons/items/icon_vrifle";
vrifle.path_gameplay_sprite = "items/weapons/w_vrifle_copper";
vrifle.projectile = "shotgun_bullet";
vrifle.path_slash_animation = "effects/slashes/slash_punch";
vrifle.base_stats["projectiles"] = 1f;
vrifle.base_stats["attack_speed"] = 0.5f;
vrifle.base_stats["accuracy"] = 0.9f;
vrifle.base_stats["damage"] = 20f;
vrifle.base_stats["critical_chance"] = 0.2f;
vrifle.base_stats["critical_damage_multiplier"] = 0.3f;
vrifle.base_stats["recoil"] = 0.1f;
vrifle.base_stats["range"] = 15f;
vrifle.name_templates = AssetLibrary<ItemAsset>.l<string>("shotgun_name");
vrifle.base_stats["targets"] = 1f;
vrifle.base_stats["damage_range"] = 0.8f;
vrifle.base_stats["mana"] = 00f;
vrifle.base_stats["stamina"] = 15f;
vrifle.equipment_value = 1100;
vrifle.gameplay_sprites = FetchSprites("vrifle");
AssetManager.items.add(vrifle);
AssetManager.items.pot_weapon_assets_all.Add(vrifle);
AssetManager.items.pot_weapon_assets_unlocked.Add(vrifle);
addWeaponsSprite(vrifle.id);

ProjectileAsset gauss = new ProjectileAsset();
            gauss.id = "gauss";
            gauss.texture = "blueplasma";
            gauss.trail_effect_enabled = false;
            gauss.trigger_on_collision = true;
            gauss.look_at_target = true;
            gauss.draw_light_area = true;
            gauss.draw_light_size = 0.1f;
            gauss.end_effect = "kameboomtest";
            gauss.terraform_option = "demon_fireball";
            gauss.terraform_range = 3;
            gauss.scale_start = 0.1f;
            gauss.scale_target = 0.1f;
            gauss.speed = 20f;
            gauss.can_be_left_on_ground = true;
            gauss.can_be_blocked = true;
            AssetManager.projectiles.add(gauss);

            EquipmentAsset grifle = AssetManager.items.clone("grifle", "$range");
            grifle.equipment_type = EquipmentType.Weapon;
            grifle.translation_key = "Gauss_cannon";
            grifle.equipment_subtype = "stick";
            grifle.material = "basic";
            grifle.group_id = "firearm";
            grifle.metallic = true;
            grifle.setCost(0, "common_metals", 4, "adamantine", 1);
            grifle.minimum_city_storage_resource_1 = 1;
            grifle.rigidity_rating = 1;
            grifle.name_templates = AssetLibrary<ItemAsset>.l<string>("shotgun_name");
            grifle.is_pool_weapon = true;
            grifle.pool_rate = 15;
            grifle.path_icon = "ui/icons/items/icon_grifle";
            grifle.path_gameplay_sprite = "items/weapons/w_grifle";
            grifle.projectile = "gauss";
            grifle.path_slash_animation = "effects/slashes/slash_punch";
            grifle.base_stats["projectiles"] = 1f;
            grifle.base_stats["accuracy"] = 0.7f;
            grifle.base_stats["attack_speed"] = -5f;
            grifle.base_stats["damage"] = 1000f;
            grifle.base_stats["critical_chance"] = 0.5f;
            grifle.base_stats["critical_damage_multiplier"] = 0.5f;
            grifle.base_stats["recoil"] = 2f;
            grifle.base_stats["range"] = 20f;
            grifle.base_stats["targets"] = 3f;
            grifle.base_stats["damage_range"] = 0.7f;
            grifle.base_stats["mana"] = 10f;
            grifle.base_stats["stamina"] = 20f;
            grifle.equipment_value = 1125;
            grifle.gameplay_sprites = FetchSprites("grifle");
            AssetManager.items.add(grifle);
            AssetManager.items.pot_weapon_assets_all.Add(grifle);
            AssetManager.items.pot_weapon_assets_unlocked.Add(grifle);
            addWeaponsSprite(grifle.id);

            EquipmentAsset Minigun = AssetManager.items.clone("Minigun", "$range");
            Minigun.equipment_type = EquipmentType.Weapon;
            Minigun.translation_key = "Minigun";
            Minigun.animated = true;
            Minigun.equipment_subtype = "stick";
            Minigun.material = "basic";
            Minigun.group_id = "firearm";
            Minigun.metallic = true;
            Minigun.setCost(0, "adamantine", 1, "stone", 10);
            Minigun.minimum_city_storage_resource_1 = 1;
            Minigun.rigidity_rating = 3;
            Minigun.is_pool_weapon = true;
            Minigun.pool_rate = 15;
            Minigun.path_icon = "ui/icons/items/icon_Minigun";
            Minigun.path_gameplay_sprite = "items/weapons/w_Minibase";
            Minigun.projectile = "shotgun_bullet";
            Minigun.path_slash_animation = "effects/slashes/slash_punch";
            Minigun.base_stats["projectiles"] = 1f;
            Minigun.base_stats["attack_speed"] = 20000f;
            Minigun.base_stats["accuracy"] = 0.3f;
            Minigun.base_stats["damage"] = 20f;
            Minigun.base_stats["critical_chance"] = 0.2f;
            Minigun.base_stats["critical_damage_multiplier"] = 0.3f;
            Minigun.base_stats["recoil"] = 0f;
            Minigun.base_stats["range"] = 15f;
             Minigun.name_templates = AssetLibrary<ItemAsset>.l<string>("shotgun_name");
            Minigun.base_stats["targets"] = 1f;
            Minigun.base_stats["damage_range"] = 0.8f;
            Minigun.base_stats["mana"] = 10f;
            Minigun.base_stats["stamina"] = 15f;
            Minigun.equipment_value = 1150;
Minigun.gameplay_sprites = FetchSprites("Minigun");
            AssetManager.items.add(Minigun);
            AssetManager.items.pot_weapon_assets_all.Add(Minigun);
            AssetManager.items.pot_weapon_assets_unlocked.Add(Minigun);
            addWeaponsSprite(Minigun.id);

            EquipmentAsset Sniper = AssetManager.items.clone("Sniper", "$range");
            Sniper.equipment_type = EquipmentType.Weapon;
            Sniper.translation_key = "Sniper";
            Sniper.animated = false;
            Sniper.equipment_subtype = "stick";
            Sniper.material = "basic";
            Sniper.group_id = "firearm";
            Sniper.metallic = true;
            Sniper.setCost(0, "wood", 5, "adamantine", 2);
            Sniper.minimum_city_storage_resource_1 = 1;
            Sniper.rigidity_rating = 1;
            Sniper.is_pool_weapon = true;
            Sniper.pool_rate = 15;
            Sniper.path_icon = "ui/icons/items/icon_Sniper";
            Sniper.path_gameplay_sprite = "items/weapons/w_Sniper";
            Sniper.projectile = "shotgun_bullet";
            Sniper.path_slash_animation = "effects/slashes/slash_punch";
            Sniper.base_stats["projectiles"] = 1f;
            Sniper.base_stats["attack_speed"] = -10f;
            Sniper.base_stats["accuracy"] = 4f;
            Sniper.base_stats["damage"] = 300f;
            Sniper.base_stats["critical_chance"] = 0.5f;
            Sniper.base_stats["critical_damage_multiplier"] = 0.8f;
            Sniper.base_stats["recoil"] = 2f;
            Sniper.base_stats["range"] = 30f;
             Sniper.name_templates = AssetLibrary<ItemAsset>.l<string>("shotgun_name");
            Sniper.base_stats["targets"] = 1f;
            Sniper.base_stats["damage_range"] = 0.1f;
            Sniper.base_stats["mana"] = 10f;
            Sniper.base_stats["stamina"] = 15f;
            Sniper.equipment_value = 1175;
Sniper.gameplay_sprites = FetchSprites("Sniper");
            AssetManager.items.add(Sniper);
            AssetManager.items.pot_weapon_assets_all.Add(Sniper);
            AssetManager.items.pot_weapon_assets_unlocked.Add(Sniper);
            addWeaponsSprite(Sniper.id);

            EquipmentAsset Musket = AssetManager.items.clone("Musket", "$range");
            Musket.equipment_type = EquipmentType.Weapon;
            Musket.translation_key = "Musket";
            Musket.animated = false;
            Musket.equipment_subtype = "stick";
            Musket.material = "basic";
            Musket.group_id = "firearm";
            Musket.metallic = true;
            Musket.setCost(0, "wood", 5, "adamantine", 2);
            Musket.minimum_city_storage_resource_1 = 1;
            Musket.rigidity_rating = 1;
            Musket.is_pool_weapon = true;
            Musket.pool_rate = 15;
            Musket.path_icon = "ui/icons/items/icon_Musket";
            Musket.path_gameplay_sprite = "items/weapons/w_Musket";
            Musket.projectile = "shotgun_bullet";
            Musket.path_slash_animation = "effects/slashes/slash_punch";
            Musket.base_stats["projectiles"] = 1f;
            Musket.base_stats["attack_speed"] = -100f;
            Musket.base_stats["accuracy"] = 4f;
            Musket.base_stats["damage"] = 150f;
            Musket.base_stats["critical_chance"] = 0.5f;
            Musket.base_stats["critical_damage_multiplier"] = 0.8f;
            Musket.base_stats["recoil"] = 20f;
            Musket.base_stats["range"] = 12f;
             Musket.name_templates = AssetLibrary<ItemAsset>.l<string>("shotgun_name");
            Musket.base_stats["targets"] = 1f;
            Musket.base_stats["damage_range"] = 0.1f;
            Musket.base_stats["mana"] = 10f;
            Musket.base_stats["stamina"] = 15f;
            Musket.equipment_value = 1175;
Musket.gameplay_sprites = FetchSprites("Musket");
            AssetManager.items.add(Musket);
            AssetManager.items.pot_weapon_assets_all.Add(Musket);
            AssetManager.items.pot_weapon_assets_unlocked.Add(Musket);
            addWeaponsSprite(Musket.id);

            EquipmentAsset Crossbow = AssetManager.items.clone("Crossbow", "$range");
            Crossbow.equipment_type = EquipmentType.Weapon;
            Crossbow.translation_key = "Crossbow";
            Crossbow.animated = false;
            Crossbow.equipment_subtype = "stick";
            Crossbow.material = "basic";
            Crossbow.group_id = "firearm";
            Crossbow.metallic = true;
            Crossbow.setCost(0, "wood", 5, "adamantine", 2);
            Crossbow.minimum_city_storage_resource_1 = 1;
            Crossbow.rigidity_rating = 1;
            Crossbow.is_pool_weapon = true;
            Crossbow.pool_rate = 15;
            Crossbow.path_icon = "ui/icons/items/icon_Crossbow";
            Crossbow.path_gameplay_sprite = "items/weapons/w_PipeRifle";
            Crossbow.projectile = "arrow";
            Crossbow.path_slash_animation = "effects/slashes/slash_punch";
            Crossbow.base_stats["projectiles"] = 1f;
            Crossbow.base_stats["attack_speed"] = 1f;
            Crossbow.base_stats["accuracy"] = 15f;
            Crossbow.base_stats["damage"] = 50f;
            Crossbow.base_stats["critical_chance"] = 0.5f;
            Crossbow.base_stats["critical_damage_multiplier"] = 0.8f;
            Crossbow.base_stats["recoil"] = 20f;
            Crossbow.base_stats["range"] = 12f;
             Crossbow.name_templates = AssetLibrary<ItemAsset>.l<string>("shotgun_name");
            Crossbow.base_stats["targets"] = 1f;
            Crossbow.base_stats["damage_range"] = 0.1f;
            Crossbow.base_stats["mana"] = 10f;
            Crossbow.base_stats["stamina"] = 15f;
            Crossbow.equipment_value = 1175;
Crossbow.gameplay_sprites = FetchSprites("Crossbow");
            AssetManager.items.add(Crossbow);
            AssetManager.items.pot_weapon_assets_all.Add(Crossbow);
            AssetManager.items.pot_weapon_assets_unlocked.Add(Crossbow);
            addWeaponsSprite(Crossbow.id);

            EquipmentAsset Flintlock = AssetManager.items.clone("Flintlock", "$range");
            Flintlock.equipment_type = EquipmentType.Weapon;
            Flintlock.translation_key = "Flintlock";
            Flintlock.animated = false;
            Flintlock.equipment_subtype = "stick";
            Flintlock.material = "basic";
            Flintlock.group_id = "firearm";
            Flintlock.metallic = true;
            Flintlock.setCost(0, "wood", 5, "adamantine", 2);
            Flintlock.minimum_city_storage_resource_1 = 1;
            Flintlock.rigidity_rating = 1;
            Flintlock.is_pool_weapon = true;
            Flintlock.pool_rate = 15;
            Flintlock.path_icon = "ui/icons/items/icon_Flintlock";
            Flintlock.path_gameplay_sprite = "items/weapons/w_piratpistol_iron";
            Flintlock.projectile = "shotgun_bullet";
            Flintlock.path_slash_animation = "effects/slashes/slash_punch";
            Flintlock.base_stats["projectiles"] = 1f;
            Flintlock.base_stats["attack_speed"] = 1f;
            Flintlock.base_stats["accuracy"] = 15f;
            Flintlock.base_stats["damage"] = 75f;
            Flintlock.base_stats["critical_chance"] = 0.5f;
            Flintlock.base_stats["critical_damage_multiplier"] = 0.8f;
            Flintlock.base_stats["recoil"] = 50f;
            Flintlock.base_stats["range"] = 22f;
             Flintlock.name_templates = AssetLibrary<ItemAsset>.l<string>("shotgun_name");
            Flintlock.base_stats["targets"] = 1f;
            Flintlock.base_stats["damage_range"] = 0.1f;
            Flintlock.base_stats["mana"] = 10f;
            Flintlock.base_stats["stamina"] = 15f;
            Flintlock.equipment_value = 1175;
Flintlock.gameplay_sprites = FetchSprites("Flintlock");
            AssetManager.items.add(Flintlock);
            AssetManager.items.pot_weapon_assets_all.Add(Flintlock);
            AssetManager.items.pot_weapon_assets_unlocked.Add(Flintlock);
            addWeaponsSprite(Flintlock.id);

                        EffectAsset firepower_boom = new EffectAsset();
firepower_boom.id = "firepower_boom";
firepower_boom.use_basic_prefab = true;
firepower_boom.sorting_layer_id = "EffectsTop";
firepower_boom.sprite_path = "effects/firepower_boom";
firepower_boom.draw_light_area = true;
AssetManager.effects_library.add(firepower_boom);

            ProjectileAsset firepower = new ProjectileAsset();
            firepower.id = "firepower";
            firepower.texture = "firepower";
            firepower.trail_effect_enabled = false;
            firepower.trigger_on_collision = true;
            firepower.look_at_target = true;
            firepower.draw_light_area = true;
            firepower.draw_light_size = 1f;
            firepower.end_effect = "firepower_boom";
            firepower.terraform_option = "demon_fireball";
            firepower.terraform_range = 1;
            firepower.scale_start = 0.1f;
            firepower.scale_target = 1.0f;
            firepower.speed = 20f;
            firepower.can_be_left_on_ground = true;
            firepower.can_be_blocked = false;
            AssetManager.projectiles.add(firepower);

            EquipmentAsset Flamethrower = AssetManager.items.clone("Flamethrower", "$range");
            Flamethrower.equipment_type = EquipmentType.Weapon;
            Flamethrower.translation_key = "flamethrower";
            Flamethrower.equipment_subtype = "stick";
            Flamethrower.material = "basic";
            Flamethrower.group_id = "firearm";
            Flamethrower.metallic = true;
            Flamethrower.setCost(0, "adamantine", 3);
            Flamethrower.minimum_city_storage_resource_1 = 1;
            Flamethrower.rigidity_rating = 1;
            Flamethrower.name_templates = AssetLibrary<ItemAsset>.l<string>("shotgun_name");
            Flamethrower.is_pool_weapon = true;
            Flamethrower.pool_rate = 15;
            Flamethrower.path_icon = "ui/icons/items/icon_Flamethrower";
            Flamethrower.path_gameplay_sprite = "items/weapons/w_RPG";
            Flamethrower.projectile = "firepower";
            Flamethrower.path_slash_animation = "effects/slashes/slash_punch";
            Flamethrower.base_stats["projectiles"] = 5f;
            Flamethrower.base_stats["accuracy"] = -1f;
            Flamethrower.base_stats["attack_speed"] = 100f;
            Flamethrower.base_stats["damage"] = 0f;
            Flamethrower.base_stats["critical_chance"] = 0f;
            Flamethrower.base_stats["critical_damage_multiplier"] = 0f;
            Flamethrower.base_stats["recoil"] = 0f;
            Flamethrower.base_stats["range"] = 10f;
            Flamethrower.base_stats["targets"] = 30f;
            Flamethrower.base_stats["damage_range"] = 0f;
            Flamethrower.base_stats["mana"] = 0f;
            Flamethrower.base_stats["stamina"] = 20f;
            Flamethrower.equipment_value = 1200;
Flamethrower.gameplay_sprites = FetchSprites("Flamethrower");
Flamethrower.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>("flame");
            AssetManager.items.add(Flamethrower);
            AssetManager.items.pot_weapon_assets_all.Add(Flamethrower);
            AssetManager.items.pot_weapon_assets_unlocked.Add(Flamethrower);
            addWeaponsSprite(Flamethrower.id);



          ProjectileAsset RPGload = new ProjectileAsset();
          RPGload.id = "RPGload";
          RPGload.texture = "RPGload";
          RPGload.trail_effect_enabled = false;
          RPGload.trigger_on_collision = true;
	      RPGload.look_at_target = true;
          RPGload.draw_light_area = true;
	      RPGload.draw_light_size = 0.1f;
          RPGload.end_effect = "fx_fireball_explosion";
          RPGload.terraform_option = "demon_fireball";
          RPGload.terraform_range = 3;
          RPGload.scale_start = 0.1f;
          RPGload.scale_target = 0.1f;
          RPGload.speed = 20f;
          RPGload.can_be_left_on_ground = true;
          RPGload.can_be_blocked = true;
          AssetManager.projectiles.add(RPGload);

            EquipmentAsset RPG = AssetManager.items.clone("RPG", "$range");
            RPG.equipment_type = EquipmentType.Weapon;
            RPG.translation_key = "Rocket-propelled grenade";
            RPG.equipment_subtype = "stick";
            RPG.material = "basic";
            RPG.group_id = "firearm";
            RPG.metallic = true;
            RPG.setCost(0, "adamantine", 5);
            RPG.minimum_city_storage_resource_1 = 1;
            RPG.rigidity_rating = 1;
            RPG.name_templates = AssetLibrary<ItemAsset>.l<string>("shotgun_name");
            RPG.is_pool_weapon = true;
            RPG.pool_rate = 15;
            RPG.path_icon = "ui/icons/items/icon_RocketLauncher";
            RPG.path_gameplay_sprite = "items/weapons/w_RPG";
            RPG.projectile = "RPGload";
            RPG.path_slash_animation = "effects/slashes/slash_punch";
            RPG.base_stats["projectiles"] = 1f;
            RPG.base_stats["accuracy"] = 0.7f;
            RPG.base_stats["attack_speed"] = -5f;
            RPG.base_stats["damage"] = 100f;
            RPG.base_stats["critical_chance"] = 0.5f;
            RPG.base_stats["critical_damage_multiplier"] = 0.5f;
            RPG.base_stats["recoil"] = 2f;
            RPG.base_stats["range"] = 20f;
            RPG.base_stats["targets"] = 3f;
            RPG.base_stats["damage_range"] = 0.7f;
            RPG.base_stats["mana"] = 10f;
            RPG.base_stats["stamina"] = 20f;
            RPG.equipment_value = 1225;
RPG.gameplay_sprites = FetchSprites("RPG");
            AssetManager.items.add(RPG);
            AssetManager.items.pot_weapon_assets_all.Add(RPG);
            AssetManager.items.pot_weapon_assets_unlocked.Add(RPG);
            addWeaponsSprite(RPG.id);

                        EquipmentAsset M32 = AssetManager.items.clone("M32", "$range");
            M32.equipment_type = EquipmentType.Weapon;
            M32.translation_key = "M32, the friendly grenade launcher!";
            M32.equipment_subtype = "stick";
            M32.material = "basic";
            M32.group_id = "firearm";
            M32.metallic = true;
            M32.setCost(0, "adamantine", 3, "stone", 5);
            M32.minimum_city_storage_resource_1 = 1;
            M32.rigidity_rating = 1;
            M32.name_templates = AssetLibrary<ItemAsset>.l<string>("shotgun_name");
            M32.is_pool_weapon = true;
            M32.pool_rate = 15;
            M32.path_icon = "ui/icons/items/icon_M32";
            M32.path_gameplay_sprite = "items/weapons/w_Sniper";
            M32.projectile = "cannonball";
            M32.path_slash_animation = "effects/slashes/slash_punch";
            M32.base_stats["projectiles"] = 1f;
            M32.base_stats["accuracy"] = 0.2f;
            M32.base_stats["attack_speed"] = 1f;
            M32.base_stats["damage"] = 70f;
            M32.base_stats["critical_chance"] = 0.5f;
            M32.base_stats["critical_damage_multiplier"] = 0.5f;
            M32.base_stats["recoil"] = 0.5f;
            M32.base_stats["range"] = 8f;
            M32.base_stats["targets"] = 3f;
            M32.base_stats["damage_range"] = 0.7f;
            M32.base_stats["mana"] = 10f;
            M32.base_stats["stamina"] = 20f;
            M32.equipment_value = 1250;
M32.gameplay_sprites = FetchSprites("M32");
            AssetManager.items.add(M32);
            AssetManager.items.pot_weapon_assets_all.Add(M32);
            AssetManager.items.pot_weapon_assets_unlocked.Add(M32);
            addWeaponsSprite(M32.id);

            EffectAsset fx_MGLexplosion_t = new EffectAsset();
fx_MGLexplosion_t.id = "fx_MGLexplosion_t";
fx_MGLexplosion_t.use_basic_prefab = true;
fx_MGLexplosion_t.sorting_layer_id = "EffectsTop";
fx_MGLexplosion_t.sprite_path = "effects/fx_MGLexplosion_t";
fx_MGLexplosion_t.draw_light_area = true;
AssetManager.effects_library.add(fx_MGLexplosion_t);

 ProjectileAsset MGLload = new ProjectileAsset();
          MGLload.id = "MGLload";
          MGLload.texture = "MGLload";
          MGLload.trail_effect_enabled = false;
          MGLload.trigger_on_collision = true;
	      MGLload.look_at_target = true;
          MGLload.draw_light_area = true;
	      MGLload.draw_light_size = 0.1f;
          MGLload.end_effect = "fx_MGLexplosion_t";
          MGLload.terraform_option = "acid_ball";
          MGLload.terraform_range = 3;
          MGLload.scale_start = 0.1f;
          MGLload.scale_target = 1.0f;
          MGLload.speed = 20f;
          MGLload.can_be_left_on_ground = true;
          MGLload.can_be_blocked = false;
          AssetManager.projectiles.add(MGLload);

            EquipmentAsset MGL = AssetManager.items.clone("MGL", "$range");
            MGL.equipment_type = EquipmentType.Weapon;
            MGL.translation_key = "madness_gas_launcher";
            MGL.equipment_subtype = "stick";
            MGL.material = "basic";
            MGL.group_id = "firearm";
            MGL.metallic = true;
            MGL.setCost(0, "common_metals", 10, "adamantine", 5);
            MGL.minimum_city_storage_resource_1 = 1;
            MGL.rigidity_rating = 1;
            MGL.name_templates = AssetLibrary<ItemAsset>.l<string>("shotgun_name");
            MGL.is_pool_weapon = true;
            MGL.pool_rate = 15;
            MGL.path_icon = "ui/icons/items/icon_MGL";
            MGL.path_gameplay_sprite = "items/weapons/w_MGL";
            MGL.projectile = "MGLload";
            MGL.path_slash_animation = "effects/slashes/slash_punch";
            MGL.base_stats["projectiles"] = 10f;
            MGL.base_stats["accuracy"] = -1f;
            MGL.base_stats["attack_speed"] = 1f;
            MGL.base_stats["damage"] = 0f;
            MGL.base_stats["critical_chance"] = 0f;
            MGL.base_stats["critical_damage_multiplier"] = 0f;
            MGL.base_stats["recoil"] = 0f;
            MGL.base_stats["range"] = 10f;
            MGL.base_stats["targets"] = 1f;
            MGL.base_stats["damage_range"] = 0f;
            MGL.base_stats["mana"] = 10f;
            MGL.base_stats["stamina"] = 20f;
            MGL.equipment_value = 1275;
MGL.gameplay_sprites = FetchSprites("MGL");
MGL.item_modifier_ids = AssetLibrary<EquipmentAsset>.a<string>("poison");
            AssetManager.items.add(MGL);
            addWeaponsSprite(MGL.id);


            ProjectileAsset mininuke = new ProjectileAsset();
            mininuke.id = "mininuke";
            mininuke.texture = "mininuke";
            mininuke.trail_effect_enabled = false;
            mininuke.trigger_on_collision = true;
            mininuke.look_at_target = true;
            mininuke.draw_light_area = true;
            mininuke.draw_light_size = 0.1f;
            mininuke.end_effect = "fx_explosion_middle";
            mininuke.terraform_option = "atomic_bomb";
            mininuke.terraform_range = 2;
            mininuke.scale_start = 0.1f;
            mininuke.scale_target = 0.1f;
            mininuke.speed = 20f;
            mininuke.can_be_left_on_ground = true;
            mininuke.can_be_blocked = true;
            AssetManager.projectiles.add(mininuke);

            EquipmentAsset bigboy = AssetManager.items.clone("bigboy", "$range");
            bigboy.equipment_type = EquipmentType.Weapon;
            bigboy.translation_key = "Fatman";
            bigboy.equipment_subtype = "stick";
            bigboy.material = "basic";
            bigboy.group_id = "firearm";
            bigboy.metallic = true;
            bigboy.setCost(0, "adamantine", 20);
            bigboy.minimum_city_storage_resource_1 = 1;
            bigboy.rigidity_rating = 1;
            bigboy.name_templates = AssetLibrary<ItemAsset>.l<string>("shotgun_name");
            bigboy.is_pool_weapon = true;
            bigboy.pool_rate = 15;
            bigboy.path_icon = "ui/icons/items/icon_bigboy";
            bigboy.path_gameplay_sprite = "items/weapons/w_bigboy";
            bigboy.projectile = "mininuke";
            bigboy.path_slash_animation = "effects/slashes/slash_punch";
            bigboy.base_stats["projectiles"] = 1f;
            bigboy.base_stats["accuracy"] = 0.7f;
            bigboy.base_stats["attack_speed"] = -10f;
            bigboy.base_stats["damage"] = 100f;
            bigboy.base_stats["critical_chance"] = 0.5f;
            bigboy.base_stats["critical_damage_multiplier"] = 0.5f;
            bigboy.base_stats["recoil"] = 2f;
            bigboy.base_stats["range"] = 20f;
            bigboy.base_stats["targets"] = 1f;
            bigboy.base_stats["damage_range"] = 0.7f;
            bigboy.base_stats["mana"] = 10f;
            bigboy.base_stats["stamina"] = 20f;
            bigboy.equipment_value = 1300;
            bigboy.gameplay_sprites = FetchSprites("bigboy");
            AssetManager.items.add(bigboy);
            AssetManager.items.pot_weapon_assets_all.Add(bigboy);
            AssetManager.items.pot_weapon_assets_unlocked.Add(bigboy);
            addWeaponsSprite(bigboy.id);

            EquipmentAsset BudgetMIRV = AssetManager.items.clone("BudgetMIRV", "$range");
            BudgetMIRV.equipment_type = EquipmentType.Weapon;
            BudgetMIRV.translation_key = "Budget MIRV";
            BudgetMIRV.equipment_subtype = "stick";
            BudgetMIRV.material = "basic";
            BudgetMIRV.group_id = "firearm";
            BudgetMIRV.metallic = true;
            BudgetMIRV.setCost(0, "common_metals", 5);
            BudgetMIRV.minimum_city_storage_resource_1 = 1;
            BudgetMIRV.rigidity_rating = 1;
            BudgetMIRV.name_templates = AssetLibrary<ItemAsset>.l<string>("shotgun_name");
            BudgetMIRV.is_pool_weapon = true;
            BudgetMIRV.pool_rate = 15;
            BudgetMIRV.path_icon = "ui/icons/items/icon_BudgetMIRV";
            BudgetMIRV.path_gameplay_sprite = "items/weapons/w_BudgetMIRV";
            BudgetMIRV.projectile = "fireball";
            BudgetMIRV.path_slash_animation = "effects/slashes/slash_punch";
            BudgetMIRV.base_stats["range"] = 200f;
            BudgetMIRV.base_stats["accuracy"] = 0f;
            BudgetMIRV.base_stats["attack_speed"] = 40f;
            BudgetMIRV.base_stats["damage"] = 993f;
            BudgetMIRV.base_stats["health"] = 10f;
            BudgetMIRV.equipment_value = 1225;
            BudgetMIRV.gameplay_sprites = FetchSprites("BudgetMIRV");
            AssetManager.items.add(BudgetMIRV);
            AssetManager.items.pot_weapon_assets_all.Add(BudgetMIRV);
            AssetManager.items.pot_weapon_assets_unlocked.Add(BudgetMIRV);
            addWeaponsSprite(BudgetMIRV.id);

            EquipmentAsset DecentMIRV = AssetManager.items.clone("DecentMIRV", "$range");
            DecentMIRV.equipment_type = EquipmentType.Weapon;
            DecentMIRV.translation_key = "Decent MIRV";
            DecentMIRV.equipment_subtype = "stick";
            DecentMIRV.material = "basic";
            DecentMIRV.group_id = "firearm";
            DecentMIRV.metallic = true;
            DecentMIRV.setCost(0, "common_metals", 5);
            DecentMIRV.minimum_city_storage_resource_1 = 1;
            DecentMIRV.rigidity_rating = 1;
            DecentMIRV.name_templates = AssetLibrary<ItemAsset>.l<string>("shotgun_name");
            DecentMIRV.is_pool_weapon = true;
            DecentMIRV.pool_rate = 15;
            DecentMIRV.path_icon = "ui/icons/items/icon_DecentMIRV";
            DecentMIRV.path_gameplay_sprite = "items/weapons/w_DecentMIRV";
            DecentMIRV.projectile = "fireball";
            DecentMIRV.path_slash_animation = "effects/slashes/slash_punch";
            DecentMIRV.base_stats["range"] = 500f;
            DecentMIRV.base_stats["accuracy"] = 0f;
            DecentMIRV.base_stats["attack_speed"] = 70f;
            DecentMIRV.base_stats["damage"] = 993f;
            DecentMIRV.base_stats["health"] = 10f;
            DecentMIRV.equipment_value = 1225;
            DecentMIRV.gameplay_sprites = FetchSprites("DecentMIRV");
            AssetManager.items.add(DecentMIRV);
            AssetManager.items.pot_weapon_assets_all.Add(DecentMIRV);
            AssetManager.items.pot_weapon_assets_unlocked.Add(DecentMIRV);
            addWeaponsSprite(DecentMIRV.id);

            EquipmentAsset MIRV = AssetManager.items.clone("MIRV", "$range");
            MIRV.equipment_type = EquipmentType.Weapon;
            MIRV.translation_key = "MIRV";
            MIRV.equipment_subtype = "stick";
            MIRV.material = "basic";
            MIRV.group_id = "firearm";
            MIRV.metallic = true;
            MIRV.setCost(0, "common_metals", 5);
            MIRV.minimum_city_storage_resource_1 = 1;
            MIRV.rigidity_rating = 1;
            MIRV.name_templates = AssetLibrary<ItemAsset>.l<string>("shotgun_name");
            MIRV.is_pool_weapon = true;
            MIRV.pool_rate = 15;
            MIRV.path_icon = "ui/icons/items/icon_MIRV";
            MIRV.path_gameplay_sprite = "items/weapons/w_MIRV";
            MIRV.projectile = "fireball";
            MIRV.path_slash_animation = "effects/slashes/slash_punch";
            MIRV.base_stats["range"] = 50000f;
            MIRV.base_stats["accuracy"] = 0f;
            MIRV.base_stats["attack_speed"] = 101f;
            MIRV.base_stats["damage"] = 993f;
            MIRV.base_stats["health"] = 10f;
            MIRV.equipment_value = 1225;
            MIRV.gameplay_sprites = FetchSprites("MIRV");
            AssetManager.items.add(MIRV);
            AssetManager.items.pot_weapon_assets_all.Add(MIRV);
            AssetManager.items.pot_weapon_assets_unlocked.Add(MIRV);
            addWeaponsSprite(MIRV.id);

            EquipmentAsset MIRVBomb = AssetManager.items.clone("MIRVBomb", "$range");
            MIRVBomb.equipment_type = EquipmentType.Weapon;
            MIRVBomb.translation_key = "MIRVBomb";
            MIRVBomb.equipment_subtype = "stick";
            MIRVBomb.material = "basic";
            MIRVBomb.group_id = "firearm";
            MIRVBomb.metallic = true;
            MIRVBomb.setCost(0, "common_metals", 5);
            MIRVBomb.minimum_city_storage_resource_1 = 1;
            MIRVBomb.rigidity_rating = 1;
            MIRVBomb.name_templates = AssetLibrary<ItemAsset>.l<string>("shotgun_name");
            MIRVBomb.is_pool_weapon = true;
            MIRVBomb.pool_rate = 15;
            MIRVBomb.path_icon = "ui/icons/items/icon_MIRVBomb";
            MIRVBomb.path_gameplay_sprite = "items/weapons/w_MIRVBomb";
            MIRVBomb.projectile = "fireball";
            MIRVBomb.path_slash_animation = "effects/slashes/slash_punch";
            MIRVBomb.base_stats["range"] = 0f;
            MIRVBomb.base_stats["accuracy"] = 0f;
            MIRVBomb.base_stats["attack_speed"] = 101f;
            MIRVBomb.base_stats["damage"] = 993f;
            MIRVBomb.base_stats["health"] = 10f;
            MIRVBomb.equipment_value = 1225;
            MIRVBomb.gameplay_sprites = FetchSprites("MIRVBomb");
            AssetManager.items.add(MIRVBomb);
            AssetManager.items.pot_weapon_assets_all.Add(MIRVBomb);
            AssetManager.items.pot_weapon_assets_unlocked.Add(MIRVBomb);
            addWeaponsSprite(MIRVBomb.id);

            EquipmentAsset STRONGMIRV = AssetManager.items.clone("STRONGMIRV", "$range");
            STRONGMIRV.equipment_type = EquipmentType.Weapon;
            STRONGMIRV.translation_key = "STRONGMIRV";
            STRONGMIRV.equipment_subtype = "stick";
            STRONGMIRV.material = "basic";
            STRONGMIRV.group_id = "firearm";
            STRONGMIRV.metallic = true;
            STRONGMIRV.setCost(0, "common_metals", 5);
            STRONGMIRV.minimum_city_storage_resource_1 = 1;
            STRONGMIRV.rigidity_rating = 1;
            STRONGMIRV.name_templates = AssetLibrary<ItemAsset>.l<string>("shotgun_name");
            STRONGMIRV.is_pool_weapon = true;
            STRONGMIRV.pool_rate = 15;
            STRONGMIRV.path_icon = "ui/icons/items/icon_STRONGMIRV";
            STRONGMIRV.path_gameplay_sprite = "items/weapons/w_STRONGMIRV";
            STRONGMIRV.projectile = "fireball";
            STRONGMIRV.path_slash_animation = "effects/slashes/slash_punch";
            STRONGMIRV.base_stats["range"] = 50000f;
            STRONGMIRV.base_stats["accuracy"] = 0f;
            STRONGMIRV.base_stats["attack_speed"] = 1001f;
            STRONGMIRV.base_stats["damage"] = 993f;
            STRONGMIRV.base_stats["health"] = 10f;
            STRONGMIRV.equipment_value = 1225;
            STRONGMIRV.gameplay_sprites = FetchSprites("STRONGMIRV");
            AssetManager.items.add(STRONGMIRV);
            AssetManager.items.pot_weapon_assets_all.Add(STRONGMIRV);
            AssetManager.items.pot_weapon_assets_unlocked.Add(STRONGMIRV);
            addWeaponsSprite(STRONGMIRV.id);

            ///////////////ARMOR///////////////////////////////////////////////////////////////

            EquipmentAsset ModernArmor = AssetManager.items.clone("modernarmor", "$armor");
            ModernArmor.equipment_type = EquipmentType.Armor;
            ModernArmor.material = "basic";
            ModernArmor.metallic = true;
            ModernArmor.path_icon = "ui/icons/items/icon_modernarmor";
            ModernArmor.name_templates = AssetLibrary<ItemAsset>.l<string>("armor_name");
            ModernArmor.setCost(0, "wood", 1);
            ModernArmor.minimum_city_storage_resource_1 = 1;
            ModernArmor.rigidity_rating = 4;
            ModernArmor.base_stats["armor"] = 20f;
            ModernArmor.base_stats["stamina"] = 20f;
            ModernArmor.equipment_value = 700;
            AssetManager.items.add(ModernArmor);


            EquipmentAsset ModernHelmet = AssetManager.items.clone("modernhelmet", "$helmet");
            ModernHelmet.equipment_type = EquipmentType.Helmet;
            ModernHelmet.material = "basic";
            ModernHelmet.metallic = true;
            ModernHelmet.path_icon = "ui/icons/items/icon_modernhelmet";
            ModernHelmet.setCost(0, "wood", 1);
            ModernHelmet.name_templates = AssetLibrary<ItemAsset>.l<string>("helmet_name");
            ModernHelmet.minimum_city_storage_resource_1 = 1;
            ModernHelmet.rigidity_rating = 4;
            ModernHelmet.base_stats["armor"] = 5f;
            ModernHelmet.base_stats["mana"] = 10f;
            ModernHelmet.base_stats["accuracy"] = 0.6f;
            ModernHelmet.equipment_value = 700;
            AssetManager.items.add(ModernHelmet);

            EquipmentAsset ModernBoots = AssetManager.items.clone("modernboots", "$boots");
            ModernBoots.equipment_type = EquipmentType.Boots;
            ModernBoots.metallic = true;
            ModernBoots.material = "basic";
            ModernBoots.setCost(0, "wood", 1);
            ModernBoots.path_icon = "ui/icons/items/icon_modernboots";
            ModernBoots.name_templates = AssetLibrary<ItemAsset>.l<string>("boots_name");
            ModernBoots.minimum_city_storage_resource_1 = 1;
            ModernBoots.rigidity_rating = 4;
            ModernBoots.base_stats["armor"] = 5f;
            ModernBoots.base_stats["speed"] = 15f;
            ModernBoots.base_stats["stamina"] = 3f;
            ModernBoots.equipment_value = 700;
            AssetManager.items.add(ModernBoots);

            //////////////////////////Equipments/////////////////////////////////////////////////

            EquipmentAsset GrenadeBelt = AssetManager.items.clone("grenadebelt", "$amulet");
            GrenadeBelt.equipment_type = EquipmentType.Ring;
            GrenadeBelt.group_id = "ring";
            GrenadeBelt.equipment_subtype = "ring";
            GrenadeBelt.material = "basic";
            GrenadeBelt.metallic = true;
            GrenadeBelt.name_templates = AssetLibrary<ItemAsset>.l<string>("ring_name");
            GrenadeBelt.setCost(0, "common_metals", 1);
            GrenadeBelt.path_icon = "ui/icons/items/MK2Ariel";
            GrenadeBelt.minimum_city_storage_resource_1 = 1;
            GrenadeBelt.rigidity_rating = 2;
            GrenadeBelt.base_stats["damage"] = 10f;
            GrenadeBelt.base_stats["critical_chance"] = 0.3f;
            GrenadeBelt.base_stats["stamina"] = 10f;
            GrenadeBelt.equipment_value = 700;
            AssetManager.items.add(GrenadeBelt);

            EquipmentAsset MedicPack = AssetManager.items.clone("medicpack", "$ring");
            MedicPack.equipment_type = EquipmentType.Amulet;
            MedicPack.metallic = true;
            MedicPack.equipment_subtype = "amulet";
            MedicPack.group_id = "amulet";
            MedicPack.path_icon = "ui/icons/items/MEDIC";
            MedicPack.name_templates = AssetLibrary<ItemAsset>.l<string>("amulet_name");
            MedicPack.material = "basic";
            MedicPack.setCost(0, "herbs", 1);
            MedicPack.minimum_city_storage_resource_1 = 1;
            MedicPack.rigidity_rating = 2;
            MedicPack.base_stats["health"] = 30f;
            MedicPack.base_stats["stamina"] = 20f;
            MedicPack.equipment_value = 700;
            AssetManager.items.add(MedicPack);



            ////////////////////////////////////////////////////////////////////////////
            /// Drugs
            ///
            EquipmentAsset BathSalts = AssetManager.items.clone("BathSalts", "$armor");
            BathSalts.equipment_type = EquipmentType.Armor;
            BathSalts.material = "basic";
            BathSalts.metallic = true;
            BathSalts.translation_key = "Bath Salts";
            BathSalts.path_icon = "ui/icons/items/icon_BathSalts";
            BathSalts.name_templates = AssetLibrary<ItemAsset>.l<string>("armor_name");
            BathSalts.setCost(0, "common_metals", 1);
            BathSalts.minimum_city_storage_resource_1 = 1;
            BathSalts.rigidity_rating = 4;
            BathSalts.base_stats["damage"] = -20f;
            BathSalts.base_stats["range"] = -220f;
            BathSalts.base_stats["accuracy"] = -2200f;
            BathSalts.equipment_value = 700;
            AssetManager.items.add(BathSalts);

            EquipmentAsset Fentanyl = AssetManager.items.clone("Fentanyl", "$armor");
            Fentanyl.equipment_type = EquipmentType.Armor;
            Fentanyl.material = "basic";
            Fentanyl.metallic = true;
            Fentanyl.translation_key = "Fentanyl";
            Fentanyl.path_icon = "ui/icons/items/icon_Fentanyl";
            Fentanyl.name_templates = AssetLibrary<ItemAsset>.l<string>("armor_name");
            Fentanyl.setCost(0, "common_metals", 1);
            Fentanyl.minimum_city_storage_resource_1 = 1;
            Fentanyl.rigidity_rating = 4;
            Fentanyl.base_stats["health"] = -90000f;
            Fentanyl.equipment_value = 700;
            AssetManager.items.add(Fentanyl);

            EquipmentAsset Morphine = AssetManager.items.clone("Morphine", "$armor");
            Morphine.equipment_type = EquipmentType.Armor;
            Morphine.material = "basic";
            Morphine.metallic = true;
            Morphine.translation_key = "Morphine";
            Morphine.path_icon = "ui/icons/items/icon_Morphine";
            Morphine.name_templates = AssetLibrary<ItemAsset>.l<string>("armor_name");
            Morphine.setCost(0, "common_metals", 1);
            Morphine.minimum_city_storage_resource_1 = 1;
            Morphine.rigidity_rating = 4;
            Morphine.base_stats["health"] = 900f;
            Morphine.base_stats["armor"] = 200f;
            Morphine.equipment_value = 700;
            AssetManager.items.add(Morphine);

            EquipmentAsset Oxycodone = AssetManager.items.clone("Oxycodone", "$armor");
            Oxycodone.equipment_type = EquipmentType.Armor;
            Oxycodone.material = "basic";
            Oxycodone.metallic = true;
            Oxycodone.translation_key = "Oxycodone";
            Oxycodone.path_icon = "ui/icons/items/icon_Oxycodone";
            Oxycodone.name_templates = AssetLibrary<ItemAsset>.l<string>("armor_name");
            Oxycodone.setCost(0, "common_metals", 1);
            Oxycodone.minimum_city_storage_resource_1 = 1;
            Oxycodone.rigidity_rating = 4;
            Oxycodone.base_stats["health"] = 900f;
            Oxycodone.base_stats["armor"] = 200f;
            Oxycodone.equipment_value = 700;
            AssetManager.items.add(Oxycodone);

            EquipmentAsset Ritalin = AssetManager.items.clone("Ritalin", "$armor");
            Ritalin.equipment_type = EquipmentType.Armor;
            Ritalin.material = "basic";
            Ritalin.metallic = true;
            Ritalin.translation_key = "Ritalin";
            Ritalin.path_icon = "ui/icons/items/icon_Ritalin";
            Ritalin.name_templates = AssetLibrary<ItemAsset>.l<string>("armor_name");
            Ritalin.setCost(0, "common_metals", 1);
            Ritalin.minimum_city_storage_resource_1 = 1;
            Ritalin.rigidity_rating = 4;
            Ritalin.base_stats["critical_chance"] = 90f;
            Ritalin.base_stats["attack_speed"] = 200f;
            Ritalin.equipment_value = 700;
            AssetManager.items.add(Ritalin);

            /////////////////////////////////////////////////////////////////////////////
            CustomItemsList.InitCustomItems();

            if (!AssetManager.items.equipment_by_subtypes.ContainsKey("firearm"))
            {
                AssetManager.items.equipment_by_subtypes.Add("firearm", new List<EquipmentAsset>());
            }

            if (!AssetManager.items.equipment_by_subtypes.ContainsKey("stick"))
            {
                AssetManager.items.equipment_by_subtypes.Add("stick", new List<EquipmentAsset>());
            }

            AssetManager.items.equipment_by_subtypes["stick"].Add(Glock17);
            AssetManager.items.equipment_by_subtypes["stick"].Add(AK);
            AssetManager.items.equipment_by_subtypes["stick"].Add(RPG);
            AssetManager.items.equipment_by_subtypes["stick"].Add(Minigun);
            AssetManager.items.equipment_by_subtypes["stick"].Add(Sniper);
            AssetManager.items.equipment_by_subtypes["stick"].Add(FAMAS);
            AssetManager.items.equipment_by_subtypes["stick"].Add(M4A1);
            AssetManager.items.equipment_by_subtypes["stick"].Add(ThompsonM1A1);
            AssetManager.items.equipment_by_subtypes["stick"].Add(SGT44);
            AssetManager.items.equipment_by_subtypes["stick"].Add(XM8);
            AssetManager.items.equipment_by_subtypes["stick"].Add(AK103);
            AssetManager.items.equipment_by_subtypes["stick"].Add(Uzi);
            AssetManager.items.equipment_by_subtypes["stick"].Add(Malorian);
            AssetManager.items.equipment_by_subtypes["stick"].Add(DesertEagle);
            AssetManager.items.equipment_by_subtypes["stick"].Add(M16);
            AssetManager.items.equipment_by_subtypes["stick"].Add(HK416);
            AssetManager.items.equipment_by_subtypes["stick"].Add(MP7);
            AssetManager.items.equipment_by_subtypes["stick"].Add(M32);
            AssetManager.items.equipment_by_subtypes["stick"].Add(Americanshotgun);
            AssetManager.items.equipment_by_subtypes["stick"].Add(Sluggershotgun);
            AssetManager.items.equipment_by_subtypes["stick"].Add(Flamethrower);
            AssetManager.items.equipment_by_subtypes["stick"].Add(vrifle);
            AssetManager.items.equipment_by_subtypes["stick"].Add(bigboy);
            AssetManager.items.equipment_by_subtypes["stick"].Add(grifle);
            AssetManager.items.equipment_by_subtypes["stick"].Add(MGL);
            AssetManager.items.equipment_by_subtypes["stick"].Add(BudgetMIRV);
            AssetManager.items.equipment_by_subtypes["stick"].Add(DecentMIRV);
            AssetManager.items.equipment_by_subtypes["stick"].Add(MIRV);
            AssetManager.items.equipment_by_subtypes["stick"].Add(MIRVBomb);
            AssetManager.items.equipment_by_subtypes["stick"].Add(STRONGMIRV);
            AssetManager.items.equipment_by_subtypes["stick"].Add(greenheavyblaster);
            AssetManager.items.equipment_by_subtypes["stick"].Add(Musket);
            AssetManager.items.equipment_by_subtypes["stick"].Add(Flintlock);
            AssetManager.items.equipment_by_subtypes["stick"].Add(Crossbow);

            if (!AssetManager.items.pot_equipment_by_groups_all.ContainsKey("firearm"))
            {
                AssetManager.items.pot_equipment_by_groups_all.Add("firearm", new List<EquipmentAsset>());
            }
            AssetManager.items.pot_equipment_by_groups_all["firearm"].Add(Glock17);
            AssetManager.items.pot_equipment_by_groups_all["firearm"].Add(AK);
            AssetManager.items.pot_equipment_by_groups_all["firearm"].Add(RPG);
            AssetManager.items.pot_equipment_by_groups_all["firearm"].Add(Minigun);
            AssetManager.items.pot_equipment_by_groups_all["firearm"].Add(Sniper);
            AssetManager.items.pot_equipment_by_groups_all["firearm"].Add(FAMAS);
            AssetManager.items.pot_equipment_by_groups_all["firearm"].Add(M4A1);
            AssetManager.items.pot_equipment_by_groups_all["firearm"].Add(ThompsonM1A1);
            AssetManager.items.pot_equipment_by_groups_all["firearm"].Add(SGT44);
            AssetManager.items.pot_equipment_by_groups_all["firearm"].Add(XM8);
            AssetManager.items.pot_equipment_by_groups_all["firearm"].Add(AK103);
            AssetManager.items.pot_equipment_by_groups_all["firearm"].Add(Uzi);
            AssetManager.items.pot_equipment_by_groups_all["firearm"].Add(Malorian);
            AssetManager.items.pot_equipment_by_groups_all["firearm"].Add(DesertEagle);
            AssetManager.items.pot_equipment_by_groups_all["firearm"].Add(M16);
            AssetManager.items.pot_equipment_by_groups_all["firearm"].Add(HK416);
            AssetManager.items.pot_equipment_by_groups_all["firearm"].Add(MP7);
            AssetManager.items.pot_equipment_by_groups_all["firearm"].Add(M32);
            AssetManager.items.pot_equipment_by_groups_all["firearm"].Add(Americanshotgun);
            AssetManager.items.pot_equipment_by_groups_all["firearm"].Add(Sluggershotgun);
            AssetManager.items.pot_equipment_by_groups_all["firearm"].Add(Flamethrower);
            AssetManager.items.pot_equipment_by_groups_all["firearm"].Add(vrifle);
            AssetManager.items.pot_equipment_by_groups_all["firearm"].Add(bigboy);
            AssetManager.items.pot_equipment_by_groups_all["firearm"].Add(grifle);
            AssetManager.items.pot_equipment_by_groups_all["firearm"].Add(MGL);
            AssetManager.items.pot_equipment_by_groups_all["firearm"].Add(BudgetMIRV);
            AssetManager.items.pot_equipment_by_groups_all["firearm"].Add(DecentMIRV);
            AssetManager.items.pot_equipment_by_groups_all["firearm"].Add(MIRV);
            AssetManager.items.pot_equipment_by_groups_all["firearm"].Add(MIRVBomb);
            AssetManager.items.pot_equipment_by_groups_all["firearm"].Add(STRONGMIRV);
            AssetManager.items.pot_equipment_by_groups_all["firearm"].Add(greenheavyblaster);
            AssetManager.items.pot_equipment_by_groups_all["firearm"].Add(Musket);
            AssetManager.items.pot_equipment_by_groups_all["firearm"].Add(Flintlock);
            AssetManager.items.pot_equipment_by_groups_all["firearm"].Add(Crossbow);

            if (!AssetManager.items.pot_equipment_by_groups_unlocked.ContainsKey("firearm"))
            {
                AssetManager.items.pot_equipment_by_groups_unlocked.Add("firearm", new List<EquipmentAsset>());
            }
            AssetManager.items.pot_equipment_by_groups_unlocked["firearm"].Add(Glock17);
            AssetManager.items.pot_equipment_by_groups_unlocked["firearm"].Add(AK);
            AssetManager.items.pot_equipment_by_groups_unlocked["firearm"].Add(RPG);
            AssetManager.items.pot_equipment_by_groups_unlocked["firearm"].Add(Minigun);
            AssetManager.items.pot_equipment_by_groups_unlocked["firearm"].Add(Sniper);
            AssetManager.items.pot_equipment_by_groups_unlocked["firearm"].Add(FAMAS);
            AssetManager.items.pot_equipment_by_groups_unlocked["firearm"].Add(M4A1);
            AssetManager.items.pot_equipment_by_groups_unlocked["firearm"].Add(ThompsonM1A1);
            AssetManager.items.pot_equipment_by_groups_unlocked["firearm"].Add(SGT44);
            AssetManager.items.pot_equipment_by_groups_unlocked["firearm"].Add(XM8);
            AssetManager.items.pot_equipment_by_groups_unlocked["firearm"].Add(AK103);
            AssetManager.items.pot_equipment_by_groups_unlocked["firearm"].Add(Uzi);
            AssetManager.items.pot_equipment_by_groups_unlocked["firearm"].Add(Malorian);
            AssetManager.items.pot_equipment_by_groups_unlocked["firearm"].Add(DesertEagle);
            AssetManager.items.pot_equipment_by_groups_unlocked["firearm"].Add(M16);
            AssetManager.items.pot_equipment_by_groups_unlocked["firearm"].Add(HK416);
            AssetManager.items.pot_equipment_by_groups_unlocked["firearm"].Add(MP7);
            AssetManager.items.pot_equipment_by_groups_unlocked["firearm"].Add(M32);
            AssetManager.items.pot_equipment_by_groups_unlocked["firearm"].Add(Americanshotgun);
            AssetManager.items.pot_equipment_by_groups_unlocked["firearm"].Add(Sluggershotgun);
            AssetManager.items.pot_equipment_by_groups_unlocked["firearm"].Add(Flamethrower);
            AssetManager.items.pot_equipment_by_groups_unlocked["firearm"].Add(vrifle);
            AssetManager.items.pot_equipment_by_groups_unlocked["firearm"].Add(bigboy);
            AssetManager.items.pot_equipment_by_groups_unlocked["firearm"].Add(grifle);
            AssetManager.items.pot_equipment_by_groups_unlocked["firearm"].Add(MGL);
            AssetManager.items.pot_equipment_by_groups_unlocked["firearm"].Add(BudgetMIRV);
            AssetManager.items.pot_equipment_by_groups_unlocked["firearm"].Add(DecentMIRV);
            AssetManager.items.pot_equipment_by_groups_unlocked["firearm"].Add(MIRV);
            AssetManager.items.pot_equipment_by_groups_unlocked["firearm"].Add(MIRVBomb);
            AssetManager.items.pot_equipment_by_groups_unlocked["firearm"].Add(STRONGMIRV);
            AssetManager.items.pot_equipment_by_groups_unlocked["firearm"].Add(greenheavyblaster);
            AssetManager.items.pot_equipment_by_groups_unlocked["firearm"].Add(Musket);
            AssetManager.items.pot_equipment_by_groups_unlocked["firearm"].Add(Flintlock);
            AssetManager.items.pot_equipment_by_groups_unlocked["firearm"].Add(Crossbow);
            
            /////////////////////////////////////////////////////////////////////////////


             if (!AssetManager.items.pot_equipment_by_groups_all.ContainsKey("boots"))
            {
                AssetManager.items.pot_equipment_by_groups_all.Add("boots", new List<EquipmentAsset>());
            }
            AssetManager.items.pot_equipment_by_groups_all["boots"].Add(ModernBoots);


            if (!AssetManager.items.pot_equipment_by_groups_all.ContainsKey("helmet"))
            {
                AssetManager.items.pot_equipment_by_groups_all.Add("helmet", new List<EquipmentAsset>());
            }
            AssetManager.items.pot_equipment_by_groups_all["helmet"].Add(ModernHelmet);


              if (!AssetManager.items.pot_equipment_by_groups_all.ContainsKey("armor"))
            {
                AssetManager.items.pot_equipment_by_groups_all.Add("armor", new List<EquipmentAsset>());
            }
            AssetManager.items.pot_equipment_by_groups_all["armor"].Add(ModernArmor);
            AssetManager.items.pot_equipment_by_groups_all["armor"].Add(BathSalts);
            AssetManager.items.pot_equipment_by_groups_all["armor"].Add(Fentanyl);
            AssetManager.items.pot_equipment_by_groups_all["armor"].Add(Morphine);
            AssetManager.items.pot_equipment_by_groups_all["armor"].Add(Oxycodone);
            AssetManager.items.pot_equipment_by_groups_all["armor"].Add(Ritalin);





            /////////////////////////////////////////////////////////////////////////////
            if (!AssetManager.items.pot_equipment_by_groups_all.ContainsKey("ring"))
            {
                AssetManager.items.pot_equipment_by_groups_all.Add("ring", new List<EquipmentAsset>());
            }
            AssetManager.items.pot_equipment_by_groups_all["ring"].Add(GrenadeBelt);


            if (!AssetManager.items.pot_equipment_by_groups_all.ContainsKey("amulet"))
            {
                AssetManager.items.pot_equipment_by_groups_all.Add("amulet", new List<EquipmentAsset>());
            }
            AssetManager.items.pot_equipment_by_groups_all["amulet"].Add(MedicPack);


                if (!AssetManager.items.pot_equipment_by_groups_unlocked.ContainsKey("amulet"))
            {
                AssetManager.items.pot_equipment_by_groups_unlocked.Add("amulet", new List<EquipmentAsset>());
            }
              AssetManager.items.pot_equipment_by_groups_unlocked["amulet"].Add(MedicPack);


                if (!AssetManager.items.pot_equipment_by_groups_unlocked.ContainsKey("ring"))
            {
                AssetManager.items.pot_equipment_by_groups_unlocked.Add("ring", new List<EquipmentAsset>());
            }
           AssetManager.items.pot_equipment_by_groups_unlocked["ring"].Add(GrenadeBelt);


         if (!AssetManager.items.equipment_by_subtypes.ContainsKey("amulet"))
            {
                AssetManager.items.equipment_by_subtypes.Add("amulet", new List<EquipmentAsset>());
            }
         AssetManager.items.equipment_by_subtypes["amulet"].Add(MedicPack);


         if (!AssetManager.items.equipment_by_subtypes.ContainsKey("ring"))
            {
                AssetManager.items.equipment_by_subtypes.Add("ring", new List<EquipmentAsset>());
            }
         AssetManager.items.equipment_by_subtypes["ring"].Add(GrenadeBelt);


        }

        public static void addWeaponsSprite(string id)
        {
         //   ModernBoxLogger.Log("[addWeaponsSprite] Attempting to load sprite for weapon: " + id);

            EquipmentAsset item = AssetManager.items.get(id);

            if (item == null)
            {
          //      ModernBoxLogger.Error("[addWeaponsSprite] Item not found in AssetManager: " + id);
                return;
            }

            item.gameplay_sprites = FetchSprites(id);
        //    ModernBoxLogger.Log("[addWeaponsSprite] Sprite assignment complete for: " + id);
        }
        
        public static Sprite[] FetchSprites(string id)
        {
        //    ModernBoxLogger.Log("[FetchSprites] Fetching sprites for weapon: " + id);

            EquipmentAsset item = AssetManager.items.get(id);
            if (item == null)
            {
        //        ModernBoxLogger.Error("[FetchSprites] Item not found in AssetManager: " + id);
                return Array.Empty<Sprite>();
            }

            if (item.animated)
            {
                List<Sprite> spriteList = new List<Sprite>();
                int frameIndex = 0;
                bool framesFound = false;

                while (true)
                {
                    string path1 = "weapons/" + id + "_" + frameIndex;
                    Sprite frameSprite = Resources.Load<Sprite>(path1);

                    if (frameSprite != null)
                    {
           //             ModernBoxLogger.Log("[FetchSprites] Loaded sprite: " + path1);
                        spriteList.Add(frameSprite);
                        framesFound = true;
                        frameIndex++;
                        continue;
                    }

                    string path2 = "weapons/" + id + frameIndex;
                    frameSprite = Resources.Load<Sprite>(path2);

                    if (frameSprite != null)
                    {
          //              ModernBoxLogger.Log("[FetchSprites] Loaded sprite: " + path2);
                        spriteList.Add(frameSprite);
                        framesFound = true;
                        frameIndex++;
                        continue;
                    }

                    if (framesFound)
                    {
           //             ModernBoxLogger.Log("[FetchSprites] Finished loading sequence frames after index " + frameIndex);
                        break;
                    }

                    string path3 = "weapons/" + id + "/main_0_" + frameIndex;
                    frameSprite = Resources.Load<Sprite>(path3);

                    if (frameSprite != null)
                    {
          //              ModernBoxLogger.Log("[FetchSprites] Loaded sprite: " + path3);
                        spriteList.Add(frameSprite);
                        framesFound = true;
                        frameIndex++;
                        continue;
                    }

                    if (frameIndex > 0)
                    {
          //              ModernBoxLogger.Log("[FetchSprites] No more frames found beyond index: " + frameIndex);
                        break;
                    }

                    string bulkPath = "weapons/" + id;
                    Sprite[] sprites = Resources.LoadAll<Sprite>(bulkPath);
                    if (sprites != null && sprites.Length > 0)
                    {
           //             ModernBoxLogger.Log("[FetchSprites] Loaded " + sprites.Length + " sprites from: " + bulkPath);
                        spriteList.AddRange(sprites);
                        framesFound = true;
                    }
                    else
                    {
           //             ModernBoxLogger.Warning("[FetchSprites] No sprites found with LoadAll at: " + bulkPath);
                    }

                    break;

                    if (frameIndex > 20)
                    {
           //             ModernBoxLogger.Warning("[FetchSprites] Exceeded frame index limit (20). Aborting load.");
                        break;
                    }
                }

                if (framesFound && spriteList.Count > 0)
                {
           //         ModernBoxLogger.Log("[FetchSprites] Returning " + spriteList.Count + " animated sprites for: " + id);
                    return spriteList.ToArray();
                }
                else
                {
            //        ModernBoxLogger.Error("[FetchSprites] No animations found for: " + id);
                    var fallbackSprite = Resources.Load<Sprite>("weapons/" + id);
                    if (fallbackSprite != null)
                    {
            //            ModernBoxLogger.Log("[FetchSprites] Fallback sprite loaded for: " + id);
                        return new Sprite[] { fallbackSprite };
                    }
                    else
                    {
             //           ModernBoxLogger.Error("[FetchSprites] No fallback sprite found for: " + id);
                        return Array.Empty<Sprite>();
                    }
                }
            }
            else
            {
                var sprite = Resources.Load<Sprite>("weapons/" + id);
                if (sprite != null)
                {
           //         ModernBoxLogger.Log("[FetchSprites] Loaded non-animated sprite for: " + id);
                    return new Sprite[] { sprite };
                }
                else
                {
            //        ModernBoxLogger.Error("[FetchSprites] No sprite found for non-animated weapon: " + id);
                    return Array.Empty<Sprite>();
                }
            }
        }
    }
}
