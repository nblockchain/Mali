﻿using System.Collections.Generic;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 4992, "CollectionView doesn't resize on orientation change",
		PlatformAffected.Android)]
	public sealed partial class Issue4992 : TestShell
	{
		public List<string> People { get; set; }

		public Issue4992()
		{
#if APP
			InitializeComponent();

			People = new List<string>
			{
				"Alan",
				"Betty",
				"Charles",
				"David",
				"Edward",
				"Francis",
				"Gary",
				"Helen",
				"Ivan",
				"Joel",
				"Kelly",
				"Larry",
				"Mary",
				"Nancy",
				"Olivia",
				"Peter",
				"Quincy",
				"Robert",
				"Stephen",
				"Timothy",
				"Ursula",
				"Vincent",
				"William",
				"Xavier",
				"Yvonne",
				"Zack"
			};

			CV.BindingContext = this;
			CV.ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical); // Vertical is default
#endif
		}

		protected override void Init()
		{
		}
	}
}