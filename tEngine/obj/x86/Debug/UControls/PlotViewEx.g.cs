﻿#pragma checksum "..\..\..\..\UControls\PlotViewEx.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "5B36DBBCCDB407D48062ACF0932F54817C69FA63FE12CAD6BD251383EAE74CAC"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;
using tEngine.MVVM.Converters;


namespace tEngine.UControls {
    
    
    /// <summary>
    /// PlotViewEx
    /// </summary>
    public partial class PlotViewEx : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 8 "..\..\..\..\UControls\PlotViewEx.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal tEngine.UControls.PlotViewEx root;
        
        #line default
        #line hidden
        
        
        #line 51 "..\..\..\..\UControls\PlotViewEx.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid PlotContainer;
        
        #line default
        #line hidden
        
        
        #line 55 "..\..\..\..\UControls\PlotViewEx.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image Image;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/tEngine;component/ucontrols/plotviewex.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\UControls\PlotViewEx.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.root = ((tEngine.UControls.PlotViewEx)(target));
            
            #line 7 "..\..\..\..\UControls\PlotViewEx.xaml"
            this.root.SizeChanged += new System.Windows.SizeChangedEventHandler(this.PlotViewEx_OnSizeChanged);
            
            #line default
            #line hidden
            return;
            case 2:
            
            #line 31 "..\..\..\..\UControls\PlotViewEx.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.ButtonReset_OnClick);
            
            #line default
            #line hidden
            return;
            case 3:
            
            #line 42 "..\..\..\..\UControls\PlotViewEx.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.ButtonSettings_OnClick);
            
            #line default
            #line hidden
            return;
            case 4:
            this.PlotContainer = ((System.Windows.Controls.Grid)(target));
            return;
            case 5:
            this.Image = ((System.Windows.Controls.Image)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}
