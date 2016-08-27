using System.dll;

namespace testProj4_modding
{
	public class ModTest
	{
		public void Main()
		{
			//triggered when the mod is loaded
			System.Console.WriteLine("Counter mod has been loaded");
		}
		
		public void Start(Mod mod)
		{
			//triggered when everything starts
			mod.setLocalVar("time", 0);
			mod.setLocalVar("second", 0);
			System.Console.WriteLine("Counter mod has been started");
		}
		
		public void Step(Mod mod)
		{
			//triggered every step
			object time = mod.getLocalVar("time");
			if((int)time % 60 == 0)
			{
				string out_ = "Steps: " + System.Convert.ToString(time);
				object sec = mod.getLocalVar("second");
				out_ += ", Seconds: " + sec.ToString();
				mod.setLocalVar("second", (int)sec + 1);
				System.Console.WriteLine(out_);
			}
			mod.setLocalVar("time", (int)time + 1);
		}
	}
}