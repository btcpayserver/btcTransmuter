using System;
using System.ComponentModel.DataAnnotations;

namespace BtcTransmuter.Extension.Timer.Triggers.Timer
{
    public class TimerTriggerParameters
    {
        public DateTime? LastTriggered { get; set; }    
        public DateTime? StartOn { get; set; }
        [Required]
        public int TriggerEveryAmount { get; set; }
        [Required]
        public TimerResetEvery TriggerEvery { get; set; }
        
        public enum TimerResetEvery
        {
            Minute,
            Hour,
            Day,
            Month,
            Year
        }
    }
}