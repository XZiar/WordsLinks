using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Main.Model;
using Main.Util;
using System.Text;
using System.Text.RegularExpressions;

namespace Main.Service
{
    public class Quiz
    {
        public string quest { get; internal set; }
        public Tuple<string, bool>[] choices { get; internal set; }
        private bool[] ans = new bool[5];
        public int leftCount { get; private set; }
        public bool isAllRight { get; private set; }

        public void init()
        {
            leftCount = choices.Count(c => c.Item2);
            Array.Clear(ans, 0, 5);
            isAllRight = true;
        }

        public bool? Test(int idx)
        {
            if (leftCount == 0 || ans[idx])
                return null;
            ans[idx] = true;
            var isRight = choices[idx].Item2;
            if (isRight)
                leftCount--;
            else
                isAllRight = false;
            return isRight;
        }

        public IEnumerable<string> GetDescription(int idx)
        {
            IEnumerable<WordElement> trans = DBService.GetMeansByWord(choices[idx].Item1);
            if (trans == null)
                trans = DBService.GetWordsByMean(choices[idx].Item1);
            return trans.Select(x => x.GetStr());
        }

        public void EndTest()
        {
            if (leftCount > 0)
                return;
            if(isAllRight)
            {
                DBService.Report(quest, -3);
            }
            else
            {
                DBService.Report(quest, 3);
                int a = 0;
                foreach(var it in choices)
                {
                    if (it.Item2 != ans[a++])
                        DBService.Report(it.Item1, 1);
                }
            }
        }
    }
    public static class QuizService
    {
        private static Random rand = new Random();
        private static int lastRand = 65536;
        
        private static bool NormalQuiz(Quiz q, out WordElement[] waitList)
        {
            int r = lastRand, max = 2 * Math.Max(DBService.WordsCount, DBService.MeansCount);
            float ratio = DBService.WordsCount * 1.0f / DBService.MeansCount;
            while (r == lastRand)
                r = rand.Next(max);
            lastRand = r;
            bool isWord = (r % 2 == 1);
            r /= 2;
            //get basic data
            if (isWord)
            {
                var word = DBService.WordAt(ratio < 1.0f ? (int)(r * ratio) : r);
                q.quest = word.Letters;
                waitList = DBService.GetMeansByWId(word.Id);
            }
            else
            {
                var mean = DBService.MeanAt(ratio > 1.0f ? (int)(r / ratio) : r);
                q.quest = mean.Meaning;
                waitList = DBService.GetWordsByMId(mean.Id);
            }
            return isWord;
        }
        private static bool AdaptQuiz(Quiz q, out WordElement[] waitList)
        {
            int r = lastRand;
            while (r == lastRand)
                r = rand.Next(DBService.WrongCount);
            var stat = DBService.EleAt(lastRand = r);
            q.quest = stat.str;

            bool isWord = true;
            waitList = DBService.GetMeansByWord(stat);
            if (waitList == null)
            {
                isWord = false;
                waitList = DBService.GetWordsByMean(stat);
            }
            return isWord;
        }
        public static Quiz GetQuiz(bool type)
        {
            Quiz q = new Quiz();
            WordElement[] waitList;
            bool isWord = type ? NormalQuiz(q, out waitList) : AdaptQuiz(q, out waitList);
            
            //insert answers
            var anss = new List<Tuple<string, bool>>();
            int r = rand.Next(waitList.Length);
            anss.Add(new Tuple<string, bool>(waitList[r].GetStr(), true));
            for (int a = 1; a < 5; a++)
            {
                WordElement ele;
                do
                {
                    r = rand.Next(isWord ? DBService.MeansCount : DBService.WordsCount);
                    ele = (isWord ? DBService.MeanAt(r) as WordElement : DBService.WordAt(r) as WordElement);
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
