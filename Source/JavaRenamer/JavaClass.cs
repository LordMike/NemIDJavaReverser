using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ConsoleApplication10
{
    public class JavaClass
    {
        public string FileName { get; set; }

        public string Name { get; set; }
        public string FullName { get; set; }
        public string SuperClass { get; set; }
        public string[] Interfaces { get; set; }

        public List<JavaMethod> Methods { get; set; }
        public List<JavaField> Fields { get; set; }
        public Dictionary<string, JavaConstant> Constants { get; set; }
        public HashSet<string> ReferencedTypes { get; set; }

        public JavaClass(string fileName)
        {
            FileName = fileName;

            string[] directives = File.ReadAllLines(fileName).Where(s => s.StartsWith(".")).ToArray();

            string className = directives.Single(s => s.StartsWith(".class"));
            string superClass = directives.Single(s => s.StartsWith(".super"));
            string[] superInterfaces = directives.Where(s => s.StartsWith(".implements")).ToArray();
            string[] methods = directives.Where(s => s.StartsWith(".method")).ToArray();
            string[] fields = directives.Where(s => s.StartsWith(".field")).ToArray();
            string[] constants = directives.Where(s => s.StartsWith(".const")).ToArray();

            Constants = constants.Select(s => new JavaConstant(s)).ToDictionary(s => s.Name, s => s);

            FullName = ResolveConstants(className).Split(' ').Last();
            Name = FullName.Split('/').Last();
            SuperClass = ResolveConstants(superClass).Split(' ').Last();
            Interfaces = superInterfaces.Select(s => ResolveConstants(s.Split(' ').Last())).ToArray();

            Methods = methods.Select(s => new JavaMethod(this, s)).ToList();
            Fields = fields.Select(s => new JavaField(this, s)).ToList();

            Regex classNameRegex = new Regex(@"[^\s;\(\)]*;?", RegexOptions.Multiline | RegexOptions.Compiled);
            IEnumerable<MatchCollection> allMatches = directives.Select(s => classNameRegex.Matches(s));

            ReferencedTypes = new HashSet<string>();
            foreach (MatchCollection matchCollection in allMatches)
                foreach (Match match in matchCollection)
                {
                    if (match.Value.Contains("/"))
                    {
                        if (match.Value.EndsWith(";"))
                            ReferencedTypes.Add(match.Value.Substring(match.Value.IndexOf('L') + 1).TrimEnd(';'));
                        else
                            ReferencedTypes.Add(match.Value);
                    }
                }
        }

        public string ResolveConstants(string key)
        {
            bool replaced;
            do
            {
                replaced = false;
                string[] tokens = key.Split(' ');

                foreach (string token in tokens)
                {
                    if (Constants.ContainsKey(token))
                    {
                        key = key.Replace(token, Constants[token].Value);
                        replaced = true;
                    }
                }

                // Recursive solving, Java is awesome ... -.-
            } while (replaced);

            return key;
        }

        public override string ToString()
        {
            return "{" + FullName + "}";
        }
    }
}