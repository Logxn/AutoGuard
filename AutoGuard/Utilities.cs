using AutoGuard.SteamAPI;
using System;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace AutoGuard
{
    public class Utilities
    {

        public static string EncryptPassword(RsaParameters rsaParam)
        {
            // Convert the public keys to BigIntegers 
            var modulus = CreateBigInteger(rsaParam.Modulus);
            var exponent = CreateBigInteger(rsaParam.Exponent);

            //modulus has 256 bytes multiplied by 8 bits equals 2048 
            var encryptedNumber = Pkcs1Pad2(rsaParam.Password, (2048 + 7) >> 3);

            // And now, the RSA encryption 
            encryptedNumber = BigInteger.ModPow(encryptedNumber, exponent, modulus);

            //Reverse number and convert to base64 
            var encryptedString = Convert.ToBase64String(encryptedNumber.ToByteArray().Reverse().ToArray());

            return encryptedString;
        }

        public static BigInteger Pkcs1Pad2(string data, int keySize)
        {
            if (keySize < data.Length + 11)
                return new BigInteger();

            var buffer = new byte[256];
            var i = data.Length - 1;

            while (i >= 0 && keySize > 0)
            {
                buffer[--keySize] = (byte)data[i--];
            }

            var random = new Random();
            buffer[--keySize] = 0;
            while (keySize > 2)
            {
                buffer[--keySize] = (byte)random.Next(1, 256);
            }

            buffer[--keySize] = 2;
            buffer[--keySize] = 0;

            Array.Reverse(buffer);

            return new BigInteger(buffer);
        }

        public static BigInteger CreateBigInteger(string hex)
        {
            return BigInteger.Parse("00" + hex, NumberStyles.AllowHexSpecifier);
        }
    }

    public class RsaParameters
    {
        public string Exponent;
        public string Modulus;
        public string Password;
    }
}
