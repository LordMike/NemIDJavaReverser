using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ConsoleApplication10
{
    public class Program
    {
        private static string[] _keywords = new[] { "abstract", "continue", "for", "new", "switch", "assert", "default", "goto", "package", "synchronized", "boolean", "do", "if", "private", "this", "break", "double", "implements", "protected", "throw", "byte", "else", "import", "public", "throws", "case", "enum", "instanceof", "return", "transient", "catch", "extends", "int", "short", "try", "char", "final", "interface", "static", "void", "class", "finally", "long", "strictfp", "volatile", "const", "float", "native", "super", "while", "Wuddlecakes", "Woogycute", "Loverschnookumlove", "Foofieface", "Schmoopiecake", "Wooglecakes", "Cuddlypoo", "Poofcuddle", "Moopsiewookie", "Wookumdarling", "SnookieKissie" };

        static void Main(string[] args)
        {
            // TOOD: Point this to the disassembled directory
            const string dir = @"..\..\..\..\NemID\disassembled";

            {
                JavaApp app = new JavaApp();
                string[] files = Directory.GetFiles(dir, "*.j", SearchOption.AllDirectories);

                // Read files initially
                {
                    Console.WriteLine("Parsing " + files.Length + " classes");

                    HashSet<string> allReferencedTypes = new HashSet<string>();
                    Regex constRegex = new Regex(@"^\.const .*$", RegexOptions.Multiline | RegexOptions.Compiled);

                    foreach (string file in files)
                    {
                        JavaClass container = new JavaClass(file);

                        // Substitute constants
                        if (container.Constants.Any())
                        {
                            // This is a test
                            string fileText = File.ReadAllText(file);
                            List<string> keys = container.Constants.Keys.ToList();

                            foreach (string key in keys)
                            {
                                container.Constants[key].Value = container.ResolveConstants(container.Constants[key].Value);
                            }

                            foreach (string key in keys)
                            {
                                if ((container.Constants[key].Type == "String" || container.Constants[key].Type == "Utf8") && !container.Constants[key].Value.StartsWith("'") && !container.Constants[key].Value.StartsWith("\""))
                                {
                                    // Enquote
                                    container.Constants[key].Value = "'" + container.Constants[key].Value + "'";
                                }
                            }

                            fileText = constRegex.Replace(fileText, string.Empty);
                            fileText = fileText.Trim();

                            foreach (string key in keys)
                            {
                                string firstToken = container.Constants[key].Value.Split(' ').First();
                                if (!firstToken.Contains("\"") && !firstToken.Contains("/") && !firstToken.Contains("'"))
                                    Console.WriteLine(firstToken);

                                fileText = fileText.Replace(key, container.Constants[key].Value);
                            }

                            File.WriteAllText(file, fileText);

                            container = new JavaClass(file);
                        }

                        foreach (string referencedType in container.ReferencedTypes)
                            allReferencedTypes.Add(referencedType);

                        app.Classes.Add(container.FullName, container);
                    }

                    foreach (JavaClass javaClassContainer in app.Classes.Values)
                        allReferencedTypes.Remove(javaClassContainer.FullName);

                    List<string> allTypes = allReferencedTypes.ToList();
                    foreach (string type in allTypes)
                        if (type.StartsWith("java"))
                            allReferencedTypes.Remove(type);

                    foreach (string allReferencedType in allReferencedTypes)
                        Console.WriteLine(allReferencedType);

                    Console.WriteLine("Parsed all " + app.Classes.Count + " classes");
                }

                // Process files
                {
                    Queue<JavaClass> classQueue = new Queue<JavaClass>(app.Classes.Values);

                    int methodNum = 0;
                    int fieldNum = 0;

                    while (classQueue.Any())
                    {
                        JavaClass currentClass = classQueue.Dequeue();

                        // Is waiting for super?
                        if (classQueue.Any(s => s.FullName == currentClass.SuperClass))
                        {
                            // Yes, requeue
                            classQueue.Enqueue(currentClass);
                            continue;
                        }

                        // Is waiting for interface?
                        if (classQueue.Any(s => currentClass.Interfaces.Any(x => x == s.FullName)))
                        {
                            // Yes, requeue
                            classQueue.Enqueue(currentClass);
                            continue;
                        }

                        // Process
                        if (app.Classes.ContainsKey(currentClass.SuperClass))
                        {
                            // Copy down fields / methods
                            JavaClass superClass = app.Classes[currentClass.SuperClass];

                            List<JavaMethod> superMethods = superClass.Methods.Where(s => !currentClass.Methods.Any(x => x.Name == s.Name && x.Type == s.Type)).ToList();
                            List<JavaField> superFields = superClass.Fields.Where(s => !currentClass.Fields.Any(x => x.Name == s.Name && x.Type == s.Type)).ToList();

                            foreach (JavaMethod method in superMethods)
                            {
                                JavaMethod newMethod = new JavaMethod(currentClass, method.Declaration);

                                currentClass.Methods.Add(newMethod);
                            }

                            foreach (JavaField field in superFields)
                            {
                                JavaField newField = new JavaField(currentClass, field.Declaration);

                                currentClass.Fields.Add(newField);
                            }
                        }

                        // Class name
                        if (ShouldRename(currentClass.Name))
                        {
                            // Rename class
                            app.ClassRenames.Add(currentClass.FullName, currentClass.FullName.Replace(currentClass.Name, "OrigClass_" + currentClass.Name));
                        }

                        // Method names
                        foreach (JavaMethod method in currentClass.Methods)
                        {
                            JavaMethod parentMethod = app.FindMethod(currentClass, method.Name, method.Type);

                            if (method.Name == "<init>" || method.Name == "<clinit>")
                                continue;

                            if (ShouldRename(parentMethod))
                            {
                                parentMethod.SetNewName("OrigMethod" + methodNum++);
                            }
                        }

                        // Field names
                        foreach (JavaField field in currentClass.Fields)
                        {
                            JavaField parentField = app.FindField(currentClass, field.Name, field.Type);

                            if (ShouldRename(parentField))
                            {
                                parentField.SetNewName("OrigField" + fieldNum++);
                            }
                        }
                    }

                    Console.WriteLine("Processed all " + app.Classes.Count + " classes");
                    Console.WriteLine("Total class renames: " + app.ClassRenames.Count);
                }

                // Rename fields and methods
                {
                    Console.WriteLine("Beginning rename pass 1");

                    // Actual renaming
                    foreach (JavaClass classContainer in app.Classes.Values)
                    {
                        string fileText = File.ReadAllText(classContainer.FileName);

                        // Method renames
                        // Single methods
                        foreach (JavaMethod method in classContainer.Methods)
                        {
                            JavaMethod parentMethod = app.FindMethod(classContainer, method.Name, method.Type);

                            if (!parentMethod.IsRenamed)
                                // No need for renaming
                                continue;

                            method.SetNewName(parentMethod.NewName);

                            string oldDec = method.Declaration;
                            string newDec = method.NewDeclaration;

                            fileText = fileText.Replace(oldDec, newDec);
                        }

                        // Field renames
                        // Single fields
                        foreach (JavaField field in classContainer.Fields)
                        {
                            JavaField parentField = app.FindField(classContainer, field.Name, field.Type);

                            if (!parentField.IsRenamed)
                                // No need for renaming
                                continue;

                            field.SetNewName(parentField.NewName);

                            string oldDec = field.Declaration;
                            string newDec = field.NewDeclaration;

                            fileText = fileText.Replace(oldDec, newDec);
                        }

                        File.WriteAllText(classContainer.FileName, fileText);
                    }

                    Console.WriteLine("Completed rename pass 1");

                    List<JavaMethod> allMethodRenames = app.Classes.Values.SelectMany(s => s.Methods.Where(x => x.IsRenamed)).ToList();
                    List<JavaField> allFieldRenames = app.Classes.Values.SelectMany(s => s.Fields.Where(x => x.IsRenamed)).ToList();

                    Console.WriteLine("Total method renames: " + allMethodRenames.Count);
                    Console.WriteLine("Total field renames: " + allFieldRenames.Count);

                    Console.WriteLine("Beginning rename pass 2");

                    foreach (JavaClass classContainer in app.Classes.Values)
                    {
                        string fileText = File.ReadAllText(classContainer.FileName);

                        // All other methods
                        foreach (JavaMethod method in allMethodRenames)
                        {
                            // Rename
                            List<string> typeSubstitutes = classContainer.Constants.Where(s => s.Value.Value == method.Type).Select(s => s.Key).ToList();
                            typeSubstitutes.Add(method.Type);

                            foreach (string typeSubstitute in typeSubstitutes)
                            {
                                string oldUsage = method.OwningClass.FullName + " " + method.Name + " " + typeSubstitute;
                                string newUsage = method.OwningClass.FullName + " " + method.NewName + " " + typeSubstitute;

                                fileText = fileText.Replace(oldUsage, newUsage);
                            }
                        }

                        // All other fields
                        foreach (JavaField field in allFieldRenames)
                        {
                            // Rename
                            List<string> typeSubstitutes = classContainer.Constants.Where(s => s.Value.Value == field.Type).Select(s => s.Key).ToList();
                            typeSubstitutes.Add(field.Type);

                            foreach (string typeSubstitute in typeSubstitutes)
                            {
                                string oldUsage = field.OwningClass.FullName + " " + field.Name + " " + typeSubstitute;
                                string newUsage = field.OwningClass.FullName + " " + field.NewName + " " + typeSubstitute;

                                fileText = fileText.Replace(oldUsage, newUsage);
                            }
                        }

                        File.WriteAllText(classContainer.FileName, fileText);
                    }

                    Console.WriteLine("Completed rename pass 2");
                }

                // Rename classes
                {
                    Console.WriteLine("Beginning class rename");

                    foreach (JavaClass classContainer in app.Classes.Values)
                    {
                        string fileText = File.ReadAllText(classContainer.FileName);

                        foreach (KeyValuePair<string, string> map in app.ClassRenames)
                        {
                            fileText = fileText.Replace(map.Key, map.Value);
                        }

                        File.WriteAllText(classContainer.FileName, fileText);
                    }

                    // Move files
                    foreach (KeyValuePair<string, string> map in app.ClassRenames)
                    {
                        File.Move(Path.Combine(dir, map.Key + ".j"), Path.Combine(dir, map.Value + ".j"));
                    }

                    Console.WriteLine("Completed class rename");
                }
            }

            // Replace string encryption
            // TODO: If you're not working with NemID, comment out the line below.
            DecryptStrings(dir);

            Console.WriteLine("All done.");
        }

        private static void DecryptStrings(string dir)
        {
            Console.WriteLine("Translating encrypted strings");
            using (StreamWriter logger = new StreamWriter("translate.txt"))
            {
                string[] classCalls = new[] { "dk/pbs/applet/bootstrap/Wuddlecahes", "dk/danid/plugins/Woogucyte", "dk/danid/plugins/PoodleCakes", "dk/danid/plugins/Woddlecakes", "dk/danid/plugins/Wuddlekakes", "dk/danid/plugins/Wudlecakes", "dk/danid/plugins/Woogycote" };

                List<Regex> calls = new List<Regex>();
                foreach (string classCall in classCalls)
                {
                    calls.Add(new Regex(@"invokestatic " + classCall + @" [\S]*? \(Ljava/lang/String;\)Ljava/lang/String;"));
                }

                // invokestatic .. int (Ljava/lang/String;)Ljava/lang/String;
                Regex txtRegex = new Regex("(ldc|ldc_w) (['\"].*['\"])");
                string[] files = Directory.GetFiles(dir, "*.j", SearchOption.AllDirectories);

                foreach (string file in files)
                {
                    string text = File.ReadAllText(file);

                    MatchCollection matches = txtRegex.Matches(text);

                    foreach (Match match in matches)
                    {
                        string oldText = ParseJavaString(match.Groups[2].Value);
                        string newText = WuddleCahes(oldText);

                        newText = EncodeJavaString(newText);
                        text = text.Replace(match.Groups[2].Value, newText);

                        logger.WriteLine("Translated " + EncodeJavaString(newText) + " <- " + EncodeJavaString(oldText));
                    }

                    // Remove calls
                    foreach (Regex call in calls)
                    {
                        text = call.Replace(text, string.Empty);
                    }

                    File.WriteAllText(file, text);
                }

                logger.WriteLine("Done");
            }
            Console.WriteLine("Done.");
        }

        private static bool ShouldRename(JavaMember item)
        {
            return !item.IsRenamed && (_keywords.Contains(item.Name, StringComparer.InvariantCultureIgnoreCase) || item.Name.Any(s => !char.IsLetterOrDigit(s)));
        }

        private static bool ShouldRename(string name)
        {
            return _keywords.Contains(name, StringComparer.InvariantCultureIgnoreCase) || name.Any(s => !char.IsLetterOrDigit(s));
        }

        public static string WuddleCahes(string input)
        {
            int i = 42;
            int j = input.Length - 4;
            char[] chars = new char[j];

            for (int k = 0; k < j; k++)
            {
                int m = input[k + 4];
                if (m > 31 && m < 127)
                {
                    char tmp = ((char)((i + input[k] + m) % 95 + 32));
                    i = tmp;
                    chars[k] = tmp;
                }
                else
                {
                    chars[k] = (char)m;
                }
            }

            return new string(chars);
        }

        private static string ParseJavaString(string input)
        {
            if (input[0] != input[input.Length - 1] || (input[0] != '"' && input[0] != '\''))
                throw new ArgumentException("Input must be quoted");

            // Remove quoting
            input = input.Substring(1, input.Length - 2);

            List<char> chars = input.ToCharArray().ToList();
            for (int i = 0; i < chars.Count; i++)
            {
                if (chars[i] == '\\')
                {
                    // Remove escaper
                    chars.RemoveAt(i);

                    // Insert special character
                    if (chars[i] == 'n')
                        chars[i] = '\n';
                    if (chars[i] == 'r')
                        chars[i] = '\r';
                    if (chars[i] == 't')
                        chars[i] = '\t';

                    i++;
                }
            }

            // Remove beginning and ending quotes
            return new string(chars.ToArray());
        }

        private static string EncodeJavaString(string input)
        {
            List<char> res = new List<char>();

            res.Add('"');

            foreach (char ch in input)
            {
                if (ch == '\r')
                    res.AddRange(new[] { '\\', 'r' });
                else if (ch == '\n')
                    res.AddRange(new[] { '\\', 'n' });
                else if (ch == '\t')
                    res.AddRange(new[] { '\\', 't' });
                else if (ch == '\'')
                    res.AddRange(new[] { '\\', '\'' });
                else if (ch == '"')
                    res.AddRange(new[] { '\\', '"' });
                else if (char.IsControl(ch))
                    throw new Exception();
                else
                    res.Add(ch);
            }

            res.Add('"');

            return new string(res.ToArray());
        }
    }
}