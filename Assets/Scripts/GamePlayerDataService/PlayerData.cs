using System;
using Core.Models;
using Newtonsoft.Json;
using UnityEngine;

namespace Services.GamePlayerDataService
{
    public class PlayerData
    {
        [JsonProperty]
        public int MaxScore;
        
        [JsonProperty]
        public DateTime InstallDate;
        
        [JsonProperty]
        public DateTime LastSessionDate;
        
        [JsonProperty]
        public DateTime AdsLastShownDate;
        
        [JsonProperty]
        public bool SoundEnabled;
        
        [JsonProperty]
        public bool MusicEnabled;

        [JsonProperty]
        public string InstallVersion;
        
        [JsonProperty]
        public int GamesPlayed;
        
        [JsonProperty]
        public BoardModel BoardModel;
        
        public PlayerData()
        {
            SoundEnabled = true;
            MusicEnabled = true;
            InstallDate = DateTime.Now;
            LastSessionDate = DateTime.Now;
            InstallVersion = Application.version;
        }
    }
}