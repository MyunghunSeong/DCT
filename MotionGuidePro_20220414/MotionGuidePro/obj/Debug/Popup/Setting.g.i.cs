﻿#pragma checksum "..\..\..\Popup\Setting.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "BE6C52975676D4CD6DA74B00C2B363B87A71623A"
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
    /// Setting
    /// </summary>
    public partial class Setting : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 60 "..\..\..\Popup\Setting.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox FilePathText;
        
        #line default
        #line hidden
        
        
        #line 77 "..\..\..\Popup\Setting.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox LoadFileText;
        
        #line default
        #line hidden
        
        
        #line 110 "..\..\..\Popup\Setting.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox ServerAddressText;
        
        #line default
        #line hidden
        
        
        #line 119 "..\..\..\Popup\Setting.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox ClientAddressText;
        
        #line default
        #line hidden
        
        
        #line 133 "..\..\..\Popup\Setting.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox TimeoutText;
        
        #line default
        #line hidden
        
        
        #line 149 "..\..\..\Popup\Setting.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox ErrorStateTimeText;
        
        #line default
        #line hidden
        
        
        #line 165 "..\..\..\Popup\Setting.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox MonitoringTimeText;
        
        #line default
        #line hidden
        
        
        #line 180 "..\..\..\Popup\Setting.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox DataReceiveTimeText;
        
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
            System.Uri resourceLocater = new System.Uri("/MotionGuidePro;component/popup/setting.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\Popup\Setting.xaml"
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
            
            #line 4 "..\..\..\Popup\Setting.xaml"
            ((CrevisLibrary.Setting)(target)).Loaded += new System.Windows.RoutedEventHandler(this.SettingDiaglog_OnLoaded);
            
            #line default
            #line hidden
            
            #line 4 "..\..\..\Popup\Setting.xaml"
            ((CrevisLibrary.Setting)(target)).KeyDown += new System.Windows.Input.KeyEventHandler(this.SettingDiaglog_OnKeyDown);
            
            #line default
            #line hidden
            return;
            case 2:
            this.FilePathText = ((System.Windows.Controls.TextBox)(target));
            return;
            case 3:
            
            #line 63 "..\..\..\Popup\Setting.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.BaseFilePathBtn_OnClick);
            
            #line default
            #line hidden
            return;
            case 4:
            this.LoadFileText = ((System.Windows.Controls.TextBox)(target));
            return;
            case 5:
            
            #line 80 "..\..\..\Popup\Setting.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.LoadFileBtn_OnClick);
            
            #line default
            #line hidden
            return;
            case 6:
            this.ServerAddressText = ((System.Windows.Controls.TextBox)(target));
            return;
            case 7:
            this.ClientAddressText = ((System.Windows.Controls.TextBox)(target));
            return;
            case 8:
            this.TimeoutText = ((System.Windows.Controls.TextBox)(target));
            
            #line 133 "..\..\..\Popup\Setting.xaml"
            this.TimeoutText.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.TimeoutText_TextChanged);
            
            #line default
            #line hidden
            return;
            case 9:
            this.ErrorStateTimeText = ((System.Windows.Controls.TextBox)(target));
            return;
            case 10:
            this.MonitoringTimeText = ((System.Windows.Controls.TextBox)(target));
            return;
            case 11:
            this.DataReceiveTimeText = ((System.Windows.Controls.TextBox)(target));
            return;
            case 12:
            
            #line 223 "..\..\..\Popup\Setting.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.OKBtn_OnClick);
            
            #line default
            #line hidden
            return;
            case 13:
            
            #line 224 "..\..\..\Popup\Setting.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.CancelBtn_OnClick);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}
