using System;
using System.Linq.Expressions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BtcTransmuter.Data.Encryption
{
    class EncryptedConverter : ValueConverter<string, string>
    {
        private static IDataProtector _dataProtector;

        public EncryptedConverter(IDataProtector dataProtector, ConverterMappingHints mappingHints = default)
            : base(EncryptExpr, DecryptExpr, mappingHints)
        {
            _dataProtector = dataProtector;
        }

        static Expression<Func<string, string>> DecryptExpr = x => _dataProtector.Unprotect(x);
        static Expression<Func<string, string>> EncryptExpr = x => _dataProtector.Protect(x);
    }
}