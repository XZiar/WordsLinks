using Main.Service;
using Main.Util;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using WordsLinks.ViewModel;
using Xamarin.Forms;
using static Main.Util.SpecificUtils;

namespace WordsLinks.View
{
	public partial class SettingPage : ContentPage
    {
        SelectCellGroup netChoiceGroup, quizAdaptGroup;
        public SettingPage()
		{
			InitializeComponent();
            netChoiceGroup = new SelectCellGroup(false, false);
            var dat = NetService.GetChoices();
            netChoiceGroup.Set(dat.Item2);
            netChoiceGroup.SetTo(netSect);
            netChoiceGroup.Select += (sender, e) =>
            {
                if (e.isSelect)
                    NetService.Choose(e.idx);
            };

            quizAdaptGroup = new SelectCellGroup(false, false);
            quizAdaptGroup.Set(new string[] { "随机出题", "适应词频" });
            quizAdaptGroup.Select += (o, e) => 
            {
                if (e.isSelect)
                    QuizService.isAdapt = (e.idx == 1);
            };
            quizAdaptGroup.SetTo(null as TableSection);
            quizSect.Add(quizAdaptGroup.sectdatas);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            var dat = NetService.GetChoices();
            netChoiceGroup.Choose(dat.Item1);

            quizAdaptGroup.Choose(QuizService.isAdapt ? 1 : 0);

            RefreshWords();
        }

        private void RefreshWords()
        {
            wordsSect.Title = $"单词本\t（{DBService.WordsCount}个单词）";
        }

        private async Task<bool> ImportChoose()
        {
            var ret = await DisplayActionSheet("是否覆盖现有单词本？", "合并", null, "覆盖");
            return ret == "覆盖";
        }
        private async void OnDBCellTapped(object sender, EventArgs args)
        {
            if (sender == exportCell)
            {
                try
                {
                    var ret = DBService.Export();
                    hudPopup.Show(msg: "导出中");
                    byte[] data = await ret;
                    if (await imgUtil.SaveImage(data))
                        hudPopup.Show(HUDType.Success, "导出成功");
                    else
                        hudPopup.Show(HUDType.Fail, "导出失败");
                }
                catch (Exception e)
                {
                    e.CopeWith("exportDB");
                }
            }
            else if (sender == importCell)
            {
                try
                {
                    var pic = imgUtil.GetImage();
                    if ((await pic) != null)
                    {
                        var ret = DBService.Import(pic.Result, await ImportChoose());
                        hudPopup.Show(msg: "导入中");
                        if (await ret)
                            hudPopup.Show(HUDType.Success, "导入成功");
                        else
                            hudPopup.Show(HUDType.Fail, "导入失败");
                        RefreshWords();
                    }
                }
                catch (Exception e)
                {
                    e.CopeWith("importDB");
                }
            }
            else if (sender == clearCell)
            {
                try
                {
                    var ret = await DisplayAlert("清空单词本", "此操作无法恢复", "确认", "不了");
                    if (ret)
                    {
                        DBService.Clear();
                        RefreshWords();
                    }
                }
                catch (Exception e)
                {
                    e.CopeWith("clearDB");
                }
            }
            else if (sender == debugCell)
            {
                DBService.debugInfo();
            }
        }
    }
}
