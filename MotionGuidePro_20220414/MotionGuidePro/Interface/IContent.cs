using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CrevisLibrary
{
    public interface IContent
    {
        // 컨텐츠 이름
        String Name { get; set; }

        //컨텐츠 타입
        ContentType Type { get; set; }

        // 데이터 초기화 함수
        void DataReset(Object sender);

        // 데이터 저장 함수
        void SaveData(IParam Param);

        // 모두 선택 함수
        void AllCheck(Object sender);

        // 모두 해제 함수
        void AllUnCheck(Object sender);

        // 통신 함수
        void Communication(Object sender, Boolean IsUpdate=false);

        // 상위 디바이스 정보
        IDevice MyDevice { get; set; }
    }
}
