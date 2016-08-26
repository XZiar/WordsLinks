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
        SelectCellGroup netChoiceGroup;//, quizAdaptGroup;
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

            isAdapt.On = QuizService.isAdapt;
            isAdapt.OnChanged += (o, args) =>
            {
                QuizService.isAdapt = args.Value;
            };
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            var dat = NetService.GetChoices();
            netChoiceGroup.Choose(dat.Item1);

            RefreshWords();
        }

        private void RefreshWords()
        {
            wordsSect.Title = $"单词本\t（{DictService.WordsCount}个单词）";
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
                    var ret = DictService.Export();
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
                        var ret = DictService.Import(pic.Result, await ImportChoose());
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
                        DictService.Clear();
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
                DictService.debugInfo();
            }
        }
    }
}
