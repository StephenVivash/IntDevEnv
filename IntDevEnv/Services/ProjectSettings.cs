namespace IntDevEnv.Services;

internal sealed record ProjectSettings
{
	public string? Type { get; init; }
	public string? Reference { get; init; }
	public string? CompilerFlags { get; init; }
	public string? Defines { get; init; }
	public ProjectBuildSettings? Debug { get; init; }
	public ProjectBuildSettings? Release { get; init; }

	public string GetOutputType()
	{
		return string.IsNullOrWhiteSpace(Type) ? "exe" : Type.Trim();
	}

	public IReadOnlyList<string> GetReferences()
	{
		return SplitValues(Reference);
	}

	public string GetCompilerFlags(bool debug)
	{
		ProjectBuildSettings? buildSettings = debug ? Debug : Release;
		return JoinValues(CompilerFlags, buildSettings?.CompilerFlags);
	}

	public string GetDefineFlags(bool debug)
	{
		ProjectBuildSettings? buildSettings = debug ? Debug : Release;
		return string.Join(" ",
			SplitValues(Defines)
				.Concat(SplitValues(buildSettings?.Defines))
				.Select(FormatDefine));
	}

	private static string JoinValues(params string?[] values)
	{
		return string.Join(" ", values.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value!.Trim()));
	}

	private static IReadOnlyList<string> SplitValues(string? value)
	{
		if (string.IsNullOrWhiteSpace(value))
			return [];

		return value
			.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
			.Where(entry => !string.IsNullOrWhiteSpace(entry))
			.ToArray();
	}

	private static string FormatDefine(string value)
	{
		if (value.StartsWith("/D", StringComparison.OrdinalIgnoreCase) ||
			value.StartsWith("-D", StringComparison.OrdinalIgnoreCase))
			return value;

		return $"/D{QuoteIfNeeded(value)}";
	}

	private static string QuoteIfNeeded(string value)
	{
		return value.Any(char.IsWhiteSpace) ? $"\"{value}\"" : value;
	}
}

internal sealed record ProjectBuildSettings
{
	public string? CompilerFlags { get; init; }
	public string? Defines { get; init; }
}

