﻿using TKeyboard = Tizen.UIExtensions.Common.Keyboard;
using AutoCapital = Tizen.NUI.InputMethod.AutoCapitalType;

namespace Microsoft.Maui.Platform
{
	public static class KeyboardExtensions
	{
		public static TKeyboard ToPlatform(this Keyboard keyboard)
		{
			if (keyboard == Keyboard.Numeric)
			{
				return TKeyboard.Numeric;
			}
			else if (keyboard == Keyboard.Telephone)
			{
				return TKeyboard.PhoneNumber;
			}
			else if (keyboard == Keyboard.Email)
			{
				return TKeyboard.Email;
			}
			else if (keyboard == Keyboard.Url)
			{
				return TKeyboard.Url;
			}
			else
			{
				return TKeyboard.Normal;
			}
		}

		public static AutoCapital ToAutoCapital(this KeyboardFlags keyboardFlags)
		{
			if (keyboardFlags.HasFlag(KeyboardFlags.CapitalizeSentence))
			{
				return AutoCapital.Sentence;
			}
			else if (keyboardFlags.HasFlag(KeyboardFlags.CapitalizeWord))
			{
				return AutoCapital.Word;
			}
			else if (keyboardFlags.HasFlag(KeyboardFlags.CapitalizeCharacter))
			{
				return AutoCapital.Allcharacter;
			}
			else
			{
				return AutoCapital.None;
			}
		}
	}
}