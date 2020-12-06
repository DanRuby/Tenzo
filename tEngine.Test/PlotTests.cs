using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OxyPlot.Wpf;
using tEngine.PlotCreator;

namespace tEngine.Test {
    [TestClass]
    public class PlotTests {
        [TestMethod]
        public void CreatePlot() {
            var pv = new PlotView();
            pv.Title = "title was create";
            pv.Axes.Add( new LinearAxis(){Title = "axes1"} );
            pv.Axes.Add( new LinearAxis(){Title = "axes2"} );
            var pme1 = new PlotModelEx();
            var pme2 = new PlotModelEx(pv);

            return;

        }
    }
}
