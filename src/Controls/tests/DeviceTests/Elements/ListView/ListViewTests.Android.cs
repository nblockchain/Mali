﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Platform;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Xunit;
using Android.Views;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.ListView)]
	public partial class ListViewTests
	{
		void ValidatePlatformCells(ListView listView)
		{
			var renderer = (ListViewRenderer)listView.Handler;

			var viewCells = renderer
				.Control
				.GetChildrenOfType<ViewCellRenderer.ViewCellContainer>()
				.ToList();

			// This validates that all the cells created/added their correct
			// number of viewz
			foreach (var cell in viewCells)
			{
				Assert.Equal(1, cell.ChildCount);

				var renderedView = cell.GetChildAt(0) as ViewGroup;
				Assert.Equal(1, renderedView.ChildCount);
			}
		}
	}
}