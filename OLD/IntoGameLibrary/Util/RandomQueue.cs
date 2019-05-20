using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace IntroGameLibrary.Util
{
    class RandomQueue
    {
        Queue<double> RandQueue;
        int size;
        public int Size
        {
            get { return size; }
        }

        Random r;
        TimeSpan t;

        public RandomQueue()
        {
            r = new Random();
            t = new TimeSpan(1);    //Very short time
            size = 100000;          //100000 random numbers
            RandQueue = new Queue<double>();
            this.FillQueue();
        }

        public void FillQueue()
        {
            while (RandQueue.Count < this.size)
            {
                RandQueue.Enqueue(r.NextDouble());
                Thread.Sleep(t);
            }
        }

        public double GetRandom()
        {
            return this.GetRandom(1);
        }

        public double GetRandom(int Max)
        {
            if (RandQueue.Count == 0)
            {
                this.FillQueue();
            }
            return RandQueue.Dequeue() * Max;
        }
    }

}
