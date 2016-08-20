using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Main.Model;
using Main.Util;

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
    }
    public static class QuizService
    {
        private static Random rand = new Random();
        private static int lastRand = 65536;
        public static Quiz GetQuiz(bool type)
        {
            Quiz q = new Quiz();
            int r = lastRand, sum = DBService.WordsCount + DBService.MeansCount;
            while (r == lastRand)
                r = rand.Next(sum);
            lastRand = r;
            WordElement[] waitList;
            bool isWord = r < DBService.WordsCount;

            //get basic data
            if (isWord)
            {
                var word = DBService.WordAt(r);
                q.quest = word.Letters;
                waitList = DBService.GetMeansByWId(word.Id);
            }
            else
            {
                var mean = DBService.MeanAt(r - DBService.WordsCount);
                q.quest = mean.Meaning;
                waitList = DBService.GetWordsByMId(mean.Id);
            }

            //insert answers
            var anss = new List<Tuple<string, bool>>();
            r = rand.Next(waitList.Length);
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
