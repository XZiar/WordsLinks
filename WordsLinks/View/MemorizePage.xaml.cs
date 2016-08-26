using Main.Service;
using Main.Util;
using System;
using System.Diagnostics;
using System.Linq;
using WordsLinks.Widget;
using Xamarin.Forms;

namespace WordsLinks.View
{
    public partial class MemorizePage : ContentPage
    {
        private bool mode = true;
        private FrameEx[] choices;
        private Label[] chdess;
        private FrameEx quest;
        private Quiz curQuiz;
        public MemorizePage()
        {
            InitializeComponent();

            var panGes = new PanGestureRecognizer();
            panGes.PanUpdated += OnGrapPage;
            quizLayout.GestureRecognizers.Add(panGes);

            var tapGes = new TapGestureRecognizer();
            tapGes.Tapped += OnClickAnswer;

            chdess = quizLayout.Children.OfType<Label>().ToArray();
            var btns = quizLayout.Children.OfType<FrameEx>();
            foreach (var ele in btns)
                ele.GestureRecognizers.Add(tapGes);

            quest = btns.ElementAt(0);
            choices = btns.Skip(1).ToArray();

            ChangeMode(true);
        }

        private DateTime lastDragTime = DateTime.Now;
        private double dragX, dragY;
        private void OnGrapPage(object sender, PanUpdatedEventArgs e)
        {
            if(e.StatusType == GestureStatus.Running)
            {
                dragX = e.TotalX; dragY = e.TotalY;
            }
            else if(e.StatusType == GestureStatus.Completed)
            {
                if (Math.Abs(dragX) < 80)
                    return;
                var time = (DateTime.Now - lastDragTime);
                if (time.TotalMilliseconds < 500)
                {
                    lastDragTime.AddMilliseconds(-1000);
                    OnClickAnswer(quest, null);
                }
                else
                    lastDragTime = DateTime.Now;
            }
        }

        private void OnClickAnswer(object sender, EventArgs e)
        {
            if (sender == quest)
            {
                RefreshQuiz();
            }
            else//click answer
            {
                int idx = Array.IndexOf(choices, sender);
                bool? isRight = curQuiz?.Test(idx);
                if (!isRight.HasValue)
                    return;
                choices[idx].OutlineColor = isRight.Value ? Color.Green : Color.Red;
                choices[idx].BackgroundColor = isRight.Value ? Color.FromHex("E0FFE0") : Color.FromHex("FFE0E0");
                chdess[idx].Text = string.Join(",", curQuiz.GetDescription(idx));
                chdess[idx].TextColor = isRight.Value ? Color.Green : Color.Red;
                if (curQuiz.leftCount == 0)
                    EndQuiz();
            }
        }

        private void EndQuiz()
        {
            quest.OutlineColor = curQuiz.isAllRight ? Color.Green : Color.Red;
            quest.BackgroundColor = curQuiz.isAllRight ? Color.FromHex("E0FFE0") : Color.FromHex("FFE0E0");
        }

        private void RefreshQuiz()
        {
            try
            {
                curQuiz?.EndTest();
                curQuiz = QuizService.GetQuiz(mode);
                curQuiz?.init();
            }
            catch (Exception e)
            {
                e.CopeWith("get quiz");
                return;
            }
            
            quest.OutlineColor = Color.Silver;
            quest.BackgroundColor = Color.White;
            (quest.Content as Label).Text = curQuiz.quest;

            int a = 0;
            foreach (var f in choices)
            {
                f.OutlineColor = Color.Silver;
                f.BackgroundColor = Color.White;
                (f.Content as Label).Text = curQuiz.choices[a++].Item1;
            }
            foreach (var l in chdess)
                l.Text = " ";
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
                chgMode.Text = "随机模式";
            }
            else
            {
                modeRect.BackgroundColor = Color.FromHex("1F3FFF");
                chgMode.Text = "考察模式";
            }
        }
    }
}
