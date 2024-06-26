﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery
{
	public class RelativeLayoutGallery : ContentPage
	{
		public RelativeLayoutGallery()
		{
			var layout = new RelativeLayout();

			var box1 = new ContentView
			{
				BackgroundColor = Colors.Gray,
				Content = new Label
				{
					Text = "0"
				}
			};

			double padding = 10;
			layout.Children.Add(box1, () => new Rect(((layout.Width + padding) % 60) / 2, padding, 50, 50));

			var last = box1;
			for (int i = 0; i < 200; i++)
			{
				var relativeTo = last; // local copy
				var box = new ContentView
				{
					BackgroundColor = Colors.Gray,
					Content = new Label
					{
						Text = (i + 1).ToString()
					}
				};

				Func<View, bool> pastBounds = view => relativeTo.Bounds.Right + padding + relativeTo.Width > layout.Width;
				layout.Children.Add(box, () => new Rect(pastBounds(relativeTo) ? box1.X : relativeTo.Bounds.Right + padding,
													 pastBounds(relativeTo) ? relativeTo.Bounds.Bottom + padding : relativeTo.Y,
													 relativeTo.Width,
													 relativeTo.Height));

				last = box;
			}

			Content = new ScrollView { Content = layout, Padding = new Thickness(50) };
		}
	}
}
