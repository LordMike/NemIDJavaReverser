namespace ConsoleApplication10
{
    public abstract class JavaMember
    {
        public string Declaration { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }

        public string NewName { get; set; }
        public string NewDeclaration { get; set; }

        public JavaClass OwningClass { get; set; }

        public bool IsRenamed { get { return NewName != null; } }

        public JavaMember(JavaClass owningClass)
        {
            OwningClass = owningClass;
        }

        public void SetNewName(string newName)
        {
            NewName = newName;
            NewDeclaration = Declaration.Replace(Name, newName);
        }

        public override string ToString()
        {
            if (IsRenamed)
                return NewName + " (renamed)";
            return Name;
        }
    }
}