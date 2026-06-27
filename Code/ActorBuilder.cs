using System;
using System.Collections.Generic;
using UnityEngine;
using ReflectionUtility; 
using NCMS;
using NCMS.Utils;

public class ActorBuilder
{
    private ActorAsset actor;
    private string id;

    public ActorBuilder(string id, string baseToClone = "$mob$")
    {
        this.id = id;
        actor = AssetManager.actor_library.clone(id, baseToClone);
        actor.name_locale = id; 
    }

    public ActorBuilder SetIsHumanoid(bool value) { actor.is_humanoid = value; return this; }
    public ActorBuilder SetCiv(bool value) { actor.civ = value; return this; }
    public ActorBuilder SetAnimationSpeedBasedOnWalkSpeed(bool value) { actor.animation_speed_based_on_walk_speed = value; return this; }
    public ActorBuilder SetHasAvatarPrefab(bool value) { actor.has_avatar_prefab = value; return this; }
    public ActorBuilder SetHasOverrideAvatarFrames(bool value) { actor.has_override_avatar_frames = value; return this; }
    public ActorBuilder SetImmuneToSlowness(bool value) { actor.immune_to_slowness = value; return this; }
    public ActorBuilder SetEffectDamage(bool value) { actor.effect_damage = value; return this; }
    public ActorBuilder SetUnitOther(bool value) { actor.unit_other = value; return this; }
    public ActorBuilder SetAffectedByDust(bool value) { actor.affected_by_dust = value; return this; }
    public ActorBuilder SetShowOnMetaLayer(bool value) { actor.show_on_meta_layer = value; return this; }
    public ActorBuilder SetShowInKnowledgeWindow(bool value) { actor.show_in_knowledge_window = value; return this; }
    public ActorBuilder SetShowInTaxonomyTooltip(bool value) { actor.show_in_taxonomy_tooltip = value; return this; }
    public ActorBuilder SetRenderStatusEffects(bool value) { actor.render_status_effects = value; return this; }
    public ActorBuilder SetUsePhenotypes(bool value) { actor.use_phenotypes = value; return this; }
    public ActorBuilder SetDeathAnimationAngle(bool value) { actor.death_animation_angle = value; return this; }
    public ActorBuilder SetCanBeInspected(bool value) { actor.can_be_inspected = value; return this; }
    public ActorBuilder SetUpdateZ(bool value) { actor.update_z = value; return this; }
    public ActorBuilder SetDisableJumpAnimation(bool value) { actor.disable_jump_animation = value; return this; }
    public ActorBuilder SetCanBeMovedByPowers(bool value) { actor.can_be_moved_by_powers = value; return this; }
    public ActorBuilder SetCanFlip(bool value) { actor.can_flip = value; return this; }
    public ActorBuilder SetDieInLava(bool value) { actor.die_in_lava = value; return this; }
    public ActorBuilder SetVisibleOnMinimap(bool value) { actor.visible_on_minimap = value; return this; }
    public ActorBuilder SetCanHaveSubspecies(bool value) { actor.can_have_subspecies = value; return this; }
    public ActorBuilder SetInspectChildren(bool value) { actor.inspect_children = value; return this; }
    public ActorBuilder SetSpecial(bool value) { actor.special = value; return this; }
    public ActorBuilder SetHasAdvancedTextures(bool value) { actor.has_advanced_textures = value; return this; }
    public ActorBuilder SetInspectSex(bool value) { actor.inspect_sex = value; return this; }
    public ActorBuilder SetInspectShowSpecies(bool value) { actor.inspect_show_species = value; return this; }
    public ActorBuilder SetInspectGeneration(bool value) { actor.inspect_generation = value; return this; }
    public ActorBuilder SetNeedsToBeExplored(bool value) { actor.needs_to_be_explored = value; return this; }
    public ActorBuilder SetForceLandCreature(bool value) { actor.force_land_creature = value; return this; }

    public ActorBuilder SetNameLocale(string name) { actor.name_locale = name; return this; }
    public ActorBuilder SetShadowTexture(string texture) { actor.shadow_texture = texture; return this; }
    public ActorBuilder SetCollectiveTerm(string term) { actor.collective_term = term; return this; }
    public ActorBuilder SetDefaultAttack(string attack) { actor.default_attack = attack; return this; }
    public ActorBuilder SetKingdomIdCivilization(string kingdom) { actor.kingdom_id_civilization = kingdom; return this; }
    public ActorBuilder SetBuildOrderTemplateId(string template) { actor.build_order_template_id = template; return this; }
    public ActorBuilder SetKingdomIdWild(string kingdom) { actor.kingdom_id_wild = kingdom; return this; }
    public ActorBuilder SetIcon(string icon) { actor.icon = icon; return this; }
    public ActorBuilder SetColorHex(string hex) { actor.color_hex = hex; return this; }

