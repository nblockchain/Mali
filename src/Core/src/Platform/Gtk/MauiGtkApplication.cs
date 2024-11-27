using System;
using System.Text.RegularExpressions;
using Gtk;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;

namespace Microsoft.Maui
{
	public abstract class MauiGtkApplication : IPlatformApplication
	{
		/// <param name="enforceUniqueness">If true, only one instance of application can run at any moment.</param>
		public MauiGtkApplication(bool enforceUniqueness = false)
		{
			EnforceUniqueness = enforceUniqueness;
		}

		protected abstract MauiApp CreateMauiApp();

		static readonly Regex InvalidGtkApplicationIdElementCharRegex = new Regex("[^A-Za-z0-9_\\-]");

		// https://docs.gtk.org/gio/type_func.Application.id_is_valid.html
		public virtual string ApplicationId
		{
			get
			{
				var name = InvalidGtkApplicationIdElementCharRegex.Replace(Name!, "_");
				if (name.Length == 0 || (name[0] >= '0' || name[0] <= '9'))
					name = "_" + name;
				return $"{typeof(MauiGtkApplication).Namespace}.{nameof(MauiGtkApplication)}.{name}".PadRight(255, ' ').Substring(0, 255).Trim();
			}
		}

		string? _name;

		// https://docs.gtk.org/gio/type_func.Application.id_is_valid.html
		public string? Name
		{
			get
			{
				if (_name is null)
				{
					var exeName = Environment.GetCommandLineArgs()[0];
					_name = string.IsNullOrEmpty(exeName) ? "Unknown" : System.IO.Path.GetFileName(exeName);
				}
				return _name;
			}
			set { _name = value; }
		}

		// https://docs.gtk.org/gtk3/class.Application.html
		public static Gtk.Application CurrentGtkApplication { get; internal set; } = null!;

		public static MauiGtkApplication Current { get; internal set; } = null!;

		public MauiGtkMainWindow MainWindow { get; internal set; } = null!; // set is not protected because it needs to be accessed from ApplicationExtensions.cs

		public IServiceProvider Services { get; protected set; } = null!;

		public IApplication Application { get; protected set; } = null!;
		public bool EnforceUniqueness { get; }

		public void Run(params string[] args)
		{
			Launch(EventArgs.Empty);
		}

		protected void RegisterLifecycleEvents(Gtk.Application app)
		{
			app.Startup += OnStartup!;
			app.Shutdown += OnShutdown!;
			app.Opened += OnOpened;
			app.WindowAdded += OnWindowAdded;
			app.Activated += OnActivated!;
			app.WindowRemoved += OnWindowRemoved;
			app.CommandLine += OnCommandLine;
		}

		protected void OnStartup(object sender, EventArgs args)
		{
			Services.InvokeLifecycleEvents<GtkLifecycle.OnStartup>(del => del(CurrentGtkApplication, args));
		}

		protected void OnOpened(object o, GLib.OpenedArgs args)
		{
			Services?.InvokeLifecycleEvents<GtkLifecycle.OnOpened>(del => del(CurrentGtkApplication, args));
		}

		protected void OnActivated(object sender, EventArgs args)
		{
			StartupLauch(sender, args);

			Services?.InvokeLifecycleEvents<GtkLifecycle.OnApplicationActivated>(del => del(CurrentGtkApplication, args));
		}

		protected void OnShutdown(object sender, EventArgs args)
		{
			Services?.InvokeLifecycleEvents<GtkLifecycle.OnShutdown>(del => del(CurrentGtkApplication, args));

			Dispatcher.DispatchPendingEvents();
		}

		protected void OnCommandLine(object o, GLib.CommandLineArgs args)
		{
			// future use: to resolove command line arguments at cross platform Application level
		}

		protected void OnWindowRemoved(object o, WindowRemovedArgs args)
		{
			// future use: to have notifications at cross platform Window level
		}

		protected void OnWindowAdded(object o, WindowAddedArgs args)
		{
			// future use: to have notifications at cross platform Window level
		}

		protected void StartupLauch(object sender, EventArgs args)
		{
			IPlatformApplication.Current = this;

			var mauiApp = CreateMauiApp();

			Services = mauiApp.Services;
			Services.InvokeLifecycleEvents<GtkLifecycle.OnLaunching>(del => del(this, args));

			var rootContext = new MauiContext(Services);
			var applicationContext = rootContext.MakeApplicationScope(CurrentGtkApplication);

			Services.InvokeLifecycleEvents<GtkLifecycle.OnMauiContextCreated>(del => del(applicationContext));

			Application = Services.GetRequiredService<IApplication>();

			CurrentGtkApplication.SetApplicationHandler(Application, applicationContext);

			CurrentGtkApplication.CreatePlatformWindow(Application, new PersistedState());

			Services?.InvokeLifecycleEvents<GtkLifecycle.OnLaunched>(del => del(CurrentGtkApplication, args));
		}

		protected void Launch(EventArgs args)
		{
			Gtk.Application.Init();
			var flags = EnforceUniqueness ? GLib.ApplicationFlags.None : GLib.ApplicationFlags.NonUnique;
			var app = new Gtk.Application(ApplicationId, flags);

			RegisterLifecycleEvents(app);

			CurrentGtkApplication = app;

			Current = this;

			((GLib.Application)app).Run();
		}
	}
}