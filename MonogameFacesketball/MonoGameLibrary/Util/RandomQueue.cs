using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework;

namespace MonoGameLibrary.Util
{

    interface IRandQueue { }
    
    public class RandomQueue : Microsoft.Xna.Framework.GameComponent, IRandQueue
    {
        static Queue<double> RQueue;
        

        public static double MemInMB { get; set;}
        static uint size;
        public static uint Size
        {
            get { return size; }
        }

        static Random r, rf;
        static TimeSpan t;
        static QueueState queueState;
        
        public static QueueState CurrentQueueState { get { return queueState; } }
        List<double> drand;
        public RandomQueue(Game game): base(game)
        {
            r = new Random();
            rf = new Random();
            t = new TimeSpan(1);    //Very short time
            //default memory use
            if(MemInMB <= 0)
                MemInMB = 4;
            size = (uint)((double)1048576 * MemInMB) / sizeof(double);
            RQueue = new Queue<double>();
            
            queueState = QueueState.Emptry;
            this.FillQueue();
            game.Services.AddService(typeof(IRandQueue), this);
        }

        public void FillQueue()
        {
            queueState = QueueState.Filling;
            Thread thread = new Thread(
                new ThreadStart(DoFillQueue));
            thread.Name = string.Format("{0} FillQueue", this);
            thread.Start();
        }

        static void DoFillQueue()
        {
                /*
                int startCount = RQueue.Count;
                lock (RQueue)
                {
                    System.Threading.Tasks.Parallel.For(startCount, size, i =>
                    {
                        RQueue.Enqueue(r.NextDouble());
                        Thread.Sleep(t);
                    });
                }
                 */
                
                while (RQueue.Count <= size)
                {
                    RQueue.Enqueue(rf.NextDouble());
                    //Thread.Sleep(t);
                }       
                queueState = QueueState.Good;
        }

        public double GetRandom()
        {
            return this.GetRandom(1);
        }

        public double GetRandom(int Max)
        {
            //fill at 2% careful that you don't pull too fast
            if ((RQueue.Count < Size * .02) &&
                (queueState == QueueState.Good))
            {
                //r = new Random(System.DateTime.Now.Millisecond);
                queueState = QueueState.Filling;
                this.FillQueue();
            }
            
            //return random from queue if we have one
            if (queueState != QueueState.Filling)
            {
                return RQueue.Dequeue() * Max;
            }
            else
            {
                //no more in queue change the fillrate but still return a double
                //TODO set fillrate percentage and make it dynamic
                return r.NextDouble() * Max;
            }
        }

        public List<double> GetRandoms(int NumRandoms)
        {
            drand = new List<double>();
            System.Threading.Tasks.Parallel.For(0, NumRandoms - 8, i =>
            {
                drand.Add(this.GetRandom());
            });
            
            //for (int i = 0; i < NumRandoms; i++)
            //{
            //    drand.Add(this.GetRandom());
            //}
            return drand;
        }

        public List<double> GetRandoms(int Max, int NumRandoms)
        {
            drand = new List<double>();

            //fill in parallel
            /*
            System.Threading.Tasks.Parallel.For(0, NumRandoms, i =>
            {
                drand.Add(this.GetRandom(Max));
            });
            */
            
            for (int i = 0; i < NumRandoms; i++)
            {
                drand.Add(this.GetRandom(Max));
            }
            
            return drand;
        }

        public string GetInfo()
        {
            return string.Format("Size in MB {0} \nCount {1}\nState {2}\nCount {3}", MemInMB, Size, queueState, RQueue.Count);
        }

        public enum QueueState { Emptry, Filling, Good };   
    }

}