    public ActorBuilder SetOverrideAvatar(string spritePath, float scale = 1f, float offsetY = 0f)
    {
        actor.get_override_avatar_frames = (Actor pActor) => new Sprite[] { SpriteTextureLoader.getSprite(spritePath) };
        actor.has_override_avatar_frames = true;
        actor.has_avatar_prefab = false;
        actor.inspect_avatar_scale = scale;
        actor.inspect_avatar_offset_y = offsetY;
        return this;
    }

    public ActorBuilder SetAlwaysFlippable()
    {
        actor.can_flip = true;
        actor.check_flip = (BaseSimObject _, WorldTile _) => true;
        return this;
    }

    public ActorBuilder SetTexturePath(string folderPath, bool advancedTextures = false)
    {
        actor.texture_asset = new ActorTextureSubAsset(folderPath, advancedTextures);
        actor.has_advanced_textures = advancedTextures;
        return this;
    }

    public ActorBuilder SetAnimationWalk(string[] frames) 
    { 
        actor.animation_walk = frames; 
        return this; 
    }

    public ActorBuilder SetAnimationIdle(string[] frames)
    {
        actor.animation_idle = frames;
        return this;
    }

    public ActorBuilder SetAnimationSwim(string[] frames)
    {
        actor.animation_swim = frames;
        return this;
    }

    public ActorBuilder SetActorSize(ActorSize size) { actor.actor_size = size; return this; }

    public ActorBuilder SetJob(string jobKey) { actor.job = AssetLibrary<ActorAsset>.a<string>(jobKey); return this; }
    public ActorBuilder SetNameTemplateSet(string setKey) { actor.name_template_sets = AssetLibrary<ActorAsset>.a<string>(setKey); return this; }

    public ActorBuilder AddDecision(string decision) { actor.addDecision(decision); return this; }
    public ActorBuilder AddTrait(string trait) { actor.addTrait(trait); return this; }
    public ActorBuilder AddResource(string resource, int amount) { actor.addResource(resource, amount); return this; }

    public ActorBuilder SetExperienceGiven(int xp) { actor.experience_given = xp; return this; }

    public ActorBuilder SetLifespan(float value) { actor.base_stats["lifespan"] = value; return this; }
    public ActorBuilder SetMass2(float value) { actor.base_stats["mass_2"] = value; return this; }
    public ActorBuilder SetMass(float value) { actor.base_stats["mass"] = value; return this; }
    public ActorBuilder SetStamina(float value) { actor.base_stats["stamina"] = value; return this; }
    public ActorBuilder SetScale(float value) { actor.base_stats["scale"] = value; return this; }
    public ActorBuilder SetSize(float value) { actor.base_stats["size"] = value; return this; }
    public ActorBuilder SetHealth(float value) { actor.base_stats["health"] = value; return this; }
    public ActorBuilder SetSpeed(float value) { actor.base_stats["speed"] = value; return this; }
    public ActorBuilder SetArmor(float value) { actor.base_stats["armor"] = value; return this; }
    public ActorBuilder SetAttackSpeed(float value) { actor.base_stats["attack_speed"] = value; return this; }
    public ActorBuilder SetDamage(float value) { actor.base_stats["damage"] = value; return this; }
    public ActorBuilder SetKnockback(float value) { actor.base_stats["knockback"] = value; return this; }
    public ActorBuilder SetAccuracy(float value) { actor.base_stats["accuracy"] = value; return this; }
    public ActorBuilder SetTargets(float value) { actor.base_stats["targets"] = value; return this; }
    public ActorBuilder SetAreaOfEffect(float value) { actor.base_stats["area_of_effect"] = value; return this; }
    public ActorBuilder SetRange(float value) { actor.base_stats["range"] = value; return this; }
    public ActorBuilder SetCriticalDamageMultiplier(float value) { actor.base_stats["critical_damage_multiplier"] = value; return this; }
    public ActorBuilder SetMultiplierSupplyTimer(float value) { actor.base_stats["multiplier_supply_timer"] = value; return this; }

    public ActorBuilder SetBaseStat(string key, float value) { actor.base_stats[key] = value; return this; }

    public void Build()
    {
        AssetManager.actor_library.add(actor);
        Localization.addLocalization(actor.name_locale, actor.name_locale);
    }
}