﻿#pragma checksum "..\..\..\..\Popup\Measure\MeasureControl.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "80D5FEAEDCFE67652A8AFC2139D57D0B6E18682D"
//------------------------------------------------------------------------------
// <auto-generated>
//     이 코드는 도구를 사용하여 생성되었습니다.
//     런타임 버전:4.0.30319.42000
//
//     파일 내용을 변경하면 잘못된 동작이 발생할 수 있으며, 코드를 다시 생성하면
//     이러한 변경 내용이 손실됩니다.
// </auto-generated>
//------------------------------------------------------------------------------

using DevExpress.Xpf.DXBinding;
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


namespace CrevisLibrary {
    
    
    /// <summary>
    /// MeasureControl
    /// </summary>
    public partial class MeasureControl : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 30 "..\..\..\..\Popup\Measure\MeasureControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TabControl tabControl;
        
        #line default
        #line hidden
        
        
        #line 34 "..\..\..\..\Popup\Measure\MeasureControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.StackPanel mainStack;
        
        #line default
        #line hidden
        
        
        #line 35 "..\..\..\..\Popup\Measure\MeasureControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Border MeasurePane;
        
        #line default
        #line hidden
        
        
        #line 38 "..\..\..\..\Popup\Measure\MeasureControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button MeasureBtn;
        
        #line default
        #line hidden
        
        
        #line 99 "..\..\..\..\Popup\Measure\MeasureControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid TunningGrid;
        
        #line default
        #line hidden
        
        
        #line 113 "..\..\..\..\Popup\Measure\MeasureControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Border TunningPane;
        
        #line default
        #line hidden
        
        
        #line 116 "..\..\..\..\Popup\Measure\MeasureControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button TunningBtn;
        
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
            System.Uri resourceLocater = new System.Uri("/MotionGuidePro;component/popup/measure/measurecontrol.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\Popup\Measure\MeasureControl.xaml"
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
            
            #line 1 "..\..\..\..\Popup\Measure\MeasureControl.xaml"
            ((CrevisLibrary.MeasureControl)(target)).Loaded += new System.Windows.RoutedEventHandler(this.MeasureControl_OnLoaded);
            
            #line default
            #line hidden
            return;
            case 2:
            this.tabControl = ((System.Windows.Controls.TabControl)(target));
            return;
            case 3:
            this.mainStack = ((System.Windows.Controls.StackPanel)(target));
            return;
            case 4:
            this.MeasurePane = ((System.Windows.Controls.Border)(target));
            return;
            case 5:
            this.MeasureBtn = ((System.Windows.Controls.Button)(target));
            
            #line 39 "..\..\..\..\Popup\Measure\MeasureControl.xaml"
            this.MeasureBtn.Click += new System.Windows.RoutedEventHandler(this.MeasureAddBtn_OnClick);
            
            #line default
            #line hidden
            return;
            case 6:
            this.TunningGrid = ((System.Windows.Controls.Grid)(target));
            return;
            case 7:
            this.TunningPane = ((System.Windows.Controls.Border)(target));
            return;
            case 8:
            this.TunningBtn = ((System.Windows.Controls.Button)(target));
            
            #line 117 "..\..\..\..\Popup\Measure\MeasureControl.xaml"
            this.TunningBtn.Click += new System.Windows.RoutedEventHandler(this.SelectParamBtn_OnClick);
            
            #line default
            #line hidden
            return;
            case 9:
            
            #line 122 "..\..\..\..\Popup\Measure\MeasureControl.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.SaveParamBtn_OnClick);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}
