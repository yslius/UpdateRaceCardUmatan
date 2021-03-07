using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using Microsoft.VisualBasic;

namespace UpdateRaceCardUmatan
{
    class clcRaceCardUmatan
    {
        Form1 _form1;
        private OperateForm cOperateForm;
        private ClassLog cLog;
        ClassCSV cCSV;

        public clcRaceCardUmatan(clcCommon cCommon, OperateForm cOperateForm1,
            Form1 form1)
        {
            _form1 = form1;
            cOperateForm = cOperateForm1;
            cLog = new ClassLog();
            cCSV = new ClassCSV();
        }

        public void update(string pathTarg)
        {
            cLog.writeLog("update");
            cOperateForm.disableButton();

            DateTime datetimeTarg;
            
            string pathFileR;

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

            // 日付の変換
            datetimeTarg = DateTime.Parse(cCSV.getData(2, 1));

            // 馬単オッズの取得
            loopGetUmatannOdds();

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

        void loopGetUmatannOdds(DateTime datetimeTarg)
        {
            string strDateTarg = datetimeTarg.ToString("yyyyMMdd");
            bool isRealTime = false;

            // 速報系か蓄積系か判断
            int retval = checkJVRTOpen(strDateTarg);
            if (retval < -1)
                return;
            if (retval == -1)
                isRealTime = true;

        }

        public void getUmatanOddsEachRace(string strDateTarg, 
            string placeTarg, string racenumTarg, bool isRealTime)
        {
            cLog.writeLog("getUmatanOddsAll");
            
            string pathTarg;

            
            // 追加項目を記入
            writeHeadData(cCSV);
            

            if (isRealTime)
            {
                cUmatanOddsStock.GetStockDataDetailData(cCSV, strDateTarg, placeTarg, racenumTarg);
            }
            else
            {
                cUmatanOddsRT.GetRTDataDetailData(cCSV, strDateTarg, placeTarg, racenumTarg);
            }

            // CSVに追記
            racenumTarg = Strings.StrConv(racenumTarg, VbStrConv.Wide);
            

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

        void writeHeadData(ClassCSV cCSV)
        {
            long rowTarget = 1;
            cCSV.setData(rowTarget, 1, "目1");
            cCSV.setData(rowTarget, 2, "目2");
            cCSV.setData(rowTarget, 3, "馬単オッズ");
            cCSV.setData(rowTarget, 4, "人気1");
            cCSV.setData(rowTarget, 5, "人気2");
            cCSV.setData(rowTarget, 6, "馬単票数");
            cCSV.setData(rowTarget, 7, "馬単裏");
            cCSV.setData(rowTarget, 8, "馬単合成");
            cCSV.setData(rowTarget, 9, "3連単1・2着軸総流し");
        }

    }
}
