﻿#pragma checksum "..\..\..\..\View\PdfSave.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "6B9233A79D68400A34C2A4371A2EAC80CCB676752BC016AB88F343482871EE19"
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
using TenzoMeterGUI.View;
using tEngine.UControls;


namespace TenzoMeterGUI.View {
    
    
    /// <summary>
    /// PdfSave
    /// </summary>
    public partial class PdfSave : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 23 "..\..\..\..\View\PdfSave.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal tEngine.UControls.PlotViewEx pve;
        
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
            System.Uri resourceLocater = new System.Uri("/TenzoMeterGUI;component/view/pdfsave.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\View\PdfSave.xaml"
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
            
            #line 5 "..\..\..\..\View\PdfSave.xaml"
            ((TenzoMeterGUI.View.PdfSave)(target)).Closing += new System.ComponentModel.CancelEventHandler(this.Window_OnClosing);
            
            #line default
            #line hidden
            
            #line 12 "..\..\..\..\View\PdfSave.xaml"
            ((TenzoMeterGUI.View.PdfSave)(target)).Loaded += new System.Windows.RoutedEventHandler(this.PdfSave_OnLoaded);
            
            #line default
            #line hidden
            return;
            case 2:
            this.pve = ((tEngine.UControls.PlotViewEx)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

