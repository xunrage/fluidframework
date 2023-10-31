using System;
using System.Collections;

namespace FluidFramework.Utilities
{
    /// <summary>
    /// This class filters a SQL text by removing the comments and empty lines.
    /// </summary>
    public static class FilterScript
    {
        /// <summary>
        /// The style used to trim the lines of text.
        /// </summary>
        public enum TrimStyle
        {
            /// <summary>
            /// No trimming is applied.
            /// </summary>
            None,
            /// <summary>
            /// Removes all leading and trailing white-space characters
            /// </summary>
            Trim,
            /// <summary>
            /// Removes all leading white-space characters
            /// </summary>
            TrimStart,
            /// <summary>
            /// Removes all trailing white-space characters
            /// </summary>
            TrimEnd
        }

        /// <summary>
        /// Returns a new string instance with the sql comments removed. 
        /// </summary>
        public static string CleanSqlComment(string s)
        {
            bool inside = false;
            bool singleQuotes = true;
            bool hasEmptyLine = false;
            StringStream result = new StringStream();
            for (int i = 0; i < s.Length; i++)
            {
                // Single Quotes
                if (s[i] == Convert.ToChar(0x27))
                {
                    if(inside)
                    {
                        if (singleQuotes) inside = false;                        
                    }
                    else
                    {
                        inside = true;
                        singleQuotes = true;
                    }
                }
                // Double Quotes
                if (s[i] == Convert.ToChar(0x22))
                {
                    if(inside)
                    {
                        if (!singleQuotes) inside = false;
                    }
                    else
                    {
                        inside = true;
                        singleQuotes = false;
                    }
                }
                // Long Sql Comment
                if ((s[i] == '/') && (!inside))
                {
                    if (i < s.Length - 1)
                    {
                        if (s[i + 1] == '*')
                        {
                            bool search = true;
                            i++;
                            while ((i < s.Length - 1) && (search))
                            {
                                i++;
                                if (s[i] == '*')
                                {
                                    if (i < s.Length - 1)
                                    {
                                        if (s[i + 1] == '/')
                                        {
                                            i++;
                                            search = false;
                                        }
                                    }
                                }
                            }
                            result.Add(' ');
                            hasEmptyLine = false;
                        }
                        else
                        {
                            result.Add(s[i]);
                            hasEmptyLine = false;
                        }
                    }
                    else
                    {
                        result.Add(s[i]);
                        hasEmptyLine = false;
                    }
                }
                else
                {
                    // Line Sql Comment
                    if ((s[i] == '-') && (!inside))
                    {
                        if (i < s.Length - 1)
                        {
                            if (s[i + 1] == '-')
                            {
                                bool search = true;
                                i++;
                                while ((i < s.Length - 1) && (search))
                                {
                                    i++;
                                    if (s[i] == Convert.ToChar(0x0A))
                                    {
                                        search = false;
                                    }
                                }
                                if (!hasEmptyLine)
                                {
                                    result.Add(Environment.NewLine);
                                    hasEmptyLine = true;
                                }
                            }
                            else
                            {
                                result.Add(s[i]);
                                hasEmptyLine = false;
                            }
                        }
                        else
                        {
                            result.Add(s[i]);
                            hasEmptyLine = false;
                        }
                    }
                    else
                    {
                        result.Add(s[i]);
                        hasEmptyLine = false;
                    }
                }
            }
            return result.Content;
        }

        /// <summary>
        /// Returns a new string instance with the sql comments empty.
        /// </summary>
        public static string CleanSqlCommentContent(string s)
        {
            bool inside = false;
            bool singleQuotes = true;
            StringStream result = new StringStream();
            for (int i = 0; i < s.Length; i++)
            {
                // Single Quotes
                if (s[i] == Convert.ToChar(0x27))
                {
                    if (inside)
                    {
                        if (singleQuotes) inside = false;
                    }
                    else
                    {
                        inside = true;
                        singleQuotes = true;
                    }
                }
                // Double Quotes
                if (s[i] == Convert.ToChar(0x22))
                {
                    if (inside)
                    {
                        if (!singleQuotes) inside = false;
                    }
                    else
                    {
                        inside = true;
                        singleQuotes = false;
                    }
                }
                // Long Sql Comment
                if ((s[i] == '/') && (!inside))
                {
                    if (i < s.Length - 1)
                    {
                        if (s[i + 1] == '*')
                        {
                            bool search = true;
                            i++;
                            while ((i < s.Length - 1) && (search))
                            {
                                i++;
                                if (s[i] == '*')
                                {
                                    if (i < s.Length - 1)
                                    {
                                        if (s[i + 1] == '/')
                                        {
                                            i++;
                                            search = false;
                                        }
                                    }
                                }
                            }
                            result.Add("/*");
                            if (!search) result.Add("*/");
                        }
                        else
                        {
                            result.Add(s[i]);
                        }
                    }
                    else
                    {
                        result.Add(s[i]);
                    }
                }
                else
                {
                    // Line Sql Comment
                    if ((s[i] == '-') && (!inside))
                    {
                        if (i < s.Length - 1)
                        {
                            if (s[i + 1] == '-')
                            {
                                bool search = true;
                                i++;
                                while ((i < s.Length - 1) && (search))
                                {
                                    i++;
                                    if (s[i] == Convert.ToChar(0x0A))
                                    {
                                        search = false;
                                    }
                                }
                                result.Add("--");
                                if (!search) result.Add(Environment.NewLine);
                            }
                            else
                            {
                                result.Add(s[i]);
                            }
                        }
                        else
                        {
                            result.Add(s[i]);
                        }
                    }
                    else
                    {
                        result.Add(s[i]);
                    }
                }
            }
            return result.Content;
        }

