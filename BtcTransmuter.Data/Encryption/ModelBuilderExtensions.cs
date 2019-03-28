using System.Linq;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

namespace BtcTransmuter.Data.Encryption
{
    public static class ModelBuilderExtensions
    {
        public static void AddEncryptionValueConvertersToDecoratedEncryptedColumns(this ModelBuilder modelBuilder,
            IDataProtector dataProtector)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    var attributes = property.PropertyInfo.GetCustomAttributes(typeof(EncryptedAttribute), false);
                    if (attributes.Any())
                    {
                        property.SetValueConverter(new EncryptedConverter(dataProtector));
                    }
                }
            }
        }
    }
}