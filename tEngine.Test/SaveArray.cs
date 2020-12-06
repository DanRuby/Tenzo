using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using tEngine.Helpers;
using tEngine.TActual.DataModel;

namespace tEngine.Test {
    [TestClass]
    public class SaveArray {
        private List<short> Const { get; set; }

        [TestMethod]
        public void TestMethod1() {
            Const = Enumerable.Range( 0, 500 ).Select( Convert.ToInt16 ).ToList();

            var array = Const.SelectMany( BitConverter.GetBytes ).ToArray();

            var lst = array.Where( ( b, i ) => i%2 == 0 ).Select( ( b, i ) => {
                return BitConverter.ToInt16( array, i*2 );
            } ).ToList();
        }

        [TestMethod]
        public void TestArrayCat() {

            var ar1 = Enumerable.Range( 0, 0 ).Select( i => (byte)0xAA ).ToArray();
            var ar2 = Enumerable.Range( 0, 3 ).Select( i => (byte)0x11 ).ToArray();
            var ar3 = Enumerable.Range( 0, 3 ).Select( i => (byte)0x22 ).ToArray();
            var ar4 = Enumerable.Range( 0, 3 ).Select( i => (byte)0x33 ).ToArray();
            var ar5 = Enumerable.Range( 0, 3 ).Select( i => (byte)0x44 ).ToArray();
            
            var result = BytesPacker.PackBytes(ar1, ar2, ar3, ar4, ar5, ar1);

            var result2 = BytesPacker.UnpackBytes( result );
        }

        [TestMethod]
        public void SaveSlide() {
            var s1 = new tEngine.TActual.DataModel.Slide() {Name = "test", Grade = Slide.SlideGrade.Essential, Index = 2, IsShow = false};
            var array = s1.ToByteArray();
            var s2 = new tEngine.TActual.DataModel.Slide();
            s2.LoadFromArray( array );
        }
    }
}