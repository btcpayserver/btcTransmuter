using NBXplorer.Models;

namespace BtcTransmuter.Extension.NBXplorer.Models
 {
     public class NBXplorerSummary
     {
         public NBXplorerState State { get; set; }
         public StatusResult Status { get; set; }
         public string Error { get; set; }
     }
 }