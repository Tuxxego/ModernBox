using System;
using System.Collections.Generic;
using UnityEngine;

public class SaveReader
{
    private string _savePath;
    private SavedMap _savedMapData;

    public SaveReader(int slot)
    {
        _savePath = SaveManager.getSlotSavePath(slot);
    }

    public SaveReader(string customPath)
    {
        _savePath = SaveManager.folderPath(customPath);
    }

    public bool ReadFile()
    {
        if (!SaveManager.doesSaveExist(_savePath))
        {
            Debug.LogWarning($"SaveReader: No valid save found at {_savePath}");
            return false;
        }

        _savedMapData = SaveManager.getMapFromPath(_savePath);
        return _savedMapData != null;
    }

    public SavedMap GetRawSaveData()
    {
        return _savedMapData;
    }

    public MapMetaData ReadMetaData()
    {

        return SaveManager.getMetaFor(_savePath);
    }

    public MapStats ReadMapStats()
    {
        EnsureDataLoaded();
        return _savedMapData.mapStats;
    }

    public List<KingdomData> ReadKingdoms()
    {
        EnsureDataLoaded();
        return _savedMapData.kingdoms;
    }

    public List<CityData> ReadCities()
    {
        EnsureDataLoaded();
        return _savedMapData.cities;
    }

    public List<ActorData> ReadActors()
    {
        EnsureDataLoaded();
        return _savedMapData.actors_data;
    }

    public List<CultureData> ReadCultures()
    {
        EnsureDataLoaded();
        return _savedMapData.cultures;
    }

    public List<ClanData> ReadClans()
    {
        EnsureDataLoaded();
        return _savedMapData.clans;
    }

    public List<AllianceData> ReadAlliances()
    {
        EnsureDataLoaded();
        return _savedMapData.alliances;
    }

    public List<WarData> ReadWars()
    {
        EnsureDataLoaded();
        return _savedMapData.wars;
    }

    public List<PlotData> ReadPlots()
    {
        EnsureDataLoaded();
        return _savedMapData.plots;
    }

    public WorldLaws ReadWorldLaws()
    {
        EnsureDataLoaded();
        return _savedMapData.worldLaws;
    }

    public bool DoesDatabaseExist()
    {
        return SaveManager.doesStatsDBExist(_savePath);
    }

    public string GetMapCreationTime()
    {
        return SaveManager.getMapCreationTime(_savePath);
    }

    public void Dispose()
    {
        _savedMapData = null;
    }

    private void EnsureDataLoaded()
    {
        if (_savedMapData == null)
        {
            throw new InvalidOperationException("Save data has not been loaded. Call ReadFile() before attempting to extract data.");
        }
    }
}