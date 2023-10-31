using System.Collections.Generic;
using System.IO;

namespace FluidFramework.Utilities
{
    /// <summary>
    /// Helper class used to execute scripts.
    /// </summary>
    public class Script
    {
        private List<MagicSplit.Block> _script;

        /// <summary>
        /// Constructor
        /// </summary>
        public Script(string source, bool isfile = false, bool filter = false)
        {
            if (isfile)
            {
                source = File.ReadAllText(source);
            }
            if (filter)
            {
                source = FilterScript.CleanSqlComment(source);
            }
            _script = MagicSplit.Split(ref source);
        }

        private int GetBlockCount()
        {
            return _script.Count;
        }

        private bool IsBlockValid(int index)
        {
            return _script[index].Data.Length > 0;
        }

        private string GetBlock(int index)
        {
            return _script[index].Data;
        }

        private int GetRepeatCount(int index)
        {
            return _script[index].Repeat;
        }

        /// <summary>
        /// Executes the script against the connection specified by the connection string.
        /// </summary>        
        public void ExecuteScript(string connectionString, bool useTransaction = true, int commandTimeout = 3600)
        {
            DataUtils.ExecuteCustomSqlScript(connectionString, GetBlockCount, IsBlockValid, GetBlock, GetRepeatCount, null, null, useTransaction, commandTimeout);
        }
    }
}
