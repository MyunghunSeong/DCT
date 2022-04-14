using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using DevExpress.Xpf.Docking;

namespace CrevisLibrary
{
    public class UserManualContent : IContent
    {
        /// <summary>
        /// 에러 처리 핸들러
        /// </summary>
        public event EventHandler<LogExecuteEventArgs> LogEvent;

        private String m_Name;
        public String Name
        {
            get { return this.m_Name; }
            set { this.m_Name = value; }
        }

        private ContentType m_Type;
        public ContentType Type
        {
            get { return this.m_Type; }
            set { this.m_Type = value; }
        }

        private IDevice m_MyDevice;
        public IDevice MyDevice
        {
            get { return this.m_MyDevice; }
            set { this.m_MyDevice = value; }
        }

        public UserManualContent()
        {
            this.m_Name = String.Empty;
            this.m_Type = ContentType.UserManualContent;
            this.m_MyDevice = null;
        }

        // 모두 선택 함수
        public void AllCheck(Object sender)
        {
            try
            {
                ParameterView prop = sender as ParameterView;
                ParameterModel Model = prop.Tag as ParameterModel;
                foreach (DevParam categoryParam in Model.Root.Children)
                {
                    foreach (DevParam param in categoryParam.Children)
                        param.IsChecked = true;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        // 모두 해제 함수
        public void AllUnCheck(Object sender)
        {
            try
            {
                ParameterView prop = sender as ParameterView;
                ParameterModel Model = prop.Tag as ParameterModel;
                foreach (DevParam categoryParam in Model.Root.Children)
                {
                    foreach (DevParam param in categoryParam.Children)
                        param.IsChecked = false;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        // 통신 함수
        public void Communication(Object sender, Boolean IsUpdate=false)
        {
            try
            {
                ParameterView prop = sender as ParameterView;
                ParameterModel Model = prop.Tag as ParameterModel;
                IParam recvParam = null;
                foreach(DevParam CategoryParam in Model.Root.Children)
                {
                    for (int i = 0; i < CategoryParam.Children.Count; i++)
                    {
                        if ((CategoryParam.Children[i] as DevParam).IsChecked)
                        {
                            if (IsUpdate)
                            {
                                AccessMode mode = CategoryParam.Children[i].AccessMode;
                                ParamRepresentation Representation = CategoryParam.Children[i].Representation;
                                recvParam = this.MyDevice.Read(CategoryParam.Children[i]);
                                if (recvParam == null)
                                {
                                    PublicVar.MainWnd.Process_Connect(false);
                                    throw new Exception("Failed to read data");
                                }
                                (recvParam as DevParam).m_DevParamInfo = (CategoryParam.Children[i] as DevParam).m_DevParamInfo;
                                recvParam.AccessMode = mode;
                                (recvParam as DevParam).m_Representation = Representation;
                                CategoryParam.Children[i] = recvParam;
                            }
                            else
                            {
                                if (CategoryParam.Children[i].AccessMode.Equals(AccessMode.ReadOnly))
                                {
                                    ParamRepresentation Representation = CategoryParam.Children[i].Representation;
                                    recvParam = this.MyDevice.Read(CategoryParam.Children[i]);
                                    if (recvParam == null)
                                    {
                                        PublicVar.MainWnd.Process_Connect(false);
                                        throw new Exception("Failed to read data");
                                    }
                                    (recvParam as DevParam).m_DevParamInfo = (CategoryParam.Children[i] as DevParam).m_DevParamInfo;
                                    recvParam.AccessMode = AccessMode.ReadOnly;
                                    (recvParam as DevParam).m_Representation = Representation;
                                    CategoryParam.Children[i] = recvParam;
                                }
                                else if (CategoryParam.Children[i].AccessMode.Equals(AccessMode.ReadWrite))
                                {
                                    //Write 후에 Read한다.
                                    AccessMode mode = CategoryParam.Children[i].AccessMode;
                                    this.MyDevice.Write(CategoryParam.Children[i]);
                                    recvParam = this.MyDevice.Read(CategoryParam.Children[i]);
                                    if (recvParam == null)
                                    {
                                        PublicVar.MainWnd.Process_Connect(false);
                                        throw new Exception("Failed to read data");
                                    }
                                    (recvParam as DevParam).m_DevParamInfo = (CategoryParam.Children[i] as DevParam).m_DevParamInfo;
                                    recvParam.AccessMode = mode;
                                    CategoryParam.Children[i] = recvParam;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                if (this.LogEvent != null)
                    this.LogEvent(this, new LogExecuteEventArgs(LogState.Error, err.Message, DateTime.Now.ToString("HH:mm:ss"), err.StackTrace));
            }
        }

        public void DataReset(Object sender)
        {
            try
            {
                //현재 기능을 수행하는 Content의 View와 정보를 가져온다.
                ParameterView prop = sender as ParameterView;
                DevParam Param = (prop.Tag as ParameterModel).Root;

                foreach (DevParam subParam in Param.Children)
                {
                    //초기 값(기본 값) 정보를 가져온다.
                    String InitValue = subParam.m_DevParamInfo.m_InitValue;
                    if (InitValue.Equals(String.Empty) && subParam.ParamInfo.ParamType != ParamType.Enum)
                        continue;
                    //파라미터 타입에 따라서 캐스팅을 해서 Value을 바꿔준다.
                    switch (subParam.ParamInfo.ParamType)
                    {
                        case ParamType.Integer:
                            subParam.Value = Convert.ToInt32(InitValue);
                            break;
                        case ParamType.String:
                            subParam.Value = InitValue;
                            break;
                        case ParamType.Byte:
                            subParam.Value = Convert.ToByte(InitValue);
                            break;
                        case ParamType.ByteArray:
                            subParam.Value = Encoding.Default.GetBytes(InitValue);
                            break;
                        case ParamType.Boolean:
                            Int32 tmp = Convert.ToInt32(InitValue);
                            subParam.Value = Convert.ToBoolean(tmp);
                            break;
                        case ParamType.Short:
                            subParam.Value = Convert.ToInt16(InitValue);
                            break;
                        case ParamType.Float:
                            subParam.Value = Convert.ToSingle(InitValue);
                            break;
                        case ParamType.Enum:
                            if (subParam.AccessMode == AccessMode.ReadOnly)
                                return;

                            Int32 enumVal = (InitValue == String.Empty) ? 0 : Convert.ToInt32(InitValue);
                            (subParam as IEnumParam).EnumIntValue = enumVal;
                            break;
                    }
                }

                //초기화 된 값으로 새로운 모델을 생성하고 View를 바꿔준다.
                ParameterModel newModel = new ParameterModel(Param, false);
                prop._tree.Model = newModel;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void SaveData(IParam Param)
        {
            try
            {
                this.MyDevice.Write(Param);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
