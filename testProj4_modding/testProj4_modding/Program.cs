using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;

namespace testProj4_modding
{
    class Program
    {
        //here are the unchangeable constants that shouldnt be changed
        public const string EXECUTION_METHOD = "Main",
                             EXECUTION_FILE = "main.cs";
        //here we do the modification list
        private static List<Mod> modList;
        //here we make the global variable list
        private static Dictionary<string, object> globalVarList = new Dictionary<string, object>();
        static void Main(string[] args)
        {
            //make the mods folder if we dont already have it
            Directory.CreateDirectory(@".\mods\");
            //get all of the paths for each of the folders in the mods folder
            string[] paths = Directory.GetDirectories(@".\mods\", "*", SearchOption.TopDirectoryOnly);
            //here we create the modification list
            modList = new List<Mod>();
            //tries to compile and execute each mod
            foreach(string path in paths)
            {
                Console.WriteLine("Executing mod from " + path + "\\" + EXECUTION_FILE);
                Mod mod;
                //try to execute the mod
                List<string> result = CodeFile.executeFile(path + "\\" + EXECUTION_FILE, true, out mod);
                //if its not null then the mod did something right
                if (result != null)
                {
                    //writes out each result item
                    foreach (string item in result)
                    {
                        Console.WriteLine(" -" + item);
                    }
                    //adds the mod to the modification list
                    modList.Add(mod);
                }
                else
                {
                    //it failed for some reason
                    //todo: some way to give more detail on the problem
                    Console.WriteLine("  -Failed to execute mod");
                }
            }
            //call the start event of each mod
            callModEvent("Start");

            //the value that has only 3 options, 0 [17ms], 1 [17ms], or 2 [16ms]
            //this will make it try to make sure that there is 60 step events per second
            byte num0 = 0;
            while (true)
            {
                if(num0 == 2)
                {
                    Thread.Sleep(16);
                    num0 = 0;
                }
                else
                {
                    Thread.Sleep(17);
                    num0 += 1;
                }
                //call the step event of each modification
                callModEvent("Step");
            }
        }
        public void setGlobalVar(string name, object value)
        {
            //make sure that there is something to change, or just skip ahead to the add
            while(globalVarList.ContainsKey(name))
            {
                globalVarList.Remove(name);
            }
            //add the variable when it can
            globalVarList.Add(name, value);
        }

        public object getGlobalVar(string name)
        {
            //makes sure the variable exists
            if(globalVarList.ContainsKey(name))
            {
                //return the value if it exists
                return globalVarList[name];
            }
            //if it doesnt exist return null
            return null;
        }

        static public void callModEvent(string id)
        {
            foreach (Mod mod in modList)
            {
                //executes the method of each mod
                mod.Execute(id, new object[] { mod });
            }
        }
    }
}
