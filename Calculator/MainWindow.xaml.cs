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
    public class InvalidInputException : ApplicationException
    {
        public InvalidInputException() : base() { }
        public InvalidInputException( string? message ) : base( message ) { }
        public InvalidInputException( string? message, Exception? InnerException ) : base( message, InnerException ) { }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static int CurrentIndex = 1;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void NumButton( object sender, RoutedEventArgs e )
        {
            if ( sender is Button btn )
            {
                Calc.Text = Calc.Text.Insert( CurrentIndex, btn.Name[ 1.. ] );
                CurrentIndex += btn.Name.Length - 1;
            }
        }

        private enum MathFuncs
        {
            NONE        = 0,
            PLUS        = 1 << 0,
            MINUS       = 1 << 1,
            MULTIPLY    = 1 << 2,
            DIVIDE      = 1 << 3,
        }

        private void MetaFunc( object sender, RoutedEventArgs e )
        {
            if ( sender is Button btn )
            {
                switch ( btn.Name )
                {
                    case "Enter":
                    {
                        try
                        {
                            string Expression = Calc.Text;
                            List<(MathFuncs, int)> FunctionList = new();
                            for ( int i = 0; i < Expression.Length; ++i )
                            {
                                switch ( Expression[ i ] )
                                {
                                    case '/':
                                    {
                                        FunctionList.Add( (MathFuncs.DIVIDE, i) );
                                        break;
                                    }
                                    case '*':
                                    {
                                        FunctionList.Add( (MathFuncs.MULTIPLY, i) );
                                        break;
                                    }
                                    case '+':
                                    {
                                        FunctionList.Add( (MathFuncs.PLUS, i) );
                                        break;
                                    }
                                    case '-':
                                    {
                                        FunctionList.Add( (MathFuncs.MINUS, i) );
                                        break;
                                    }

                                    default:
                                    break;
                                }
                            }
                            float[] Values = new float[ FunctionList.Count + 1 ];
                            int StringIndex = 0;
                            for ( int i = 0; i < Values.Length; ++i )
                            {
                                int StringNextIndex = i != Values.Length - 1 ? FunctionList[ i ].Item2 : Expression.Length;

                                if ( StringNextIndex == StringIndex )
                                    throw new InvalidInputException( "Two operators next to each other, or one at the start" );

                                string StringValue = Expression.Substring( StringIndex, StringNextIndex - StringIndex );
                                if ( !float.TryParse( StringValue, out Values[ i ] ) )
                                    throw new InvalidInputException( "Couldn't parse to float" );

                                StringIndex = StringNextIndex + 1;
                            }
                            float ret = Values[ 0 ];
                            for ( int i = 1; i < Values.Length; ++i )
                            {
                                float Val = Values[ i ];
                                switch ( FunctionList[ i - 1 ].Item1 )
                                {
                                    case MathFuncs.PLUS:
                                    {
                                        ret += Val;
                                        break;
                                    }
                                    case MathFuncs.MINUS:
                                    {
                                        ret -= Val;
                                        break;
                                    }
                                    case MathFuncs.MULTIPLY:
                                    {
                                        ret *= Val;
                                        break;
                                    }
                                    case MathFuncs.DIVIDE:
                                    {
                                        ret /= Val;
                                        break;
                                    }

                                    case MathFuncs.NONE:
                                    {
                                        Assert( false );
                                        break;
                                    }

                                    default:
                                    {
                                        Assert( false, "Function not in list" );
                                        break;
                                    }
                                }
                            }
                            FunctionList.Clear();
                            Calc.Text = ret.ToString();
                            CurrentIndex = Calc.Text.Length;
                        }
                        catch ( InvalidInputException ex )
                        {
                            Calc.Text = "Invalid Input";
                            _ = MessageBox.Show( ex.StackTrace, ex.Message );
                        }
                        break;
                    }
                    case "Delete":
                    {
                        if ( CurrentIndex <= 0 )
                            break;

                        Calc.Text = Calc.Text.Remove( CurrentIndex );
                        --CurrentIndex;
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



        private void FuncButton( object sender, RoutedEventArgs e )
        {
            if ( sender is Button btn )
            {
                switch ( btn.Name )
                {
                    case "pls":
                    {
                        Calc.Text = Calc.Text.Insert( CurrentIndex, "+" );
                        ++CurrentIndex;
                        break;
                    }
                    case "sbt":
                    {
                        Calc.Text = Calc.Text.Insert( CurrentIndex, "-" );
                        ++CurrentIndex;
                        break;
                    }
                    case "mult":
                    {
                        Calc.Text = Calc.Text.Insert( CurrentIndex, "*" );
                        ++CurrentIndex;
                        break;
                    }
                    case "div":
                    {
                        Calc.Text = Calc.Text.Insert( CurrentIndex, "/" );
                        ++CurrentIndex;
                        break;
                    }
                    case "dot":
                    {
                        Calc.Text = Calc.Text.Insert( CurrentIndex, "." );
                        ++CurrentIndex;
                        break;
                    }
                    default:
                    {
                        Assert( false, "Button not in table!" );
                        break;
                    }
                }
            }
        }
    }
}
