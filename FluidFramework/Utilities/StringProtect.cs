using System;
using System.Security.Cryptography;
using System.Text;

namespace FluidFramework.Utilities
{
    /// <summary>
    /// Provides a mechanism to protect strings.
    /// </summary>
    public class StringProtect : SecurityBase
    {
        #region Private Members
        private string _token;
        #endregion

        #region Private Methods

        private string GetPassword()
        {
            if (_token == null)
            {
                throw new ArgumentNullException("token");                
            }
            
            byte[] pool = Encoding.UTF8.GetBytes(_token);
            byte[] passbytes = new byte[16];
            for (int j = 0; j < 16; j++)
            {
                passbytes[j] = pool[pool[j * 2] + pool[j * 2 + 1]];
            }
            return Encoding.UTF8.GetString(passbytes);
        }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>        
        public StringProtect(string token)
        {
            if (token == null)
            {
                throw new ArgumentNullException("token");
            }
            if (token.Length != 256)
            {
                throw new ArgumentException("Token requires a length of 256 characters.", "token");
            }
            _token = token;
        }

        #region Public Methods

        /// <summary>
        /// Creates a token.
        /// </summary>
        public string CreateToken(int? seed = null)
        {
            byte[] token = new byte[256];
            Random random = seed.HasValue ? new Random(seed.Value) : new Random();
            for (int i = 0; i < 256; i++)
            {
                token[i] = Convert.ToByte(random.Next(35, 127));
            }
            return Encoding.UTF8.GetString(token);
        }

        /// <summary>
        /// Encrypts and encodes the content.
        /// </summary>
        public string EncryptString(string pContent)
        {
            if(String.IsNullOrEmpty(pContent)) return pContent;
            byte[] dataBytes = EncryptData(Encoding.UTF8.GetBytes(pContent), GetPassword());
            return Convert.ToBase64String(dataBytes);
        }

        /// <summary>
        /// Decodes and decrypts the content.
        /// </summary>
        public string DecryptString(string pContent)
        {
            if(String.IsNullOrEmpty(pContent)) return pContent;
            byte[] dataBytes = Convert.FromBase64String(pContent);
            return Encoding.UTF8.GetString(DecryptData(dataBytes, GetPassword()));
        }

        /// <summary>
        /// Returns the MD5 hexadecimal hash of the content.
        /// </summary>
        public string CalculateMD5Hash(string pContent)
        {
            MD5 md5 = MD5.Create();
            byte[] dataBytes = Encoding.UTF8.GetBytes(pContent);
            byte[] hash = md5.ComputeHash(dataBytes);

            StringBuilder sb = new StringBuilder();
            foreach (byte t in hash)
            {
                sb.Append(t.ToString("x2"));
            }
            return sb.ToString();
        }

        #endregion
    }
}
