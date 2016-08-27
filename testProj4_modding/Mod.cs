using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.CodeDom.Compiler;

namespace testProj4_modding
{
    public class Mod
    {
        private CompilerResults results;
        private const string EXECUTION_METHOD = "Main";
        private Dictionary<string, string> localVarList = new Dictionary<string, string>();
        public Mod(CompilerResults CompilerResults)
        {
            results = CompilerResults;
        }
        public object Execute()
        {
            return Execute(EXECUTION_METHOD);
        }
        public object Execute(string MethodName, object[] MethodParameters = null)
        {
            Type[] objectTypes = results.CompiledAssembly.GetTypes();
            if(objectTypes.Length > 0)
            {
                string name = objectTypes[0].FullName;
                object compiledObject = results.CompiledAssembly.CreateInstance(name);
                MethodInfo method = compiledObject.GetType().GetMethod(MethodName);
                return method.Invoke(compiledObject, MethodParameters);
            }
            return null;
        }
        public void setLocalVar(string name, string value)
        {
            while(localVarList.ContainsKey(name))
            {
                localVarList.Remove(name);
            }
            localVarList.Add(name, value);
        }

        public string getLocalVar(string name)
        {
            if(localVarList.ContainsKey(name))
            {
                return localVarList[name];
            }
            return null;
        }
    }
}