        /// <summary>
        /// Returns a new string instance with empty lines removed and the rest of the lines trimmed.
        /// </summary>
        public static string CleanEmptyLines(string s, TrimStyle trimStyle)
        {
            int i;
            StringStream filter = new StringStream();
            // Convert Tab to Space
            for (i = 0; i < s.Length; i++)
            {
                if (s[i] == Convert.ToChar(0x09))
                {
                    filter.Add(Convert.ToChar(0x20));
                }
                else
                {
                    filter.Add(s[i]);
                }
            }
            s = filter.Content;

            i = 0;
            string line = string.Empty;
            StringStream result = new StringStream();
            while (i < s.Length)
            {
                if (s[i] == Convert.ToChar(0x0A))
                {
                    if (line.Trim() != string.Empty)
                    {
                        switch(trimStyle)
                        {
                            case TrimStyle.Trim: line = line.Trim(); break;
                            case TrimStyle.TrimStart: line = line.TrimStart(); break;
                            case TrimStyle.TrimEnd: line = line.TrimEnd(); break;
                        }
                        result.Add(line + Environment.NewLine);
                    }
                    line = string.Empty;
                }
                else
                    if (s[i] >= Convert.ToChar(0x20))
                    {
                        line += s[i];
                    }
                i++;
            }
            if (line.Trim() != string.Empty)
            {
                switch (trimStyle)
                {
                    case TrimStyle.Trim: line = line.Trim(); break;
                    case TrimStyle.TrimStart: line = line.TrimStart(); break;
                    case TrimStyle.TrimEnd: line = line.TrimEnd(); break;
                }
                result.Add(line + Environment.NewLine);
            }
            return result.Content;
        }
    }

    /// <summary>
    /// This class keeps a string as an array of characters.
    /// </summary>
    public class StringSegment
    {
        private char[] data;
        private int index;

        /// <summary>
        /// Constructor
        /// </summary>
        public StringSegment()
        {
            data = new char[1024];
            index = -1;
        }

        /// <summary>
        /// Read-only property that returns the size of the string.
        /// </summary>
        public int Length
        {
            get
            {
                return index + 1;
            }
        }

        /// <summary>
        /// Read-only property that return the content of the string.
        /// </summary>
        public string Content
        {
            get
            {
                string s = string.Empty;
                for (int i = 0; i <= index; i++) s += data[i];
                return s;
            }
        }

        /// <summary>
        /// Appends a character at the end of the current string.
        /// </summary>
        public bool Add(char c)
        {
            if (index < 1023)
            {
                index++;
                data[index] = c;
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// This class keeps a string as an array of segments.
    /// </summary>
    public class StringStream
    {
        private ArrayList segments = new ArrayList();

        /// <summary>
        /// Constructor
        /// </summary>
        public StringStream()
        {
            segments.Add(new StringSegment());
        }

        /// <summary>
        /// Read-only property that returns the size of the string.
        /// </summary>
        public int Length
        {
            get
            {
                int length = 0;
                for (int i = 0; i < segments.Count; i++) length += ((StringSegment)segments[i]).Length;
                return length;
            }
        }

        /// <summary>
        /// Read-only property that return the content of the string.
        /// </summary>
        public string Content
        {
            get
            {
                string s = string.Empty;
                for (int i = 0; i < segments.Count; i++) s += ((StringSegment)segments[i]).Content;
                return s;
            }
        }

        /// <summary>
        /// Appends a character at the end of the current string.
        /// </summary>
        public void Add(char c)
        {
            StringSegment s = (StringSegment)segments[segments.Count - 1];
            if (!s.Add(c))
            {
                segments.Add(new StringSegment());
                s = (StringSegment)segments[segments.Count - 1];
                s.Add(c);
            }
        }

        /// <summary>
        /// Appends a string at the end of the current string.
        /// </summary>
        public void Add(string s)
        {
            foreach (char c in s) Add(c);
        }
    }
}
