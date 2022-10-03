using System.Text;

namespace MM.Multiplayer.SourceGen
{
    public class SourceWriter
    {
        private int indentLevel;
        private readonly StringBuilder str;
        private bool indent;

        public SourceWriter()
        {
            str = new StringBuilder();
        }

        public SourceWriter(int cap)
        {
            str = new StringBuilder(cap);
        }

        public void Clear()
        {
            indentLevel = 0;
            str.Clear();
        }

        public SourceWriter Comment(string s)
        {
            Write("// ").WriteLine(s);
            return this;
        }

        public SourceWriter Comment(string key, string value)
        {
            Write("// ").Write(key).Write(": ").WriteLine(value);
            return this;
        }

        public SourceWriter Write(string s)
        {
            if (indent)
            {
                str.Append(' ', indentLevel * 4);
                indent = false;
            }
            str.Append(s);
            return this;
        }

        public SourceWriter Write(char c)
        {
            if (indent)
            {
                str.Append(' ', indentLevel * 4);
                indent = false;
            }
            str.Append(c);
            return this;
        }

        public SourceWriter WriteLine(string s)
        {
            if (indent)
            {
                str.Append(' ', indentLevel * 4);
            }
            str.AppendLine(s);
            indent = true;
            return this;
        }
        
        public SourceWriter WriteLine(char c)
        {
            if (indent)
            {
                str.Append(' ', indentLevel * 4);
            }
            str.Append(c).AppendLine();
            indent = true;
            return this;
        }

        public SourceWriter WriteLine()
        {
            str.AppendLine();
            indent = true;
            return this;
        }

        public SourceWriter Indent()
        {
            indentLevel++;
            return this;
        }

        public SourceWriter Outdent()
        {
            if (indentLevel > 0)
                indentLevel--;
            return this;
        }

        public override string ToString() => str.ToString();
    }
}
