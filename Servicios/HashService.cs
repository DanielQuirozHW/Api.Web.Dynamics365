using Api.Web.Dynamics365.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace Api.Web.Dynamics365.Servicios
{
    public class HashService
    {
        public Hash Hash(string texto)
        {
            var sal = new byte[16];

            using(var random = RandomNumberGenerator.Create())
            {
                random.GetBytes(sal);
            }

            return Hash(texto, sal);
        }

        public Hash Hash(string texto, byte[] sal)
        {
            var llaveDerivada = KeyDerivation.Pbkdf2(password: texto,
                salt: sal, prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 32);

            var hash = Convert.ToBase64String(llaveDerivada);

            return new Hash
            {
                hash = hash,
                sal = sal
            };
        }
    }
}
