using System;
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
        public bool MuteSound;

        [JsonProperty]
        public string InstallVersion;
        
        public PlayerData()
        {
            InstallDate = DateTime.Now;
            LastSessionDate = DateTime.Now;
            InstallVersion = Application.version;
        }
    }
}