using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsForms.IMEHelper.Core
{
    public partial class Form1 : Form
    {
        IMEHandler imeHandler;
        string inputContent = string.Empty;

        const int UnicodeSimplifiedChineseMin = 0x4E00;
        const int UnicodeSimplifiedChineseMax = 0x9FA5;
        const string DefaultChar = "?";

        public static TSF TSF;


        public Form1()
        {
            InitializeComponent();

            imeHandler = new IMEHandler(this, false);
            Application.Idle += Application_Idle;

            imeHandler.ResultReceived += (s, e) =>
            {
                switch ((int)e.Result)
                {
                    case 8:
                        if (inputContent.Length > 0)
                            inputContent = inputContent.Remove(inputContent.Length - 1, 1);
                        break;
                    case 27:
                    case 13:
                        inputContent = "";
                        break;
                    default:
                        if (e.Result > UnicodeSimplifiedChineseMax)
                            inputContent += DefaultChar;
                        else
                            inputContent += e.Result;
                        break;
                }

                textBoxResult.Text = inputContent;
            };

            this.KeyDown += Form1_KeyDown;

            this.CenterToScreen();
            textBoxResult.Text = string.Empty;

            TSF = new TSF();//Need to init at STA Thread
            TSF.AssociateFocus(Handle);//Window need to create in a STA Thread
            TSF.Active();
            TSF.CreateContext(Handle);
            TSF.PushContext();
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            TSF.PumpMsg(Handle);
            FakeDraw();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1)
            {
                imeHandler.Enabled = !imeHandler.Enabled;
            }
        }

        private void FakeDraw()
        {
            Color compColor = Color.White;

            var compString = string.Empty;
            for (int i = 0; i < imeHandler.Composition.Length; i++)
            {
                string val = imeHandler.Composition[i].ToString();
                //switch (imeHandler.GetCompositionAttr(i))
                //{
                //    case CompositionAttributes.Converted: compColor = Color.LightGreen; break;
                //    case CompositionAttributes.FixedConverted: compColor = Color.Gray; break;
                //    case CompositionAttributes.Input: compColor = Color.Orange; break;
                //    case CompositionAttributes.InputError: compColor = Color.Red; break;
                //    case CompositionAttributes.TargetConverted: compColor = Color.Yellow; break;
                //    case CompositionAttributes.TargetNotConverted: compColor = Color.SkyBlue; break;
                //}

                if (val[0] > UnicodeSimplifiedChineseMax)
                    val = DefaultChar;

                compString += val;
            }

            labelComp.Text = compString;

            var candidatesList = string.Empty;
            for (uint i = imeHandler.CandidatesPageStart;
                i < Math.Min(imeHandler.CandidatesPageStart + imeHandler.CandidatesPageSize, imeHandler.Candidates.Length);
                i++)
            {
                if (imeHandler.Candidates[i][0] > UnicodeSimplifiedChineseMax)
                    imeHandler.Candidates[i] = DefaultChar;

                candidatesList += string.Format("{0}.{1}\r\n", i + 1 - imeHandler.CandidatesPageStart, imeHandler.Candidates[i]);
            }

            textBoxCandidates.Text = candidatesList;
        }
    }
}
