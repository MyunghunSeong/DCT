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
    public class MonitoringContent : IContent
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

        public MonitoringContent()
        {
            this.m_Name = String.Empty;
            this.m_Type = ContentType.MonitoringContent;
            this.m_MyDevice = null;
        }

        // 모두 선택 함수
        public void AllCheck(Object sender)
        {
            try
            {
                ParameterView prop = sender as ParameterView;
                ParameterModel Model = prop.Tag as ParameterModel;
                foreach (DevParam param in Model.Root.Children)
                    param.IsChecked = true;
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
                foreach (DevParam param in Model.Root.Children)
                    param.IsChecked = false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // 통신 함수
        public void Communication(Object sender, Boolean IsUpdate=false)
        {
            IParam TargetParam = null;

            try
            {
                ParameterView prop = sender as ParameterView;
                ParameterModel Model = prop.Tag as ParameterModel;
                IParam recvParam = null;

                for (int i = 0; i < Model.Root.Children.Count; i++)
                {
                    if ((Model.Root.Children[i] as DevParam).IsChecked)
                    {
                        if (IsUpdate)
                        {
                            AccessMode mode = Model.Root.Children[i].AccessMode;
                            ParamRepresentation Representation = Model.Root.Children[i].Representation;
                            recvParam = this.MyDevice.Read(Model.Root.Children[i]);
                            if (recvParam == null)
                            {
                                PublicVar.MainWnd.Process_Connect(false);
                                throw new Exception("Failed to read data");
                            }
                            (recvParam as DevParam).m_DevParamInfo = (Model.Root.Children[i] as DevParam).m_DevParamInfo;
                            recvParam.AccessMode = mode;
                            (recvParam as DevParam).m_Representation = Representation;
                            Model.Root.Children[i] = recvParam;
                        }
                        else
                        {
                            if (Model.Root.Children[i].AccessMode.Equals(AccessMode.ReadOnly))
                            {
                                ParamRepresentation Representation = Model.Root.Children[i].Representation;
                                recvParam = this.MyDevice.Read(Model.Root.Children[i]);
                                if (recvParam == null)
                                {
                                    PublicVar.MainWnd.Process_Connect(false);
                                    throw new Exception("Failed to read data");
                                }
                                (recvParam as DevParam).m_DevParamInfo = (Model.Root.Children[i] as DevParam).m_DevParamInfo;
                                recvParam.AccessMode = AccessMode.ReadOnly;
                                (recvParam as DevParam).m_Representation = Representation;
                                Model.Root.Children[i] = recvParam;
                            }
                            else if (Model.Root.Children[i].AccessMode.Equals(AccessMode.ReadWrite))
                            {
                                AccessMode mode = Model.Root.Children[i].AccessMode;
                                this.MyDevice.Write(Model.Root.Children[i]);
                                recvParam = this.MyDevice.Read(Model.Root.Children[i]);
                                if (recvParam == null)
                                {
                                    PublicVar.MainWnd.Process_Connect(false);
                                    throw new Exception("Failed to read data");
                                }
                                (recvParam as DevParam).m_DevParamInfo = (Model.Root.Children[i] as DevParam).m_DevParamInfo;
                                recvParam.AccessMode = mode;
                                Model.Root.Children[i] = recvParam;
                            }
                        }
                    }

                    TargetParam = Model.Root.Children[i];
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
                AccessMode backUpMode = new AccessMode();

                foreach (DevParam subParam in Param.Children)
                {
                    backUpMode = subParam.AccessMode;
                    subParam.AccessMode = AccessMode.WriteOnly;
                    //초기 값(기본 값) 정보를 가져온다.
                    String InitValue = subParam.m_DevParamInfo.m_InitValue;
                    //파라미터 타입에 따라서 캐스팅을 해서 Value을 바꿔준다.
                    switch (subParam.ParamInfo.ParamType)
                    {
                        case ParamType.Integer:
                            if (InitValue.Equals(String.Empty))
                                subParam.Value = 0;
                            else
                                subParam.Value = Convert.ToInt32(InitValue);
                            break;
                        case ParamType.String:
                            if (InitValue.Equals(String.Empty))
                                subParam.Value = String.Empty;
                            else
                                subParam.Value = InitValue;

                            if (backUpMode == AccessMode.ReadOnly)
                                subParam.AccessMode = AccessMode.ReadOnly;
                            break;
                        case ParamType.Byte:
                            if (InitValue.Equals(String.Empty))
                                subParam.Value = 0x00;
                            else
                                subParam.Value = Convert.ToByte(InitValue);
                            break;
                        case ParamType.ByteArray:
                            if (InitValue.Equals(String.Empty))
                                subParam.Value = 0x00;
                            else
                                subParam.Value = Encoding.Default.GetBytes(InitValue);
                            break;
                        case ParamType.Boolean:
                            Int32 tmp = 0;
                            if (InitValue.Equals(String.Empty))
                            {
                                tmp = 0;
                                subParam.Value = Convert.ToBoolean(tmp);
                            }
                            else
                            {
                                tmp = Convert.ToInt32(InitValue);
                                subParam.Value = Convert.ToBoolean(tmp);
                            }
                            break;
                        case ParamType.Short:
                            if (InitValue.Equals(String.Empty))
                                subParam.Value = (Int16)0;
                            else
                                subParam.Value = Convert.ToInt16(InitValue);
                            break;
                        case ParamType.Float:
                            if (InitValue.Equals(String.Empty))
                                subParam.Value = 0.0f;
                            else
                                subParam.Value = Convert.ToSingle(InitValue);
                            break;
                        case ParamType.Enum:
                            if (subParam.AccessMode == AccessMode.ReadOnly)
                                return;

                            Int32 enumVal = (InitValue == String.Empty) ? 0 : Convert.ToInt32(InitValue);
                            (subParam as IEnumParam).EnumIntValue = enumVal;
                            break;
                    }

                    if (backUpMode == AccessMode.ReadOnly)
                        subParam.AccessMode = AccessMode.ReadOnly;
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
