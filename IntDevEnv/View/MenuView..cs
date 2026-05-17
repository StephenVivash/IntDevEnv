
using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core;

using IntDevEnv.Services;
using IntDevEnv.Pages;

using Gui.Controls;
using Gui.Services;

namespace IntDevEnv.Views;

public partial class MenuView : ContentView
{
	//private static readonly Color _darkBackgroundColor = Color.FromArgb("#1F1F1F");
	//private static Color _bgColor = App.Current.RequestedTheme == AppTheme.Dark ? _darkBackgroundColor : Colors.White;
	private ePages _ePage = ePages.eNone;
	private StackLayout? _slMenu;

	private ButtonEx? _btnClean;
	private ButtonEx? _btnRebuild;
	private ButtonEx? _btnRebuildAll;
	private ButtonEx? _btnRun;
	private ButtonEx? _btnSetting;

	private Picker? _pkProject;
	private Picker? _pkMode;
	private List<String> _modes = ["Debug", "Release"];
	private readonly ObservableCollection<ProjectPickerItem> _projects = [];
	private bool _isUpdatingProjectSelection;

#if WINDOWS || MACCATALYST
	public const DockPosition VerticalDockPosition = DockPosition.Left;
#elif ANDROID || IOS
	public const DockPosition VerticalDockPosition = DockPosition.Right;
#endif

	public MenuView()
	{
		BindingContext = this;
		//BackgroundColor = _bgColor;
	}

	public string? Mode
	{
		get => _pkMode?.SelectedItem as string;
	}

	public event EventHandler<ProjectPickerItem>? ProjectChanged;

	protected override void OnSizeAllocated(double width, double height)
	{
		if ((width == -1) || (height == -1))
			return;

		double w = width - 10;

		w = width - 40;

		w /= 2;
		w -= 10;

		w /= 2;

		base.OnSizeAllocated(width, height);
	}

	public void Create(ePages ePage, StackOrientation orientation)
	{
		var o = orientation == StackOrientation.Vertical ? "V" : "H";
		var b = new RegisterInViewDirectoryBehavior() { Key = $"{ePage}{o}Menu" };
		Behaviors.Add(b);

		//_ePage = ePage;

		Border border = new()
		{
			//BackgroundColor = Colors.Transparent,
		};

		_slMenu = new StackLayout()
		{
			Orientation = orientation,
			//BackgroundColor = _bgColor,
			Margin = new Thickness(0, 0, 0, 0),
			Spacing = 0,
		};

		_btnClean = new ButtonEx()
		{
			Icon = FluentIcons.Delete,
		};
		_btnRebuild = new ButtonEx()
		{
			Icon = FluentIcons.Lightning,
		};
		_btnRebuildAll = new ButtonEx()
		{
			Icon = FluentIcons.Repeat,
		};
		_btnRun = new ButtonEx()
		{
			Icon = FluentIcons.Play,
		};
		_btnSetting = new ButtonEx()
		{
			Icon = FluentIcons.Setting,
		};
		_pkMode = new Picker()
		{
			ItemsSource = _modes,
			SelectedIndex = 0,
		};
		_pkProject = new Picker()
		{
			ItemsSource = _projects,
			ItemDisplayBinding = new Binding(nameof(ProjectPickerItem.Name)),
			//WidthRequest = orientation == StackOrientation.Vertical ? 160 : 220,
		};

		if (orientation == StackOrientation.Vertical)
#if WINDOWS || MACCATALYST
			_slMenu.WidthRequest = 220;
#elif ANDROID || IOS
			_slMenu.WidthRequest = 220;
#endif
		else
			_slMenu.HeightRequest = 45;

		border.Content = _slMenu;

		ToolTipProperties.SetText(_btnClean, "Clean");
		ToolTipProperties.SetText(_btnRebuild, "Rebuild");
		ToolTipProperties.SetText(_btnRebuildAll, "Rebuild All");
		ToolTipProperties.SetText(_btnRun, "Run");
		ToolTipProperties.SetText(_btnSetting, "Settings");

		_slMenu.Children.Add(_btnClean);
		_slMenu.Children.Add(_btnRebuild);
		_slMenu.Children.Add(_btnRebuildAll);
		_slMenu.Children.Add(_btnRun);
		_slMenu.Children.Add(_btnSetting);
		_slMenu.Children.Add(_pkProject);
		_slMenu.Children.Add(_pkMode);

		_btnClean.Clicked += btnClean_Clicked!;
		_btnRebuild.Clicked += btnRebuild_Clicked!;
		_btnRebuildAll.Clicked += btnRebuildAll_Clicked!;
		_btnRun.Clicked += btnRun_Clicked!;
		_btnSetting.Clicked += btnSetting_Clicked!;

		_pkProject.SelectedIndexChanged += pkProject_SelectedIndexChanged!;
		_pkMode.SelectedIndexChanged += pkMode_SelectedIndexChanged!;
		Content = border;
	}

	public void SetProjects(IEnumerable<ProjectPickerItem> projects)
	{
		string? selectedProjectPath = (_pkProject?.SelectedItem as ProjectPickerItem)?.Path;
		ProjectPickerItem[] nextProjects = projects.ToArray();

		_isUpdatingProjectSelection = true;
		try
		{
			_projects.Clear();
			foreach (ProjectPickerItem project in nextProjects)
			{
				_projects.Add(project);
			}
		}
		finally
		{
			_isUpdatingProjectSelection = false;
		}

		SetSelectedProject(selectedProjectPath);
	}

	public void SetSelectedProject(string? selectedProjectPath)
	{
		_isUpdatingProjectSelection = true;
		try
		{
			ProjectPickerItem? selectedProject = _projects.FirstOrDefault(project =>
				string.Equals(project.Path, selectedProjectPath, StringComparison.OrdinalIgnoreCase));

			_pkProject!.SelectedItem = selectedProject;
		}
		finally
		{
			_isUpdatingProjectSelection = false;
		}
	}

	private void btnClean_Clicked(object sender, EventArgs e)
	{
		UI.Call<WorkspacePage>(p => p.OnClean());
	}

	private void btnRebuild_Clicked(object sender, EventArgs e)
	{
		UI.Call<WorkspacePage>(p => p.OnRebuild());
	}

	private void btnRebuildAll_Clicked(object sender, EventArgs e)
	{
		UI.Call<WorkspacePage>(p => p.OnRebuildAll());
	}

	private void btnRun_Clicked(object sender, EventArgs e)
	{
		UI.Call<WorkspacePage>(p => p.OnRun());
	}

	private void btnSetting_Clicked(object sender, EventArgs e)
	{
	}

	private void pkProject_SelectedIndexChanged(object? sender, EventArgs e)
	{
		if (_isUpdatingProjectSelection || _pkProject?.SelectedItem is not ProjectPickerItem selectedProject)
			return;

		ProjectChanged?.Invoke(this, selectedProject);
	}

	private void pkMode_SelectedIndexChanged(object sender, EventArgs e)
	{
		//if (Mode == "Debug")
	}

	public sealed record ProjectPickerItem(string Name, string Path);

	/*
	public static readonly BindableProperty CardColorProperty = BindableProperty.Create(nameof(CardColor),
		typeof(Color), typeof(MenuView), App.Current.RequestedTheme == AppTheme.Dark ? _darkBackgroundColor : Colors.White);

	public Color CardColor
	{
		get => (Color)GetValue(CardColorProperty);
		set => SetValue(CardColorProperty, value);
	}*/
}
