using System;
using UnityEngine;
using UnityEngine.Events;
using NCMS.Utils;
using NeoModLoader.General;
using ReflectionUtility;
using System.Reflection;
using ModernBox;

public class ButtonBuilder
{
    private string id;
    private Sprite sprite;
    private string title;
    private string description;
    private Vector2 position;
    private ButtonType type;
    private Transform parent;
    private UnityAction function;

    private int gridX = 0;
    private int gridY = 0;

    private bool isUnitSpawner = false;
    private string actorAssetId = null;
    private PowerRank? powerRank = null;
    private bool? showUnitStatsOverview = null;
    private bool? unselectWhenWindow = null;
    private bool? showSpawnEffect = null;
    private bool? multipleSpawnTip = null;
    private float? actorSpawnHeight = null;
    private string godPowerIconPath = null;
    private string godPowerName = null;
    private string godPowerDesc = null;
    private PowerActionWithID customClickAction = null;

    public ButtonBuilder(string id)
    {
        this.id = id;
    }

    public ButtonBuilder SetSprite(Sprite sprite)
    {
        this.sprite = sprite;
        return this;
    }

    public ButtonBuilder SetTitle(string title)
    {
        this.title = title;
        return this;
    }

    public ButtonBuilder SetDescription(string description)
    {
        this.description = description;
        return this;
    }

    public ButtonBuilder SetPosition(int xGrid, int yGrid)
    {
        gridX = xGrid;
        gridY = yGrid;
        float xPos = 72 + (36 * xGrid);
        float yPos = 18 - (36 * yGrid);
        position = new Vector2(xPos, yPos);
        return this;
    }

    public ButtonBuilder SetPositionWindow(int xGrid, int yGrid)
    {
        gridX = xGrid;
        gridY = yGrid;
        float xPos = 0 + (36 * xGrid);
        float yPos = 18 - (36 * yGrid);
        position = new Vector2(xPos, yPos);
        return this;
    }

    public ButtonBuilder SetType(ButtonType type)
    {
        this.type = type;
        return this;
    }

    public ButtonBuilder SetTransform(Transform parent)
    {
        this.parent = parent;
        return this;
    }

    public ButtonBuilder SetFunction(UnityAction action)
    {
        this.function = action;
        return this;
    }

    public ButtonBuilder AsUnitSpawner(string actorAssetId)
    {
        this.isUnitSpawner = true;
        this.actorAssetId = actorAssetId;
        return this;
    }

    public ButtonBuilder SetPowerRank(PowerRank rank)
    {
        this.powerRank = rank;
        return this;
    }

    public ButtonBuilder SetShowUnitStatsOverview(bool value)
    {
        this.showUnitStatsOverview = value;
        return this;
    }

    public ButtonBuilder SetUnselectWhenWindow(bool value)
    {
        this.unselectWhenWindow = value;
        return this;
    }

    public ButtonBuilder SetShowSpawnEffect(bool value)
    {
        this.showSpawnEffect = value;
        return this;
    }

    public ButtonBuilder SetMultipleSpawnTip(bool value)
    {
        this.multipleSpawnTip = value;
        return this;
    }

    public ButtonBuilder SetActorSpawnHeight(float value)
    {
        this.actorSpawnHeight = value;
        return this;
    }

    public ButtonBuilder SetGodPowerIconPath(string path)
    {
        this.godPowerIconPath = path;
        return this;
    }

    public ButtonBuilder SetGodPowerName(string name)
    {
        this.godPowerName = name;
        return this;
    }

    public ButtonBuilder SetCustomClickAction(PowerActionWithID action)
    {
        this.customClickAction = action;
        return this;
    }

    public void Build()
    {
        if (isUnitSpawner)
        {
            GodPower power = AssetManager.powers.clone(id, "$template_spawn_actor$");
            power.id = id;
            power.name = godPowerName ?? title ?? id;
            power.type = PowerActionType.PowerSpawnActor;
            power.actor_asset_id = actorAssetId;
            if (powerRank.HasValue) power.rank = powerRank.Value;
            if (showUnitStatsOverview.HasValue) power.show_unit_stats_overview = showUnitStatsOverview.Value;
            if (unselectWhenWindow.HasValue) power.unselect_when_window = unselectWhenWindow.Value;
            if (showSpawnEffect.HasValue) power.show_spawn_effect = showSpawnEffect.Value;
            if (multipleSpawnTip.HasValue) power.multiple_spawn_tip = multipleSpawnTip.Value;
            if (actorSpawnHeight.HasValue) power.actor_spawn_height = actorSpawnHeight.Value;
            if (customClickAction != null) power.click_action = customClickAction;

            AssetManager.powers.add(power);

            Sprite useSprite = sprite;
            if (useSprite == null && !string.IsNullOrEmpty(godPowerIconPath))
                useSprite = Resources.Load<Sprite>(godPowerIconPath);
            if (useSprite == null)
                useSprite = Resources.Load<Sprite>("ui/icons/" + id);

            PowerButtons.CreateButton(
                id,
                useSprite,
                power.name,
                description ?? power.name,
                position,
                ButtonType.GodPower,
                parent,
                null
            );
        }
        else
        {
			LM.AddToCurrentLocale(id, title);
            LM.AddToCurrentLocale(id + "_description", description);
            LM.ApplyLocale(true);
            switch (type)
            {
                case ButtonType.Click:
                    PowerButtons.CreateButton(id, sprite, title, description, position, type, parent, function);
                    ModernBoxLogger.Log($"[ButtonBuilder] Created button with: ID={id}, Type=Click, Title={title}, Position={position}, Parent={parent?.name}");
                    break;

                case ButtonType.Toggle:
                    PowerButtons.CreateButton(id, sprite, title, description, position, type, parent, function);
                    ModernBoxLogger.Log($"[ButtonBuilder] Created button with: ID={id}, Type=Toggle, Title={title}, Position={position}, Parent={parent?.name}");
                    break;

                case ButtonType.GodPower:
                    PowerButtons.CreateButton(id, sprite, title, description, position, type, parent, function);
                    ModernBoxLogger.Log($"[ButtonBuilder] Created button with: ID={id}, Type=GodPower, Title={title}, Position={position}, Parent={parent?.name}");
                    break;

                default:
                    return;
            }
        }

    }
}

/*

new ButtonBuilder("spawnGodzilla")
    .AsUnitSpawner("godzilla")
    .SetGodPowerName("Godzilla")
    .SetDescription("uwu")
    .SetGodPowerIconPath("ui/icons/godzilla")
    .SetPosition(0, 0)
    .SetTransform(tab.transform)
    .Build();


new ButtonBuilder("MOABbutton")
    .SetSprite(Resources.Load<Sprite>("ui/icons/MOAB"))
    .SetTitle("Super-Nuke")
    .SetDescription("MOTHER OF ALL BOMBS")
    .SetPosition(14, 0)
    .SetType(ButtonType.GodPower)
    .SetTransform(tab.transform)
    .Build();

    new ButtonBuilder("toggleButton")
    .SetSprite(sprite)
    .SetTitle("Toggle Button")
    .SetPosition(0, 0)
    .SetType(ButtonType.Toggle)
    .SetTransform(parent)
    .Build();


*/
