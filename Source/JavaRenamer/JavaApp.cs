using System.Collections.Generic;
using System.Linq;

namespace ConsoleApplication10
{
    public class JavaApp
    {
        public Dictionary<string, JavaClass> Classes { get; set; }
        public Dictionary<string, string> ClassRenames { get; set; }

        public JavaApp()
        {
            Classes = new Dictionary<string, JavaClass>();
            ClassRenames = new Dictionary<string, string>();
        }

        public JavaMethod FindMethod(JavaClass thisClass, string name, string type)
        {
            JavaMethod foundMethod;

            // Super class
            if (Classes.ContainsKey(thisClass.SuperClass))
            {
                foundMethod = FindMethod(Classes[thisClass.SuperClass], name, type);
                if (foundMethod != null)
                    return foundMethod;
            }

            // Interfaces
            foreach (string @interface in thisClass.Interfaces)
                if (Classes.ContainsKey(@interface))
                {
                    foundMethod = FindMethod(Classes[@interface], name, type);
                    if (foundMethod != null)
                        return foundMethod;
                }

            // Last resort, this class
            foundMethod = thisClass.Methods.SingleOrDefault(s => s.Name == name && s.Type == type);
            if (foundMethod != null)
                return foundMethod;

            return null;
        }

        public JavaField FindField(JavaClass thisClass, string name, string type)
        {
            JavaField foundField;

            // Super class
            if (Classes.ContainsKey(thisClass.SuperClass))
            {
                foundField = FindField(Classes[thisClass.SuperClass], name, type);
                if (foundField != null)
                    return foundField;
            }

            // Interfaces
            foreach (string @interface in thisClass.Interfaces)
                if (Classes.ContainsKey(@interface))
                {
                    foundField = FindField(Classes[@interface], name, type);
                    if (foundField != null)
                        return foundField;
                }

            // Last resort, this class
            foundField = thisClass.Fields.SingleOrDefault(s => s.Name == name && s.Type == type);
            if (foundField != null)
                return foundField;

            return null;
        }
    }
}