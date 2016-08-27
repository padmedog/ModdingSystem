using System;
using System.Collections.Generic;
using System.Reflection;
using System.CodeDom.Compiler;

namespace testProj4_modding
{
    public class Mod
    {
        //make the CompilerResults to we have the actual mod to call from
        private CompilerResults results;
        //make the constant variables
        private const string EXECUTION_METHOD = "Main";
        //make the local variable list
        private Dictionary<string, object> localVarList = new Dictionary<string, object>();
        public Mod(CompilerResults CompilerResults)
        {
            //constructor that sets this to the actual mod
            results = CompilerResults;
        }
        public object Execute()
        {
            //executes the 'Main' function
            return Execute(EXECUTION_METHOD);
        }
        public object Execute(string MethodName, object[] MethodParameters = null)
        {
            //get the types of the compiled thing
            Type[] objectTypes = results.CompiledAssembly.GetTypes();
            //make sure there are actually some types to use
            if(objectTypes.Length > 0)
            {
                //get the full name
                string name = objectTypes[0].FullName;
                //get the compiled object using the full name
                object compiledObject = results.CompiledAssembly.CreateInstance(name);
                //get the method to call using the method name
                MethodInfo method = compiledObject.GetType().GetMethod(MethodName);
                //call and return the method, using the compiled object and the inputted parameters
                return method.Invoke(compiledObject, MethodParameters);
            }
            //if there are no types to use, return null
            return null;
        }
        public void setLocalVar(string name, object value)
        {
            //remove any keys of the name so we can add the updated one
            while(localVarList.ContainsKey(name))
            {
                //remove the key
                localVarList.Remove(name);
            }
            //add the updated key and value
            localVarList.Add(name, value);
        }
        public object getLocalVar(string name)
        {
            //make sure there is a key to get
            if(localVarList.ContainsKey(name))
            {
                //return the key
                return localVarList[name];
            }
            //if there isnt, return null
            return null;
        }
    }
}
