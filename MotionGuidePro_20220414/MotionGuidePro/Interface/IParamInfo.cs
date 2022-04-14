using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrevisLibrary
{
    public interface IParamInfo
    {
        // 파라미터 이름
        String ParamName { get; }
        // 파라미터 타입
        ParamType ParamType { get; }
        // 파라미터 설명
        String Description { get; }
        // Address 
        int Address { get; }
        // Data크기
        int Length { get; }
        // 기본 값
        String InitValue { get; }
        //최소값
        Double Min { get; }
        //최대값
        Double Max { get; }

        //Default
        Boolean Default { get; set; }

        // 어디로 통신 할지
        PortType PortType { get; }
        // NA를 위한거 SlotIndex :-1 면 Device로 통신 아니면 Slot으로 통신
        int SlotIndex { get; }
    }
}
