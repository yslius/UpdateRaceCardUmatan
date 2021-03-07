using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using System.Threading;

namespace UpdateRaceCardUmatan
{
    class clcRaceCardUmatan
    {
        Form1 _form1;
        private OperateForm cOperateForm;
        private ClassLog cLog;
        private UmatanOddsStock cUmatanOddsStock;
        private UmatanOddsRT cUmatanOddsRT;
        ClassCSV cCSV;
        clcCommon _cCommon;

        public clcRaceCardUmatan(clcCommon cCommon, OperateForm cOperateForm1,
            Form1 form1)
        {
            _form1 = form1;
            cOperateForm = cOperateForm1;
            cLog = new ClassLog();
            cUmatanOddsRT = new UmatanOddsRT(cCommon, form1);
            cUmatanOddsStock = new UmatanOddsStock(cCommon, form1);
            cCSV = new ClassCSV();
            _cCommon = cCommon;
        }

        public void update(string pathTarg)
        {
            cLog.writeLog("update");
            cOperateForm.disableButton();

            DateTime datetimeTarg;
            string pathFileR;
            bool isRealTime = false;

            // 出馬表の読み込み
            pathFileR = GetRaceCardFile(pathTarg);
            if (pathFileR == "")
            {
                cOperateForm.enableButton();
                return;
            }
            var encoding = Encoding.GetEncoding("shift_jis");
            cCSV.linedataCsvAll = File.ReadAllLines(pathFileR, encoding);
            cCSV.createCSVarrdata();
            //CSVの最大幅を把握するために、レース数取得
            int cntRace = getRaceNum();
            int colMax = 53 + cntRace * 10;
            cCSV.createCSVarrdata(colMax);

            _form1.prgDownload.Maximum = cntRace+1;
            _form1.prgDownload.Value = 0;

            // 日付の変換
            datetimeTarg = DateTime.Parse(cCSV.getData(2, 1));
            string strDateTarg = datetimeTarg.ToString("yyyyMMdd");

            // 速報系か蓄積系か判断
            int retval = checkJVRTOpen(strDateTarg);
            if (retval < -1)
            {
                _form1.rtbData.Text = string.Format("エラー {}", retval);
                return;
            }
            if (retval > -1)
                isRealTime = true;

            // 馬単オッズの取得
            loopGetUmatannOdds(strDateTarg, isRealTime);

            // ファイル出力
            cCSV.createCSVdataAll();
            File.WriteAllText(pathFileR, cCSV.dataCsvAll, encoding);

            _form1.rtbData.Text = datetimeTarg.ToShortDateString() +
                " 出馬表更新完了しました。";
            
            // 終了処理
            _form1.axJVLink1.JVClose();
            System.Media.SystemSounds.Asterisk.Play();
            cOperateForm.enableButton();
        }

