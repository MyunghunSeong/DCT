using Aga.Controls.Tree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrevisLibrary
{
    class ParameterModel : ITreeModel
    {
        public String TitleName { get; set; }
        public String Description { get; set; }
        public DevParam Root { get; private set; }

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="Root">해당 데이터의 Root 데이터</param>
        /// <param name="IsVisibleRoot">
        /// <para> True : Root 요소를 출력</para>
        /// <para> False : Root 요소를 미출력</para>
        /// </param>
        public ParameterModel(IParam Root, Boolean IsVisibleRoot=true)
        {
            if (IsVisibleRoot)
            {
                this.Root = new DevParam("ParameterModel Root", ParamType.Category, null, "ParameterModel Root");
                this.Root.AddChild(Root);
            }
            else
                this.Root = Root as DevParam;
        }

        public System.Collections.IEnumerable GetChildren(Object parent)
        {
            if (parent == null)
                parent = Root;
            return (parent as IParam).Children;
        }

        public bool HasChildren(Object parent)
        {
            return (parent as IParam).Children.Count > 0;
        }
    }
}
