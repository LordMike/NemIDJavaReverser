namespace ConsoleApplication10
{
    public class JavaField : JavaMember
    {
        public JavaField(JavaClass owningClass, string declaration)
            : base(owningClass)
        {
            Declaration = declaration;

            if (declaration.IndexOf('=') != -1)
                // Declaration has an assignment, cut it off.
                declaration = declaration.Remove(declaration.IndexOf('=') - 1);

            string[] tokens = declaration.Split(' ');

            Name = tokens[tokens.Length - 2];
            Type = owningClass.ResolveConstants(tokens[tokens.Length - 1]);

            //if (ShouldRename(name))
            //{
            //    string newFieldName = "origField" + fieldNameMap.Count;

            //    string fullFieldName = fullClassName + " " + name + " " + type;
            //    string newFullName = fullClassName + " " + newFieldName + " " + type;

            //    string newDeclaration = ReplaceFirst(fieldLine, name, newFieldName);

            //    fieldNameMap.Add(new JavaMember(file, fieldLine, fullFieldName), new JavaMember(file, newDeclaration, newFullName));

            //    Console.WriteLine(newFieldName + " <- " + fullFieldName);
            //    logger.WriteLine(newFieldName + ": " + fullFieldName);
            //}
        }
    }
}