using DragonLens.Core.Systems.ToolSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using static Terraria.ModLoader.Core.TmodFile;

namespace DragonLens.Content.Tools.Developer
{
	internal class ShaderCompiler : Tool
	{
		public static readonly string cachePath = Path.Join(Main.SavePath, "DragonLensShaderCache");

		public static readonly string compilerPath = Path.Join(Main.SavePath, "DragonLensShaderCache", "EasyXnb.exe");

		public static bool busy;

		public override string IconKey => "ShaderCompiler";

		public override string DisplayName => "Shader Reloader";

		public override string Description => "Re-compile and re-load all shaders";

		public ShaderCompiler() : base()
		{
			if (!File.Exists(compilerPath))
				Setup();
		}

		public override void OnActivate()
		{
			if (busy)
			{
				Main.NewText("Shader compiler is busy! Please wait...", Color.Orange);
				return;
			}

			Main.NewText("Triggered shader re-compile...");
			busy = true;

			Stopwatch watch = new();

			var thread = new Thread(() =>
			{
				watch.Start();

				foreach (Mod mod in ModLoader.Mods)
				{
					if (PrepAllShaders(mod))
					{
						CompileShaders(mod);
						CopyShadersBack(mod);
						LoadAllShaders(mod);
					}
				}

				watch.Stop();
				Main.NewText($"Shader re-compilation completed in {watch.ElapsedMilliseconds}");
				busy = false;
			});
			thread.Start();
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
		private void CopyFile(string name)
		{
			File.WriteAllBytes(Path.Combine(cachePath, name), ModLoader.GetMod("DragonLens").GetFileBytes($"EasyXnb/{name}"));
		}

		public bool PrepAllShaders(Mod mod)
		{
			if (Main.dedServ)
				return false;

			MethodInfo info = typeof(Mod).GetProperty("File", BindingFlags.NonPublic | BindingFlags.Instance).GetGetMethod(true);
			var file = (TmodFile)info.Invoke(mod, null);

			if (file is null)
				return false;

			IEnumerable<FileEntry> shaders = file.Where(n => n.Name.StartsWith("Effects/") && n.Name.Count(a => a == '/') <= 1 && n.Name.EndsWith(".fx"));

			if (shaders.Count() <= 0)
				return false;

			foreach (FileEntry entry in shaders)
			{
				string fileName = entry.Name.Replace("Effects/", "");
				File.WriteAllBytes(Path.Combine(cachePath, fileName), mod.GetFileBytes(entry.Name));
			}

			return true;
		}

		public void CompileShaders(Mod mod)
		{
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

			process.Start();

			string output = process.StandardOutput.ReadToEnd();
			Mod.Logger.Info(output);
			string err = process.StandardError.ReadToEnd();
			Mod.Logger.Error(err);

			Main.NewTextMultiline(output, true, Color.White);

			process.WaitForExit();

			Main.NewText($"Shaders for {mod.DisplayName} recompiled!", Color.SkyBlue);

			if (!Directory.Exists(Path.Combine(cachePath, mod.Name)))
				Directory.CreateDirectory(Path.Combine(cachePath, mod.Name));

			//Copy the compiled shaders into the mod-specific cache subdir
			IEnumerable<string> files = Directory.GetFiles(cachePath).Where(n => n.EndsWith(".xnb"));

			foreach (string path in files)
			{
				string target = Path.Combine(cachePath, mod.Name, Path.GetFileName(path));

				if (File.Exists(target))
					File.Replace(path, target, null);
				else
					File.Copy(path, target);
			}
		}

		public void CopyShadersBack(Mod mod)
		{
			MethodInfo info = typeof(Mod).GetProperty("File", BindingFlags.NonPublic | BindingFlags.Instance).GetGetMethod(true);
			var file = (TmodFile)info.Invoke(mod, null);

			if (file is null)
				return;

			if (file.path.StartsWith(ModLoader.ModPath) && Directory.Exists(Path.Combine(cachePath, mod.Name)))
			{
				IEnumerable<string> files = Directory.GetFiles(Path.Combine(cachePath, mod.Name)).Where(n => n.EndsWith(".xnb"));

				foreach (string path in files)
				{
					string target = Path.Join(file.path.Replace("\\Mods\\", "\\ModSources\\").Replace(".tmod", ""), "Effects", Path.GetFileName(path));

					if (File.Exists(target))
						File.Replace(path, target, null);
					else
						File.Copy(path, target);
				}
			}
		}

		public void LoadAllShaders(Mod mod)
		{
			if (Main.dedServ)
				return;

			MethodInfo info = typeof(Mod).GetProperty("File", BindingFlags.NonPublic | BindingFlags.Instance).GetGetMethod(true);
			var file = (TmodFile)info.Invoke(mod, null);

			if (file is null)
				return;

			IEnumerable<FileEntry> shaders = file.Where(n => n.Name.StartsWith("Effects/") && n.Name.Count(a => a == '/') <= 1 && n.Name.EndsWith(".xnb"));

			foreach (FileEntry entry in shaders)
			{
				string name = entry.Name.Replace(".xnb", "").Replace("Effects/", "");
				string path = entry.Name.Replace(".xnb", "");
				LoadShader(name, path, mod);
			}
		}

		public static void LoadShader(string name, string path, Mod mod)
		{
			var screenRef = new Ref<Effect>(mod.Assets.Request<Effect>(path, ReLogic.Content.AssetRequestMode.ImmediateLoad).Value);
			Terraria.Graphics.Effects.Filters.Scene[name] = new Filter(new ScreenShaderData(screenRef, name + "Pass"), EffectPriority.High);
			Terraria.Graphics.Effects.Filters.Scene[name].Load();
		}
	}
}
