using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GW2Messenger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private Gw2Data.Sale[] _gemSaleList;
        private Gw2Data.Outfit[] _outfitList;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var outfitList = WebReq.UpdateOutfitList();
            
            UpdateGemStoreList();
        }

        private void UpdateGemStoreList()
        {
            _gemSaleList = WebReq.UpdateGemSaleList();
            foreach (var item in _gemSaleList)
            {
                var text = new TextBlock();
                text.Text = $"{item.Name} - {item.GemCost}";

                GemStoreStackPanel.Children.Add(text);
            }
        }
    }
}
