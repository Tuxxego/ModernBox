using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using ReflectionUtility;
using NCMS;

public class CloudBuilder
{
    private string id;
    private string name;
    private string colorHex = "#FFFFFF";
    private float speedMax = 4f;
    private string[] pathSprites;
    private CloudAction cloudAction;

    private string dropTexturePath;
    private float dropScale = 0.2f;

    private DropsAction actionCloudSpawn; 

    private DropsAction actionCloudRain;  

    private float fallingChance = 0.01f;
    private PowerAction clickPowerAction;

    public CloudBuilder(string id)
    {
        this.id = id;
    }

    public CloudBuilder SetName(string name)
    {
        this.name = name;
        return this;
    }

    public CloudBuilder SetColorHex(string hex)
    {
        this.colorHex = hex;
        return this;
    }

    public CloudBuilder SetSpeedMax(float speed)
    {
        this.speedMax = speed;
        return this;
    }

    public CloudBuilder SetPathSprites(string[] sprites)
    {
        this.pathSprites = sprites;
        return this;
    }

    public CloudBuilder SetCloudAction(CloudAction action)
    {
        this.cloudAction = action;
        return this;
    }

    public CloudBuilder SetDropTexturePath(string path)
    {
        this.dropTexturePath = path;
        return this;
    }

    public CloudBuilder SetDropScale(float scale)
    {
        this.dropScale = scale;
        return this;
    }

    public CloudBuilder SetActionCloudSpawn(DropsAction action)
    {
        this.actionCloudSpawn = action;
        return this;
    }

    public CloudBuilder SetActionCloudRain(DropsAction action)
    {
        this.actionCloudRain = action;
        return this;
    }

    public CloudBuilder SetFallingChance(float chance)
    {
        this.fallingChance = chance;
        return this;
    }

    public CloudBuilder SetClickPowerAction(PowerAction action)
    {
        this.clickPowerAction = action;
        return this;
    }

    public void Build()
    {

        string spawnCloudDropId = $"spawn_{id}";
        string spawnRainDropId = $"spawn_{id}_2";

        CloudAsset cloud = new CloudAsset
        {
            id = this.id,
            color_hex = this.colorHex,
            drop_id = spawnRainDropId, 

            cloud_action_1 = this.cloudAction,
            path_sprites = this.pathSprites,
            speed_max = this.speedMax
        };

        FieldInfo cachedField = typeof(CloudAsset).GetField("cached_sprites", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (cachedField != null && pathSprites != null && pathSprites.Length > 0)
        {
            List<Sprite> loadedSprites = new List<Sprite>(pathSprites.Length);
            for (int i = 0; i < pathSprites.Length; i++)
            {
                if (string.IsNullOrEmpty(pathSprites[i])) continue;

                Sprite sprite = Resources.Load<Sprite>(pathSprites[i]);
                if (sprite == null)
                {
                    Debug.LogWarning($"[CloudBuilder] Failed to load sprite: {pathSprites[i]}");
                    continue;
                }
                loadedSprites.Add(sprite);
            }
            cachedField.SetValue(cloud, loadedSprites.ToArray());
        }
        else if (cachedField == null)
        {
            Debug.LogWarning("[CloudBuilder] Could not find cached_sprites field via reflection!");
        }

        AssetManager.clouds.add(cloud);

        DropAsset cloudSpawnDrop = new DropAsset
        {
            id = spawnCloudDropId,
            path_texture = dropTexturePath,
            default_scale = dropScale,
            random_frame = true,
            random_flip = true,
            action_landed = actionCloudSpawn
        };
        AssetManager.drops.add(cloudSpawnDrop);

        DropAsset payloadDrop = new DropAsset
        {
            id = spawnRainDropId,
            path_texture = dropTexturePath,
            default_scale = dropScale,
            random_frame = true,
            random_flip = true,
            action_landed = actionCloudRain
        };
        AssetManager.drops.add(payloadDrop);

        GodPower power = new GodPower
        {
            id = this.id,
            name = string.IsNullOrEmpty(this.name) ? this.id : this.name,
            hold_action = true,
            show_tool_sizes = true,
            ignore_cursor_icon = true,
            falling_chance = this.fallingChance,
            drop_id = spawnCloudDropId, 

            click_power_action = this.clickPowerAction,
            click_power_brush_action = new PowerAction((WorldTile pTile, GodPower pPower) =>
            {
                return (bool)AssetManager.powers.CallMethod("loopWithCurrentBrushPowerForDropsFull", pTile, pPower);
            })
        };

        FieldInfo dropField = typeof(GodPower).GetField("cached_drop_asset", BindingFlags.NonPublic | BindingFlags.Instance);
        if (dropField != null)
        {
            dropField.SetValue(power, cloudSpawnDrop);
        }

        AssetManager.powers.add(power);
    }
}