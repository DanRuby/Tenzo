using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using MathNet.Numerics.IntegralTransforms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MathNet.Numerics;

namespace tEngine.Test {
    [TestClass]
    public class MathTest {
        // Считать спектр с модификатором MatLab 
        // 
        [TestMethod]
        public void FourierImp() {
            var tau = 3;
            var T = 15;
            //var imp = Enumerable.Range( 0, T ).Select( y => y > tau ? 0 : 10 ).ToArray();
            var sin1 = GetSinus( (360/T)*1, 0, T );
            var sin2 = GetSinus( (360/T)*3, 0, T );
            var sin3 = GetSinus( (360/T)*5, 11, T );

            var imp = Enumerable.Range( 0, T ).Select( i => sin1[i] + sin2[i] + sin3[i] ).ToArray();

            var print = "";

            var cmpl = imp.Select( i => new Complex( i, 0 ) ).ToArray();

            Console.WriteLine( "Исходный сигнал" );
            print = string.Join( "; ", imp.Select( c => string.Format( "{0:F0}", c ) ).ToArray() );
            Console.WriteLine( print );

            Console.WriteLine( "" );
            Fourier.Forward( cmpl, FourierOptions.Matlab );
            print = string.Join( "; ",
                cmpl.Select( ( c, i ) => string.Format( "{0:F0}", c.Magnitude*(i == 0 ? 1 : 2)/T ) ).ToArray() );
            Console.WriteLine( print );

            Fourier.Inverse( cmpl, FourierOptions.Matlab );
            print = string.Join( "; ", cmpl.Select( c => string.Format( "{0:F0}", c.Real ) ).ToArray() );
            Console.WriteLine( print );

            Console.WriteLine( "" );
            cmpl = imp.Select( i => new Complex( i, 0 ) ).ToArray();
            Fourier.Forward( cmpl, FourierOptions.NoScaling );
            print = string.Join( "; ",
                cmpl.Select( ( c, i ) => string.Format( "{0:F0}", (c.Magnitude*(i == 0 ? 1 : 2)/T) ) ).ToArray() );
            Console.WriteLine( print );


            //Console.WriteLine("");
            //Console.WriteLine(
            //    string.Join( Environment.NewLine,
            //        cmpl.Select(
            //            complex =>
            //                string.Format( "{0:F1} + {1:F1}i\t: {2:F1}", complex.Real, complex.Imaginary,
            //                    complex.Magnitude ) ) ) );
        }

        // Fourier.Forward - самый быстрый для случайного набора чисел
        [TestMethod]
        public void FourierSpeed() {
            var r = new Random( 12234 );
            var size = 1000;
            var data = Enumerable.Range( 0, size ).Select( i => r.Next( i ) ).ToArray();
            var cmpl = data.Select( i => new Complex( i, 0 ) ).ToArray();

            var volume = 1;
            var start = DateTime.Now;
            for( int i = 0; i < volume; i++ ) {
                Fourier.Forward( cmpl, FourierOptions.Matlab );
            }
            var time = (DateTime.Now - start).TotalMilliseconds/volume;
            Console.WriteLine( "Fourier {1} - {0:F3} ms", time, size, volume );
        }

        [TestMethod]
        public void TestCorr() {
            var T = 20;
            var tau = 5;
            var data = Enumerable.Range( 0, T ).Select( i => i > tau ? 0 : 10 ).ToArray();
            var cmpl = data.Select( i => new Complex( i, 0 ) ).ToArray();
            var print = "";
            
            Console.WriteLine( "Исходный сигнал" );
            print = string.Join( "; ", data.Select( c => string.Format( "{0:F0}", c ) ).ToArray() );
            Console.WriteLine( print );

            Fourier.Forward( cmpl, FourierOptions.Matlab );
            var mod = cmpl.Select( complex => complex.Magnitude*complex.Magnitude ).ToArray();
            var cmpl2 = mod.Select( i => new Complex( i, 0 ) ).ToArray();
            Fourier.Inverse( cmpl2, FourierOptions.Matlab );
            print = string.Join( "; ",
                cmpl2.Select( ( c, i ) => string.Format( "{0:F1}", (c.Magnitude / cmpl2[0].Magnitude) ) ).ToArray() );
            Console.WriteLine( print );

            var cc = MathNet.Numerics.Statistics.Correlation.Pearson( data.Select( i => (double)i ), data.Select( i => (double)i ) );
            Console.WriteLine( cc );
        }

        private double[] GetSinus( double f, double a, int N ) {
            return Enumerable.Range( 0, N ).Select( i => Math.Sin( i*Math.PI/180*f )*100 + a ).ToArray();
        }
    }
}