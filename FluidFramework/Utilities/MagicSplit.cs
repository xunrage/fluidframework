using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace FluidFramework.Utilities
{
    /// <summary>
    /// This class allows the splitting of SQL text in blocks separated by the GO statement.
    /// </summary>
    public static class MagicSplit
    {
        /// <summary>
        /// A structure type for the block of text.
        /// </summary>
        public struct Block
        {
            /// <summary>
            /// The block content
            /// </summary>
            public string Data;

            /// <summary>
            /// The block repetition
            /// </summary>
            public int Repeat;
        }

        private enum QuotesType { Unknown, Single, Double }
        private enum CommentType { Unknown, Long, Line }

        private static bool ShouldMerge(ref string block)
        {
            bool insideQuotes = false;
            bool insideComment = false;
            QuotesType quotes = QuotesType.Unknown;
            CommentType comment = CommentType.Unknown;

            for (int i = 0; i < block.Length; i++)
            {
                switch (block[i])
                {
                    // Single Quotes
                    case '\x0027':
                        if (!insideComment)
                        {
                            if (insideQuotes)
                            {
                                if (quotes == QuotesType.Single)
                                {
                                    insideQuotes = false;
                                    quotes = QuotesType.Unknown;
                                }
                            }
                            else
                            {
                                insideQuotes = true;
                                quotes = QuotesType.Single;
                            }
                        }
                        break;
                    // Double Quotes
                    case '\x0022':
                        if (!insideComment)
                        {
                            if (insideQuotes)
                            {
                                if (quotes == QuotesType.Double)
                                {
                                    insideQuotes = false;
                                    quotes = QuotesType.Unknown;
                                }
                            }
                            else
                            {
                                insideQuotes = true;
                                quotes = QuotesType.Double;
                            }
                        }
                        break;
                    // Start Long Sql Comment
                    case '/':
                        if (!insideQuotes && !insideComment)
                        {
                            if (i < block.Length - 1 && block[i + 1] == '*')
                            {
                                insideComment = true;
                                comment = CommentType.Long;
                                i++;
                            }
                        }
                        break;
                    // End Long Sql Comment
                    case '*':
                        if (insideComment && comment == CommentType.Long)
                        {
                            if (i < block.Length - 1 && block[i + 1] == '/')
                            {
                                insideComment = false;
                                comment = CommentType.Unknown;
                                i++;
                            }
                        }
                        break;
                    // Start Line Sql Comment
                    case '-':
                        if (!insideQuotes && !insideComment)
                        {
                            if (i < block.Length - 1 && block[i + 1] == '-')
                            {
                                insideComment = true;
                                comment = CommentType.Line;
                                i++;
                            }
                        }
                        break;
                    // End Line Sql Comment
                    case '\x000A':
                        if (insideComment && comment == CommentType.Line)
                        {
                            insideComment = false;
                            comment = CommentType.Unknown;
                        }
                        break;
                }
            }
            return insideQuotes || insideComment;
        }

        private static int GetBatchRepeat(ref string block)
        {
            int i = 0;
            string line = string.Empty;
            while (i < block.Length)
            {
                if (block[i] == Convert.ToChar(0x0A))
                {
                    break;
                }
                if (block[i] >= Convert.ToChar(0x20))
                {
                    line += block[i];
                }
                i++;
            }

            block = i + 1 < block.Length ? block.Substring(i + 1) : string.Empty;
            line = line.Trim();
            if (line.Length > 0)
            {
                i = 0;
                string val = string.Empty;
                while (i < line.Length)
                {
                    if ((line[i] >= '0') && (line[i] <= '9'))
                    {
                        val += line[i];
                    }
                    else
                    {
                        break;
                    }
                    i++;
                }
                if (val.Length > 0)
                {
                    return Convert.ToInt32(val);
                }
                return 1;
            }
            return 1;
        }

        private static List<Block> SplitScript(ref string script, string goRegularExpression, RegexOptions goRegularOptions)
        {
            Regex regex = new Regex(goRegularExpression, goRegularOptions);

            string[] data = regex.Split(script);
            bool[] merged = new bool[data.Length];
            int[] repeat = new int[data.Length];
            string[] matches = new string[data.Length];

            int i;
            for (i = 0; i < data.Length; i++)
            {
                merged[i] = false;
                repeat[i] = 1;
            }

            i = 0;
            matches[0] = String.Empty;
            foreach (Match match in Regex.Matches(script, goRegularExpression, goRegularOptions))
            {
                i++;
                matches[i] = match.Value;
            }

            if (data.Length > 1)
            {
                for (i = 0; i < data.Length - 1; i++)
                {
                    if (ShouldMerge(ref data[i]))
                    {
                        merged[i] = true;
                        data[i + 1] = data[i] + matches[i + 1] + data[i + 1];
                        data[i] = string.Empty;
                        matches[i + 1] = matches[i];
                    }
                    if (!merged[i])
                    {
                        repeat[i] = GetBatchRepeat(ref data[i + 1]);
                    }
                }
            }
            List<Block> blocks = new List<Block>();
            for (i = 0; i < data.Length; i++)
            {
                if (!merged[i] && data[i].Trim() != "") blocks.Add(new Block { Data = data[i], Repeat = repeat[i] });

            }
            return blocks;
        }

        /// <summary>
        /// Splits the script in blocks of text.
        /// </summary>
        public static List<Block> Split(ref string script)
        {
            List<Block> blocks = SplitScript(ref script, @"^[ \t]*GO$", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            for (int i = 0; i < blocks.Count; i++)
            {
                string block = blocks[i].Data;
                List<Block> subBlocks = SplitScript(ref block, @"^[ \t]*GO(?=\P{L}[ \t]*)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                if (subBlocks.Count > 1)
                {
                    blocks.RemoveAt(i);
                    blocks.InsertRange(i, subBlocks);
                }
            }
            return blocks;
        }
    }
}