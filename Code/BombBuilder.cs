using System;
using System.Reflection;
using UnityEngine;
using ReflectionUtility;
using NCMS;

public class BombBuilder
{
    private string id;
    private string texturePath;
    private float dropScale = 0.2f;
    private float fallingChance = 0.01f;
    private PowerAction clickPowerAction;
    private DropsAction onDropLandAction;
    private WorldAction bombEffectAction;
    private float damage = 5000;
    private int explodeStrength = 50;

    public BombBuilder(string id)
    {
        this.id = id;
    }

    public BombBuilder SetTexturePath(string path)
    {
        texturePath = path;
        return this;
    }

    public BombBuilder SetDropScale(float scale)
    {
        dropScale = scale;
        return this;
    }

    public BombBuilder SetFallingChance(float chance)
    {
        fallingChance = chance;
        return this;
    }

    public BombBuilder SetDropLandAction(DropsAction action)
    {
        onDropLandAction = action;
        return this;
    }

    public BombBuilder SetClickPowerAction(PowerAction action)
    {
        clickPowerAction = action;
        return this;
    }

    public BombBuilder SetBombEffectAction(WorldAction action)
    {
        bombEffectAction = action;
        return this;
    }

    public BombBuilder SetDamage(float dmg)
    {
        damage = dmg;
        return this;
    }

    public BombBuilder SetExplodeStrength(int strength)
    {
        explodeStrength = strength;
        return this;
    }

    public void Build()
    {

        DropAsset drop = new DropAsset
        {
            id = $"spawn_{id}",
            path_texture = texturePath,
            default_scale = dropScale,
            random_frame = true,
            random_flip = true,
            action_landed = onDropLandAction
        };
        AssetManager.drops.add(drop);

        GodPower power = new GodPower
        {
            id = $"{id}_button",
            name = $"{id}_button",
            hold_action = true,
            show_tool_sizes = true,
            ignore_cursor_icon = true,
            falling_chance = fallingChance,
            drop_id = drop.id,
            click_power_action = clickPowerAction,
            click_power_brush_action = new PowerAction((WorldTile pTile, GodPower pPower) =>
            {
                return (bool)AssetManager.powers.CallMethod("loopWithCurrentBrushPowerForDropsFull", pTile, pPower);
            })
        };

        FieldInfo dropField = typeof(GodPower).GetField("cached_drop_asset", BindingFlags.NonPublic | BindingFlags.Instance);
        if (dropField != null)
            dropField.SetValue(power, drop);

        AssetManager.powers.add(power);

        TerraformOptions options = new TerraformOptions
        {
            id = id,
            damage = (int)damage,
            flash = true,
            shake = true,
            apply_force = true,
            explode_tile = true,
            explosion_pixel_effect = true,
            damage_buildings = true,
            remove_tornado = true,
            remove_frozen = true,
            explode_and_set_random_fire = true,
            attack_type = AttackType.Explosion,
            explode_strength = explodeStrength,
            bomb_action = bombEffectAction
        };

        AssetManager.terraform.add(options);
    }
}