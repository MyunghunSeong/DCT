using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;
using System.Collections.ObjectModel;
using CrevisLibrary;
using DCT_Graph;

namespace CrevisLibrary
{
    public static class XmlParser
    {
        // Xml처리를 위한 객체
        private static XmlDocument m_XmlDoc = new XmlDocument();

        // XML로 저장
        public static void SaveParam(DevParam Param, String FilePath)
        {
            try
            {
                // 기존꺼 몽땅 지우고
                m_XmlDoc.RemoveAll();

                // 새로 받아서
                XmlElement NewElem = DevParam2Xml(Param);

                // <?xml version="1.0" encoding="utf-8"?> 추가
                m_XmlDoc.AppendChild(m_XmlDoc.CreateXmlDeclaration("1.0", "utf-8", ""));

                // 추가
                m_XmlDoc.AppendChild(NewElem);

                // 저장
                m_XmlDoc.Save(FilePath);
            }
            catch (Exception)
            {
                throw;
            }
        }

        // XML에서 가져 오기 : DevPara으로 가져오기
        public static DevParam LoadParam(String FilePath)
        {
            try
            {
                // 기존꺼 몽땅 지우고
                m_XmlDoc.RemoveAll();

                // XML로드
                m_XmlDoc.Load(FilePath);

                // Root 주고 파싱
                DevParam LoadParam = Xml2DevParam(m_XmlDoc.DocumentElement);

                // 리턴
                return LoadParam;
            }
            catch (FileNotFoundException)
            {
                return null;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // Param 정보를 XmlElement로 만들기
        private static XmlElement DevParam2Xml(DevParam Param)
        {
            try
            {
                // 일단 노드 하나 만들고 : Root, Category, Parameter로 구분
                String NodeName = String.Empty;
                if (Param.ParamInfo.ParamType == ParamType.Category)
                    NodeName = "Category";
                else
                    NodeName = "Parameter";

                XmlElement Element = m_XmlDoc.CreateElement(NodeName);

                // 기본값 넣어 주고
                SetXmlAttributesFromIParam(Element, Param);

                // ParamType 별로 만들어와야됨 
                switch (Param.ParamInfo.ParamType)
                {
                    case ParamType.Category:
                        break;
                    case ParamType.Integer:
                        if (Param.Representation == ParamRepresentation.Linear)
                            SetXmlIntegerAttributesFromIParam(Element, Param);
                        break;
                    case ParamType.Float:
                    case ParamType.String:
                    case ParamType.FileSelect:
                        break;
                    case ParamType.Command:
                        break;
                    case ParamType.Enum:
                        SetXmlElementFromEnumParam(Element, Param);
                        break;
                    case ParamType.Boolean:
                    case ParamType.Byte:
                    case ParamType.Short:
                    case ParamType.ByteArray:
                    case ParamType.TextModify:
                        break;
                    default:
                        throw new Exception("정의 되지 않은 Param.ParamType 입니다.");
                }

                // 자식 노드 있으면 만들어서 붙여줘
                foreach (DevParam chield in Param.Children)
                {
                    Element.AppendChild(DevParam2Xml(chield));
                }

                return Element;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // 파라미터 기본값을 Element에 추가
        private static void SetXmlAttributesFromIParam(XmlElement Element, DevParam Param)
        {
            try
            {
                // 파라미터 이름
                XmlAttribute Attr = m_XmlDoc.CreateAttribute("Name");
                Attr.Value = Param.ParamInfo.ParamName;
                Element.Attributes.Append(Attr);

                // ParamType : Category는 생략
                if (Param.ParamInfo.ParamType != ParamType.Category)
                {
                    Attr = m_XmlDoc.CreateAttribute("ParamType");
                    Attr.Value = Param.ParamInfo.ParamType.ToString();
                    Element.Attributes.Append(Attr);
                }

                // Value
                if (Param.Value != null)
                {
                    Attr = m_XmlDoc.CreateAttribute("Value");
                    if (Param.ParamInfo.ParamType == ParamType.ByteArray)
                        Attr.Value = String.Join(" ", (Byte[])Param.Value);
                    else
                        Attr.Value = Param.Value.ToString();
                    Element.Attributes.Append(Attr);
                }

                // AccessMode : Category는 생략
                if (Param.ParamInfo.ParamType != ParamType.Category)
                {
                    Attr = m_XmlDoc.CreateAttribute("AccessMode");
                    Attr.Value = Param.AccessMode.ToString();
                    Element.Attributes.Append(Attr);
                }

                // Description : 비어 있으면 생략
                if (!Param.ParamInfo.Description.Equals(String.Empty))
                {
                    Attr = m_XmlDoc.CreateAttribute("Description");
                    Attr.Value = Param.ParamInfo.Description;
                    Element.Attributes.Append(Attr);
                }

                // IsProtected : false 면 생략
                if (Param.IsProtected)
                {
                    Attr = m_XmlDoc.CreateAttribute("IsProtected");
                    Attr.Value = Param.IsProtected.ToString();
                    Element.Attributes.Append(Attr);
                }

                // Category는 생략
                if (Param.ParamInfo.ParamType != ParamType.Category)
                {
                    // Representation :  PureNumber 면 생략 
                    if (Param.Representation != ParamRepresentation.PureNumber)
                    {
                        Attr = m_XmlDoc.CreateAttribute("Representation");
                        Attr.Value = Param.Representation.ToString();
                        Element.Attributes.Append(Attr);
                    }

                    // PortType : None이면 생략
                    if (Param.ParamInfo.PortType == PortType.None)
                    {
                        // Address 
                        Attr = m_XmlDoc.CreateAttribute("Address");
                        Attr.Value = "0x" + Param.ParamInfo.Address.ToString("X");
                        Element.Attributes.Append(Attr);

                        //Unit
                        Attr = m_XmlDoc.CreateAttribute("Unit");
                        Attr.Value = Param.m_DevParamInfo.Unit;
                        Element.Attributes.Append(Attr);

                        // Length
                        Attr = m_XmlDoc.CreateAttribute("Length");
                        Attr.Value = Param.ParamInfo.Length.ToString();
                        Element.Attributes.Append(Attr);

                        // PortType
                        Attr = m_XmlDoc.CreateAttribute("PortType");
                        Attr.Value = Param.ParamInfo.PortType.ToString();
                        Element.Attributes.Append(Attr);

                        //InitValue
                        Attr = m_XmlDoc.CreateAttribute("InitValue");
                        Attr.Value = Param.ParamInfo.InitValue;
                        Element.Attributes.Append(Attr);

                        //Min
                        Attr = m_XmlDoc.CreateAttribute("Min");
                        Attr.Value = Param.ParamInfo.Min.ToString();
                        Element.Attributes.Append(Attr);

                        //Max
                        Attr = m_XmlDoc.CreateAttribute("Max");
                        Attr.Value = Param.ParamInfo.Max.ToString();
                        Element.Attributes.Append(Attr);

                        //Default
                        Attr = m_XmlDoc.CreateAttribute("Default");
                        Attr.Value = Param.ParamInfo.Default.ToString();
                        Element.Attributes.Append(Attr);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        // Param : Integer Type, Element 에 Min, Max, Inc Attribute 추가
        private static void SetXmlIntegerAttributesFromIParam(XmlElement Element, DevParam Param)
        {
            try
            {
                // Min
                XmlAttribute MinAttr = m_XmlDoc.CreateAttribute("Min");
                MinAttr.Value = Param.Min.ToString();
                Element.Attributes.Append(MinAttr);

                // Max
                XmlAttribute MaxAttr = m_XmlDoc.CreateAttribute("Max");
                MaxAttr.Value = Param.Max.ToString();
                Element.Attributes.Append(MaxAttr);

                // Inc
                XmlAttribute IncAttr = m_XmlDoc.CreateAttribute("Inc");
                IncAttr.Value = Param.Inc.ToString();
                Element.Attributes.Append(IncAttr);
            }
            catch (Exception)
            {
                throw;
            }
        }

        // Param : Enum Type, Element 에 EnumEntry 추가
        private static void SetXmlElementFromEnumParam(XmlElement Element, DevParam Param)
        {
            try
            {
                foreach (EnumEntry Entry in Param.EnumEntries.SourceCollection)
                {
                    // EnumEntry 노드 생성
                    XmlElement EnumEntryElement = m_XmlDoc.CreateElement("EnumEntry");

                    // Value
                    XmlAttribute ValueAttr = m_XmlDoc.CreateAttribute("Value");
                    ValueAttr.Value = Entry.EnumValue.ToString();
                    EnumEntryElement.Attributes.Append(ValueAttr);

                    // DisplayName
                    XmlAttribute DisplayNameAttr = m_XmlDoc.CreateAttribute("DisplayName");
                    DisplayNameAttr.Value = Entry.DisplayName;
                    EnumEntryElement.Attributes.Append(DisplayNameAttr);

                    // Element에 추가
                    Element.AppendChild(EnumEntryElement);
                }

            }
            catch (Exception)
            {
                throw;
            }
        }

        // XmlElement 정보를  IParam로 만들기
        private static DevParam Xml2DevParam(XmlElement Element)
        {
            try
            {
                // XML에서 DevParam 생성
                DevParam Param = GetIParamFromXmlElement(Element);

                // 차일드꺼 만들기
                foreach (XmlNode chield in Element)
                {
                    if (chield.NodeType != XmlNodeType.Element)
                        continue;

                    if (chield.Name.Equals("EnumEntry"))    //Integer Parameter에서 파싱 됨
                        continue;

                    Param.AddChild(Xml2DevParam((XmlElement)chield));
                }

                return Param;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // 파리미터 정보 채우기
        private static DevParam GetIParamFromXmlElement(XmlElement Element)
        {
            try
            {
                // 파라미터 이름
                String ParamName = Element.Attributes["Name"].Value;

                // ParamType : Category는 생략 가능
                ParamType eParamType = ParamType.Category;
                if (Element.Attributes["ParamType"] != null)
                {
                    eParamType = (ParamType)Enum.Parse(typeof(ParamType), Element.Attributes["ParamType"].Value);
                }

                // DescriptionAttr : 비어 있으면 생략
                if (ParamName.Equals("U Phase Current"))
                    Console.WriteLine();
                String Description = String.Empty;
                if (Element.Attributes["Description"] != null)
                {
                    Description = Element.Attributes["Description"].Value;
                }

                // 파라미터 만들고
                DevParam Param = new DevParam(ParamName, eParamType, null, Description);

                // AccessMode
                if (Element.Attributes["AccessMode"] != null)
                {
                    String sAccessMode = Element.Attributes["AccessMode"].Value;
                    Param.AccessMode = (AccessMode)Enum.Parse(typeof(AccessMode), sAccessMode);
                }

                // IsProtected : 비어 있으면 생략
                if (Element.Attributes["IsProtected"] != null)
                {
                    Boolean IsProtected = Boolean.Parse(Element.Attributes["IsProtected"].Value);
                    Param.IsProtected = IsProtected;
                }

                // Representation 
                if (Element.Attributes["Representation"] != null)
                {
                    String Representation = Element.Attributes["Representation"].Value;
                    Param.Representation = (ParamRepresentation)Enum.Parse(typeof(ParamRepresentation), Representation);
                }

                if (eParamType != ParamType.Category)
                {
                    // Value
                    if (Element.Attributes["Value"] != null)
                    {
                        switch (eParamType)
                        {
                            case ParamType.Category:
                                break;
                            case ParamType.Integer:
                                if (Element.Attributes["Value"].Value.Equals(String.Empty))
                                    Param.m_Value = 0;
                                else
                                {
                                    Param.m_Value = int.Parse(Element.Attributes["Value"].Value);
                                    if (Param.Representation == ParamRepresentation.Linear)
                                        SetIntegerParamFromXmlElement(Param, Element);
                                }
                                break;
                            case ParamType.Float:
                                if (Element.Attributes["Value"].Value.Equals(String.Empty))
                                    Param.m_Value = 0.0f;
                                else
                                    Param.m_Value = float.Parse(Element.Attributes["Value"].Value);
                                break;
                            case ParamType.String:
                            case ParamType.FileSelect:
                            case ParamType.TextModify:
                                Param.m_Value = Element.Attributes["Value"].Value;
                                break;
                            case ParamType.Command:
                                if (Element.Attributes["Value"].Value.Equals(String.Empty))
                                    Param.m_Value = 0;
                                else
                                    Param.m_Value = int.Parse(Element.Attributes["Value"].Value);
                                break;
                            case ParamType.Enum:
                                if (Element.Attributes["Value"].Value.Equals(String.Empty))
                                    Param.m_Value = 0;
                                else
                                {
                                    int number;
                                    bool success = Int32.TryParse(Element.Attributes["Value"].Value, out number);
                                    if (success)
                                        Param.m_Value = number;
                                    else
                                        Param.m_Value = Element.Attributes["Value"].Value;
                                }
                                SetEnumEntryFromXmlElement(Param, Element);
                                break;
                            case ParamType.Boolean:
                                if (Element.Attributes["Value"].Value.Equals(String.Empty))
                                    Param.m_Value = Boolean.FalseString;
                                else
                                    Param.m_Value = Boolean.Parse(Element.Attributes["Value"].Value);
                                break;
                            case ParamType.Byte:
                                if (Element.Attributes["Value"].Value.Equals(String.Empty))
                                    Param.m_Value = 0x00;
                                else
                                    Param.m_Value = Byte.Parse(Element.Attributes["Value"].Value);
                                break;
                            case ParamType.Short:
                                if (Element.Attributes["Value"].Value.Equals(String.Empty))
                                    Param.m_Value = 0;
                                else
                                    Param.m_Value = Int16.Parse(Element.Attributes["Value"].Value);
                                break;
                            case ParamType.ByteArray:
                                if (Element.Attributes["Value"].Value.Equals(String.Empty))
                                    Param.m_Value = 0x00;
                                else
                                    Param.m_Value = Array.ConvertAll(Element.Attributes["Value"].Value.Split(' '), Byte.Parse);
                                break;
                            default:
                                throw new Exception("정의 되지 않은 ParamType 입니다.");
                        }
                    }

                    // Address 
                    if (Element.Attributes["Address"] != null)
                    {
                        if (!Element.Attributes["Address"].Value.Equals(String.Empty))
                        {
                            String address = Element.Attributes["Address"].Value;
                            if (address.ToLower().Contains("0x"))
                            {
                                Int32 index = address.IndexOf("0x");
                                address = address.Substring(index + 2, address.Length - 2);
                            }
                            //Param.m_DevParamInfo.m_Address = int.Parse(Element.Attributes["Address"].Value);
                            Param.m_DevParamInfo.m_Address = Convert.ToInt32(address, 16);
                        }
                    }

                    // Unit
                    if (Element.Attributes["Unit"] != null)
                    {
                        if (!Element.Attributes["Unit"].Value.Equals(String.Empty))
                            Param.m_DevParamInfo.m_Unit = Element.Attributes["Unit"].Value;
                    }

                    // InitValue
                    if (Element.Attributes["InitValue"] != null)
                    {
                        if (!Element.Attributes["InitValue"].Value.Equals(String.Empty))
                            Param.m_DevParamInfo.m_InitValue = Element.Attributes["InitValue"].Value;
                    }

                    // Max
                    if (Element.Attributes["Max"] != null)
                    {
                        if (!Element.Attributes["Max"].Value.Equals(String.Empty))
                            Param.m_DevParamInfo.m_Max = Convert.ToDouble(Element.Attributes["Max"].Value);
                        else
                            Param.m_DevParamInfo.m_Max = -1;
                    }

                    // Min
                    if (Element.Attributes["Min"] != null)
                    {
                        if (!Element.Attributes["Min"].Value.Equals(String.Empty))
                            Param.m_DevParamInfo.m_Min = Convert.ToDouble(Element.Attributes["Min"].Value);
                        else
                            Param.m_DevParamInfo.m_Min = -1;
                    }

                    //Default
                    if (Element.Attributes["Default"] != null)
                    {
                        if (!Element.Attributes["Default"].Value.Equals(String.Empty))
                            Param.m_DevParamInfo.m_Default = Convert.ToBoolean(Element.Attributes["Default"].Value);
                        else
                            Param.m_DevParamInfo.m_Default = true;
                    }

                    ////Description
                    //if (Element.Attributes["Description"] != null)
                    //{
                    //    if (!Element.Attributes["Description"].Value.Equals(String.Empty))
                    //        Param.m_DevParamInfo.m_Description = Element.Attributes["Description"].Value;
                    //    else
                    //        Param.m_DevParamInfo.m_Description = String.Empty;
                    //}

                    // Length 
                    if (Element.Attributes["Length"] != null)
                    {
                        if (!Element.Attributes["Length"].Value.Equals(String.Empty))
                            Param.m_DevParamInfo.m_Length = int.Parse(Element.Attributes["Length"].Value);
                    }

                    // PortType 
                    if (Element.Attributes["PortType"] != null)
                    {
                        if (!Element.Attributes["PortType"].Value.Equals(String.Empty))
                        {
                            String PortType = Element.Attributes["PortType"].Value;
                            Param.m_DevParamInfo.m_PortType = (PortType)Enum.Parse(typeof(PortType), PortType);
                        }
                    }

                    // SlotIndex 
                    if (Element.Attributes["SlotIndex"] != null)
                    {
                        if (!Element.Attributes["SlotIndex"].Value.Equals(String.Empty))
                            Param.m_DevParamInfo.m_SlotIndex = int.Parse(Element.Attributes["SlotIndex"].Value);
                    }
                }
                else
                {
                    if (Element.Attributes["Value"] != null)
                        Param.m_Value = Element.Attributes["Value"].Value;
                }

                return Param;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // Element : Integer Type , Param 에 Min, Max, Inc 추가
        private static void SetIntegerParamFromXmlElement(DevParam Param, XmlElement Element)
        {
            try
            {
                // Min
                Param.Min = int.Parse(Element.Attributes["Min"].Value);

                // Max
                Param.Max = int.Parse(Element.Attributes["Max"].Value);

                // Inc
                Param.Inc = int.Parse(Element.Attributes["Inc"].Value);

            }
            catch (Exception)
            {
                throw;
            }

        }

        // Element : Enum Type , Param 에 EnumEntry 가져와서 추가
        private static void SetEnumEntryFromXmlElement(DevParam Param, XmlElement Element)
        {
            try
            {
                foreach (XmlNode chield in Element)
                {
                    int Val = int.Parse(chield.Attributes["Value"].Value);
                    String DisplayName = chield.Attributes["DisplayName"].Value;
                    Param.AddEnumEntry(Val, DisplayName);
                }

            }
            catch (Exception)
            {
                throw;
            }

        }

        //FaultConfig Parser
        public static Dictionary<String, String> FaultConfigParse(Dictionary<String, String> list, String FilePath)
        {
            try
            {
                //리스트 초기화
                list.Clear();

                //파일 로드
                m_XmlDoc.Load(FilePath);

                //정보 가져와서 저장
                foreach (XmlNode node in m_XmlDoc.DocumentElement.ChildNodes)
                {
                    String key = node.Attributes["Key"].Value;
                    String value = node.Attributes["Value"].Value;

                    list.Add(key, value);
                }

                //리스트 리턴
                return list;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public static String[] GetFileConfiguration()
        {
            String[] FilePathArr = new string[2];

            String FilePath = AppDomain.CurrentDomain.BaseDirectory + "\\resource\\FileConfig\\FileConfig.xml";
            m_XmlDoc.Load(FilePath);

            foreach (XmlNode node in m_XmlDoc.DocumentElement.ChildNodes)
            {
                FilePathArr[0] = node.Attributes["LoadFile"].Value;
                FilePathArr[1] = node.Attributes["BaseFilePath"].Value;
            }

            return FilePathArr;
        }

        public static void SetFileConfiguration(String FileName, String BaseFilePath)
        {
            String LoadFilePath = AppDomain.CurrentDomain.BaseDirectory + "\\resource\\FileConfig\\FileConfig.xml";
            m_XmlDoc.Load(LoadFilePath);

            foreach (XmlNode node in m_XmlDoc.DocumentElement.ChildNodes)
            {
                node.Attributes["LoadFile"].Value = FileName;
                node.Attributes["BaseFilePath"].Value = BaseFilePath;
            }

            m_XmlDoc.Save(LoadFilePath);
        }

        public static void GetAxisInformation(OscilloscopeContent Content)
        {
            //축 정보 XML파일 로드
            String LoadFilePath = AppDomain.CurrentDomain.BaseDirectory + "\\resource\\AxisConfig\\AxisConfig.xml";
            m_XmlDoc.Load(LoadFilePath);

            List<String> AxisNameList = new List<String>();
            Dictionary<Int32, String> AxisNameMap = new Dictionary<Int32, String>();
            Dictionary<Int32, DataInformation> DataInfoList = new Dictionary<Int32, DataInformation>();
            Content.DigitalSignalMap.Clear();
            foreach (XmlNode node in m_XmlDoc.DocumentElement.ChildNodes)
            {
                DigitalSignal DigitalSignalData = new DigitalSignal();
                DigitalSignalData.MyContent = Content;

                //데이터 정보
                DataInformation DataInfo = new DataInformation();
                DataInfo.Name = node.Attributes["Name"].Value;
                DataInfo.DataMin = (node.Attributes["Min"].Value.Equals(String.Empty)) ? 0.0d
                    : Convert.ToDouble(node.Attributes["Min"].Value);
                DataInfo.DataMax = (node.Attributes["Max"].Value.Equals(String.Empty)) ? 0.0d
                    : Convert.ToDouble(node.Attributes["Max"].Value);
                DataInfo.Unit = node.Attributes["Unit"].Value;
                Int32 Index = Convert.ToInt32(node.Attributes["Index"].Value);
                DataInfoList.Add(Index, DataInfo);

                DigitalSignalData.DataInformObj = DataInfo;

                String AxisName = (node.Attributes["Name"].Value == "None") ? "  ----------  " : node.Attributes["Name"].Value;

                AxisNameList.Add(AxisName);
                AxisNameMap.Add(Index, AxisName);

                String EnumName = DataInfo.Name.Replace(" ", "_");
                OscilloscopeParameterType type = new OscilloscopeParameterType();
                Enum.TryParse(EnumName, out type);
                Content.DigitalSignalMap.Add(type, DigitalSignalData);
            }

            for (int i = 0; i < 5; i++)
            {
                ShowAxisInformation info = new ShowAxisInformation(Content);
                info.InitializeViewModel();
                info.ChannelName = (i + 1) + "Channel";
                info.Channel = i;
                info.IsChannelSelected = true;
                info.AxisNameList = AxisNameList;
                //TextBlock[] tmp = new TextBlock[AxisNameList.Count];
                //AxisNameList.CopyTo(tmp, 0);

                //info.AxisNameList = tmp.Clone() as TextBlock[];
                info.DataInfoObj = DataInfoList[i];
                info.IsEnabledChannelComboBox = true;

                String Name = AxisNameMap[i];
                Int32 count = 0;
                foreach (String AxisName in AxisNameList)
                {
                    if (AxisName.Equals(Name))
                        break;

                    count++;
                }

                info.CurrentSelectedAxisIndex = count;
                String CurrentAxisName = AxisNameList[count];
                info.CurrentAxisColor = Content.BrushColorArray[i];
                info.CurrentSelectedAxisName = CurrentAxisName;

                String EnumName = CurrentAxisName.Replace(" ", "_");
                OscilloscopeParameterType type = new OscilloscopeParameterType();
                Enum.TryParse(EnumName, out type);

                info.ParamType = type;
                info.LogEvent += PublicVar.MainWnd.ViewModel.Log_Maker;

                Content.AxisInfoList.Add(info);
            }
        }
    }
}
