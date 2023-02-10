﻿using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Button)]
	public partial class ButtonTests : ControlsHandlerTestBase
	{
		[Theory]
		[ClassData(typeof(TextTransformCases))]
		public async Task InitialTextTransformApplied(string text, TextTransform transform, string expected)
		{
			var control = new Button() { Text = text, TextTransform = transform };
			var platformText = await GetPlatformText(await CreateHandlerAsync<ButtonHandler>(control));
			Assert.Equal(expected, platformText);
		}

		[Theory]
		[ClassData(typeof(TextTransformCases))]
		public async Task TextTransformUpdated(string text, TextTransform transform, string expected)
		{
			var control = new Button() { Text = text };
			var handler = await CreateHandlerAsync<ButtonHandler>(control);
			await InvokeOnMainThreadAsync(() => control.TextTransform = transform);
			var platformText = await GetPlatformText(handler);
			Assert.Equal(expected, platformText);
		}

		[Theory(DisplayName = "LineBreakMode Initializes Correctly")]
		[InlineData(LineBreakMode.MiddleTruncation)]
		[InlineData(LineBreakMode.HeadTruncation)]
		[InlineData(LineBreakMode.TailTruncation)]
		[InlineData(LineBreakMode.WordWrap)]
		[InlineData(LineBreakMode.CharacterWrap)]
		[InlineData(LineBreakMode.NoWrap)]
		public async Task LineBreakModeInitializesCorrectly(LineBreakMode lineBreakMode)
		{
			var xplatLineBreakMode = lineBreakMode;

			var button = new Button()
			{
				LineBreakMode = xplatLineBreakMode
			};

			var expectedValue = xplatLineBreakMode.ToPlatform();

			var handler = await CreateHandlerAsync<ButtonHandler>(button);

			await InvokeOnMainThreadAsync(() =>
			{
				Assert.Equal(expectedValue, GetPlatformLineBreakMode(handler));
			});
		}
	}
}