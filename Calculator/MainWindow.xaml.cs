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
using static System.Diagnostics.Debug;

namespace Calculator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static int CurrentIndex;

        public MainWindow()
        {
            InitializeComponent();
        }

        public static unsafe float InverseSquareRoot( float number )
        {
            int i;
            float x2, y;
            const float threehalfs = 1.5F;
            x2 = number * 0.5F;
            y = number;
            i = *(int*)&y;                                  // evil floating point bit level hacking
            i = 0x5f3759df - ( i >> 1 );                    // what the fuck? 
            y = *(float*)&i;
            y = y * ( threehalfs - ( x2 * y * y ) );        // 1st iteration
            // y  = y * ( threehalfs - ( x2 * y * y ) );    // 2nd iteration, this can be removed
            return y;
        }


        private void NumButton( object sender, RoutedEventArgs e )
        {
            if ( sender is Button btn )
            {
                Calc.Text = Calc.Text.Insert( CurrentIndex, btn.Name[ 1.. ] );
                CurrentIndex += btn.Name.Length - 1;
            }
        }

        private void MetaFunc( object sender, RoutedEventArgs e )
        {
            if ( sender is Button btn )
            {
                switch ( btn.Name )
                {
                    case "Enter":
                    {
                        string Expression = Calc.Text;
                        break;
                    }
                    case "Delete":
                    {
                        Calc.Text = Calc.Text.Remove( CurrentIndex, 1 );
                        break;
                    }
                    case "Left":
                    {
                        if ( CurrentIndex > 0 )
                            --CurrentIndex;
                        break;
                    }
                    case "Right":
                    {
                        if ( CurrentIndex < Calc.Text.Length )
                            ++CurrentIndex;
                        break;
                    }
                    default:
                    {
                        Assert( false, "Button not in table!" );
                        return;
                    }
                }
            }
        }
    }
}
