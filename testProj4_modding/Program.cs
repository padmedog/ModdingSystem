using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;

namespace testProj4_modding
{
    class Program
    {
        private const string EXECUTION_METHOD = "Main";
        private static List<Mod> modList;
        private static Dictionary<string, string> globalVarList = new Dictionary<string, string>();
        static void Main(string[] args)
        {
            Directory.CreateDirectory(@".\mods\");
            string[] paths = Directory.GetDirectories(@".\mods\", "*", SearchOption.TopDirectoryOnly);
            modList = new List<Mod>();
            foreach(string path in paths)
            {
                Console.WriteLine("Executing mod from " + path);
                Mod mod;
                List<string> result = executeFile(path + @"\main.cs", true, out mod);
                if (result != null)
                {
                    foreach (string item in result)
                    {
                        Console.WriteLine(" -" + item);
                    }
                    modList.Add(mod);
                }
                else
                {
                    Console.WriteLine("Failed to execute mod");
                }
            }

            foreach (Mod mod in modList)
            {
                mod.Execute("Start", new object[] { mod });
            }
            while (true)
            {
                Thread.Sleep(1000);
                foreach(Mod mod in modList)
                {
                    mod.Execute("Step", new object[] { mod } );
                }
            }
        }

        public static List<String> executeFile(string filePath, bool output, out Mod mod)
        {
            mod = null;
            if (File.Exists(filePath))
            {
                string[] code = File.ReadAllLines(filePath);
                string name, module;
                List<string> constructors, methods, members, fields, properties, result;
                CodeFile codeFile = new CodeFile();
                result = codeFile.SetAndCompileCSCode(code, out name, out module, out constructors, out members, out fields, out methods, out properties);
                if(codeFile.CompiledOK)
                {
                    if(output)
                    {
                        Console.WriteLine("Name: " + name);
                        Console.WriteLine("Module: " + module);
                        if (constructors.Count > 0)
                        {
                            if (constructors.Count == 1)
                                Console.WriteLine("Constructor:");
                            else
                                Console.WriteLine("Constructors:");
                            foreach (string con in constructors)
                            {
                                Console.WriteLine(" - " + con);
                            }
                        }
                        else
                            Console.WriteLine("No constructors");
                        
                        if (methods.Count > 0)
                        {
                            if (methods.Count == 1)
                                Console.WriteLine("Method:");
                            else
                                Console.WriteLine("Methods:");
                            foreach (string method in methods)
                            {
                                Console.WriteLine(" - " + method);
                            }
                        }
                        else
                            Console.WriteLine("No methods");

                        if (properties.Count > 0)
                        {
                            if (properties.Count == 1)
                                Console.WriteLine("Property:");
                            else
                                Console.WriteLine("Properties:");
                            foreach (string prop in properties)
                            {
                                Console.WriteLine(" - " + prop);
                            }
                        }
                        else
                            Console.WriteLine("No properties");

                        if (members.Count > 0)
                        {
                            if (members.Count == 1)
                                Console.WriteLine("Member:");
                            else
                                Console.WriteLine("Members:");
                            foreach (string member in members)
                            {
                                Console.WriteLine(" - " + member);
                            }
                        }
                        else
                            Console.WriteLine("No members");

                        if (fields.Count > 0)
                        {
                            if (fields.Count == 1)
                                Console.WriteLine("Field:");
                            else
                                Console.WriteLine("Fields:");
                            foreach (string field in fields)
                            {
                                Console.WriteLine(" - " + field);
                            }
                        }
                        else
                            Console.WriteLine("No fields");
                    }

                    //we'll execute now
                    CodeDOMProcessor dom = new CodeDOMProcessor();
                    Object[] MethodParams = new Object[] { };
                    dom.CompileAndExecute(codeFile.Code2Use, codeFile.RefAssemblies, codeFile.MainClassName, EXECUTION_METHOD, out mod);

                    return result;
                }
            }
            return null;
        }

        public void setGlobalVar(string name, string value)
        {
            while(globalVarList.ContainsKey(name))
            {
                globalVarList.Remove(name);
            }
            globalVarList.Add(name, value);
        }

        public string getGlobalVar(string name)
        {
            if(globalVarList.ContainsKey(name))
            {
                return globalVarList[name];
            }
            return null;
        }
    }
}
