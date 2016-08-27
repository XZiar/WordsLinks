using Main.Model;
using Main.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Main.Service
{
    public class Quiz
    {
        public enum QuizState { NoAns, Ansing, Finish};
        public struct QuizElement
        {
            public string str;
            public bool ans;
            public bool choice;
            public bool isRight;
        }
        public string quest { get; internal set; }
        public Tuple<string, bool>[] choices { get; internal set; }
        private bool[] ans = new bool[5];
        private int leftCount;
        public bool isAllRight;
        public QuizState state { get; private set; }

        public void init()
        {
            leftCount = choices.Count(c => c.Item2);
            Array.Clear(ans, 0, 5);
            isAllRight = true;
            state = QuizState.NoAns;
        }

        public bool? Test(int idx)
        {
            if (state == QuizState.Finish || ans[idx])
                return null;
            state = QuizState.Ansing;
            ans[idx] = true;
            if (choices[idx].Item2)
            {
                if (--leftCount == 0)
                    state = QuizState.Finish;
                return true;
            }
            else
                return isAllRight = false;
        }

        public IEnumerable<string> GetDescription(int idx)
        {
            IEnumerable<WordElement> trans = DictService.GetMeansByWord(choices[idx].Item1);
            if (trans == null)
                trans = DictService.GetWordsByMean(choices[idx].Item1);
            return trans.Select(x => x.GetStr());
        }

        public void EndTest()
        {
            if (state != QuizState.Finish)
                return;
            int a = 0;
            if (isAllRight)
            {
                DictService.Report(quest, -2);
                foreach (var it in choices)
                {
                    if (it.Item2 == ans[a++])
                        DictService.Report(it.Item1, -1);
                }
            }
            else
            {
                DictService.Report(quest, 3);
                foreach(var it in choices)
                {
                    if (it.Item2 != ans[a++])
                        DictService.Report(it.Item1, 1);
                }
            }
        }
    }

    public static class QuizService
    {
        private static Random rand = new Random();
        private static int lastRand = 65536;

        public static bool isAdapt = false;
        
        private static bool NormalQuiz(Quiz q, out WordElement[] waitList)
        {
            int r = lastRand, max = 2 * Math.Max(DictService.WordsCount, DictService.MeansCount);
            float ratio = DictService.WordsCount * 1.0f / DictService.MeansCount;
            while (r == lastRand)
                r = rand.Next(max);
            lastRand = r;
            bool isWord = (r % 2 == 1);
            r /= 2;
            //get basic data
            if (isWord)
            {
                var word = DictService.WordAt(ratio < 1.0f ? (int)(r * ratio) : r);
                q.quest = word.Letters;
                waitList = DictService.GetMeansByWId(word.Id);
            }
            else
            {
                var mean = DictService.MeanAt(ratio > 1.0f ? (int)(r / ratio) : r);
                q.quest = mean.Meaning;
                waitList = DictService.GetWordsByMId(mean.Id);
            }
            return isWord;
        }
        private static bool AdaptQuiz(Quiz q, out WordElement[] waitList)
        {
            int r = lastRand;
            while (r == lastRand)
                r = rand.Next(DictService.WrongCount);
            var stat = DictService.EleAt(lastRand = r);
            q.quest = stat.str;

            bool isWord = true;
            waitList = DictService.GetMeansByWord(stat);
            if (waitList == null)
            {
                isWord = false;
                waitList = DictService.GetWordsByMean(stat);
            }
            return isWord;
        }
        public static Quiz GetQuiz()
        {
            Quiz q = new Quiz();
            WordElement[] waitList;
            bool isWord = isAdapt ? AdaptQuiz(q, out waitList) : NormalQuiz(q, out waitList);
            
            //insert answers
            var anss = new List<Tuple<string, bool>>();
            int r = rand.Next(waitList.Length);
            anss.Add(new Tuple<string, bool>(waitList[r].GetStr(), true));
            for (int a = 1; a < 5; a++)
            {
                WordElement ele;
                do
                {
                    r = rand.Next((int)((isWord ? DictService.MeansCount : DictService.WordsCount) * 1.3));
                    ele = (isWord ? DictService.MeanAt(r) as WordElement : DictService.WordAt(r) as WordElement);
                    ele = ele ?? waitList.ElementAt(r % waitList.Count());
                }
                while (anss.Exists(x => ele.GetStr() == x.Item1));
                bool isRight = waitList.Any(x => x.GetId() == ele.GetId());
                anss.Add(new Tuple<string, bool>(ele.GetStr(), isRight));
            }
            q.choices = anss.Shuffle(rand).ToArray();
            return q;
        }
    }
}
