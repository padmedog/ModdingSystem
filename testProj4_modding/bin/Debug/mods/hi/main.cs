using System.dll;

namespace testProj4_modding
{
	public class ModTest
	{
		public void Main()
		{
			//triggered when the mod is loaded
		}
		
		public void Start(Mod mod)
		{
			//triggered when everything starts
			mod.setLocalVar("time","1");
		}
		
		public void Step(Mod mod)
		{
			//triggered every step
			string timeS = mod.getLocalVar("time");
			System.Console.WriteLine(timeS);
			int time = System.Convert.ToInt32(timeS);
			mod.setLocalVar("time", System.Convert.ToString(time + 1));
		}
	}
}