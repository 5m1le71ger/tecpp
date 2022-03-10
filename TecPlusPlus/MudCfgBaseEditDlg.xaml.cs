using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace TecPlusPlus
{
    /// <summary>
    /// MudCfgBaseEditDlg.xaml 的交互逻辑
    /// </summary>
    public partial class MudCfgBaseEditDlg : Window
    {
        private string filename { get; set; }
        public MudCfg mudCfg { get; set; }
        public MudCfgBaseEditDlg(MudCfg cfg,string filenameParam)
        {
            InitializeComponent();
            TextBoxName.Text = cfg.Name;
            TextBoxAddress.Text = cfg.Address;
            TextBoxPort.Text = cfg.Port.ToString();
            TextBoxEncode.Text = cfg.Encode;
            filename = filenameParam;
            mudCfg = new MudCfg()
            {
                Name = cfg.Name,
                Address = cfg.Address,
                Port = cfg.Port,
                Encode = cfg.Encode,
                ColorRule = cfg.ColorRule
            };
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            mudCfg.Name = TextBoxName.Text;
            mudCfg.Address = TextBoxAddress.Text;
            mudCfg.Port = int.Parse(TextBoxPort.Text);
            mudCfg.Encode = TextBoxEncode.Text;
            DialogResult = true;
        }
    }
}
