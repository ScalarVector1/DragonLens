using DragonLens.Core.Systems.ToolSystem;
using DragonLens.Helpers;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using static Terraria.ModLoader.Core.TmodFile;

namespace DragonLens.Content.Tools.Developer
{
	internal class ShaderCompiler
	{
		public static readonly string cachePath = Path.Join(Main.SavePath, "DragonLensShaderCache");

		public static readonly string compilerPath = Path.Join(Main.SavePath, "DragonLensShaderCache", "EasyXnb.exe");

		private static readonly SemaphoreSlim shaderBuildSemaphore = new(1, 1);

		public ShaderCompiler() : base()
		{
			if (!File.Exists(compilerPath))
				Setup();
		}

		/// <summary>
		/// Setup the cache folder for first time use
		/// </summary>
		private void Setup()
		{
			Directory.CreateDirectory(cachePath);
			CopyFile("EasyXnb.exe");
			CopyFile("EasyXnb.exe.config");
			CopyFile("Microsoft.Xna.Framework.Content.Pipeline.dll");
			CopyFile("Microsoft.Xna.Framework.Content.Pipeline.EffectImporter.dll");
			CopyFile("Microsoft.Xna.Framework.Content.Pipeline.FBXImporter.dll");
			CopyFile("Microsoft.Xna.Framework.Content.Pipeline.TextureImporter.dll");
			CopyFile("Microsoft.Xna.Framework.dll");
			CopyFile("Microsoft.Xna.Framework.Game.dll");
			CopyFile("Microsoft.Xna.Framework.Graphics.dll");
		}

		/// <summary>
		/// Helper to copy files from the tmod to the cache folder
		/// </summary>
		/// <param name="name"></param>
		private static void CopyFile(string name)
		{
			File.WriteAllBytes(Path.Combine(cachePath, name), ModLoader.GetMod("DragonLens").GetFileBytes($"EasyXnb/{name}"));
		}

		/// <summary>
		/// Starts an async shader build
		/// </summary>
		/// <param name="path"></param>
		public async Task<bool> StartShaderBuild(string path, string dest)
		{
			await shaderBuildSemaphore.WaitAsync();

			try
			{
				return await Task.Run(() => BuildShader(path, dest));
			}
			finally
			{
				shaderBuildSemaphore.Release();
			}
		}

		/// <summary>
		/// Scans the source for include statements and tries to copy them over if applicable
		/// </summary>
		/// <param name="sourcePath"></param>
		private void TryCopyIncludes(string sourcePath)
		{
			var lines = File.ReadAllLines(sourcePath);

			foreach(string include in lines.Where(n => n.StartsWith("#include")))
			{
				var groups = Regex.Match(include, "#include[\t ]+\"(.*)\"").Groups;

				if (groups.Count > 1)
				{
					string includePath = groups[1].Value;
					includePath = Path.Combine(Path.GetDirectoryName(sourcePath), includePath);

					if (File.Exists(includePath))
					{
						string target = Path.Combine(cachePath, "Source", Path.GetFileName(includePath));
						Directory.CreateDirectory(Path.GetDirectoryName(target));
						File.Copy(includePath, target, true);

						DragonLens.instance.Logger.Info($"Copying import {includePath}");
					}
				}
			}
		}

		/// <summary>
		/// Inner logic to build the effect
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		private bool BuildShader(string path, string dest)
		{
			string target = Path.Combine(cachePath, "Source", Path.GetFileName(path));
			Directory.CreateDirectory(Path.GetDirectoryName(target));
			File.Copy(path, target, true);

			TryCopyIncludes(path);

			Process process = new()
			{
				StartInfo = {
					FileName = compilerPath,
					WorkingDirectory = cachePath,
					UseShellExecute = false,
					CreateNoWindow = true,
					RedirectStandardOutput = true,
					RedirectStandardError = true
				}
			};

			string output = "";
			string error = "";

			process.OutputDataReceived += (sender, args) => { if (args.Data != null) output += args.Data + "\n"; };
			process.ErrorDataReceived += (sender, args) => { if (args.Data != null) error += args.Data + "\n"; };

			process.Start();

			process.BeginOutputReadLine();
			process.BeginErrorReadLine();

			process.WaitForExit();

			if (!string.IsNullOrEmpty(output))
				DragonLens.instance.Logger.Info(output);

			if (!string.IsNullOrEmpty(error))
				DragonLens.instance.Logger.Error(error);

			foreach (var file in Directory.EnumerateFiles(Path.GetDirectoryName(target)))
			{
				File.Delete(file);
			}

			string compiledPath = Path.ChangeExtension(Path.Combine(cachePath, Path.GetFileName(path)), "xnb");

			if (File.Exists(compiledPath))
			{
				File.Move(compiledPath, dest, true);
				Main.NewText(LocalizationHelper.GetToolText("ShaderCompiler.Recompiled", path), Color.SkyBlue);
				return true;
			}
			else
			{
				Main.NewText($"Failed to compile: {error}", Color.Red);
				return false;
			}
		}
	}
}