        void loopGetUmatannOdds(string strDateTarg, bool isRealTime)
        {
            string placeTarg;
            string racenumTarg;
            long colT;

            // 繰り返し処理
            long rowTarget = 2;
            colT = 53;
            while (rowTarget < cCSV.getDataMaxRow())
            {
                // 場所とレース番号取得
                placeTarg = cCSV.getData(rowTarget, 3).Substring(1,1);
                placeTarg = _cCommon.ShortJyo2Jyo(placeTarg);
                if(placeTarg == "")
                {
                    MessageBox.Show("出馬表が読み取れません", "エラー",
                    MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    return;
                }
                racenumTarg =cCSV.getData(rowTarget, 6);
                racenumTarg = racenumTarg.PadLeft(2, '0');
                //racenumTarg = string.Format("{D2}", racenumTarg);

                _form1.rtbData.Text = placeTarg + racenumTarg +
                    " 取得中";
                _form1.prgJVRead.Value = 0;
                _form1.Refresh();

                // ヘッダの書き込み
                cCSV.setData(1, colT,
                    placeTarg + Strings.StrConv(racenumTarg, VbStrConv.Wide));
                // 各レースの取得
                getUmatanOddsEachRace(strDateTarg, placeTarg, racenumTarg,
                    isRealTime, colT);

                _form1.prgDownload.Value += 2;
                _form1.prgDownload.Value--;
                rowTarget += long.Parse(cCSV.getData(rowTarget, 4)) + 3;
                colT += 10;
                //sleepがないとPC再起動のエラーになる。
                Thread.Sleep(1000);
            }
            _form1.prgDownload.Value =
                _form1.prgDownload.Maximum;
            _form1.prgDownload.Maximum--;

        }

        public void getUmatanOddsEachRace(string strDateTarg, 
            string placeTarg, string racenumTarg, 
            bool isRealTime, long colT)
        {
            List<clsUmatanOdds> listUmatanOdds = new List<clsUmatanOdds>();

            cLog.writeLog("getUmatanOddsAll");
            
            // 追加項目を記入
            writeHeadData(cCSV, colT);
            
            if (isRealTime)
            {
                listUmatanOdds = cUmatanOddsRT.GetRTDataDetailData(
                    strDateTarg, placeTarg, racenumTarg);
            }
            else
            {
                listUmatanOdds = cUmatanOddsStock.GetStockDataDetailData(
                    strDateTarg, placeTarg, racenumTarg);
            }

            //racenumTarg = Strings.StrConv(racenumTarg, VbStrConv.Wide);

            // 票数がなければ空欄としたいため、フラグを作る
            bool isExistHyou = false;
            foreach (clsUmatanOdds UmatanOdds in listUmatanOdds)
            {
                if (UmatanOdds.Hyou > 0)
                {
                    isExistHyou = true;
                    break;
                }
            }

            long rowWrite = 3;
            foreach (clsUmatanOdds UmatanOdds in listUmatanOdds)
            {
                string strOdds = string.Format("{0:0.0}", UmatanOdds.Odds);
                if (strOdds.Substring(strOdds.Length - 1, 1) == "0")
                    strOdds = ((int)UmatanOdds.Odds).ToString();
                string strRevOdds = string.Format("{0:0.0}", UmatanOdds.RevOdds);
                if (strRevOdds.Substring(strRevOdds.Length - 1, 1) == "0")
                    strRevOdds = ((int)UmatanOdds.RevOdds).ToString();
                string strSyntheticOdds1 = string.Format("{0:0.0}", UmatanOdds.SyntheticOdds1);
                if (strSyntheticOdds1.Substring(strSyntheticOdds1.Length - 1, 1) == "0")
                    strSyntheticOdds1 = Convert.ToInt32(UmatanOdds.SyntheticOdds1).ToString();

                cCSV.setData(rowWrite, colT + 0, UmatanOdds.Umaban1.ToString());
                cCSV.setData(rowWrite, colT + 1, UmatanOdds.Umaban2.ToString());
                cCSV.setData(rowWrite, colT + 2, strOdds);
                cCSV.setData(rowWrite, colT + 3, UmatanOdds.Ninki1.ToString());
                cCSV.setData(rowWrite, colT + 4, UmatanOdds.Ninki2.ToString());
                if (isExistHyou)
                    cCSV.setData(rowWrite, colT + 5, UmatanOdds.Hyou.ToString());
                else
                    cCSV.setData(rowWrite, colT + 5, "");
                cCSV.setData(rowWrite, colT + 6, strRevOdds);
                cCSV.setData(rowWrite, colT + 7, strSyntheticOdds1);
                if (_form1.checkBox1.Checked)
                {
                    string strSyntheticOdds2 = string.Format("{0:0.0}", UmatanOdds.SyntheticOdds2);
                    if (strSyntheticOdds2.Substring(strSyntheticOdds2.Length - 1, 1) == "0")
                        strSyntheticOdds2 = Convert.ToInt32(UmatanOdds.SyntheticOdds2).ToString();
                    cCSV.setData(rowWrite, colT + 8, strSyntheticOdds2);
                }

                rowWrite++;
            }
        }

        public string GetRaceCardFile(string pathTarg)
        {
            string path = pathTarg + "01出馬表.csv";
            if (!File.Exists(path))
            {
                MessageBox.Show("出馬表が見つかりません。", "エラー",
                    MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return "";
            }
            return path;
        }

        int getRaceNum()
        {
            int cntRace = 0;
            long rowTarget = 2;
            while (rowTarget < cCSV.getDataMaxRow())
            {
                cntRace += 1;
                rowTarget += long.Parse(cCSV.getData(rowTarget, 4)) + 3;
            }
            return cntRace;
        }

        private int checkJVRTOpen(string strDateTarg)
        {
            string dataspec = "0B14";

            int ret = _form1.axJVLink1.JVClose();
            if (_form1.axJVLink1.JVClose() != 0)
                MessageBox.Show("JVClose エラー：" + ret);

            ret = _form1.axJVLink1.JVRTOpen(dataspec, strDateTarg);

            return ret;
        }

        void writeHeadData(ClassCSV cCSV, long colT)
        {
            long rowTarget = 2;
            cCSV.setData(rowTarget, colT + 0, "目1");
            cCSV.setData(rowTarget, colT + 1, "目2");
            cCSV.setData(rowTarget, colT + 2, "馬単オッズ");
            cCSV.setData(rowTarget, colT + 3, "人気1");
            cCSV.setData(rowTarget, colT + 4, "人気2");
            cCSV.setData(rowTarget, colT + 5, "馬単票数");
            cCSV.setData(rowTarget, colT + 6, "馬単裏");
            cCSV.setData(rowTarget, colT + 7, "馬単合成");
            cCSV.setData(rowTarget, colT + 8, "3連単1・2着軸総流し");
        }

    }
}
