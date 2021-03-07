﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UpdateRaceCardUmatan
{
    public partial class Form1 : Form
    {

        private OperateForm cOperateForm;
        clcCommon cCommon;
        private ClassLog cLog = new ClassLog();
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            cLog.writeLog("Form1_Load " + this.Text);

            cCommon = new clcCommon(this);
            if (cCommon.checkInit() != 0)
            {
                //return;
            }

            cOperateForm = new OperateForm(this);

        }

        private void mnuConfJV_Click(object sender, EventArgs e)
        {
            cLog.writeLog("mnuConfJV_Click");
            cCommon.callMenu();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            cLog.writeLog("button1_Click");
            prgJVRead.Value = 0;
            prgDownload.Value = 0;
            cOperateForm.readDate();
        }

        private void btnGetJVData_Click(object sender, EventArgs e)
        {
            //var sw = new System.Diagnostics.Stopwatch();
            //sw.Start();
            cLog.writeLog("btnGetJVData_Click");

            if (this.textBox1.Text == "")
            {
                System.Media.SystemSounds.Asterisk.Play();
                MessageBox.Show("出馬表を格納しているフォルダを選択してください。");
                cOperateForm.enableButton();
                return;
            }

            // メイン処理
            clcRaceCardUmatan cRaceCard = 
                new clcRaceCardUmatan(cCommon, cOperateForm, this);
            cRaceCard.update(textBox1.Text);

            //sw.Stop();
            //TimeSpan ts = sw.Elapsed;
            //rtbData.Text = $"{ts}";

        }

    }
}
