using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WordsLinks.Model;
using WordsLinks.Util;

namespace WordsLinks.Service
{
    class Quiz
    {
        public string quest { get; internal set; }
        public Tuple<string, bool>[] answers { get; internal set; }
        public int leftChoice { get; private set; }
        internal void init()
        {
            leftChoice = 0;
            foreach (var a in answers)
                if (a.Item2)
                    leftChoice++;
        }
        public bool Test(int idx)
        {
            bool isRight = answers[idx].Item2;
            if (isRight)
                leftChoice--;
            return isRight;
        }
    }
    static class QuizService
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
            Debug.WriteLine($"the rand {r}/{sum}, isWord={isWord}");
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
            Debug.WriteLine($"receive {waitList} length: {waitList.Length}");
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
            q.answers = anss.Shuffle(rand).ToArray();
            return q;
        }
    }
}
