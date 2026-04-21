using System.Text.Json.Serialization;

namespace IntDevEnv.Services;

internal sealed class WorkspaceItem(string name, string path, WorkspaceItemType itemType, int depth, string projectType = "")
{
	public string Name { get; } = name;
	public string Path { get; } = path;
	public WorkspaceItemType Type { get; } = itemType;
	public string ProjectType { get; } = projectType;
	public bool IsWorkspace => Type == WorkspaceItemType.Workspace;
	public bool IsProject => Type == WorkspaceItemType.Project;
	public bool IsFolder => Type == WorkspaceItemType.Folder;
	public bool IsFile => Type == WorkspaceItemType.File;
	public Thickness Indent { get; } = new(depth * 20, 0, 0, 0);
}

internal enum WorkspaceItemType
{
	Workspace,
	Project,
	Folder,
	File,
}

internal sealed record WorkspaceManifest
{
	[JsonPropertyName("project")]
	public string? Project { get; init; }

	[JsonPropertyName("projects")]
	public List<WorkspaceProject>? Projects { get; init; }
}

internal sealed record WorkspaceProject([property: JsonPropertyName("path")] string Path);

