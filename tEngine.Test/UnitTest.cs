using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OxyPlot;
using tEngine.DataModel;
using tEngine.Helpers;
using tEngine.TMeter.DataModel;

namespace tEngine.Test {
    [TestClass]
    public class UnitTest {
        [TestMethod]
        public void MyTestMethod() {
            var p = Enumerable.Range( 0, 100 ).Select( i => new DataPoint( i, i ) );
            var first = p.FirstOrDefault( i => i.X > 10000);

            var b = first.IsDefined();
            Console.Write( b );
        }

        [TestMethod]
        public void teststring() {

            var test = GetExp(12345);

            test = GetExp( 100 );
            test = GetExp( 1000 );
            test = GetExp( 10000 );

            test = GetExp( 0.0001 );
            test = GetExp( 0.123 );
            
            test = GetExp( 0 );
            test = GetExp( 1 );

            var ll = Math.Log10( 12345 );
            ll = Math.Log10( 0.123456d );
            ll = Math.Log10( 123456d );
            ll = Math.Log10( 1000 );
            ll = Math.Log10( 10000 );
            var str = string.Format( "0:F{0}", 5 );

            var tt = string.Format( "{" + str + "}", 0.123456d );

            tt = string.Format( "{0:}", 0.1234567d );


        }

        private int GetExp(double value) {
            if( value == 0 ) return 0;
            var tmp = value;
            var result = 0;
            while( Math.Log10( tmp ) < 1 ) {
                tmp *= 10;
                result --;
            }
            return (int) (Math.Log10( tmp )) + result;
        }


        [TestMethod]
        public void Correctlkash() {
            var cDirectory = AppSettings.GetValue( User.FOLDER_KEY, "" );
            var filepath = cDirectory.CorrectSlash();

            var path = @"";
            path.CorrectSlash();

            path = @"papka/";
            path.CorrectSlash();

            path = @"papka";
            path.CorrectSlash();

            path = @"papka";
            path = path.CorrectSlash();
        }

        [TestMethod]
        public void EnumTest() {
            var ms = MasterState.Info;
            var ms1 = (MasterState) ((int) ms + 1);
            var ms2 = (MasterState) ((int) ms + 2);
            var ms3 = (MasterState) ((int) ms + 3);
            var ms4 = (MasterState) ((int) ms + 4);
            var ms5 = (MasterState) ((int) ms + 5);
            var ms6 = MasterState.Analysis;
            var ms7 = (MasterState) 2;
            var b = (MasterState.Analysis == MasterState.Msm);
            var max = (int) Enum.GetValues( typeof( MasterState ) ).Cast<MasterState>().Distinct().Count();
        }

        [TestMethod]
        public void TestDirectory() {
            var pic = Environment.GetFolderPath( Environment.SpecialFolder.MyPictures );
            // забивате maxCount рисунков из папки рисунков
            var dinfo = new DirectoryInfo( pic + @"\tenzoTest" );
            if( dinfo.Exists == false )
                dinfo = new DirectoryInfo( pic.ToString() );
        }

        [TestMethod]
        public void TestMethod() {
            var user = new User();
            for( int i = 0; i < 100; i++ )
                user.FName += "123456789123456789";
        }

        [TestMethod]
        public void TestMsmCreate() {
            var result = Msm.GetTestMsm();
        }

        [TestMethod]
        public void RegexTest() {
            var rgx = new Regex( @"[*. \-_a-zA-Z0-9а-яА-Я]*" );
            var name = @"Па_ша* Pa-sh.a %";
            var str = rgx.Replace( name, "" );

            var input = "This is   text with   far  too   much   " +
                     "whitespace.";
            var pattern = "\\s+";
            var replacement = " ";
            rgx = new Regex( pattern );
            var result = rgx.Replace( input, replacement );
        }

        private enum MasterState {
            Info = 0,
            ImageSelect = 1,
            Msm = 2,
            Analysis = 2,
            Result = 3
        }
    }
}