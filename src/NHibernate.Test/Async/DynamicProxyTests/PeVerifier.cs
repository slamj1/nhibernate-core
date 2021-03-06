﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;

namespace NHibernate.Test.DynamicProxyTests
{
	using System.Threading.Tasks;
	public partial class PeVerifier
	{

		public async Task AssertIsValidAsync()
		{
			var process = new Process
			{
				StartInfo =
				{
					FileName = _peVerifyPath,
					RedirectStandardOutput = true,
					UseShellExecute = false,
					Arguments = "\"" + _assemlyLocation + "\" /VERBOSE",
					CreateNoWindow = true
				}
			};

			process.Start();
			var processOutput = await (process.StandardOutput.ReadToEndAsync());
			process.WaitForExit();

			var result = process.ExitCode + " code ";

			if (process.ExitCode != 0)
				Assert.Fail("PeVerify reported error(s): " + Environment.NewLine + processOutput, result);
		}
	}
}
