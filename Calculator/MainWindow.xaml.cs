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
using System.Globalization;
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
        public MainWindow()
        {
            InitializeComponent();
        }

        private void NumButton( object sender, RoutedEventArgs e )
        {
            if ( sender is Button btn )
            {
                Calc.Text += btn.Name[ 1.. ];
            }
        }

        [Flags]
        private enum MathFuncs
        {
            MULTIPLY    = 1 << 0,
            DIVIDE      = 1 << 1,
            PLUS        = 1 << 2,
            MINUS       = 1 << 3,
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
                            Calc.Text = EvaluateString( Calc.Text ).ToString( CultureInfo.CurrentCulture );
                        }
                        catch ( InvalidInputException ex )
                        {
                            Calc.Text = "Invalid Input";
                            _ = MessageBox.Show( ex.StackTrace, ex.Message );
                        }
                        catch ( ArgumentOutOfRangeException ex )
                        {
                            Calc.Text = "Please check your parenthases";
                            _ = MessageBox.Show( ex.StackTrace, ex.Message );
                        }
                        break;
                    }
                    case "Delete":
                    {
                        Calc.Text = Calc.Text[ ..^1 ];
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

        private float EvaluateString( string Expression )
        {
            Expression = Expression.Replace( "pi", MathF.PI.ToString( CultureInfo.CurrentCulture ), StringComparison.CurrentCultureIgnoreCase );
            Expression = Expression.Replace( "e", MathF.E.ToString( CultureInfo.CurrentCulture ), StringComparison.CurrentCulture );
            Expression = Expression.Replace( ")(", ")*(", StringComparison.CurrentCulture );

            //if there are parenthases, find the maximum enclosed section, and get it's value
            //while loop for parallel parenthases like "(2+3)*(4+5)
            while ( Expression.Contains( '(' ) || Expression.Contains( ')' ) )
            {
                int StartPerimIndex = Expression.IndexOf( '(' );
                int LeftPerims = 0;
                int RightPerims = 0;
                int EndPerimIndex = 0;
                for ( int i = StartPerimIndex; i < Expression.Length; ++i )
                {
                    if ( Expression[ i ] == '(' )
                        ++LeftPerims;
                    if ( Expression[ i ] == ')' )
                        ++RightPerims;

                    if ( LeftPerims == RightPerims )
                    {
                        EndPerimIndex = i;
                        break;
                    }
                }
                Assert( EndPerimIndex != 0 );
                string EnclosedSectionString = Expression[ (StartPerimIndex + 1)..EndPerimIndex ];
                //condense it down to a single number represented as a string
                string EnclosedSectionValue = EvaluateString( EnclosedSectionString ).ToString( CultureInfo.CurrentCulture );
                Expression = Expression[ 0..StartPerimIndex ] + EnclosedSectionValue + Expression[ ( EndPerimIndex + 1 ).. ];
            }


            //get the functions from the long string
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
                        //+ is also used in cases like "2E+10", so check for that
                        if ( Expression[ i - 1 ] == 'E' )
                            break;

                        FunctionList.Add( (MathFuncs.PLUS, i) );
                        break;
                    }
                    case '-':
                    {
                        //see above
                        if ( Expression[ i - 1 ] == 'E' )
                            break;

                        FunctionList.Add( (MathFuncs.MINUS, i) );
                        break;
                    }

                    default:
                    break;
                }
            }

            //turn the long string into values
            int ValueCount = FunctionList.Count + 1;
            List<(float, int)> Values = new( ValueCount );
            int StringIndex = 0;
            for ( int i = 0; i < ValueCount; ++i )
            {
                int StringNextIndex = i != ValueCount - 1 ? FunctionList[ i ].Item2 : Expression.Length;

                if ( StringNextIndex == StringIndex )
                    throw new InvalidInputException( "Two operators next to each other, or one at the start" );

                string StringValue = Expression[ StringIndex..StringNextIndex ];
                if ( float.TryParse( StringValue, out float f ) )
                    Values.Add( (f, StringIndex) );
                else
                    throw new InvalidInputException( "Couldn't parse to float" );

                StringIndex = StringNextIndex + 1;
            }

            //if there are parenthases, get the value inside of them before proceeding


            //do order of operations by condensing values by their MathFuncs enum top to bottom
            int MathFuncNum = Enum.GetValues( typeof( MathFuncs ) ).Length;
            for ( int i = 0; i < MathFuncNum; ++i )
            {
                for ( int j = FunctionList.Count; --j >= 0; )
                {
                    MathFuncs f = (MathFuncs) ( 1 << i );
                    if ( FunctionList[ j ].Item1 == f )
                    {
                        switch ( f )
                        {
                            case MathFuncs.MULTIPLY:
                            {
                                Values[ j ] = (Values[ j ].Item1 * Values[ j + 1 ].Item1, Values[ j ].Item2);
                                Values.RemoveAt( j + 1 );
                                break;
                            }
                            case MathFuncs.DIVIDE:
                            {
                                Values[ j ] = (Values[ j ].Item1 / Values[ j + 1 ].Item1, Values[ j ].Item2);
                                Values.RemoveAt( j + 1 );
                                break;
                            }
                            case MathFuncs.PLUS:
                            {
                                Values[ j ] = (Values[ j ].Item1 + Values[ j + 1 ].Item1, Values[ j ].Item2);
                                Values.RemoveAt( j + 1 );
                                break;
                            }
                            case MathFuncs.MINUS:
                            {
                                Values[ j ] = (Values[ j ].Item1 - Values[ j + 1 ].Item1, Values[ j ].Item2);
                                Values.RemoveAt( j + 1 );
                                break;
                            }

                            default:
                            {
                                throw new MissingMemberException( "Catastrophic failure: I don't even know what could possibly cause this" );
                            }
                        }
                        FunctionList.RemoveAt( j );
                    }
                }
            }

            Assert( Values.Count == 1 );
            return Values[ 0 ].Item1;
        }

        private void FuncButton( object sender, RoutedEventArgs e )
        {
            if ( sender is Button btn )
            {
                switch ( btn.Name )
                {
                    case "pls":
                    {
                        Calc.Text += "+";
                        break;
                    }
                    case "sbt":
                    {
                        Calc.Text += "-";
                        break;
                    }
                    case "mult":
                    {
                        Calc.Text += "*";
                        break;
                    }
                    case "div":
                    {
                        Calc.Text += "/";
                        break;
                    }
                    case "dot":
                    {
                        Calc.Text += ".";
                        break;
                    }
                    case "leftperim":
                    {
                        Calc.Text += "(";
                        break;
                    }
                    case "rightperim":
                    {
                        Calc.Text += ")";
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
