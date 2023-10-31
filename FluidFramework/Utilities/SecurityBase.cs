using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace FluidFramework.Utilities
{
    /// <summary>
    /// Base security class
    /// </summary>
    public class SecurityBase
    {
        /// <summary>
        /// Reverses the order of the bytes in the array.
        /// </summary>
        private byte[] ReverseArray(byte[] array)
        {
            byte[] reverse = new byte[array.Length];
            for(int i=0; i<array.Length; i++)
            {
                reverse[i] = array[i];
            }
            Array.Reverse(reverse);
            return reverse;
        }

        /// <summary>
        /// Creates a 16 byte key from the supplied password.
        /// </summary>
        private byte[] MakeKey(string password)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] key = new byte[16];
            if (passwordBytes.Length >= 16)
            {
                for(int i=0; i<16; i++)
                {
                    key[i] = passwordBytes[i];
                }
            }
            else
            {
                for (int i = 0; i < passwordBytes.Length; i++)
                {
                    key[i] = passwordBytes[i];
                }               
                for (int i = passwordBytes.Length; i < 16; i++)
                {
                    key[i] = Convert.ToByte(i % 10);
                }
            }
            return key;
        }

        /// <summary>
        /// Encrypts data with the given password using Rijndael.
        /// </summary>
        protected byte[] EncryptData(byte[] pData, string password)
        {
            byte[] encrKey = MakeKey(password);
            byte[] encrIV = ReverseArray(encrKey);
            MemoryStream mStream = new MemoryStream();
            // Defaults are: KeySize=256, BlockSize=128, Mode=CBC, PaddingMode=PKCS7
            ICryptoTransform transform = (new RijndaelManaged { KeySize = 128, Mode = CipherMode.CBC }).CreateEncryptor(encrKey, encrIV);
            CryptoStream cStream = new CryptoStream(mStream, transform, CryptoStreamMode.Write);
            cStream.Write(pData, 0, pData.Length);
            cStream.FlushFinalBlock();
            cStream.Close();
            return mStream.ToArray();
        }

        /// <summary>
        /// Decrypts data with the given password using Rijndael.
        /// </summary>
        protected byte[] DecryptData(byte[] pData, string password)
        {
            byte[] decrKey = MakeKey(password);
            byte[] decrIV = ReverseArray(decrKey);
            MemoryStream mStream = new MemoryStream();
            // Defaults are: KeySize=256, BlockSize=128, Mode=CBC, PaddingMode=PKCS7
            ICryptoTransform transform = (new RijndaelManaged { KeySize = 128, Mode = CipherMode.CBC }).CreateDecryptor(decrKey, decrIV);
            CryptoStream dStream = new CryptoStream(mStream, transform, CryptoStreamMode.Write);
            dStream.Write(pData, 0, pData.Length);
            dStream.FlushFinalBlock();
            dStream.Close();
            return mStream.ToArray();
        }
    }
}
