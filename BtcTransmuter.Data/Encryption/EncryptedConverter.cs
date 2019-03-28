using System;
using System.Linq.Expressions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BtcTransmuter.Data.Encryption
{
    class EncryptedConverter : ValueConverter<string, string>
    {
        private static IDataProtector _dataProtector;
        private static string encryptedMarker = "ENCRYPTED:";

        public EncryptedConverter(IDataProtector dataProtector, ConverterMappingHints mappingHints = default)
            : base(EncryptExpr, DecryptExpr, mappingHints)
        {
            _dataProtector = dataProtector;
        }

        static Expression<Func<string, string>> DecryptExpr = x => x.StartsWith(encryptedMarker)?  _dataProtector.Unprotect(x.Substring(encryptedMarker.Length)): x;
        static Expression<Func<string, string>> EncryptExpr = x => $"{encryptedMarker}{_dataProtector.Protect(x)}";
    }
}