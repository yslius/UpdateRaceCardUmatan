using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpdateRaceCardUmatan
{
    public class UmatanOddsStock
    {
        Form1 _form1;
        private clsCodeConv objCodeConv;
        private ClassLog cLog;
        int size = 0;
        int count = 0;
        clcCommon cCommon;
        public List<clsUmatanOdds> listUmatanOddsH1;
        public List<clsRaceUma> listUmatanOddsO1;
        public List<clsUmatanOdds> listUmatanOdds;
        public List<clsOddsSanrentan> listOddsSanrentan;

        public UmatanOddsStock(clcCommon cCommon1, Form1 form1)
        {
            _form1 = form1;
            cCommon = cCommon1;
            cLog = new ClassLog();
            this.objCodeConv = new clsCodeConv();
        }

        public List<clsUmatanOdds> GetStockDataDetailData(
            string strDateTarg, string placeTarg, string racenumTarg)
        {
            List<clsUmatanOdds> listRet = new List<clsUmatanOdds>();
            //データ取得する
            _form1.axJVLink1.JVClose();
            if (cCommon.checkInit() != 0)
                return listRet;

            if (GetStockDataDetailData1(strDateTarg, placeTarg, racenumTarg) < 0)
            {
                _form1.axJVLink1.JVClose();
                return listRet;
            }
            //_form1.prgDownload.Value = 51;
            //_form1.prgDownload.Value--;


            //計算する
            listRet = cCommon.CreateCompositeOdds(
                listUmatanOddsH1, listUmatanOddsO1,
                listUmatanOdds, listOddsSanrentan);

            //_form1.prgDownload.Maximum++;
            //_form1.prgDownload.Value = _form1.prgDownload.Maximum;
            //_form1.prgDownload.Maximum--;

            return listRet;
        }

        int GetStockDataDetailData1(string strDateTarg, string placeTarg, string racenumTarg)
        {
            DateTime datetimeTarg;
            DateTime dateTime;
            datetimeTarg = DateTime.Parse(strDateTarg.Insert(4, "/").Insert(7, "/"));

            TimeSpan timeSpan = new TimeSpan(1, 0, 0, 0);
            string strDate =
                (datetimeTarg - timeSpan).ToString("yyyyMMdd");
            bool isFind = false;
            string retbuff;
            long cntLoop = 0;

            size = 840000;
            count = 256;
            int option = DateTime.Now >
                datetimeTarg.AddYears(1) ? 4 : 1;
            _form1.prgJVRead.Value = 0;
            if (!cCommon.isJVOpen("RACE", strDate, option))
            {
                return -1;
            }
            do
            {
                retbuff = cCommon.loopJVRead(size, count, true);
                if (retbuff == "" || retbuff == "END")
                    break;
                Console.WriteLine(retbuff.Substring(11, 8).Insert(4, "/").Insert(7, "/"));
                dateTime = DateTime.Parse(retbuff.Substring(11, 8).Insert(4, "/").Insert(7, "/"));
                if (isFind && dateTime > datetimeTarg)
                    break;
                //票数1
                if (retbuff.Substring(0, 2) == "H1")
                {
                    //setDataH1(retbuff, strDateTarg, placeTarg, racenumTarg);
                    if(listUmatanOddsH1 == null)
                        listUmatanOddsH1 = cCommon.setDataH1(retbuff, strDateTarg,
                            placeTarg, racenumTarg);
                }
                //オッズ（単複枠）
                if (retbuff.Substring(0, 2) == "O1")
                {
                    //setDataO1(retbuff, strDateTarg, placeTarg, racenumTarg);
                    if (listUmatanOddsO1 == null)
                        listUmatanOddsO1 = cCommon.setDataO1(retbuff, strDateTarg,
                            placeTarg, racenumTarg);
                }
                //オッズ（馬単）
                if (retbuff.Substring(0, 2) == "O4")
                {
                    //setDataO4(retbuff, strDateTarg, placeTarg, racenumTarg);
                    if (listUmatanOdds == null)
                        listUmatanOdds = cCommon.setDataO4(retbuff, strDateTarg,
                            placeTarg, racenumTarg);
                }
                //３連単オッズ
                if (retbuff.Substring(0, 2) == "O6")
                {
                    //if (setDataO6(retbuff, strDateTarg, placeTarg, racenumTarg))
                    //    isFind = true;
                    if (listOddsSanrentan == null)
                        listOddsSanrentan = cCommon.setDataO6(retbuff, strDateTarg,
                        placeTarg, racenumTarg);
                    if(listOddsSanrentan != null)
                        isFind = true;
                }
                cntLoop++;
            }
            while (cntLoop <= 100000);
            _form1.prgJVRead.Maximum++;
            _form1.prgJVRead.Value =
                _form1.prgJVRead.Maximum;
            _form1.prgJVRead.Maximum--;

            int retJVClose = _form1.axJVLink1.JVClose();
            if (retJVClose != 0)
            {
                cLog.writeLog("[GetStockDataDetailData1]JVClose エラー：" +
                    retJVClose);
            }

            if (listUmatanOddsH1 == null ||
                listUmatanOddsO1 == null ||
                listUmatanOdds == null ||
                listOddsSanrentan == null)
                return -1;

            if (listUmatanOddsH1.Count == 0 ||
                listUmatanOddsO1.Count == 0 ||
                listUmatanOdds.Count == 0 ||
                listOddsSanrentan.Count == 0)
                return -1;

            return 1;
        }

        void setDataH1(string retbuff, string strDateTarg, string placeTarg, string racenumTarg)
        {
            JVData_Struct.JV_H1_HYOSU_ZENKAKE mH1Data =
                new JVData_Struct.JV_H1_HYOSU_ZENKAKE();
            mH1Data.SetDataB(ref retbuff);

            string strJyo = cCommon.JyoCord(mH1Data.id.JyoCD);
            if (!(strDateTarg == mH1Data.id.Year + mH1Data.id.MonthDay &&
               placeTarg == strJyo &&
               mH1Data.id.RaceNum == racenumTarg))
                return;

            for(int i = 0; i < 305; i++)
            {
                if (mH1Data.HyoUmatan[i].Kumi.Trim() == "")
                    continue;
                clsUmatanOdds cUmatanOdds = new clsUmatanOdds();
                cUmatanOdds.Kumi = mH1Data.HyoUmatan[i].Kumi;
                cUmatanOdds.Hyou = int.Parse(mH1Data.HyoUmatan[i].Hyo);
                listUmatanOddsH1.Add(cUmatanOdds);
            }
        }

        void setDataO1(string retbuff, string strDateTarg, string placeTarg, string racenumTarg)
        {
            JVData_Struct.JV_O1_ODDS_TANFUKUWAKU mO1Data =
                new JVData_Struct.JV_O1_ODDS_TANFUKUWAKU();
            mO1Data.SetDataB(ref retbuff);

            string strJyo = cCommon.JyoCord(mO1Data.id.JyoCD);
            if (!(strDateTarg == mO1Data.id.Year + mO1Data.id.MonthDay &&
               placeTarg == strJyo &&
               mO1Data.id.RaceNum == racenumTarg))
                return;

            for (int i = 0; i < 18; i++)
            {
                if (mO1Data.OddsTansyoInfo[i].Umaban.Trim() == "")
                    continue;
                clsRaceUma cRaceUma = new clsRaceUma();
                cRaceUma.Umaban = int.Parse(mO1Data.OddsTansyoInfo[i].Umaban);
                cRaceUma.Ninki = int.Parse(mO1Data.OddsTansyoInfo[i].Ninki);
                listUmatanOddsO1.Add(cRaceUma);
            }
        }

        void setDataO4(string retbuff, string strDateTarg, string placeTarg, string racenumTarg)
        {
            JVData_Struct.JV_O4_ODDS_UMATAN mO4Data =
                new JVData_Struct.JV_O4_ODDS_UMATAN();
            mO4Data.SetDataB(ref retbuff);

            string strJyo = cCommon.JyoCord(mO4Data.id.JyoCD);
            if (!(strDateTarg == mO4Data.id.Year + mO4Data.id.MonthDay &&
               placeTarg == strJyo &&
               mO4Data.id.RaceNum == racenumTarg))
                return;

            for (int i = 0; i < 306; i++)
            {
                if (mO4Data.OddsUmatanInfo[i].Kumi.Trim() == "" ||
                    mO4Data.OddsUmatanInfo[i].Odds.Trim() == "" ||
                    int.Parse(mO4Data.OddsUmatanInfo[i].Odds) == 0 ||
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
                cUmatanOdds.OddsInt = int.Parse(mO4Data.OddsUmatanInfo[i].Odds);
                listUmatanOdds.Add(cUmatanOdds);
            }
        }

        bool setDataO6(string retbuff, string strDateTarg, string placeTarg, 
            string racenumTarg)
        {
            DateTime dateTime;
            JVData_Struct.JV_O6_ODDS_SANRENTAN mO6Data =
                new JVData_Struct.JV_O6_ODDS_SANRENTAN();

            mO6Data.SetDataB(ref retbuff);
            dateTime = DateTime.Parse(
            (mO6Data.id.Year +
            mO6Data.id.MonthDay).Insert(4, "/").Insert(7, "/"));
            if (dateTime > DateTime.Parse(strDateTarg.Insert(4, "/").Insert(7, "/")))
                return false;

            string strJyo = cCommon.JyoCord(mO6Data.id.JyoCD);
            if (!(strDateTarg == mO6Data.id.Year + mO6Data.id.MonthDay &&
               placeTarg == strJyo &&
               mO6Data.id.RaceNum == racenumTarg))
                return false;

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
                //cOddsSanrentan.OddsSanrentan = string.Format("{0:0.0}",
                //    int.Parse(mO6Data.OddsSanrentanInfo[i].Odds));
                cOddsSanrentan.OddsSanrentan = int.Parse(mO6Data.OddsSanrentanInfo[i].Odds) / 10;
                listOddsSanrentan.Add(cOddsSanrentan);
            }

            return true;
        }
    }
}
