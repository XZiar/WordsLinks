using System;
using System.Diagnostics;
using System.Linq;
using WordsLinks.Service;
using WordsLinks.Util;
using WordsLinks.Widget;
using Xamarin.Forms;

namespace WordsLinks.View
{
    public partial class MemorizePage : ContentPage
    {
        private bool mode = true;
        private FrameEx[] answers;
        private FrameEx quest;
        private Quiz curQuiz;
        public MemorizePage()
        {
            InitializeComponent();

            var tapReg = new TapGestureRecognizer();
            tapReg.Tapped += OnClickAnswer;
            foreach (var ele in quizLayout.Children)
                ele.GestureRecognizers.Add(tapReg);

            quest = quizLayout.Children[0] as FrameEx;
            answers = quizLayout.Children.Skip(1).Cast<FrameEx>().ToArray();

            ChangeMode(true);
        }

        private void OnClickAnswer(object sender, EventArgs e)
        {
            if (sender == quest)
            {
                RefreshQuiz();
            }
            else//click answer
            {
                int idx = Array.IndexOf(answers, sender);
                bool? isRight = curQuiz?.Test(idx);
                if(isRight.HasValue)
                {
                    answers[idx].OutlineColor = isRight.Value ? Color.Green : Color.Red;
                    answers[idx].BackgroundColor = isRight.Value ? Color.FromHex("E0FFE0") : Color.FromHex("FFE0E0");
                }
                else
                    answers[idx].OutlineColor = Color.Blue;
            }
        }

        private void RefreshQuiz()
        {
            try
            {
                curQuiz = QuizService.GetQuiz(mode);
            }
            catch (Exception e)
            {
                e.CopeWith("get quiz");
                return;
            }
            (quest.Content as Label).Text = curQuiz.quest;
            int a = 0;
            foreach (var f in answers)
            {
                f.OutlineColor = Color.Silver;
                f.BackgroundColor = Color.White;
                (f.Content as Label).Text = curQuiz.answers[a++].Item1;
            }
        }

        public void OnClickMode(object sender, EventArgs args)
        {
            ChangeMode(!mode);
        }

        private void ChangeMode(bool newMode)
        {
            mode = newMode;
            if (mode)//tranMode
            {
                modeRect.BackgroundColor = Color.FromHex("20C000");
                chgMode.Text = "翻译模式";
            }
            else
            {
                modeRect.BackgroundColor = Color.FromHex("1F3FFF");
                chgMode.Text = "联想模式";
            }
        }
    }
}
