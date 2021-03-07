using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UpdateRaceCardUmatan
{
    public class clcCommon
    {
        private string sid = "Test";
        Form1 _form1;
        int readcount = 0;
        int downloadcount = 0;

        public clcCommon(Form1 form1)
        {
            _form1 = form1;
        }

        public int checkInit()
        {
            //cLog.writeLog("checkInit");

            int num = _form1.axJVLink1.JVInit(sid);
            if (num != 0)
            {
                MessageBox.Show("JVInit エラー コード：" + num + "：", "エラー",
                    MessageBoxButtons.OK, MessageBoxIcon.Hand);
                //_form1.Cursor = Cursors.Default;
            }
            //this.objCodeConv = new clsCodeConv();
            //this.objCodeConv.FileName = System.Windows.Forms.Application.StartupPath + "\\CodeTable.csv";

            return num;
        }

        public int checkClose()
        {
            int num = _form1.axJVLink1.JVClose();
            if (num != 0)
            {
                MessageBox.Show("JVClose エラー コード：" + num + "：", "エラー",
                    MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            return num;
        }

        public void callMenu()
        {
            try
            {
                int nReturnCode = _form1.axJVLink1.JVSetUIProperties();
                if (nReturnCode != 0)
                {
                    MessageBox.Show("JVSetUIPropertiesエラー コード：" +
                        nReturnCode + "：", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "エラー",
                                MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        public bool isJVOpen(string dataspec, string strDate, int option)
        {
            int retJVOpen = _form1.axJVLink1.JVOpen(dataspec,
                    strDate + "000000", option,
                    ref readcount, ref downloadcount,
                    out string _);
            if (retJVOpen != 0)
            {
                //cLog.writeLog("[isJVOpen]JVOpen エラー：" +
                //    retJVOpen);
                return false;
            }
            if (readcount == 0)
            {
                //cLog.writeLog("[isJVOpen]readcount エラー：" +
                //    retJVOpen);
                return false;
            }
            //_form1.prgJVRead.Maximum = readcount;

            return true;
        }

        public bool isJVRTOpen(string dataspec, string strDate)
        {
            int retJVRTOpen = _form1.axJVLink1.JVRTOpen(dataspec, strDate);
            if (retJVRTOpen != 0)
            {
                return false;
            }

            return true;
        }

        public string loopJVRead(int size, int count, bool isProgress)
        {
            string buff;
            bool isLoopEnd = false;
            do
            {
                //System.Windows.Forms.Application.DoEvents();
                //buff = new string(char.MinValue, size);
                //filename = new string(char.MinValue, count);
                switch (_form1.axJVLink1.JVRead(
                    out buff,
                    out size,
                    out _))
                {
                    case -503:
                        //cLog.writeLog("[loopJVRead] case -503 " +
                        //    filename + "が存在しません。");
                        return "";
                    case -203:
                        //cLog.writeLog("[loopJVRead] case -203 " +
                        //    "JVOpen が行われていません。");
                        return "";
                    case -201:
                        //cLog.writeLog("[loopJVRead] case -201 " +
                        //    "JVInit が行われていません。");
                        return "";
                    case -3:
                        continue;
                    case -1:
                        if (isProgress)
                        {
                            if (_form1.prgJVRead.Value + 1 > _form1.prgJVRead.Maximum)
                                _form1.prgJVRead.Maximum = _form1.prgJVRead.Value + 1;
                            _form1.prgJVRead.Value++;
                        }
                        continue;
                    case 0:
                        return "END";
                    default:
                        isLoopEnd = true;
                        break;
                }
            }
            while (!isLoopEnd);
            return buff;
        }

        public void writeHaitouData(ClassCSV cCSV, JVData_Struct.JV_HR_PAY mHrData, long rowTarget)
        {
            int res;
            string tmp;

            // 単勝配当
            cCSV.setData(rowTarget - 1, 16,
                createPayData(mHrData.PayTansyo[0].Pay,
                mHrData.PayTansyo[0].Umaban));
            if (int.TryParse(mHrData.PayTansyo[1].Pay, out res))
            {
                cCSV.setData(rowTarget + 0, 16,
                createPayData(mHrData.PayTansyo[1].Pay,
                mHrData.PayTansyo[1].Umaban));
                cCSV.setData(rowTarget - 1, 17,
                createPayData(mHrData.PayTansyo[2].Pay,
                mHrData.PayTansyo[2].Umaban));
            }
            // 1着複勝配当
            cCSV.setData(rowTarget + 0, 17,
                createPayData(mHrData.PayFukusyo[0].Pay,
                mHrData.PayFukusyo[0].Umaban));
            cCSV.setData(rowTarget - 1, 18,
                createPayData(mHrData.PayFukusyo[1].Pay,
                mHrData.PayFukusyo[1].Umaban));
            cCSV.setData(rowTarget + 0, 18,
                createPayData(mHrData.PayFukusyo[2].Pay,
                mHrData.PayFukusyo[2].Umaban));
            if (int.TryParse(mHrData.PayFukusyo[3].Pay, out res))
            {
                cCSV.setData(rowTarget - 1, 19,
                createPayData(mHrData.PayFukusyo[3].Pay,
                mHrData.PayFukusyo[3].Umaban));
                cCSV.setData(rowTarget + 0, 19,
                createPayData(mHrData.PayFukusyo[4].Pay,
                mHrData.PayFukusyo[4].Umaban));
            }
            // 枠連配当
            tmp = "0" + mHrData.PayWakuren[0].Umaban.Substring(0, 1) +
                "0" + mHrData.PayWakuren[0].Umaban.Substring(1, 1);
            cCSV.setData(rowTarget - 1, 20,
                createPayData(mHrData.PayWakuren[0].Pay, tmp));
            if (int.TryParse(mHrData.PayWakuren[1].Pay, out res))
            {
                tmp = "0" + mHrData.PayWakuren[1].Umaban.Substring(0, 1) +
                "0" + mHrData.PayWakuren[1].Umaban.Substring(1, 1);
                cCSV.setData(rowTarget + 0, 20,
                createPayData(mHrData.PayWakuren[1].Pay, tmp));
                tmp = "0" + mHrData.PayWakuren[2].Umaban.Substring(0, 1) +
                "0" + mHrData.PayWakuren[2].Umaban.Substring(1, 1);
                cCSV.setData(rowTarget - 1, 21,
                createPayData(mHrData.PayWakuren[2].Pay, tmp));
            }
            // 馬連配当
            cCSV.setData(rowTarget + 0, 21,
                createPayData(mHrData.PayUmaren[0].Pay,
                mHrData.PayUmaren[0].Kumi));
            if (int.TryParse(mHrData.PayUmaren[1].Pay, out res))
            {
                cCSV.setData(rowTarget - 1, 22,
                createPayData(mHrData.PayUmaren[1].Pay,
                mHrData.PayUmaren[1].Kumi));
                cCSV.setData(rowTarget + 0, 22,
                createPayData(mHrData.PayUmaren[2].Pay,
                mHrData.PayUmaren[2].Kumi));
            }
            // 馬単配当
            cCSV.setData(rowTarget - 1, 23,
                createPayData(mHrData.PayUmatan[0].Pay,
                mHrData.PayUmatan[0].Kumi));
            if (int.TryParse(mHrData.PayUmatan[1].Pay, out res))
            {
                cCSV.setData(rowTarget + 0, 23,
                createPayData(mHrData.PayUmatan[1].Pay,
                mHrData.PayUmatan[1].Kumi));
                cCSV.setData(rowTarget - 1, 24,
                createPayData(mHrData.PayUmatan[2].Pay,
                mHrData.PayUmatan[2].Kumi));
                cCSV.setData(rowTarget + 0, 24,
                createPayData(mHrData.PayUmatan[3].Pay,
                mHrData.PayUmatan[3].Kumi));
                cCSV.setData(rowTarget - 1, 25,
                createPayData(mHrData.PayUmatan[4].Pay,
                mHrData.PayUmatan[4].Kumi));
                cCSV.setData(rowTarget - 1, 25,
                createPayData(mHrData.PayUmatan[5].Pay,
                mHrData.PayUmatan[5].Kumi));
            }
            // 3連複配当
            cCSV.setData(rowTarget - 1, 26,
                createPayData(mHrData.PaySanrenpuku[0].Pay,
                mHrData.PaySanrenpuku[0].Kumi));
            if (int.TryParse(mHrData.PaySanrenpuku[1].Pay, out res))
            {
                cCSV.setData(rowTarget + 0, 26,
                createPayData(mHrData.PaySanrenpuku[1].Pay,
                mHrData.PaySanrenpuku[1].Kumi));
                cCSV.setData(rowTarget - 1, 27,
                createPayData(mHrData.PaySanrenpuku[2].Pay,
                mHrData.PaySanrenpuku[2].Kumi));
            }
            // 3連単配当
            cCSV.setData(rowTarget + 0, 27,
                createPayData(mHrData.PaySanrentan[0].Pay,
                mHrData.PaySanrentan[0].Kumi));
            if (int.TryParse(mHrData.PaySanrentan[1].Pay, out res))
            {
                cCSV.setData(rowTarget - 1, 28,
                createPayData(mHrData.PaySanrentan[1].Pay,
                mHrData.PaySanrentan[1].Kumi));
                cCSV.setData(rowTarget + 0, 28,
                createPayData(mHrData.PaySanrentan[2].Pay,
                mHrData.PaySanrentan[2].Kumi));
                cCSV.setData(rowTarget - 1, 29,
                createPayData(mHrData.PaySanrentan[3].Pay,
                mHrData.PaySanrentan[3].Kumi));
                cCSV.setData(rowTarget - 1, 29,
                createPayData(mHrData.PaySanrentan[4].Pay,
                mHrData.PaySanrentan[4].Kumi));
                cCSV.setData(rowTarget + 0, 30,
                createPayData(mHrData.PaySanrentan[5].Pay,
                mHrData.PaySanrentan[5].Kumi));
            }
            // ワイド
            cCSV.setData(rowTarget + 0, 30,
                createPayData(mHrData.PayWide[0].Pay,
                mHrData.PayWide[0].Kumi));
            cCSV.setData(rowTarget - 1, 31,
                createPayData(mHrData.PayWide[1].Pay,
                mHrData.PayWide[1].Kumi));
            cCSV.setData(rowTarget + 0, 31,
                createPayData(mHrData.PayWide[2].Pay,
                mHrData.PayWide[2].Kumi));
            if (int.TryParse(mHrData.PayWide[3].Pay, out res))
            {
                cCSV.setData(rowTarget - 1, 32,
                createPayData(mHrData.PayWide[3].Pay,
                mHrData.PayWide[3].Kumi));
                cCSV.setData(rowTarget + 0, 32,
                createPayData(mHrData.PayWide[4].Pay,
                mHrData.PayWide[4].Kumi));
                cCSV.setData(rowTarget - 1, 33,
                createPayData(mHrData.PayWide[5].Pay,
                mHrData.PayWide[5].Kumi));
                cCSV.setData(rowTarget + 0, 33,
                createPayData(mHrData.PayWide[6].Pay,
                mHrData.PayWide[6].Kumi));
            }
        }

        public string JyoCord(string cvt)
        {
            string ret = "";
            if (cvt == "01")
                ret = "札幌";
            else if (cvt == "02")
                ret = "函館";
            else if (cvt == "03")
                ret = "福島";
            else if (cvt == "04")
                ret = "新潟";
            else if (cvt == "05")
                ret = "東京";
            else if (cvt == "06")
                ret = "中山";
            else if (cvt == "07")
                ret = "中京";
            else if (cvt == "08")
                ret = "京都";
            else if (cvt == "09")
                ret = "阪神";
            else if (cvt == "10")
                ret = "小倉";
            else if (cvt == "30")
                ret = "門別";
            else if (cvt == "31")
                ret = "北見";
            else if (cvt == "32")
                ret = "岩見沢";
            else if (cvt == "33")
                ret = "帯広";
            else if (cvt == "34")
                ret = "旭川";
            else if (cvt == "35")
                ret = "盛岡";
            else if (cvt == "36")
                ret = "水沢";
            else if (cvt == "37")
                ret = "上山";
            else if (cvt == "38")
                ret = "三条";
            else if (cvt == "39")
                ret = "足利";
            else if (cvt == "40")
                ret = "宇都宮";
            else if (cvt == "41")
                ret = "高崎";
            else if (cvt == "42")
                ret = "浦和";
            else if (cvt == "43")
                ret = "船橋";
            else if (cvt == "44")
                ret = "大井";
            else if (cvt == "45")
                ret = "川崎";
            else if (cvt == "46")
                ret = "金沢";
            else if (cvt == "47")
                ret = "笠松";
            else if (cvt == "48")
                ret = "名古屋";
            else if (cvt == "49")
                ret = "紀伊三井寺";
            else if (cvt == "50")
                ret = "園田";
            else if (cvt == "51")
                ret = "姫路";
            else if (cvt == "52")
                ret = "益田";
            else if (cvt == "53")
                ret = "福山";
            else if (cvt == "54")
                ret = "高知";
            else if (cvt == "55")
                ret = "佐賀";
            else if (cvt == "56")
                ret = "荒尾";
            else if (cvt == "57")
                ret = "中津";
            else if (cvt == "58")
                ret = "札幌(地方)";
            else if (cvt == "59")
                ret = "函館(地方)";
            else if (cvt == "60")
                ret = "新潟(地方)";
            else if (cvt == "61")
                ret = "中京(地方)";

            return ret;
        }

        public string JyogyakuCord(string cvt)
        {
            string ret = "";
            if (cvt == "札幌")
                ret = "01";
            else if (cvt == "函館")
                ret = "02";
            else if (cvt == "福島")
                ret = "03";
            else if (cvt == "新潟")
                ret = "04";
            else if (cvt == "東京")
                ret = "05";
            else if (cvt == "中山")
                ret = "06";
            else if (cvt == "中京")
                ret = "07";
            else if (cvt == "京都")
                ret = "08";
            else if (cvt == "阪神")
                ret = "09";
            else if (cvt == "小倉")
                ret = "10";
            else if (cvt == "30")
                ret = "門別";
            else if (cvt == "31")
                ret = "北見";
            else if (cvt == "32")
                ret = "岩見沢";
            else if (cvt == "33")
                ret = "帯広";
            else if (cvt == "34")
                ret = "旭川";
            else if (cvt == "35")
                ret = "盛岡";
            else if (cvt == "36")
                ret = "水沢";
            else if (cvt == "37")
                ret = "上山";
            else if (cvt == "38")
                ret = "三条";
            else if (cvt == "39")
                ret = "足利";
            else if (cvt == "40")
                ret = "宇都宮";
            else if (cvt == "41")
                ret = "高崎";
            else if (cvt == "42")
                ret = "浦和";
            else if (cvt == "43")
                ret = "船橋";
            else if (cvt == "44")
                ret = "大井";
            else if (cvt == "45")
                ret = "川崎";
            else if (cvt == "46")
                ret = "金沢";
            else if (cvt == "47")
                ret = "笠松";
            else if (cvt == "48")
                ret = "名古屋";
            else if (cvt == "49")
                ret = "紀伊三井寺";
            else if (cvt == "50")
                ret = "園田";
            else if (cvt == "51")
                ret = "姫路";
            else if (cvt == "52")
                ret = "益田";
            else if (cvt == "53")
                ret = "福山";
            else if (cvt == "54")
                ret = "高知";
            else if (cvt == "55")
                ret = "佐賀";
            else if (cvt == "56")
                ret = "荒尾";
            else if (cvt == "57")
                ret = "中津";
            else if (cvt == "58")
                ret = "札幌(地方)";
            else if (cvt == "59")
                ret = "函館(地方)";
            else if (cvt == "60")
                ret = "新潟(地方)";
            else if (cvt == "61")
                ret = "中京(地方)";

            return ret;
        }

        public string Jyo2ShortJyo(string cvt)
        {
            string ret = "";
            if (cvt == "札幌")
                ret = "札";
            else if (cvt == "函館")
                ret = "函";
            else if (cvt == "福島")
                ret = "福";
            else if (cvt == "新潟")
                ret = "新";
            else if (cvt == "東京")
                ret = "東";
            else if (cvt == "中山")
                ret = "中";
            else if (cvt == "中京")
                ret = "名";
            else if (cvt == "京都")
                ret = "京";
            else if (cvt == "阪神")
                ret = "阪";
            else if (cvt == "小倉")
                ret = "小";

            return ret;
        }

        public string ShortJyo2Jyo(string cvt)
        {
            string ret = "";
            if (cvt == "札")
                ret = "札幌";
            else if (cvt == "函")
                ret = "函館";
            else if (cvt == "福")
                ret = "福島";
            else if (cvt == "新")
                ret = "新潟";
            else if (cvt == "東")
                ret = "東京";
            else if (cvt == "中")
                ret = "中山";
            else if (cvt == "名")
                ret = "中京";
            else if (cvt == "京")
                ret = "京都";
            else if (cvt == "阪")
                ret = "阪神";
            else if (cvt == "小")
                ret = "小倉";

            return ret;
        }

        public string TenkoCord(string cvt)
        {
            string ret = "";
            if (cvt == "0")
                ret = "未設定";
            else if (cvt == "1")
                ret = "晴";
            else if (cvt == "2")
                ret = "曇";
            else if (cvt == "3")
                ret = "雨";
            else if (cvt == "4")
                ret = "小雨";
            else if (cvt == "5")
                ret = "雪";
            else if (cvt == "6")
                ret = "小雪";
            return ret;
        }

        public string BabaCord(string cvt)
        {
            string ret = "";
            if (cvt == "0")
                ret = "未設定";
            else if (cvt == "1")
                ret = "良";
            else if (cvt == "2")
                ret = "稍重";
            else if (cvt == "3")
                ret = "重";
            else if (cvt == "4")
                ret = "不良";
            return ret;
        }

        public string createPayData(string strPay, string strKumi)
        {
            string ret = "";
            string tmpstrKumi = "";
            if (strPay.Length == 0)
                return ret;
            if (strPay.Replace(" ", "") == "")
                return ret;
            if (strKumi.Length >= 6)
            {
                tmpstrKumi = strKumi.Substring(0, 2) + "・" +
                    strKumi.Substring(2, 2) + "・" +
                    strKumi.Substring(4, 2);
            }
            else if (strKumi.Length >= 4)
            {
                tmpstrKumi = strKumi.Substring(0, 2) + "・" +
                    strKumi.Substring(2, 2);
            }
            else
            {
                tmpstrKumi = strKumi;
            }
            ret = int.Parse(strPay) + "(" + tmpstrKumi + ")";
            return ret;
        }

        public List<clsUmatanOdds> CreateCompositeOdds(
            List<clsUmatanOdds> listUmatanOddsH1,
            List<clsRaceUma> listUmatanOddsO1, 
            List<clsUmatanOdds> listUmatanOdds,
            List<clsOddsSanrentan> listOddsSanrentan)
        {
            int cnt = 0;
            bool isExistHyou = true;
            List<clsUmatanOdds> listRet = new List<clsUmatanOdds>();

            // 人気順を入れる
            foreach (clsRaceUma UmatanOddsO1 in listUmatanOddsO1)
            {
                cnt = 0;
                foreach (clsUmatanOdds UmatanOdds in listUmatanOdds)
                {
                    if (UmatanOdds.Umaban1 == UmatanOddsO1.Umaban)
                        listUmatanOdds[cnt].Ninki1 = UmatanOddsO1.Ninki;
                    if (UmatanOdds.Umaban2 == UmatanOddsO1.Umaban)
                        listUmatanOdds[cnt].Ninki2 = UmatanOddsO1.Ninki;
                    cnt++;
                }
            }

            // 票数1
            if (listUmatanOddsH1.Count == 0)
                isExistHyou = false;
            foreach (clsUmatanOdds UmatanOddsH1 in listUmatanOddsH1)
            {
                cnt = 0;
                foreach (clsUmatanOdds UmatanOdds in listUmatanOdds)
                {
                    if (UmatanOdds.Kumi == UmatanOddsH1.Kumi)
                    {
                        listUmatanOdds[cnt].Hyou = UmatanOddsH1.Hyou;
                        break;
                    }
                    cnt++;
                }
            }

            // 馬単裏
            for (int i = 0; i < listUmatanOdds.Count; i++)
            {
                foreach (clsUmatanOdds UmatanOdds in listUmatanOdds)
                {
                    if (listUmatanOdds[i].Umaban1 == UmatanOdds.Umaban2 &&
                        listUmatanOdds[i].Umaban2 == UmatanOdds.Umaban1)
                    {
                        listUmatanOdds[i].RevOdds = UmatanOdds.Odds;
                        break;
                    }
                }
            }

            // 馬単合成
            for (int i = 0; i < listUmatanOdds.Count; i++)
            {
                List<double> listOddsGousei = new List<double>();
                listOddsGousei.Add(listUmatanOdds[i].Odds);
                listOddsGousei.Add(listUmatanOdds[i].RevOdds);
                double denom = 0;
                for (int j = 0; j < listOddsGousei.Count; j++)
                {
                    denom += 1 / listOddsGousei[j];
                }
                if (denom > 0)
                    listUmatanOdds[i].SyntheticOdds1 = 1 / denom;
            }

            // ３連単オッズ
            if (_form1.checkBox1.Checked)
            {
                for (int i = 0; i < listUmatanOdds.Count; i++)
                {
                    List<double> listOddsGousei = new List<double>();
                    for (int j = 0; j < listOddsSanrentan.Count; j++)
                    {
                        if (listUmatanOdds[i].Umaban1 == listOddsSanrentan[j].Umaban1 &&
                            listUmatanOdds[i].Umaban2 == listOddsSanrentan[j].Umaban2)
                            listOddsGousei.Add(listOddsSanrentan[j].OddsSanrentan);
                    }
                    double denom = 0;
                    for (int j = 0; j < listOddsGousei.Count; j++)
                    {
                        if (listOddsGousei[j] > 0)
                            denom += 1 / listOddsGousei[j];
                    }
                    if (denom > 0)
                        listUmatanOdds[i].SyntheticOdds2 = 1 / denom;
                }
            }

            //ソートする
            listUmatanOdds.Sort((a, b) => b.Umaban1 - a.Umaban1);
            listUmatanOdds.Sort((a, b) => b.Umaban2 - a.Umaban2);
            listUmatanOdds.Sort((a, b) => a.OddsInt - b.OddsInt);

            return listUmatanOdds;

            //CSVに書き込み
            //long rowWrite = 2;
            //foreach (clsUmatanOdds UmatanOdds in listUmatanOdds)
            //{
            //    string strOdds = string.Format("{0:0.0}", UmatanOdds.Odds);
            //    if (strOdds.Substring(strOdds.Length - 1, 1) == "0")
            //        strOdds = ((int)UmatanOdds.Odds).ToString();
            //    string strRevOdds = string.Format("{0:0.0}", UmatanOdds.RevOdds);
            //    if (strRevOdds.Substring(strRevOdds.Length - 1, 1) == "0")
            //        strRevOdds = ((int)UmatanOdds.RevOdds).ToString();
            //    string strSyntheticOdds1 = string.Format("{0:0.0}", UmatanOdds.SyntheticOdds1);
            //    if (strSyntheticOdds1.Substring(strSyntheticOdds1.Length - 1, 1) == "0")
            //        strSyntheticOdds1 = Convert.ToInt32(UmatanOdds.SyntheticOdds1).ToString();
                
            //    cCSV.setData(rowWrite, colT, UmatanOdds.Umaban1.ToString());
            //    cCSV.setData(rowWrite, 2, UmatanOdds.Umaban2.ToString());
            //    cCSV.setData(rowWrite, 3, strOdds);
            //    cCSV.setData(rowWrite, 4, UmatanOdds.Ninki1.ToString());
            //    cCSV.setData(rowWrite, 5, UmatanOdds.Ninki2.ToString());
            //    if (isExistHyou)
            //        cCSV.setData(rowWrite, 6, UmatanOdds.Hyou.ToString());
            //    else
            //        cCSV.setData(rowWrite, 6, "");
            //    cCSV.setData(rowWrite, 7, strRevOdds);
            //    cCSV.setData(rowWrite, 8, strSyntheticOdds1);
            //    if (_form1.checkBox1.Checked)
            //    {
            //        string strSyntheticOdds2 = string.Format("{0:0.0}", UmatanOdds.SyntheticOdds2);
            //        if (strSyntheticOdds2.Substring(strSyntheticOdds2.Length - 1, 1) == "0")
            //            strSyntheticOdds2 = Convert.ToInt32(UmatanOdds.SyntheticOdds2).ToString();
            //        cCSV.setData(rowWrite, 9, strSyntheticOdds2);
            //    }

            //    rowWrite++;
            //}
        }

        public List<clsUmatanOdds> setDataH1(string retbuff,
            string strDateTarg, string placeTarg, string racenumTarg)
        {
            List<clsUmatanOdds> ret = new List<clsUmatanOdds>();
            JVData_Struct.JV_H1_HYOSU_ZENKAKE mH1Data =
                new JVData_Struct.JV_H1_HYOSU_ZENKAKE();
            mH1Data.SetDataB(ref retbuff);

            string strJyo = JyoCord(mH1Data.id.JyoCD);
            if (!(strDateTarg == mH1Data.id.Year + mH1Data.id.MonthDay &&
                placeTarg == strJyo &&
                mH1Data.id.RaceNum == racenumTarg))
                return null;

            for (int i = 0; i < 305; i++)
            {
                if (mH1Data.HyoUmatan[i].Kumi.Trim() == "")
                    continue;
                clsUmatanOdds cUmatanOdds = new clsUmatanOdds();
                cUmatanOdds.Kumi = mH1Data.HyoUmatan[i].Kumi;
                cUmatanOdds.Hyou = int.Parse(mH1Data.HyoUmatan[i].Hyo);
                ret.Add(cUmatanOdds);
            }
            return ret;
        }

        public List<clsRaceUma> setDataO1(string retbuff, string strDateTarg, string placeTarg, string racenumTarg)
        {
            List<clsRaceUma> ret = new List<clsRaceUma>();
            JVData_Struct.JV_O1_ODDS_TANFUKUWAKU mO1Data =
                new JVData_Struct.JV_O1_ODDS_TANFUKUWAKU();
            mO1Data.SetDataB(ref retbuff);

            string strJyo = JyoCord(mO1Data.id.JyoCD);
            if (!(strDateTarg == mO1Data.id.Year + mO1Data.id.MonthDay &&
               placeTarg == strJyo &&
               mO1Data.id.RaceNum == racenumTarg))
                return null;

            for (int i = 0; i < 18; i++)
            {
                if (mO1Data.OddsTansyoInfo[i].Umaban.Trim() == "")
                    continue;
                if (mO1Data.OddsTansyoInfo[i].Ninki.Contains("*"))
                    continue;
                if (mO1Data.OddsTansyoInfo[i].Ninki.Contains("-"))
                    continue;
                clsRaceUma cRaceUma = new clsRaceUma();
                cRaceUma.Umaban = int.Parse(mO1Data.OddsTansyoInfo[i].Umaban);
                cRaceUma.Ninki = int.Parse(mO1Data.OddsTansyoInfo[i].Ninki);
                ret.Add(cRaceUma);
            }
            return ret;
        }

        public List<clsUmatanOdds> setDataO4(string retbuff, string strDateTarg, string placeTarg, string racenumTarg)
        {
            List<clsUmatanOdds> ret = new List<clsUmatanOdds>();
            JVData_Struct.JV_O4_ODDS_UMATAN mO4Data =
                new JVData_Struct.JV_O4_ODDS_UMATAN();
            mO4Data.SetDataB(ref retbuff);

            string strJyo = JyoCord(mO4Data.id.JyoCD);
            if (!(strDateTarg == mO4Data.id.Year + mO4Data.id.MonthDay &&
               placeTarg == strJyo &&
               mO4Data.id.RaceNum == racenumTarg))
                return null;

            int cnt = 0;
            for (int i = 0; i < 306; i++)
            {
                if (mO4Data.OddsUmatanInfo[i].Kumi.Trim() == "" ||
                    mO4Data.OddsUmatanInfo[i].Odds.Trim() == "" ||
                    mO4Data.OddsUmatanInfo[i].Odds.Contains("-") ||
                    mO4Data.OddsUmatanInfo[i].Odds.Contains("*"))
                    continue;
                if (int.Parse(mO4Data.OddsUmatanInfo[i].Odds) == 0)
                    continue;
                clsUmatanOdds cUmatanOdds = new clsUmatanOdds();
                cUmatanOdds.Kumi = mO4Data.OddsUmatanInfo[i].Kumi;
                cUmatanOdds.Umaban1 = int.Parse(mO4Data.OddsUmatanInfo[i].Kumi.Substring(0, 2));
                cUmatanOdds.Umaban2 = int.Parse(mO4Data.OddsUmatanInfo[i].Kumi.Substring(2, 2));
                cUmatanOdds.Odds = (double)int.Parse(mO4Data.OddsUmatanInfo[i].Odds) / 10;
                cnt = 0;
                foreach (clsUmatanOdds ele in ret)
                {
                    if (ele.Odds == cUmatanOdds.Odds)
                        cnt++;
                }
                cUmatanOdds.OddsInt = int.Parse(mO4Data.OddsUmatanInfo[i].Odds +
                    string.Format("{0:D3}", cnt));
                ret.Add(cUmatanOdds);
            }
            return ret;
        }

        public List<clsOddsSanrentan> setDataO6(string retbuff, string strDateTarg, string placeTarg,
            string racenumTarg)
        {
            List<clsOddsSanrentan> ret = new List<clsOddsSanrentan>();

            DateTime dateTime;
            JVData_Struct.JV_O6_ODDS_SANRENTAN mO6Data =
                new JVData_Struct.JV_O6_ODDS_SANRENTAN();

            mO6Data.SetDataB(ref retbuff);
            dateTime = DateTime.Parse(
            (mO6Data.id.Year +
            mO6Data.id.MonthDay).Insert(4, "/").Insert(7, "/"));
            if (dateTime > DateTime.Parse(strDateTarg.Insert(4, "/").Insert(7, "/")))
                return null;

            string strJyo = JyoCord(mO6Data.id.JyoCD);
            if (!(strDateTarg == mO6Data.id.Year + mO6Data.id.MonthDay &&
               placeTarg == strJyo &&
               mO6Data.id.RaceNum == racenumTarg))
                return null;

            for (int i = 0; i < 4896; i++)
            {
                if (mO6Data.OddsSanrentanInfo[i].Kumi.Trim() == "" ||
                    mO6Data.OddsSanrentanInfo[i].Odds.Trim() == "" ||
                    mO6Data.OddsSanrentanInfo[i].Odds.Contains("-") ||
                    mO6Data.OddsSanrentanInfo[i].Odds.Contains("*"))
                    continue;
                if (int.Parse(mO6Data.OddsSanrentanInfo[i].Odds) == 0)
                    continue;
                clsOddsSanrentan cOddsSanrentan = new clsOddsSanrentan();
                cOddsSanrentan.Kumi = mO6Data.OddsSanrentanInfo[i].Kumi;
                cOddsSanrentan.Umaban1 = int.Parse(mO6Data.OddsSanrentanInfo[i].Kumi.Substring(0, 2));
                cOddsSanrentan.Umaban2 = int.Parse(mO6Data.OddsSanrentanInfo[i].Kumi.Substring(2, 2));
                cOddsSanrentan.Umaban3 = int.Parse(mO6Data.OddsSanrentanInfo[i].Kumi.Substring(4, 2));
                cOddsSanrentan.OddsSanrentan = (double)int.Parse(mO6Data.OddsSanrentanInfo[i].Odds) / 10;
                ret.Add(cOddsSanrentan);
            }

            return ret;
        }



    }
}
