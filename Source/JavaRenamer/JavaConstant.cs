using System;

namespace ConsoleApplication10
{
    public class JavaConstant
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }

        public JavaConstant(string line)
        {
            string[] splits = line.Split(new[] { '=' }, 2);

            if (splits.Length != 2)
                throw new ArgumentException("Invalid constant line");

            Name = splits[0].Substring(".const ".Length).Trim();
            Value = splits[1].Trim();

            if (Value.StartsWith("Utf8 "))
            {
                Type = "Utf8";
                Value = Value.Substring("Utf8 ".Length);
            }
            else if (Value.StartsWith("String "))
            {
                Type = "String";
                Value = Value.Substring("String ".Length);
            }
            else if (Value.StartsWith("Field "))
            {
                Type = "Field";
                Value = Value.Substring("Field ".Length);
            }
            else if (Value.StartsWith("Class "))
            {
                Type = "Class";
                Value = Value.Substring("Class ".Length);
            }
        }

        public override string ToString()
        {
            return "{" + Name + ": " + Value + " (" + Type + ")}";
        }
    }
}