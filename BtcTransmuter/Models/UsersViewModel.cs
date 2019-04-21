using System.Collections.Generic;
using BtcTransmuter.Data.Entities;

namespace BtcTransmuter.Models
{
    public class UsersViewModel
    {
        public string StatusMessage{ get; set; }
        public List<User > Users{ get; set; }
    }
}