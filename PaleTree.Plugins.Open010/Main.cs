﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PaleTree.Shared;
using PaleTree.Plugins.Open010.Properties;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace PaleTree.Plugins.Open010
{
	public class Main : Plugin
	{
		private Form form;

		public override string Name
		{
			get { return "Open010"; }
		}

		public Main(IPluginManager pluginManager)
			: base(pluginManager)
		{

		}

		public override void Initialize()
		{
			manager.AddToMenu(Name + " options", OnClickMenu);
			manager.AddToToolbar(Resources.application_go, "Open packet in 010", OnClickButton);
		}

		private void OnClickMenu(object sender, EventArgs e)
		{
			if (form == null || form.IsDisposed)
				manager.OpenCentered(form = new FrmOptions());
			else
				form.Focus();
		}

		private void OnClickButton(object sender, EventArgs e)
		{
			var palePacket = manager.GetSelectedPacket();
			if (palePacket == null)
			{
				MessageBox.Show("No packet selected.", Name);
				return;
			}

			if (string.IsNullOrWhiteSpace(Settings.Default.Folder010) || string.IsNullOrWhiteSpace(Settings.Default.FolderTpl))
			{
				MessageBox.Show("Please configure the plugin at 'Plugins > Open010 options'.", Name, MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}

			var path010 = Path.Combine(Settings.Default.Folder010, "010Editor.exe");
			if (!File.Exists(path010))
			{
				MessageBox.Show("'010Editor.exe' not found in 010 folder.", Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			if (!Directory.Exists(Settings.Default.FolderTpl))
			{
				if (MessageBox.Show("Template folder not found, do you want to open 010 regardless?", Name, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
					return;
			}

			// Create temp file for the buffer and open it in 010
			var tmpPath = Path.GetTempFileName();
			File.WriteAllBytes(tmpPath, palePacket.Packet.GetBuffer());

			Process.Start(path010, '"' + tmpPath + '"');

			// Run template if it exists
			var pathtpl = Path.Combine(Settings.Default.FolderTpl, palePacket.OpName + ".bt");
			if (File.Exists(pathtpl))
				Process.Start(path010, '"' + "-template:" + pathtpl + '"');
			else
				MessageBox.Show("No template found for '" + palePacket.OpName + "'.", Name, MessageBoxButtons.OK, MessageBoxIcon.Information);
		}
	}
}
