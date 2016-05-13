using System;
using System.Collections;
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
using System.Windows.Shapes;

namespace ScriptRunner.Control
{
    /// <summary>
    /// Interaction logic for Tenant.xaml
    /// </summary>
    public partial class Tenants : Window
    {
        private Hashtable MT = new Hashtable();

        public Tenants(Hashtable mt)
        {
            InitializeComponent();
            this.MT = mt;
            foreach (DictionaryEntry entry in MT)
            {
                tenant t = new tenant(entry.Key.ToString(), (bool)entry.Value);
                CreateRow(t);
            }
        }
        

        public System.Collections.Hashtable status
        {
            get { return MT; }
        }


        private void CreateRow(tenant name)
        {
            RowDefinition newRow = new RowDefinition();
            newRow.Height = new GridLength(0, GridUnitType.Auto);
            _mainGrid.RowDefinitions.Insert(_mainGrid.RowDefinitions.Count - 1, newRow);
            int rowIndex = _mainGrid.RowDefinitions.Count - 2;
            Grid.SetRow(name, rowIndex);
            Grid.SetColumn(name, 0);
            _mainGrid.Children.Add(name);

        }



        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            int gridrow = _mainGrid.RowDefinitions.Count;
            for (int tr = 0; tr < gridrow; tr++)
            {
                var itemsInFirstRow = _mainGrid.Children.Cast<UIElement>().Where(i => Grid.GetRow(i) == tr);                
                foreach (tenant t in itemsInFirstRow)                 
                //MT.Add(t.tenantlabel.Content.ToString(), (bool)t.tenantck.IsChecked); 
                MT[t.tenantlabel.Content.ToString()]= (bool)t.tenantck.IsChecked;
            }
            this.DialogResult = true;
        }
     
    }
}
