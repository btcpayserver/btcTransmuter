using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace BtcTransmuter.Data.Entities
{
    public class User : IdentityUser
    {
        public List<Recipe> Recipes { get; set; }
        public List<U2FDevice> U2FDevices { get; set; }
    }
}
