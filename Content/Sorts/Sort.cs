using DragonLens.Content.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DragonLens.Content.Sorts
{
	internal struct Sort
	{
		public string Name;
		public Func<BrowserButton, BrowserButton, int> Function;

		public Sort(string name, Func<BrowserButton, BrowserButton, int> function)
		{
			Name = name;
			Function = function;
		}
	}
}
