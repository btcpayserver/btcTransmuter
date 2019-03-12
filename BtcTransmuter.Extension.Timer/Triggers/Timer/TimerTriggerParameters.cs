using System;
using System.ComponentModel.DataAnnotations;

namespace BtcTransmuter.Extension.Timer.Triggers.Timer
{
    public class TimerTriggerParameters
    {
        public DateTime? LastTriggered { get; set; }    
        
        [Display(Name = "Start from")]
        public DateTime? StartOn { get; set; }
        [Required][Display(Name = "Trigger every")]
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