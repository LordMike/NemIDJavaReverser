using System.Linq;

namespace ConsoleApplication10
{
    public class JavaMethod : JavaMember
    {
        public JavaMethod(JavaClass owningClass, string declaration)
            : base(owningClass)
        {
            Declaration = declaration;

            string[] lineParts = declaration.Split(':').Select(s => s.Trim()).ToArray();

            string[] firstTokens = lineParts[0].Split(new[] { ' ' });
            Name = firstTokens.Last();

            Type = owningClass.ResolveConstants(lineParts[1]);

            //if (ShouldRename(name))
            //{
            //    string newMethodName = "origMethod" + methodNameMap.Count;

            //    string fullMethodName = fullClassName + " " + name + " " + lineParts[1];
            //    string newFullName = fullClassName + " " + newMethodName + " " + lineParts[1];

            //    string newDeclaration = ReplaceFirst(methodLine, name, newMethodName);

            //    methodNameMap.Add(new JavaMember(file, methodLine, fullMethodName), new JavaMember(file, newDeclaration, newFullName));

            //    Console.WriteLine(newMethodName + " <- " + fullMethodName);
            //    logger.WriteLine(newMethodName + ": " + fullMethodName);
            //}
        }
    }
}