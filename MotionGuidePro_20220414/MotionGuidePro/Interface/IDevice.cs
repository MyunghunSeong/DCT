using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrevisLibrary;

namespace CrevisLibrary
{
    public interface IDevice
    {
        // 컨텐츠 이름 리스트
        Dictionary<String, IContent> ContentList { get; set; }

        // 상위 모델 정보
        DevParam MyModel { get; set; }

        // 디바이스 통신 및 기본 정보
        DevParam InitParam { get; set; }

        // 통신 오픈
        Boolean Open();

        // 통신 해제
        void Close();

        // 통신 읽기
        IParam Read(IParam Param);

        // 통신 쓰기
        void Write(IParam Param);

        // 디바이스 이름
        String Name { get; set; }

        // Server(Board) Address
        String SvrAddress { get; set; }

        //Client Address
        String MyAddress { get; set; }

        //Timeout
        Double Timeout { get; set; }

        // 포트 번호
        Int32 Port { get; set; }

        // 디바이스 정보 로드
        void Load(String FilePath);

        // 디바이스 정보 저장
        void Save(IParam Param, String FilePath);
    }
}
