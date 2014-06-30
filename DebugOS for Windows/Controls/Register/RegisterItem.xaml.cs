﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DebugOS
{
	/// <summary>
	/// Interaction logic for RegisterControl.xaml
	/// </summary>
	public partial class RegisterItem : UserControl
	{
		public RegisterItem() {
			this.InitializeComponent();
		}

        public RegisterItem(string name, byte[] value, int width)
        {
            this.InitializeComponent();

            this.nameText.Text = name;
            this.SetValue(value);
        }

        public void SetValue(byte[] value)
        {
            StringBuilder text = new StringBuilder();

            for (int i = value.Length - 1; i != -1; i--)
            {
                text.Append(Utils.GetHexString(value[i], 2));
                text.Append(' ');
            }
            this.valueText.Text = text.ToString();
        }
	}
}