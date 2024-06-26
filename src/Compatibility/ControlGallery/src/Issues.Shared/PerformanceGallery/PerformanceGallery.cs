﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Plugin.DeviceInfo;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.Performance)] 
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.UwpIgnore)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 0, "Performance Testing")]
	public class PerformanceGallery : TestContentPage
	{
		const string Fail = "FAIL";
		const string Next = "Next Scenario";
		const string NextButtonId = "NextButton";
		const string TestRunRefId = "TestRunRefId";
		const string Pending = "PENDING";
		const string Success = "SUCCESS";
		const double Threshold = 0.25;

		string _DeviceIdentifier = "";
		string _DeviceIdiom;
		string _DeviceModel;
		string _DevicePlatform;
		string _DeviceVersionNumber;
		string _BuildInfo;
		PerformanceProvider _PerformanceProvider = new PerformanceProvider();
		PerformanceTracker _PerformanceTracker = new PerformanceTracker();
		List<PerformanceScenario> _TestCases = new List<PerformanceScenario>();
		int _TestNumber = 0;
		Button nextButton;
		PerformanceViewModel ViewModel => BindingContext as PerformanceViewModel;

		protected override void Init()
		{
			_BuildInfo = GetBuildNumber();

			_DeviceIdentifier = CrossDeviceInfo.Current.Id;
			_DeviceIdiom = CrossDeviceInfo.Current.Idiom.ToString();
			_DeviceModel = CrossDeviceInfo.Current.Model;
#if __ANDROID__ && !LEGACY_RENDERERS
			_DevicePlatform = "Android Fast Renderers";
#else
			_DevicePlatform = CrossDeviceInfo.Current.Platform.ToString();
#endif
			_DeviceVersionNumber = CrossDeviceInfo.Current.VersionNumber.ToString();

			MessagingCenter.Subscribe<PerformanceTracker>(this, PerformanceTracker.RenderCompleteMessage, HandleRenderComplete);

			BindingContext = new PerformanceViewModel(_PerformanceProvider);
			Performance.SetProvider(_PerformanceProvider);

			_TestCases.AddRange(InflatePerformanceScenarios());

			nextButton = new Button { Text = Pending, IsEnabled = false, AutomationId = NextButtonId };
			nextButton.Clicked += NextButton_Clicked;

			ViewModel.TestRunReferenceId = Guid.NewGuid();

			var testRunRef = new Label
			{
				AutomationId = TestRunRefId,
				Text = ViewModel.TestRunReferenceId.ToString(),
				FontSize = 6,
				HorizontalTextAlignment = TextAlignment.Center,
				VerticalTextAlignment = TextAlignment.Center,
				//BackgroundColor = Colors.Accent,
				TextColor = Colors.White
			};

			Content = new StackLayout { Children = { testRunRef, nextButton, _PerformanceTracker } };
			GetBenchmarkResults();
		}

		async void GetBenchmarkResults(int tryCount = 0)
		{
			bool success = false;
			try
			{
				ViewModel.BenchmarkResults = await PerformanceDataManager.GetScenarioResults(_DeviceIdentifier);
				success = true;
			}
			catch (Exception exc)
			{
				if (tryCount < 3)
					GetBenchmarkResults(++tryCount);
				else
					nextButton.Text = exc.ToString();
			}

			if (success)
			{
				nextButton.IsEnabled = true;
				nextButton.Text = Next;
			}
		}

		private static string GetBuildNumber()
		{
			try
			{
				var assembly = typeof(PerformanceGallery).Assembly;
				var txt = "Microsoft.Maui.Controls.ControlGallery.BuildNumber.txt";

				using (Stream s = assembly.GetManifestResourceStream(txt))
				using (StreamReader sr = new StreamReader(s))
					return sr.ReadToEnd();
			}
			catch { return "master"; }
		}

		static IEnumerable<Type> FindPerformanceScenarios()
		{
			return typeof(PerformanceGallery).Assembly.GetTypes()
				.Where(type => typeof(PerformanceScenario).IsAssignableFrom(type));
		}

		static IEnumerable<PerformanceScenario> InflatePerformanceScenarios()
		{
			var scenarios = FindPerformanceScenarios()
							.Select(o => (PerformanceScenario)Activator.CreateInstance(o))
							.Where(scenario => scenario.View != null)
							.OrderBy(scenario => scenario.Name);

			if (scenarios.GroupBy(c => c.Name).Any(c => c.Count() > 1))
				throw new InvalidOperationException("Scenario names must be unique");

			return scenarios;
		}

		PerformanceDataManager.Result DisplayResults()
		{
			ViewModel.ActualRenderTime = TimeSpan.FromTicks(_PerformanceProvider.Statistics.Where(c => !c.Value.IsDetail).Sum(c => c.Value.TotalTime)).TotalMilliseconds;

			// perf should be within threshold
			if (ViewModel.ExpectedRenderTime == 0)
			{
				ViewModel.Outcome = Fail;
				return PerformanceDataManager.Result.Inconclusive;
			}
			else if (Math.Abs(ViewModel.ActualRenderTime - ViewModel.ExpectedRenderTime) > ViewModel.ExpectedRenderTime * Threshold)
			{
				ViewModel.Outcome = Fail;
				return PerformanceDataManager.Result.Fail;
			}
			else
			{
				ViewModel.Outcome = Success;
				return PerformanceDataManager.Result.Pass;
			}
		}

		void HandleRenderComplete(PerformanceTracker obj)
		{
			var result = DisplayResults();

#pragma warning disable 4014
			PerformanceDataManager.PostScenarioResults(ViewModel.Scenario,
				result,
				ViewModel.TestRunReferenceId,
				_DeviceIdentifier,
				_DevicePlatform,
				_DeviceVersionNumber,
				_DeviceIdiom,
				_BuildInfo,
				ViewModel.ActualRenderTime,
				_PerformanceProvider.Statistics);
#pragma warning restore 4014
		}

		void NextButton_Clicked(object sender, EventArgs e)
		{
			if (_TestCases?.Count == 0 || _TestNumber + 1 > _TestCases?.Count)
				return;

			ViewModel.View = null;
			ViewModel.ActualRenderTime = 0;
			ViewModel.Outcome = Pending;
			ViewModel.RunTest(_TestCases[_TestNumber++]);
		}

#if false && UITEST

		double TopThreshold => 1 + Threshold;
		double BottomThreshold => 1 - Threshold;

		[Test]
		public void PerformanceTest()
		{
			var testCasesCount = FindPerformanceScenarios().Count();

			List<string> warnings = new List<string>();

			try
			{
				RunningApp.WaitForElement(q => q.Marked(Next), timeout: TimeSpan.FromSeconds(30));
			}
			catch (Exception)
			{
				Assert.Inconclusive("Timed out waiting for the test to initialize. May be unable to communicate with the API server to get benchmarks.");
			}

			for (int i = 0; i < testCasesCount; i++)
			{
				RunningApp.Tap(q => q.Marked(NextButtonId));

				try
				{
					RunningApp.WaitForElement(q => q.Marked(Success));
				}
				catch (Exception)
				{
					var message = GetFailureMessage();
					RunningApp.Screenshot(message);
					if (!warnings.Contains(message))
						warnings.Add(message);
				}
			}

			string testRunReferenceId = GetText(TestRunRefId);

			if (warnings.Any())
				Assert.Inconclusive($"Performance threshold exceeded.\r\n{string.Join("\r\n", warnings)}\r\nTestRunReferenceId: {testRunReferenceId}");
			else
				Assert.Pass($"TestRunReferenceId: {testRunReferenceId}");
		}

		string GetFailureMessage()
		{
			double expected = 0;
			double.TryParse(GetText(PerformanceTrackerTemplate.ExpectedId), out expected);

			var scenario = GetText(PerformanceTrackerTemplate.ScenarioId);
			var actual = GetText(PerformanceTrackerTemplate.ActualId);

			return $" - Scenario \"{scenario}\" failed. Expected {expected * BottomThreshold}-{expected * TopThreshold}ms, Actual {actual}ms.";
		}

		string GetText(string id)
		{
			return RunningApp.Query(q => q.Marked(id))[0].Text;
		}
#endif
	}
}