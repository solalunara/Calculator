﻿using System;
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
using System.Text.RegularExpressions;
using System.Reflection;

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

        [Flags]
        private enum FiveFuncs
        {
            PWR         = 1 << 0,
            MULTIPLY    = 1 << 1,
            DIVIDE      = 1 << 2,
            PLUS        = 1 << 3,
            MINUS       = 1 << 4,
        }

        private static string[] MathFuncs =
        {
            "Abs",
            "Acos",
            "Acosh",
            "Asin",
            "Asinh",
            "Atan",
            "Atan2",
            "Atanh",
            "BitDecrement",
            "BitIncrement",
            "Cbrt",
            "Ceiling",
            "Cos",
            "Cosh",
            "Exp",
            "Floor",
            "ILogB",
            "Log",
            "Log10",
            "Log2",
            "Round",
            "Sign",
            "Sin",
            "Sinh",
            "Sqrt",
            "Tan",
            "Tanh",
            "Truncate"
        };

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
                            MessageBox.Show( ex.StackTrace, ex.Message );
                        }
                        catch ( ArgumentOutOfRangeException ex )
                        {
                            Calc.Text = "Please check your parenthases";
                            MessageBox.Show( ex.StackTrace, ex.Message );
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

        private static (int, int) FindPerimPair( string Expression, int StartIndex )
        {
            int StartPerimIndex = Expression.IndexOf( '(', StartIndex );
            int LeftPerims = 0;
            int RightPerims = 0;
            int EndPerimIndex = 0;
            for ( int j = StartPerimIndex; j < Expression.Length; ++j )
            {
                if ( Expression[ j ] == '(' )
                    ++LeftPerims;
                if ( Expression[ j ] == ')' )
                    ++RightPerims;

                if ( LeftPerims == RightPerims )
                {
                    EndPerimIndex = j;
                    break;
                }
            }
            Assert( EndPerimIndex != 0 );
            return (StartPerimIndex, EndPerimIndex);
        }

        private delegate float MathClassFuncDelegate( float a );
        private static float EvaluateString( string Expression )
        {
            List<int> Negatives = new();
            for ( int i = 0; i < Expression.Length; ++i )
            {
                if ( Expression[ i ] == '-' )
                {
                    bool ShouldAddZero = true;
                    for ( int j = 0; j < 10; ++j )
                    {
                        if ( i != 0 && Expression[ i - 1 ] == j )
                            ShouldAddZero = false;
                    }
                    if ( ShouldAddZero )
                        Negatives.Add( i );
                }
            }

            //evaluate all functions that aren't one of the main 5 first, and turn them into numbers
            for ( int i = 0; i < Expression.Length; ++i )
            {
                for ( int j = 0; j < MathFuncs.Length; ++j )
                {
                    if ( i < Expression.Length - MathFuncs[ j ].Length && Expression[ i..( i + MathFuncs[ j ].Length ) ] == MathFuncs[ j ] )
                    {
                        //implicit multiplication
                        if ( i > 0 && Expression[ i - 1 ] is not '*' and not '/' and not '+' and not '-' and not '^' )
                            Expression = Expression.Insert( i, "*" );

                        (int, int) PerimPair = FindPerimPair( Expression, i );
                        float EnclosedValue = EvaluateString( Expression[ ( PerimPair.Item1 + 1 )..PerimPair.Item2 ] );
                        MethodInfo? Method = typeof( MathF ).GetMethod( MathFuncs[ j ], new[] { typeof( float ) } );

                        if ( Method is null )
                            throw new NotImplementedException( "No math class function with given name found" );

                        MathClassFuncDelegate Function = Method.CreateDelegate<MathClassFuncDelegate>();
                        float EvaluatedValue = Function( EnclosedValue );
                        Expression = Expression[ 0..( PerimPair.Item1 - MathFuncs[ j ].Length ) ] + EvaluatedValue.ToString( CultureInfo.CurrentCulture ) + Expression[ ( PerimPair.Item2 + 1 ).. ];
                    }
                }
            }

            //permis being next to something means implicit multiplication
            Expression = Expression.Replace( ")(", ")*(", StringComparison.CurrentCulture );
            for ( int i = 0; i < Expression.Length; ++i )
            {
                if ( Expression[ i ] is not 'e' and not 'p' )
                    continue;

                if ( i != 0 )
                {
                    if ( Expression[ i ] is 'e' && Expression[ i - 1 ] is not '*' and not '/' and not '+' and not '-' and not '^' and not '(' and not ')' )
                        Expression = Expression.Insert( i, "*" );
                    if ( i < Expression.Length - 1 )
                    {
                        if ( Expression[ i..( i + 2 ) ] == "pi" && Expression[ i - 1 ] is not '*' and not '/' and not '+' and not '-' and not '^' and not '(' and not ')' )
                            Expression = Expression.Insert( i, "*" );
                    }
                }
                if ( i != Expression.Length - 1 )
                {
                    if ( Expression[ i ] is 'e' && Expression[ i + 1 ] is not '*' and not '/' and not '+' and not '-' and not '^' and not '(' and not ')' )
                        Expression = Expression.Insert( i + 1, "*" );
                    if ( i != Expression.Length - 2 )
                    {
                        if ( Expression[ i..( i + 2 ) ] == "pi" && Expression[ i + 2 ] is not '*' and not '/' and not '+' and not '-' and not '^' and not '(' and not ')' )
                            Expression = Expression.Insert( i + 2, "*" );
                    }
                }
            }
            for ( int i = 1; i < Expression.Length - 1; ++i )
            {
                if ( Expression[ i ] is not '(' and not ')' )
                    continue;

                if ( Expression[ i ] is '(' && Expression[ i - 1 ] is not '*' and not '/' and not '+' and not '-' and not '^' )
                    Expression = Expression.Insert( i, "*" );
                if ( Expression[ i ] is ')' && Expression[ i + 1 ] is not '*' and not '/' and not '+' and not '-' and not '^' )
                    Expression = Expression.Insert( i + 1, "*" );
            }

            Expression = Expression.Replace( "pi", MathF.PI.ToString( CultureInfo.CurrentCulture ), StringComparison.CurrentCultureIgnoreCase );
            Expression = Expression.Replace( "e", MathF.E.ToString( CultureInfo.CurrentCulture ), StringComparison.CurrentCulture );



            //if there are parenthases, find the maximum enclosed section, and get it's value
            //while loop for parallel parenthases like "(2+3)*(4+5)"
            while ( Expression.Contains( '(' ) || Expression.Contains( ')' ) )
            {
                (int, int) PerimPair = FindPerimPair( Expression, Expression.IndexOf( '(' ) );
                string EnclosedSectionString = Expression[ (PerimPair.Item1 + 1)..PerimPair.Item2 ];
                //condense it down to a single number represented as a string
                string EnclosedSectionValue = EvaluateString( EnclosedSectionString ).ToString( CultureInfo.CurrentCulture );
                Expression = Expression[ 0..PerimPair.Item1 ] + EnclosedSectionValue + Expression[ ( PerimPair.Item2 + 1 ).. ];
            }

            

            //get the functions from the long string
            List<(FiveFuncs, int)> FiveFunctionList = new();
            for ( int i = 1; i < Expression.Length; ++i )
            {
                switch ( Expression[ i ] )
                {
                    case '^':
                    {
                        FiveFunctionList.Add( (FiveFuncs.PWR, i) );
                        break;
                    }
                    case '/':
                    {
                        FiveFunctionList.Add( (FiveFuncs.DIVIDE, i) );
                        break;
                    }
                    case '*':
                    {
                        FiveFunctionList.Add( (FiveFuncs.MULTIPLY, i) );
                        break;
                    }
                    case '+':
                    {
                        //+ is also used in cases like "2E+10", so check for that
                        if ( Expression[ i - 1 ] == 'E' )
                            break;

                        FiveFunctionList.Add( (FiveFuncs.PLUS, i) );
                        break;
                    }
                    case '-':
                    {
                        //see above
                        if ( Expression[ i - 1 ] == 'E' )
                            break;

                        //sign should be used as a negative, not as a minus
                        if ( Negatives.Contains( i ) )
                            break;

                        FiveFunctionList.Add( (FiveFuncs.MINUS, i) );
                        break;
                    }

                    default:
                    break;
                }
            }

            //turn the long string into values
            int ValueCount = FiveFunctionList.Count + 1;
            List<float> Values = new( ValueCount );
            int StringIndex = 0;
            for ( int i = 0; i < ValueCount; ++i )
            {
                int StringNextIndex = i != ValueCount - 1 ? FiveFunctionList[ i ].Item2 : Expression.Length;

                if ( StringNextIndex == StringIndex )
                    throw new InvalidInputException( "Two operators next to each other, or one at the start" );

                string StringValue = Expression[ StringIndex..StringNextIndex ];
                if ( float.TryParse( StringValue, out float f ) )
                    Values.Add( f );
                else
                    throw new InvalidInputException( "Couldn't parse to float" );

                StringIndex = StringNextIndex + 1;
            }

            //if there are parenthases, get the value inside of them before proceeding


            //do order of operations by condensing values by their MathFuncs enum top to bottom
            int FiveFunctionNum = Enum.GetValues( typeof( FiveFuncs ) ).Length;
            for ( int i = 0; i < FiveFunctionNum; ++i )
            {
                for ( int j = FiveFunctionList.Count; --j >= 0; )
                {
                    FiveFuncs f = (FiveFuncs) ( 1 << i );
                    if ( FiveFunctionList[ j ].Item1 == f )
                    {
                        switch ( f )
                        {
                            case FiveFuncs.PWR:
                            {
                                Values[ j ] = MathF.Pow( Values[ j ], Values[ j + 1 ] );
                                Values.RemoveAt( j + 1 );
                                break;
                            }
                            case FiveFuncs.MULTIPLY:
                            {
                                Values[ j ] *= Values[ j + 1 ];
                                Values.RemoveAt( j + 1 );
                                break;
                            }
                            case FiveFuncs.DIVIDE:
                            {
                                Values[ j ] /= Values[ j + 1 ];
                                Values.RemoveAt( j + 1 );
                                break;
                            }
                            case FiveFuncs.PLUS:
                            {
                                Values[ j ] += Values[ j + 1 ];
                                Values.RemoveAt( j + 1 );
                                break;
                            }
                            case FiveFuncs.MINUS:
                            {
                                Values[ j ] -= Values[ j + 1 ];
                                Values.RemoveAt( j + 1 );
                                break;
                            }

                            default:
                            {
                                throw new MissingMemberException( "Catastrophic failure: I don't even know what could possibly cause this" );
                            }
                        }
                        FiveFunctionList.RemoveAt( j );
                    }
                }
            }

            Assert( Values.Count == 1 );
            return Values[ 0 ];
        }

        private void FuncButton( object sender, RoutedEventArgs e )
        {
            if ( sender is Button btn )
            {
                Assert( btn.HasContent );
                Calc.Text += btn.Content;
            }
        }

        private void TextBoxKeyDown( object sender, KeyEventArgs e )
        {
            if ( e.Key == Key.Enter )
                MetaFunc( Enter, e );
        }
    }
}
