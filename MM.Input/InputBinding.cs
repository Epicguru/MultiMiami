using System.Text.RegularExpressions;
using Microsoft.Xna.Framework.Input;

namespace MM.Input
{
    public class InputBinding
    {
        private static string SplitCamelCase(string input) => Regex.Replace(input, "([A-Z])", " $1", RegexOptions.Compiled).TrimStart();

        public string Name = "Key binding";
        public Keys Key, Key2;
        public bool Either = true;

        public void Clear()
        {
            Key = default;
            Key2 = default;
        }

        public override string ToString()
        {
            bool empty = Key == Keys.None && Key2 == Keys.None;
            if (empty)
                return "[Name] <missing binding>";

            if (Key2 == Keys.None)
                return $"[{Name}] {SplitCamelCase(Key.ToString())}";

            char sep = Either ? '/' : '+';
            return $"[{Name}] {SplitCamelCase(Key.ToString())}{sep}{SplitCamelCase(Key2.ToString())}";
        }
    }
}