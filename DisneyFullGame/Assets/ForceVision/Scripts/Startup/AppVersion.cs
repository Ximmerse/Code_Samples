namespace Disney.ForceVision
{
	public class AppVersion
	{
		public int Major { get; private set; }

		public int Minor { get; private set; }

		public int Revision { get; private set; }

		public AppVersion(string version)
		{
			string[] parts = version.Split('.');
			Major = int.Parse(parts[0]);
			Minor = int.Parse(parts[1]);
			Revision = int.Parse(parts[2].Split('-')[0]);
		}
	}
}