﻿#pragma checksum "..\..\..\Popup\TriggerSetting.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "917E0141C8975A508AE2F4390BDF188AE6B2BE41"
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
    /// TriggerSetting
    /// </summary>
    public partial class TriggerSetting : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 59 "..\..\..\Popup\TriggerSetting.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox TriggerCombo;
        
        #line default
        #line hidden
        
        
        #line 67 "..\..\..\Popup\TriggerSetting.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox SamplingPeriodCombo;
        
        #line default
        #line hidden
        
        
        #line 96 "..\..\..\Popup\TriggerSetting.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Primitives.ToggleButton TriggerActionButton;
        
        #line default
        #line hidden
        
        
        #line 103 "..\..\..\Popup\TriggerSetting.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox SamplingTimeCombo;
        
        #line default
        #line hidden
        
        
        #line 128 "..\..\..\Popup\TriggerSetting.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox TriggerLevelBox;
        
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
            System.Uri resourceLocater = new System.Uri("/MotionGuidePro;component/popup/triggersetting.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\Popup\TriggerSetting.xaml"
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
            this.TriggerCombo = ((System.Windows.Controls.ComboBox)(target));
            
            #line 61 "..\..\..\Popup\TriggerSetting.xaml"
            this.TriggerCombo.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.TriggerCombo_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 2:
            this.SamplingPeriodCombo = ((System.Windows.Controls.ComboBox)(target));
            return;
            case 3:
            this.TriggerActionButton = ((System.Windows.Controls.Primitives.ToggleButton)(target));
            
            #line 97 "..\..\..\Popup\TriggerSetting.xaml"
            this.TriggerActionButton.Checked += new System.Windows.RoutedEventHandler(this.TriggerActionButton_Checked);
            
            #line default
            #line hidden
            
            #line 97 "..\..\..\Popup\TriggerSetting.xaml"
            this.TriggerActionButton.Unchecked += new System.Windows.RoutedEventHandler(this.TriggerActionButton_Unchecked);
            
            #line default
            #line hidden
            return;
            case 4:
            this.SamplingTimeCombo = ((System.Windows.Controls.ComboBox)(target));
            return;
            case 5:
            this.TriggerLevelBox = ((System.Windows.Controls.TextBox)(target));
            
            #line 129 "..\..\..\Popup\TriggerSetting.xaml"
            this.TriggerLevelBox.KeyDown += new System.Windows.Input.KeyEventHandler(this.TriggerLevelBox_OnKeyDown);
            
            #line default
            #line hidden
            return;
            case 6:
            
            #line 137 "..\..\..\Popup\TriggerSetting.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.OKButton_OnClick);
            
            #line default
            #line hidden
            return;
            case 7:
            
            #line 138 "..\..\..\Popup\TriggerSetting.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.CancelButton_OnClick);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}
