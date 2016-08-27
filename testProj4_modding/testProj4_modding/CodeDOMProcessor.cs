/************************************************************************************************
 * CodeDOMProcessor.cs
 * 
 * Class compile and optionally execute external, uncompiled C# code.
 * 
 * This class was written for compiling C# code, but coudle asily be adapted for VB.NET.
 * 
 * When       Who What
 * ========== === ===============================================================================
 * 03/12/2014 JDJ Genesis
 * 
 ***********************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Net;
using Microsoft.CSharp;
using System.CodeDom.Compiler;


namespace testProj4_modding
{
    public class CodeDOMProcessor
    {

        public const String COMPILER_VERSION_KEY = "CompilerVersion";

        // This could be a variable that is specified to target different frameworks.
        public const String COMPILER_VERSION_SUPPORTED = "v4.0";
        private Boolean m_blnDisposeHasBeenCalled = false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCodeToCompile"></param>
        /// <param name="pReferencedAssemblies"></param>
        /// <param name="pMainClassName"></param>
        /// <param name="pInstanceName"></param>
        /// <param name="pExecutionMethodName"></param>
        /// <param name="pMethodParameters"></param>
        /// <returns></returns>
        public List<String> CompileAndExecute(String pCodeToCompile,
                                      List<String> pReferencedAssemblies,
                                      String pMainClassName,
                                      String pExecutionMethodName,
                                      out Mod mod,
                                      Object[] pMethodParameters = null)
        {

            List<String> ReturnVal = new List<String>();

            Dictionary<String, String> DOMProviderOptions = null;

            CSharpCodeProvider DOMProvider = null;

            CompilerParameters DOMCompilerParams = null;

            CompilerResults CompileResults = null;


            try
            {

                DOMProviderOptions = new Dictionary<String, String>();

                DOMProviderOptions.Add(COMPILER_VERSION_KEY, COMPILER_VERSION_SUPPORTED);

                // Could use Microsoft.VisualBasic.VBCodeProvider for VB.NET code
                // The Dictionary specifies the compiler version. 
                DOMProvider = new CSharpCodeProvider(DOMProviderOptions);


                // Add referenced assemblies to the provider parameters
                DOMCompilerParams = new CompilerParameters();

                if (pReferencedAssemblies != null)
                {
                    if (pReferencedAssemblies.Count > 0)
                    {
                        foreach (String RefAssembly in pReferencedAssemblies)
                        {
                            if (RefAssembly != null)
                            {
                                if (RefAssembly.Length > 0)
                                {
                                    DOMCompilerParams.ReferencedAssemblies.Add(RefAssembly);
                                } // END if (File.Exists(pExecutableFullPath))
                                else
                                {
                                    ReturnVal.Add(String.Format("A reference file was empty.{0}", Environment.NewLine));
                                }
                            }  // END if (pExecutableFullPath.Length > 0)
                            else
                            {
                                ReturnVal.Add(String.Format("A reference file was null.{0}", Environment.NewLine));
                            }

                        }  // END foreach (String RefAssembly in pReferencedAssemblies)

                    }  // END if (pReferencedAssemblies.Count > 0)

                } // END if (pReferencedAssemblies != null)

                // These references will always be there to support the code compiling
                // If these are not found, be sure to add them.
                // Note references are in the form of the file name.
                // If the reference is not in the GAC, you must supply the fully
                // qualified file name of the assembly, such as C:\SomeFiles\MyDLL.dll.
                if (!DOMCompilerParams.ReferencedAssemblies.Contains("System.dll"))
                {
                    DOMCompilerParams.ReferencedAssemblies.Add("System.dll");
                }

                if (!DOMCompilerParams.ReferencedAssemblies.Contains("System.Windows.Forms.dll"))
                {
                    DOMCompilerParams.ReferencedAssemblies.Add("System.Windows.Forms.dll");
                }

                if (!DOMCompilerParams.ReferencedAssemblies.Contains("System.Runtime.Serialization.dll"))
                {
                    DOMCompilerParams.ReferencedAssemblies.Add("System.Runtime.Serialization.dll");
                }

                // Adds this executable so it self-references.
                DOMCompilerParams.ReferencedAssemblies.Add(System.Reflection.Assembly.GetEntryAssembly().Location);

                // For this example, I am generating the DLL in memory, but you could
                // also create a DLL that gets reused.
                DOMCompilerParams.GenerateInMemory = true;
                DOMCompilerParams.GenerateExecutable = false;
                DOMCompilerParams.CompilerOptions = "/optimize";
                DOMCompilerParams.IncludeDebugInformation = true;
                DOMCompilerParams.MainClass = pMainClassName;

                // Compile the code.
                CompileResults = DOMProvider.CompileAssemblyFromSource(DOMCompilerParams, pCodeToCompile);

                // Analyze the results.a
                if (CompileResults != null)
                {
                    if (CompileResults.Errors.Count != 0)
                    {

                        foreach (CompilerError oErr in CompileResults.Errors)
                        {
                            ReturnVal.Add(String.Format("Error# [{0}] - [{1}] Line# [{2}] Column# [{3}].{4}",
                                            oErr.ErrorNumber.ToString(), oErr.ErrorText, oErr.Line.ToString(),
                                            oErr.Column.ToString(), Environment.NewLine));

                        }  // END foreach (CompilerError oErr in CompileResults.Errors)

                    }  // END if (CompileResults.Errors.Count != 0)
                    else
                    {
                        // If we are here, it compiled OK, so we execute.
                        Type[] ObjectTypes = CompileResults.CompiledAssembly.GetTypes();

                        if (ObjectTypes.Length > 0)
                        {
                            String FullTypeName = ObjectTypes[0].FullName;

                            Object CompiledObject = CompileResults.CompiledAssembly.CreateInstance(FullTypeName);

                            MethodInfo CompiledMethod = CompiledObject.GetType().GetMethod(pExecutionMethodName);

                            Object ReturnValue = CompiledMethod.Invoke(CompiledObject, pMethodParameters);
                        } // END if (ObjectTypes.Length > 0)
                        else
                        {
                            ReturnVal.Add("No defined types found in the compiled object.");
                        }
                    }  // END else of [if (CompileResults.Errors.Count != 0)]
                } // END if (CompileResults != null)
                else
                {
                    ReturnVal.Add("No compiled object created.");
                }  // END else of [if (CompileResults != null)]

                

            }  // END try

            catch (Exception exUnhandled)
            {
                // Insert your exception handling code here.
                // This is only temporary.
                System.Windows.Forms.MessageBox.Show(String.Format("Error Message [{0}]{1}Error Source [{2}]",
                                                exUnhandled.Message,
                                                Environment.NewLine,
                                                exUnhandled.Source),
                                "Error",
                                System.Windows.Forms.MessageBoxButtons.OK,
                                System.Windows.Forms.MessageBoxIcon.Error);


            }  // END catch (Exception exUnhandled)
            finally
            {
                if (DOMProviderOptions != null)
                {
                    DOMProviderOptions.Clear();

                    DOMProviderOptions = null;
                }

                if (DOMProvider != null)
                {

                    DOMProvider.Dispose();

                    DOMProvider = null;
                }

                if (DOMCompilerParams != null)
                {
                    DOMCompilerParams = null;
                }
            }  // END finally


            mod = new Mod(CompileResults);
            return ReturnVal;

        }  // END public void CompileAndExecute(String pCodeToCompile ...)


        /// <summary>
        /// This compiles with additional information returned.  CompileAndExecute
        /// does not return the detailed compiled information.
        /// </summary>
        /// <param name="pCodeToCompile"></param>
        /// <param name="pReferencedAssemblies"></param>
        /// <param name="pMainClassName"></param>
        /// <param name="pInstanceName"></param>
        /// <param name="pExecutionMethodName"></param>
        /// <param name="pMethodParameters"></param>
        /// <returns></returns>
        public List<String> Compile(String pCodeToCompile,
                                    List<String> pReferencedAssemblies,
                                    String pMainClassName,
                                    out String pFullTypeName,
                                    out String pModuleName,
                                    out List<String> pConstructors,
                                    out List<String> pMembers,
                                    out List<String> pFields,
                                    out List<String> pMethods,
                                    out List<String> pProperties)
        {

            List<String> ReturnVal = new List<String>();

            Dictionary<String, String> DOMProviderOptions = null;

            CSharpCodeProvider DOMProvider = null;

            CompilerParameters DOMCompilerParams = null;

            pFullTypeName = "";
            pModuleName = "";
            pConstructors = new List<String>();
            pMembers = new List<String>();
            pFields = new List<String>();
            pMethods = new List<String>();
            pProperties = new List<String>();

            try
            {

                DOMProviderOptions = new Dictionary<String, String>();

                DOMProviderOptions.Add(COMPILER_VERSION_KEY, COMPILER_VERSION_SUPPORTED);

                DOMProvider = new CSharpCodeProvider(DOMProviderOptions);

                DOMCompilerParams = new CompilerParameters();

                if (pReferencedAssemblies != null)
                {
                    if (pReferencedAssemblies.Count > 0)
                    {
                        foreach (String RefAssembly in pReferencedAssemblies)
                        {
                            if (RefAssembly != null)
                            {
                                if (RefAssembly.Length > 0)
                                {
                                    DOMCompilerParams.ReferencedAssemblies.Add(RefAssembly);
                                } // END if (File.Exists(pExecutableFullPath))
                                else
                                {
                                    ReturnVal.Add(String.Format("A reference file was empty.{0}", Environment.NewLine));
                                }
                            }  // END if (pExecutableFullPath.Length > 0)
                            else
                            {
                                ReturnVal.Add(String.Format("A reference file was null.{0}", Environment.NewLine));
                            }

                        }  // END foreach (String RefAssembly in pReferencedAssemblies)

                    }  // END if (pReferencedAssemblies.Count > 0)

                } // END if (pReferencedAssemblies != null)

                // These references will always be there to support the code compiling
                if (!DOMCompilerParams.ReferencedAssemblies.Contains("System.dll"))
                {
                    DOMCompilerParams.ReferencedAssemblies.Add("System.dll");
                }

                if (!DOMCompilerParams.ReferencedAssemblies.Contains("System.Windows.Forms.dll"))
                {
                    DOMCompilerParams.ReferencedAssemblies.Add("System.Windows.Forms.dll");
                }

                if (!DOMCompilerParams.ReferencedAssemblies.Contains("System.Runtime.Serialization.dll"))
                {
                    DOMCompilerParams.ReferencedAssemblies.Add("System.Runtime.Serialization.dll");
                }

                // Adds this executable.
                DOMCompilerParams.ReferencedAssemblies.Add(System.Reflection.Assembly.GetEntryAssembly().Location);

                DOMCompilerParams.GenerateInMemory = true;
                DOMCompilerParams.GenerateExecutable = false;
                DOMCompilerParams.CompilerOptions = "/optimize";
                DOMCompilerParams.IncludeDebugInformation = true;
                DOMCompilerParams.MainClass = pMainClassName;

                CompilerResults CompileResults = DOMProvider.CompileAssemblyFromSource(DOMCompilerParams, pCodeToCompile);

                if (CompileResults != null)
                {
                    if (CompileResults.Errors.Count != 0)
                    {

                        foreach (CompilerError oErr in CompileResults.Errors)
                        {
                            ReturnVal.Add(String.Format("Error# [{0}] - [{1}] Line# [{2}] Column# [{3}].{4}",
                                            oErr.ErrorNumber.ToString(), oErr.ErrorText, oErr.Line.ToString(),
                                            oErr.Column.ToString(), Environment.NewLine));

                        }  // END foreach (CompilerError oErr in CompileResults.Errors)

                    }  // END if (CompileResults.Errors.Count != 0)
                    else
                    {

                        Type[] ObjectTypes = CompileResults.CompiledAssembly.GetTypes();

                        // List<String> pMembers, List<String> pFields, List<String> pMethods, List<String> pProperties
                        if (ObjectTypes.Length > 0)
                        {
                            pFullTypeName = ObjectTypes[0].FullName;

                            Object CompiledObject = CompileResults.CompiledAssembly.CreateInstance(pFullTypeName);

                            Type CompiledType = CompiledObject.GetType();

                            pModuleName = CompiledType.Module.ScopeName;

                            // Beginning here, you could create and populate class instances that
                            // contain information gleaned from constructors, methods, members, 
                            // properties, etc. and their parameters instead of passing back these 
                            // generalized descriptions.
                            ConstructorInfo[] TempConstructors = CompiledType.GetConstructors();

                            foreach (ConstructorInfo TempConstructor in TempConstructors)
                            {
                                String StringToAdd = "";

                                if (TempConstructor.Name == ".ctor")
                                {
                                    StringToAdd = "void " + ObjectTypes[0].Name;
                                }
                                else
                                {
                                    StringToAdd = "void " + TempConstructor.Name;
                                }

                                String ParmString = "";

                                if (TempConstructor.Module.ScopeName.Equals(pModuleName))
                                {

                                    ParameterInfo[] TempConstructorParam = TempConstructor.GetParameters();

                                    foreach (ParameterInfo TempParam in TempConstructorParam)
                                    {
                                        ParmString += String.Format("{0} {1}, ", TempParam.ParameterType.FullName, TempParam.Name);
                                    }
                                }

                                StringToAdd += "(" + ParmString + ")";

                                pConstructors.Add(StringToAdd);
                            }

                            MemberInfo[] TempDefaultMembers = CompiledType.GetDefaultMembers();

                            // List<String> pFields, List<String> pMethods, List<String> pProperties
                            if (TempDefaultMembers.Length > 0)
                            {

                                foreach (MemberInfo TempMember in TempDefaultMembers)
                                {
                                    if (TempMember.Module.ScopeName.Equals(pModuleName))
                                    {
                                        String StringToAdd = "";

                                        StringToAdd = String.Format("{0} {1}, ", TempMember.ReflectedType.FullName, TempMember.Name);

                                        pMembers.Add(StringToAdd);
                                    }  // END if (TempMember.Module.ScopeName.Equals(pModuleName))

                                }  // END if (TempDefaultMembers.Length > 0)

                            }  // END if (TempDefaultMembers.Length > 0)

                            FieldInfo[] TempFields = CompiledType.GetFields();

                            // List<String> pFields, List<String> pMethods, List<String> pProperties
                            if (TempFields.Length > 0)
                            {
                                foreach (FieldInfo TempField in TempFields)
                                {
                                    if (TempField.Module.ScopeName.Equals(pModuleName))
                                    {

                                        String StringToAdd = "";

                                        StringToAdd = String.Format("{0} {1}, ", TempField.ReflectedType.FullName, TempField.Name);

                                        pFields.Add(StringToAdd);
                                    }  // END if (TempField.Module.ScopeName.Equals(pModuleName))

                                }  // END foreach (FieldInfo TempField in TempFields)

                            }  // END if (TempFields.Length > 0)


                            MemberInfo[] TempMembers = CompiledType.GetMembers();

                            // List<String> pProperties
                            if (TempMembers.Length > 0)
                            {

                                foreach (MemberInfo TempMember in TempMembers)
                                {

                                    if (TempMember.Module.ScopeName.Equals(pModuleName))
                                    {
                                        String StringToAdd = "";

                                        StringToAdd = TempMember.ToString();  // String.Format("{0} {1}, ", TempMember.GetType().FullName, TempMember.Name);

                                        pMembers.Add(StringToAdd);
                                    }
                                }  // END if (TempDefaultMembers.Length > 0)

                            }  // END if (TempDefaultMembers.Length > 0)


                            MethodInfo[] TempMethods = CompiledType.GetMethods();

                            foreach (MethodInfo TempMethod in TempMethods)
                            {

                                if ((TempMethod.Module.ScopeName.Equals(pModuleName)) && (!TempMethod.IsSpecialName))
                                {
                                    String StringToAdd = "";

                                    StringToAdd = String.Format("{0} {1}, ", TempMethod.ReturnType.FullName, TempMethod.Name);

                                    ParameterInfo[] TempParams = TempMethod.GetParameters();

                                    String ParmString = "";

                                    foreach (ParameterInfo TempParam in TempParams)
                                    {
                                        String ParamName = TempParam.Name;
                                        String ParamTypeName = TempParam.ParameterType.FullName;
                                        Object DefaultValue = TempParam.DefaultValue;

                                        if (DefaultValue.ToString().Length == 0)
                                        {
                                            ParmString += String.Format("{0} {1}, ", ParamTypeName, ParamName);
                                        }
                                        else
                                        {
                                            ParmString += String.Format("{0} {1}={2}, ", ParamTypeName, ParamName, DefaultValue.ToString());

                                        }
                                    }  // END foreach (ParameterInfo TempParam in TempParams)

                                    if (ParmString.EndsWith(", "))
                                    {
                                        ParmString = ParmString.Substring(0, ParmString.Length - 2);
                                    }

                                    StringToAdd += "(" + ParmString + ")";

                                    pMethods.Add(StringToAdd);

                                }  // END if (TempMethod.Module.ScopeName.Equals(pModuleName))

                            }  // END foreach (MethodInfo TempMethod in TempMethods)


                            PropertyInfo[] TempProperties = CompiledType.GetProperties();

                            // List<String> pProperties
                            if (TempProperties.Length > 0)
                            {

                                foreach (PropertyInfo TempProperty in TempProperties)
                                {
                                    if (TempProperty.Module.ScopeName.Equals(pModuleName))
                                    {
                                        String StringToAdd = "";

                                        StringToAdd = String.Format("{0} {1}, ", TempProperty.PropertyType.FullName, TempProperty.Name);

                                        if (TempProperty.CanRead && TempProperty.CanWrite)
                                        {
                                            StringToAdd += " (get/set)";
                                        }
                                        else if (!TempProperty.CanRead && TempProperty.CanWrite)
                                        {
                                            StringToAdd += " (set ONLY)";
                                        }
                                        else if (TempProperty.CanRead && !TempProperty.CanWrite)
                                        {
                                            StringToAdd += " (get ONLY)";
                                        }
                                        else
                                        {
                                            // No action
                                        }

                                        pProperties.Add(StringToAdd);
                                    }  // END if (TempProperty.Module.ScopeName.Equals(pModuleName))

                                }  // END if (TempDefaultMembers.Length > 0)

                            }  // END if (TempDefaultMembers.Length > 0)

                        } // END if (ObjectTypes.Length > 0)
                        else
                        {
                            ReturnVal.Add("No defined types found in the compiled object.");
                        }
                    }  // END else of [if (CompileResults.Errors.Count != 0)]
                } // END if (CompileResults != null)
                else
                {
                    ReturnVal.Add("No compiled object created.");
                }  // END else of [if (CompileResults != null)]

            }  // END try

            catch (Exception exUnhandled)
            {
                // Insert your exception handling code here.
                // This is only temporary.
                System.Windows.Forms.MessageBox.Show(String.Format("Error Message [{0}]{1}Error Source [{2}]",
                                                exUnhandled.Message,
                                                Environment.NewLine,
                                                exUnhandled.Source),
                                "Error",
                                System.Windows.Forms.MessageBoxButtons.OK,
                                System.Windows.Forms.MessageBoxIcon.Error);


            }  // END catch (Exception exUnhandled)
            finally
            {
                if (DOMProviderOptions != null)
                {
                    DOMProviderOptions.Clear();

                    DOMProviderOptions = null;
                }

                if (DOMProvider != null)
                {

                    DOMProvider.Dispose();

                    DOMProvider = null;
                }

                if (DOMCompilerParams != null)
                {
                    DOMCompilerParams = null;
                }
            }  // END finally

            return ReturnVal;

        }  // END public void CompileAndExecute(String pCodeToCompile ...)



        #region IDisposable Implementation=========================

        /// <summary>
        /// This property is true if Dispose() has been called, false if not.
        ///
        /// The programmer does not have to check this property before calling
        /// the Dispose() method as the check is made internally and Dispose()
        /// is not executed more than once.
        /// </summary>
        public Boolean Disposing
        {
            get
            {
                return m_blnDisposeHasBeenCalled;
            }
        }  // END public Boolean Disposing

        /// <summary>
        /// Implement the IDisposable.Dispose() method
        /// Developers are supposed to call this method when done with this object.
        /// There is no guarantee when or if the GC will call it, so 
        /// the developer is responsible to.  GC does NOT clean up unmanaged 
        /// resources, so we have to clean those up, too.
        /// 
        /// </summary>
        public void Dispose()
        {
            try
            {
                // Check if Dispose has already been called 
                // Only allow the consumer to call it once with effect.
                if (!m_blnDisposeHasBeenCalled)
                {
                    // Call the overridden Dispose method that contains common cleanup code
                    // Pass true to indicate that it is called from Dispose
                    Dispose(true);

                    // Prevent subsequent finalization of this object. Subsequent finalization 
                    // is not needed because managed and unmanaged resources have been 
                    // explicitly released
                    GC.SuppressFinalize(this);
                }
            }

            catch (Exception exUnhandled)
            {
                // Insert your exception handling code here.
                // This is only temporary.
                System.Windows.Forms.MessageBox.Show(String.Format("Error Message [{0}]{1}Error Source [{2}]",
                                                exUnhandled.Message,
                                                Environment.NewLine,
                                                exUnhandled.Source),
                                "Error",
                                System.Windows.Forms.MessageBoxButtons.OK,
                                System.Windows.Forms.MessageBoxIcon.Error);

            }  // END Catch

        }  // END public new void Dispose()

        /// <summary>
        /// Explicit Finalize method.  The GC calls Finalize, if it is called.
        /// There are times when the GC will fail to call Finalize, which is why it is up to 
        /// the developer to call Dispose() from the consumer object.
        /// </summary>
        ~CodeDOMProcessor()
        {
            // Call Dispose indicating that this is not coming from the public
            // dispose method.
            Dispose(false);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        public void Dispose(Boolean pDisposing)
        {

            try
            {
                if (!m_blnDisposeHasBeenCalled)
                {
                    if (pDisposing)
                    {
                        // Here we dispose and clean up the unmanaged objects and managed object we created in code
                        // that are not in the IContainer child object of this object.
                        // Unmanaged objects do not have a Dispose() method, so we just set them to null
                        // to release the reference.  For managed objects, we call their respective Dispose()
                        // methods, if they have them, and then release the reference.
                        // if (m_objComputers != null)
                        //     {
                        //     m_objComputers = null;
                        //     }
                        // If the base object for this instance has a Dispose() method, call it.
                        //base.Dispose();
                    }

                    // Set the flag that Dispose has been called and executed.
                    m_blnDisposeHasBeenCalled = true;
                }

            }

            catch (Exception exUnhandled)
            {
                // Insert your exception handling code here.
                // This is only temporary.
                System.Windows.Forms.MessageBox.Show(String.Format("Error Message [{0}]{1}Error Source [{2}]",
                                                exUnhandled.Message,
                                                Environment.NewLine,
                                                exUnhandled.Source),
                                "Error",
                                System.Windows.Forms.MessageBoxButtons.OK,
                                System.Windows.Forms.MessageBoxIcon.Error);

            }  // END Catch

        }  // END public void Dispose(Boolean pDisposing)

        #endregion IDisposable Implementation======================

    }  // END public class CodeDOMProcessor



}  // END namespace RunExternal
