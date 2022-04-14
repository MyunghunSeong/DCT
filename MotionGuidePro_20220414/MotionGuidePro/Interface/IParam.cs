using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CrevisLibrary
{
    ///<summary>종속파라미터 콜백 </summary>
    public delegate void DependencyCallBack(IParam ParamMain, IParam ParamDepend);

    public interface IParam : ICloneable
    {
        Visibility Visible { get; set; }

        ///<summary>상위 파리미터 </summary>
        IParam Parent { get; set; }
        //파라미터 정보 </summary>
        IParamInfo ParamInfo { get; }
        ///<summary>파라미터 값 </summary>
        Object Value { get; set; }
        ///<summary>AccessMode </summary>
        AccessMode AccessMode { get; set; }
        ///<summary>파라미터 보호 </summary>
        Boolean IsProtected { get; set; }
        ///<summary>표시 형태 </summary>
        ParamRepresentation Representation { get; }
        ///<summary>자식노드 </summary>
        ObservableCollection<IParam> Children { get; }
        ///<summary>파리미터의 위치 </summary>
        String ParamPath { get; }
        ///<summary>종속 파라미터 </summary>
        List<IParam> DependencyParams { get; }
        /// <summary>파라미터 변경되면 호출</summary>
        DependencyCallBack DependencyParamUpdate { get; set; }
        ///<summary>자식노드 있는지 </summary>
        Boolean HasChildren();
        ///<summary>파라미터 표기 </summary>
        String ToString();
        ///<summary>파라미터 이름으로 IParam 리스트 받기 </summary>
        List<IParam> GetParamNodes(String ParamName);
        ///<summary>파라미터 이름으로 첫번째 검색된 IParam 받기 </summary>
        IParam GetParamNode(String ParamName);
        ///<summary>파라미터만(카테고리 제외) 받기</summary>
        List<IParam> GetParamNodes();
        ///<summary>Root 가져오기</summary>
        IParam GetRootParam();

        IDevice MyDevice { get; set; }
    }
}
