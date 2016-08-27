/************************************************************************************************
 * CodeFile.cs
 * 
 * Class used to analyze external, uncompiled C# code.
 * 
 * When       Who What
 * ========== === ===============================================================================
 * 03/12/2014 JDJ Genesis
 * 
 ***********************************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualBasic;

namespace testProj4_modding
{
	public class CodeFile
	{

		private List<String> m_RefAssemblies = new List<String>();
		private String m_MainClassName = "";
		private String m_Code2Use = "";
		private List<String> m_ParamTypes = new List<String>();
		private String[] m_CodeLines = null;
		private Boolean m_CompiledOK = false;

		private Boolean m_blnDisposeHasBeenCalled = false;

		/// <summary>
		/// Constructor
		/// </summary>
		public CodeFile()
		{

		}  // END public CodeFile()

		/// <summary>
		/// Sets the code into a variable, and compiles it,
		/// returning (as out variables) information about the 
		/// instance it represents.
		/// </summary>
		/// <param name="CodeLines"></param>
		/// <returns></returns>
		public List<String> SetAndCompileCSCode(String[] CodeLines, 
												out String pFullTypeName,
												out String pModuleName,
												out List<String> pConstructors,
												out List<String> pMembers,
												out List<String> pFields,
												out List<String> pMethods,
												out List<String> pProperties)
		{

			List<String> RetVal = new List<String>();

			CodeDOMProcessor oDOM = null;

			List<String> DOMRetVal = null;

			pFullTypeName = "";
			pModuleName = "";
			pConstructors = new List<String>();
			pMembers = new List<String>();
			pFields = new List<String>();
			pMethods = new List<String>();
			pProperties = new List<String>();

			try
			{
				m_CodeLines = CodeLines;
				m_RefAssemblies = new List<String>();
				m_MainClassName = "";
				Boolean OK2UseLine = true;
				m_Code2Use = "";

				// Go through code finding the "using" statements and specific execution-time information
				foreach (String CodeLine in CodeLines)
				{

					OK2UseLine = true;

					if (CodeLine.StartsWith("using ", StringComparison.CurrentCultureIgnoreCase))
					{
						// We pull out the using lines to define assemblies, and exclude
						// from the code to compile. "using" is not for the compiler, but for the IDE.
						OK2UseLine = false;

						String UsingName = Strings.Replace(CodeLine, "using", "", 1, 1, CompareMethod.Text).Trim();

						if (UsingName.EndsWith(";"))
						{
							UsingName = UsingName.Substring(0, UsingName.Length - 1);
						}

						m_RefAssemblies.Add(UsingName);

					}  // END if (CodeLine.StartsWith("using ", StringComparison.CurrentCultureIgnoreCase))

					// Here is where we get the first class name to use as the main class name.
					// This code could be refined to find all class names and let the user define the main class name.
					if ((CodeLine.IndexOf(" class ", 0, StringComparison.CurrentCultureIgnoreCase) > 0) && 
					    (m_MainClassName.Length == 0))
					{
						OK2UseLine = true;

						Int32 ClassNameStart = CodeLine.IndexOf(" class ", 0, StringComparison.CurrentCultureIgnoreCase) + 7;

						m_MainClassName = CodeLine.Substring(ClassNameStart);

					}  // END if (CodeLine.IndexOf(" class ", 0, StringComparison.CurrentCultureIgnoreCase) > 0)

					// If the line is a compilable line, it is added to the list.
					if (OK2UseLine)
					{
						m_Code2Use += CodeLine + Environment.NewLine;
					}

				}  // END foreach (String CodeLine in CodeLines)

				// Instantiate the object to process the compilable code.
				oDOM = new CodeDOMProcessor();

				// Compile the code, and get back info on it.
				// DOMRetVal is a list of messages on why it did or did not compile.
				DOMRetVal = oDOM.Compile(Code2Use, 
										RefAssemblies, 
										m_MainClassName,
										out pFullTypeName,
										out pModuleName,
										out pConstructors,
										out pMembers,
										out pFields,
										out pMethods,
										out pProperties);

				// Not having any referenced assemblies is not good.
				if (RefAssemblies.Count == 0)
				{
					RetVal.Add("No references specified." + Environment.NewLine);

				}
				else
				{
					// Tell the user what assemblies we found.
					for (int i = 0; i < RefAssemblies.Count; i++)
					{
						RetVal.Add(String.Format("Reference [{0}] is [{1}].", (i + 1).ToString(), RefAssemblies[i]) + Environment.NewLine);

					}  // END for (int i = 0; i < RefAssemblies.Count; i++)
				}

				if (DOMRetVal != null)
				{
					if (DOMRetVal.Count > 0)
					{
						foreach (String RetMsg in DOMRetVal)
						{
							RetVal.Add(RetMsg + Environment.NewLine);

						}  // END foreach (String RetMsg in DOMRetVal)

						m_CompiledOK = false;

					}  // END if (DOMRetVal.Count > 0)
					else
					{
						RetVal.Add("No compile messages." + Environment.NewLine);
						m_CompiledOK = true;

					}

				}  // END if (RetVal != null)
				else
				{
					RetVal.Add("No compile messages." + Environment.NewLine);
					m_CompiledOK = true;

				}

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

			}  // END Catch
			finally
			{
				if (oDOM != null)
				{
					oDOM.Dispose();

					oDOM = null;
				}

				if (DOMRetVal != null)
				{
					DOMRetVal.Clear();

					DOMRetVal = null;
				}

			}  // END finally

			return RetVal;

		}  // END public List<String> SetAndCompileCSCode(String[] CodeLines)


		/// <summary>
		/// 
		/// </summary>
		public List<String> RefAssemblies
		{
			get
			{
				return m_RefAssemblies;
			}
		}

		public Boolean CompiledOK
		{
			get
			{
				return m_CompiledOK;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public String MainClassName
		{
			get
			{
				return m_MainClassName;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public String Code2Use
		{
			get
			{
				return m_Code2Use;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public String[] CodeLines
		{
			get
			{
				return m_CodeLines;
			}
		}

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
		~CodeFile()
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

						if (m_RefAssemblies != null)
						{
							m_RefAssemblies.Clear();

							m_RefAssemblies = null;
						}


						//if (m_ParamTypes != null)
						//{
						//    m_ParamTypes.Clear();

						//    m_ParamTypes = null;
						//}

						if (m_CodeLines != null)
						{
							Array.Clear(m_CodeLines, 0, m_CodeLines.Length);

							m_CodeLines = null;
						}

					}  // END if (pDisposing)

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
                if (codeFile.CompiledOK)
                {
                    if (output)
                    {
                        Console.WriteLine("  -Name: " + name);
                        Console.WriteLine("  -Module: " + module);
                        if (constructors.Count > 0)
                        {
                            if (constructors.Count == 1)
                                Console.WriteLine("  -Constructor:");
                            else
                                Console.WriteLine("  -Constructors:");
                            foreach (string con in constructors)
                            {
                                Console.WriteLine("    - " + con);
                            }
                        }
                        else
                            Console.WriteLine("  -No constructors");

                        if (methods.Count > 0)
                        {
                            if (methods.Count == 1)
                                Console.WriteLine("  -Method:");
                            else
                                Console.WriteLine("  -Methods:");
                            foreach (string method in methods)
                            {
                                Console.WriteLine("    - " + method);
                            }
                        }
                        else
                            Console.WriteLine("  -No methods");

                        if (properties.Count > 0)
                        {
                            if (properties.Count == 1)
                                Console.WriteLine("  -Property:");
                            else
                                Console.WriteLine("  -Properties:");
                            foreach (string prop in properties)
                            {
                                Console.WriteLine("    - " + prop);
                            }
                        }
                        else
                            Console.WriteLine("  -No properties");

                        if (members.Count > 0)
                        {
                            if (members.Count == 1)
                                Console.WriteLine("  -Member:");
                            else
                                Console.WriteLine("  -Members:");
                            foreach (string member in members)
                            {
                                Console.WriteLine("    - " + member);
                            }
                        }
                        else
                            Console.WriteLine("  -No members");

                        if (fields.Count > 0)
                        {
                            if (fields.Count == 1)
                                Console.WriteLine("  -Field:");
                            else
                                Console.WriteLine("  -Fields:");
                            foreach (string field in fields)
                            {
                                Console.WriteLine("    - " + field);
                            }
                        }
                        else
                            Console.WriteLine("  -No fields");
                    }

                    //we'll execute now
                    CodeDOMProcessor dom = new CodeDOMProcessor();
                    Object[] MethodParams = new Object[] { };
                    List<string> ret_ = dom.CompileAndExecute(codeFile.Code2Use, codeFile.RefAssemblies, codeFile.MainClassName, Program.EXECUTION_METHOD, out mod);
                    foreach (string str in ret_)
                    {
                        Console.WriteLine(str);
                    }
                    Console.WriteLine("  ----");
                    return result;
                }
            }
            return null;
        }

    }  // END public class CodeFile

}  // END namespace RunExternal
