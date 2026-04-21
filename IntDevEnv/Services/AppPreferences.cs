
//using YijingData;

namespace IntDevEnv.Services;

public enum ePages { eNone, eWorkspace, eSettings, eAbout };

#nullable enable
/*
public enum eDiagramMode { eExplore, eAnimate, eTouchCast, eAutoCast, eMindCast };
public enum eDiagramType { eLine, eTrigram, eHexagram };
public enum eDiagramSpeed { eSlow, eMedium, eFast };
public enum eDiagramColor { eMono, eDual, eTrigram, eHexagram };

//public enum eEegDevice { eMuse, eEmotiv };
public enum eEegMode { eIdle, eLive, eReplay, eSummary };
public enum eGoal { eMeditation, eYijingCast };
public enum eTimer { eNone, eTen, eFifteen, eTwenty, eThirty, eSixty };
public enum eAmbience { eNone, eLightRain, eQuietForest, eMeadowBirds }; // , eKashmir, eMachineGun 
public enum eReplaySpeed { eNormal, eFast };
public enum eChartBands { eFront, eBack, eMixed, eAll };
public enum eChartTime { eTwoAndHalf, eFive };
public enum eTriggerBand { eDelta, eTheta, eAlpha, eBeta, eGamma };
public enum eTriggerChannel { eBackLeft, eFrontLeft, eBackCenter, eFrontRight, eBackRight };
public enum eTriggerRange { eZeroOne, eOneTwo, eTwoThree, eTwoFour, eThreeFour, eThreeFive, eFourFive, eFourSix };
public enum eTriggerHunter { eNone, eOne, eTwo, eThree, eFour, eFive, eSix, eSeven, eEight, eNine, eTen };
public enum eTriggerSchedule { eTwenty, eThirty, eForty, eSixty, eNinety, eOneTwenty };

public enum eAiEegMlModel { eNone, eStephenV }; // eJohnD

public static class AppPreferences
{
	public const string DiagramLsbKey = "DiagramLsb";
	public const string DiagramModeKey = "DiagramMode";
	public const string DiagramTypeKey = "DiagramType";
	public const string DiagramColorKey = "DiagramColor";
	public const string DiagramSpeedKey = "DiagramSpeed";
	public const string HexagramTextKey = "HexagramText";
	public const string HexagramLabelKey = "HexagramLabel";
	public const string HexagramSequenceKey = "HexagramSequence";
	public const string HexagramRatioKey = "HexagramRatio";
	public const string TrigramTextKey = "TrigramText";
	public const string TrigramLabelKey = "TrigramLabel";
	public const string TrigramSequenceKey = "TrigramSequence";
	public const string TrigramRatioKey = "TrigramRatio";
	public const string LineTextKey = "LineText";
	public const string LineLabelKey = "LineLabel";
	public const string LineSequenceKey = "LineSequence";
	public const string LineRatioKey = "LineRatio";
	public const string EegDeviceKey = "EegDevice";
	public const string EegGoalKey = "EegGoal";
	public const string AmbienceKey = "Ambience";
	public const string TimerKey = "Timer";
	public const string ReplaySpeedKey = "ReplaySpeed";
	public const string ChartBandsKey = "ChartBands";
	public const string ChartTimeKey = "ChartTime";
	public const string TriggerBandKey = "TriggerBand";
	public const string TriggerChannelKey = "TriggerChannel";
	public const string TriggerRangeKey = "TriggerRange";
	public const string TriggerHunterKey = "TriggerHunter";
	public const string TriggerScheduleKey = "TriggerSchedule";
	public const string TriggerSoundingKey = "TriggerSounding";
	public const string RawDataKey = "RawData";
	public const string AiChatServiceKey = "AiChatService";
	public const string AiEegServiceKey = "AiEegService";
	public const string AiEegMlModelKey = "AiEegMlModel";

	public const int DiagramLsbDefault = 1;
	public const int DiagramModeDefault = (int)eDiagramMode.eExplore;
	public const int DiagramTypeDefault = (int)eDiagramType.eHexagram;
	public const int DiagramColorDefault = (int)eDiagramColor.eTrigram;
	public const int DiagramSpeedDefault = (int)eDiagramSpeed.eMedium;
	public const int HexagramTextDefault = 1;
	public const int HexagramLabelDefault = 9;
	public const int HexagramSequenceDefault = 2;
	public const int HexagramRatioDefault = 0;
	public const int TrigramTextDefault = 0;
	public const int TrigramLabelDefault = 2;
	public const int TrigramSequenceDefault = 1;
	public const int TrigramRatioDefault = 0;
	public const int LineTextDefault = 0;
	public const int LineLabelDefault = 3;
	public const int LineSequenceDefault = 1;
	public const int LineRatioDefault = 1;
	public const int EegDeviceDefault = (int)1;// eEegDevice.eMuse;
	public const int EegGoalDefault = (int)eGoal.eYijingCast;
	public const int AmbienceDefault = (int)eAmbience.eLightRain;
	public const int TimerDefault = (int)eTimer.eTwenty;
	public const int ReplaySpeedDefault = (int)eReplaySpeed.eNormal;
	public const int ChartBandsDefault = (int)eChartBands.eFront;
	public const int ChartTimeDefault = (int)eChartTime.eTwoAndHalf;
	public const int TriggerBandDefault = (int)eTriggerBand.eGamma;
	public const int TriggerChannelDefault = (int)eTriggerChannel.eFrontLeft;
	public const int TriggerRangeDefault = (int)eTriggerRange.eTwoFour;
	public const int TriggerHunterDefault = (int)eTriggerHunter.eFive;
	public const int TriggerScheduleDefault = (int)eTriggerSchedule.eSixty;
	public const bool TriggerSoundingDefault = true;
	public const bool RawDataDefault = false;
	public const string AiChatServiceDefault = AiPreferences.AiServiceNone;
	public const string AiEegServiceDefault = AiPreferences.AiServiceNone;
	public const int AiEegMlModelDefault = (int)eAiEegMlModel.eNone;

	public static void Load()
	{

		DiagramLsb = Preferences.Get(DiagramLsbKey, DiagramLsbDefault);

		DiagramMode = Preferences.Get(DiagramModeKey, DiagramModeDefault);
		DiagramType = Preferences.Get(DiagramTypeKey, DiagramTypeDefault);
		DiagramColor = Preferences.Get(DiagramColorKey, DiagramColorDefault);
		DiagramSpeed = Preferences.Get(DiagramSpeedKey, DiagramSpeedDefault);

		HexagramText = Preferences.Get(HexagramTextKey, HexagramTextDefault);
		HexagramLabel = Preferences.Get(HexagramLabelKey, HexagramLabelDefault);
		HexagramSequence = Preferences.Get(HexagramSequenceKey, HexagramSequenceDefault);
		HexagramRatio = Preferences.Get(HexagramRatioKey, HexagramRatioDefault);

		TrigramText = Preferences.Get(TrigramTextKey, TrigramTextDefault);
		TrigramLabel = Preferences.Get(TrigramLabelKey, TrigramLabelDefault);
		TrigramSequence = Preferences.Get(TrigramSequenceKey, TrigramSequenceDefault);
		TrigramRatio = Preferences.Get(TrigramRatioKey, TrigramRatioDefault);

		LineText = Preferences.Get(LineTextKey, LineTextDefault);
		LineLabel = Preferences.Get(LineLabelKey, LineLabelDefault);
		LineSequence = Preferences.Get(LineSequenceKey, LineSequenceDefault);
		LineRatio = Preferences.Get(LineRatioKey, LineRatioDefault);

		EegDevice = Preferences.Get(EegDeviceKey, EegDeviceDefault);
		EegGoal = Preferences.Get(EegGoalKey, EegGoalDefault);
		Ambience = Preferences.Get(AmbienceKey, AmbienceDefault);
		Timer = Preferences.Get(TimerKey, TimerDefault);

		ReplaySpeed = Preferences.Get(ReplaySpeedKey, ReplaySpeedDefault);
		ChartBands = Preferences.Get(ChartBandsKey, ChartBandsDefault);
		ChartTime = Preferences.Get(ChartTimeKey, ChartTimeDefault);

		TriggerBand = Preferences.Get(TriggerBandKey, TriggerBandDefault);
		TriggerChannel = Preferences.Get(TriggerChannelKey, TriggerChannelDefault);
		TriggerRange = Preferences.Get(TriggerRangeKey, TriggerRangeDefault);
		TriggerHunter = Preferences.Get(TriggerHunterKey, TriggerHunterDefault);
		TriggerSchedule = Preferences.Get(TriggerScheduleKey, TriggerScheduleDefault);
		TriggerSounding = Preferences.Get(TriggerSoundingKey, TriggerSoundingDefault);
		RawData = Preferences.Get(RawDataKey, RawDataDefault);

		AiChatService = Preferences.Get(AiChatServiceKey, AiChatServiceDefault);

		AiEegService = Preferences.Get(AiEegServiceKey, AiEegServiceDefault);
		AiEegMlModel = Preferences.Get(AiEegMlModelKey, AiEegMlModelDefault);
	}

	public static void Save()
	{
		return;
		Preferences.Set(DiagramLsbKey, DiagramLsb);
		Preferences.Set(DiagramModeKey, DiagramMode);
		Preferences.Set(DiagramTypeKey, DiagramType);
		Preferences.Set(DiagramColorKey, DiagramColor);
		Preferences.Set(DiagramSpeedKey, DiagramSpeed);
		Preferences.Set(HexagramTextKey, HexagramText);
		Preferences.Set(HexagramLabelKey, HexagramLabel);
		Preferences.Set(HexagramSequenceKey, HexagramSequence);
		Preferences.Set(HexagramRatioKey, HexagramRatio);
		Preferences.Set(TrigramTextKey, TrigramText);
		Preferences.Set(TrigramLabelKey, TrigramLabel);
		Preferences.Set(TrigramSequenceKey, TrigramSequence);
		Preferences.Set(TrigramRatioKey, TrigramRatio);
		Preferences.Set(LineTextKey, LineText);
		Preferences.Set(LineLabelKey, LineLabel);
		Preferences.Set(LineSequenceKey, LineSequence);
		Preferences.Set(LineRatioKey, LineRatio);
		Preferences.Set(EegDeviceKey, EegDevice);
		Preferences.Set(EegGoalKey, EegGoal);
		Preferences.Set(AmbienceKey, Ambience);
		Preferences.Set(TimerKey, Timer);
		Preferences.Set(ReplaySpeedKey, ReplaySpeed);
		Preferences.Set(ChartBandsKey, ChartBands);
		Preferences.Set(ChartTimeKey, ChartTime);
		Preferences.Set(TriggerBandKey, TriggerBand);
		Preferences.Set(TriggerChannelKey, TriggerChannel);
		Preferences.Set(TriggerRangeKey, TriggerRange);
		Preferences.Set(TriggerHunterKey, TriggerHunter);
		Preferences.Set(TriggerScheduleKey, TriggerSchedule);
		Preferences.Set(TriggerSoundingKey, TriggerSounding);
		Preferences.Set(RawDataKey, RawData);
		Preferences.Set(AiChatServiceKey, AiChatService);
		Preferences.Set(AiEegServiceKey, AiEegService);
		Preferences.Set(AiEegMlModelKey, AiEegMlModel);
	}

	public static void Reset()
	{
		DiagramLsb = DiagramLsbDefault;
		DiagramMode = DiagramModeDefault;
		DiagramType = DiagramTypeDefault;
		DiagramColor = DiagramColorDefault;
		DiagramSpeed = DiagramSpeedDefault;
		HexagramText = HexagramTextDefault;
		HexagramLabel = HexagramLabelDefault;
		HexagramSequence = HexagramSequenceDefault;
		HexagramRatio = HexagramRatioDefault;
		TrigramText = TrigramTextDefault;
		TrigramLabel = TrigramLabelDefault;
		TrigramSequence = TrigramSequenceDefault;
		TrigramRatio = TrigramRatioDefault;
		LineText = LineTextDefault;
		LineLabel = LineLabelDefault;
		LineSequence = LineSequenceDefault;
		LineRatio = LineRatioDefault;
		EegDevice = EegDeviceDefault;
		EegGoal = EegGoalDefault;
		Ambience = AmbienceDefault;
		Timer = TimerDefault;
		ReplaySpeed = ReplaySpeedDefault;
		ChartBands = ChartBandsDefault;
		ChartTime = ChartTimeDefault;
		TriggerBand = TriggerBandDefault;
		TriggerChannel = TriggerChannelDefault;
		TriggerRange = TriggerRangeDefault;
		TriggerHunter = TriggerHunterDefault;
		TriggerSchedule = TriggerScheduleDefault;
		TriggerSounding = TriggerSoundingDefault;
		RawData = RawDataDefault;
		AiChatService = AiChatServiceDefault;
		AiEegService = AiEegServiceDefault;
		AiEegMlModel = AiEegMlModelDefault;
		Save();
	}

	public static int DiagramLsb;

	public static int DiagramMode;
	public static int DiagramType;
	public static int DiagramColor;
	public static int DiagramSpeed;

	public static int HexagramText;
	public static int HexagramLabel;
	public static int HexagramSequence;
	public static int HexagramRatio;

	public static int TrigramText;
	public static int TrigramLabel;
	public static int TrigramSequence;
	public static int TrigramRatio;

	public static int LineText;
	public static int LineLabel;
	public static int LineSequence;
	public static int LineRatio;

	public static int EegDevice;
	public static int EegGoal;

	public static int Ambience;
	public static int Timer;

	public static int ReplaySpeed;
	public static int ChartBands;
	public static int ChartTime;
	public static int TriggerBand;
	public static int TriggerChannel;
	public static int TriggerRange;

	public static int TriggerHunter;
	public static int TriggerSchedule;
	public static bool TriggerSounding;
	public static bool RawData;

	public static string AiChatService = AiChatServiceDefault;

	public static string AiEegService = AiEegServiceDefault;
	public static int AiEegMlModel;
}

public static class AiPreferences
{
	public const string AiServiceNamesKey = "AI-ServiceNames";
	public const string AiServiceNone = "None";
	public const string AiTemperatureKey = "AI-Temperature";
	public const string AiTopPKey = "AI-TopP";
	public const string AiMaxTokensKey = "AI-MaxTokens";

	public const string AiServiceNamesDefault = "OpenAI,DeepSeek,Github,Ollama";
	public const float AiTemperatureDefault = 1.0f;
	public const float AiTopPDefault = 1.0f;
	public const int AiMaxTokensDefault = 10240;

	public const string OpenAiModelDefault = "gpt-5.2";
	public const string OpenAiEndPointDefault = "https://api.openai.com/v1";
	public const string OpenAiKeyDefault = "";
	public const string DeepSeekModelDefault = "deepseek-reasoner";
	public const string DeepSeekEndPointDefault = "https://api.deepseek.com";
	public const string DeepSeekKeyDefault = "";
	public const string GithubModelDefault = "grok-3";
	public const string GithubEndPointDefault = "https://models.inference.ai.azure.com";
	public const string GithubKeyDefault = "";
	public const string OllamaModelDefault = "gpt-oss:20b";
	public const string OllamaEndPointDefault = "http://localhost:11434/v1";
	public const string OllamaKeyDefault = "ollama";

	public static void Load()
	{
		AiServiceNames = ParseServiceNames(Preferences.Get(AiServiceNamesKey, AiServiceNamesDefault));
		if (AiServiceNames.Length == 0)
			AiServiceNames = ParseServiceNames(AiServiceNamesDefault);

		AppPreferences.AiChatService = NormalizeServiceName(AppPreferences.AiChatService);
		AppPreferences.AiEegService = NormalizeServiceName(AppPreferences.AiEegService);

		AiTemperature = Preferences.Get(AiTemperatureKey, AiTemperatureDefault);
		AiTopP = Preferences.Get(AiTopPKey, AiTopPDefault);
		AiMaxTokens = Preferences.Get(AiMaxTokensKey, AiMaxTokensDefault);

		AiServices = new Dictionary<string, AiServiceInfo>(StringComparer.OrdinalIgnoreCase);
		foreach (var serviceName in AiServiceNames)
		{
			var defaults = GetServiceDefaults(serviceName);
			var modelId = Preferences.Get(ServicePreferenceKey(serviceName, nameof(AiServiceInfo.ModelId)), defaults.ModelId);
			var endPoint = Preferences.Get(ServicePreferenceKey(serviceName, nameof(AiServiceInfo.EndPoint)), defaults.EndPoint);
			var key = Preferences.Get(ServicePreferenceKey(serviceName, nameof(AiServiceInfo.Key)), defaults.Key);
			AiServices[serviceName] = new AiServiceInfo(modelId, endPoint, key);
		}
	}

	public static void Save()
	{
		Preferences.Set(AiServiceNamesKey, string.Join(",", AiServiceNames));
		Preferences.Set(AiTemperatureKey, AiTemperature);
		Preferences.Set(AiTopPKey, AiTopP);
		Preferences.Set(AiMaxTokensKey, AiMaxTokens);

		if (AiServices is null)
			return;

		foreach (var (serviceName, serviceInfo) in AiServices)
		{
			Preferences.Set(ServicePreferenceKey(serviceName, nameof(AiServiceInfo.ModelId)), serviceInfo.ModelId);
			Preferences.Set(ServicePreferenceKey(serviceName, nameof(AiServiceInfo.EndPoint)), serviceInfo.EndPoint);
			Preferences.Set(ServicePreferenceKey(serviceName, nameof(AiServiceInfo.Key)), serviceInfo.Key);
		}
	}

	public static void Reset()
	{
		AiServiceNames = ParseServiceNames(AiServiceNamesDefault);
		AiTemperature = AiTemperatureDefault;
		AiTopP = AiTopPDefault;
		AiMaxTokens = AiMaxTokensDefault;
		AiServices = new Dictionary<string, AiServiceInfo>(StringComparer.OrdinalIgnoreCase);
		foreach (var serviceName in AiServiceNames)
		{
			AiServices[serviceName] = GetServiceDefaults(serviceName);
		}
		Save();
	}

	private static string ServicePreferenceKey(string serviceName, string fieldName)
	{
		return $"{serviceName}-{fieldName}";
	}

	private static string[] ParseServiceNames(string? value)
	{
		if (string.IsNullOrWhiteSpace(value))
			return [];

		return value
			.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
			.Where(name => !name.Equals(AiServiceNone, StringComparison.OrdinalIgnoreCase))
			.Distinct(StringComparer.OrdinalIgnoreCase)
			.ToArray();
	}

	private static AiServiceInfo GetServiceDefaults(string serviceName)
	{
		return serviceName switch
		{
			"OpenAI" => new AiServiceInfo(OpenAiModelDefault, OpenAiEndPointDefault, OpenAiKeyDefault),
			"DeepSeek" => new AiServiceInfo(DeepSeekModelDefault, DeepSeekEndPointDefault, DeepSeekKeyDefault),
			"Github" => new AiServiceInfo(GithubModelDefault, GithubEndPointDefault, GithubKeyDefault),
			"Ollama" => new AiServiceInfo(OllamaModelDefault, OllamaEndPointDefault, OllamaKeyDefault),
			_ => new AiServiceInfo("", "", "")
		};
	}

	public static AiServiceInfo AiService(string? aiService)
	{
		if (IsNoneService(aiService))
			return new AiServiceInfo("", "", "");

		if (AiServices is null)
			return new AiServiceInfo("", "", "");

		if (AiServices.TryGetValue(aiService!, out var serviceInfo))
			return serviceInfo;

		return new AiServiceInfo("", "", "");
	}

	public static string NormalizeServiceName(string? serviceName)
	{
		if (string.IsNullOrWhiteSpace(serviceName))
			return AiServiceNone;

		if (serviceName.Equals(AiServiceNone, StringComparison.OrdinalIgnoreCase))
			return AiServiceNone;

		foreach (var name in AiServiceNames)
		{
			if (name.Equals(serviceName, StringComparison.OrdinalIgnoreCase))
				return name;
		}

		return AiServiceNone;
	}

	public static string[] ServicePickerNames()
	{
		if (AiServiceNames.Length == 0)
			return [AiServiceNone];

		string[] names = new string[AiServiceNames.Length + 1];
		names[0] = AiServiceNone;
		Array.Copy(AiServiceNames, 0, names, 1, AiServiceNames.Length);
		return names;
	}

	public static bool IsNoneService(string? serviceName)
	{
		return string.IsNullOrWhiteSpace(serviceName)
			|| serviceName.Equals(AiServiceNone, StringComparison.OrdinalIgnoreCase);
	}

	public static bool IsOllamaService(string? serviceName)
	{
		return serviceName?.Equals("Ollama", StringComparison.OrdinalIgnoreCase) == true;
	}
	private static void UpdateServiceInfo(string serviceName, string? modelId = null, string? endPoint = null, string? key = null)
	{
		if (AiServices is null)
			AiServices = new Dictionary<string, AiServiceInfo>(StringComparer.OrdinalIgnoreCase);

		if (!AiServices.TryGetValue(serviceName, out var serviceInfo))
			serviceInfo = new AiServiceInfo("", "", "");

		AiServices[serviceName] = new AiServiceInfo(
			modelId ?? serviceInfo.ModelId,
			endPoint ?? serviceInfo.EndPoint,
			key ?? serviceInfo.Key);
	}

	public static float AiTemperature;
	public static float AiTopP;
	public static int AiMaxTokens;

	public static string[] AiServiceNames = [];
	public record AiServiceInfo(string ModelId, string EndPoint, string Key);

	public static Dictionary<string, AiServiceInfo> AiServices = new(StringComparer.OrdinalIgnoreCase);

}




/ *
	public static void Load()
	{

		var configuration = new ConfigurationBuilder()
			.SetBasePath(AppSettings.DocumentHome())
			.AddJsonFile("appsettings.json", optional: true)
			.Build();

		if (float.TryParse(configuration["AI:Temperature"], out float temp1))
			AiTemperature = temp1;
		else
			AiTemperature = 1.0f;
		if (float.TryParse(configuration["AI:TopP"], out float temp2))
			AiTopP = temp2;
		else
			AiTopP = 1.0f;
		if (int.TryParse(configuration["AI:MaxTokens"], out int temp3))
			AiMaxTokens = temp3;
		else
			AiMaxTokens = 10240;

		AiServices = new Dictionary<string, AiServiceInfo>(StringComparer.OrdinalIgnoreCase)
		{
			[AiServiceNames[(int)eAiService.eOpenAi - 1]] = new AiServiceInfo(
				ModelId: configuration["AI:Providers:OpenAI:Model"] ?? "",
				EndPoint: configuration["AI:Providers:OpenAI:EndPoint"] ?? "",
				Key: configuration["AI:Providers:OpenAI:Key"] ?? ""),
			[AiServiceNames[(int)eAiService.eDeepseek - 1]] = new AiServiceInfo(
				ModelId: configuration["AI:Providers:Deepseek:Model"] ?? "",
				EndPoint: configuration["AI:Providers:Deepseek:EndPoint"] ?? "",
				Key: configuration["AI:Providers:Deepseek:Key"] ?? ""),
			[AiServiceNames[(int)eAiService.eGithub - 1]] = new AiServiceInfo(
				ModelId: configuration["AI:Providers:Github:Model"] ?? "",
				EndPoint: configuration["AI:Providers:Github:EndPoint"] ?? "",
				Key: configuration["AI:Providers:Github:Key"] ?? ""),
			[AiServiceNames[(int)eAiService.eOllama - 1]] = new AiServiceInfo(
				ModelId: configuration["AI:Providers:Ollama:Model"] ?? "",
				EndPoint: configuration["AI:Providers:Ollama:EndPoint"] ?? "",
				Key: configuration["AI:Providers:Ollama:Key"] ?? "")
		};
	}

private static void SaveNewDefaults()
{
	try
	{
		string settingsPath = Path.Combine(AppSettings.DocumentHome(), "appsettings.json");
		JsonObject rootObject = LoadConfigurationObject(settingsPath);
		JsonObject aiObject = rootObject["AI"] as JsonObject ?? new JsonObject();
		rootObject["AI"] = aiObject;
		JsonObject providersObject = aiObject["Providers"] as JsonObject ?? new JsonObject();
		aiObject["Providers"] = providersObject;

		bool save = false;

		JsonObject openAIObject = providersObject["OpenAI"] as JsonObject ?? new JsonObject();
		providersObject["OpenAI"] = openAIObject;
		string? openAiEndpoint = (string?)openAIObject["EndPoint"];
		if (string.IsNullOrWhiteSpace(openAiEndpoint))
		{
			openAiEndpoint = DefaultOpenAiEndpoint;
			openAIObject["EndPoint"] = openAiEndpoint;
			save = true;
		}
		UpdateServiceInfo(AiServiceName[(int)eAiService.eOpenAi - 1], endPoint: openAiEndpoint ?? "");

		JsonObject ollamaObject = providersObject["Ollama"] as JsonObject ?? new JsonObject();
		providersObject["Ollama"] = ollamaObject;
		string? ollamaKey = (string?)ollamaObject["Key"];
		if (string.IsNullOrWhiteSpace(ollamaKey))
		{
			ollamaKey = DefaultOllamaKey;
			ollamaObject["Key"] = ollamaKey;
			save = true;
		}
		UpdateServiceInfo(AiServiceName[(int)eAiService.eOllama - 1], key: ollamaKey ?? "");

		var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
		if (save)
			File.WriteAllText(settingsPath, rootObject.ToJsonString(jsonOptions));
	}
	catch (Exception) { }
}

private static JsonObject LoadConfigurationObject(string settingsPath)
{
	if (File.Exists(settingsPath))
	{
		string json = File.ReadAllText(settingsPath);
		if (!string.IsNullOrWhiteSpace(json))
		{
			try
			{
				JsonNode? parsed = JsonNode.Parse(json);
				if (parsed is JsonObject jsonObject)
					return jsonObject;
			}
			catch (JsonException)
			{
			}
		}
	}
	return new JsonObject();
}
*/
