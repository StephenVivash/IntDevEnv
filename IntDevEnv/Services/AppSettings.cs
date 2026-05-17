
//using YijingData;

namespace IntDevEnv.Services;
/*
public static class AppSettings
{
	private static string _documentHome = null;
	private static string _eegDataHome = null;
	public static DateTime _lastEegDataTime = DateTime.Now;
	public static int TriggerIndex;

	public static string DocumentHome() { return _documentHome; }
	public static string EegDataHome() { return _eegDataHome; }

	public static void Load()
	{
		SetDocumentHome();
	}

	public static async void SetDocumentHome()
	{
#if WINDOWS
		_documentHome = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
#elif ANDROID
		_documentHome = FileSystem.Current.AppDataDirectory;
#else
		_documentHome = "";
#endif
		if (Directory.Exists(_documentHome))
		{
			_documentHome = Path.Combine(_documentHome, "IntDevEnv");
			_eegDataHome = Path.Combine(_documentHome, /*AppPreferences.EegDevice == (int)eEegDevice.eEmotiv ?* / "Muse");
			if (!Directory.Exists(_documentHome))
				Directory.CreateDirectory(_documentHome);
			/*
			String strTemp = Path.Combine(_documentHome, "appsettings.json");
			if (!File.Exists(strTemp))
			{
				string strSource = "appsettings.json";
				string strDestination = strTemp;
				CopyStream(await FileSystem.OpenAppPackageFileAsync(strSource), strDestination);
			}
			* /
			String strTemp = Path.Combine(_documentHome, "Muse");
			if (!Directory.Exists(strTemp))
			{
				Directory.CreateDirectory(strTemp);
				string strSource = "Example-Muse.csv";
				string strDestination = Path.Combine(strTemp, ReverseDateString() + "-Muse.csv");
				CopyStream(await FileSystem.OpenAppPackageFileAsync(strSource), strDestination);
			}
			strTemp = Path.Combine(_documentHome, "Emotiv");
			if (!Directory.Exists(strTemp))
			{
				Directory.CreateDirectory(strTemp);
				string strSource = "Example-Emotiv.csv";
				string strDestination = Path.Combine(strTemp, ReverseDateString(1) + "-Emotiv.csv");
				CopyStream(await FileSystem.OpenAppPackageFileAsync(strSource), strDestination);
			}
			strTemp = Path.Combine(_documentHome, "Questions");
			if (!Directory.Exists(strTemp))
			{
				Directory.CreateDirectory(strTemp);
				//string strSource = "111.txt";
				//string strDestination = Path.Combine(strTemp, strSource);
				//CopyStream(await FileSystem.OpenAppPackageFileAsync(strSource), strDestination);
			}
			strTemp = Path.Combine(_documentHome, "Answers");
			if (!Directory.Exists(strTemp))
			{
				Directory.CreateDirectory(strTemp);
				//string strSource = "111.txt";
				//string strDestination = Path.Combine(strTemp, strSource);
				//CopyStream(await FileSystem.OpenAppPackageFileAsync(strSource), strDestination);
			}
		}
	}

	private static void CopyStream(Stream stream, string file)
	{
		using StreamReader reader = new StreamReader(stream);
		using StreamWriter writer = new StreamWriter(file);
		writer.Write(reader.ReadToEnd());
	}

	public static string ReverseDateString(int minutes = 0)
	{
		DateTime dt = DateTime.Now.AddMinutes(minutes);
		return $"{dt.Year}-{dt.Month,2:#00}-{dt.Day,2:#00}-{dt.Hour,2:#00}-{dt.Minute,2:#00}-{dt.Second,2:#00}";
	}
}
*/
