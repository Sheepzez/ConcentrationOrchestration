﻿using Emotiv;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConcentrationOrchestration
{
    static class Program
    {
        private static readonly int fps = 60;
        private static readonly int sleepMilliSeconds = (int)Math.Round(1.0 / fps * 1000);
        private static EmoEngine engine = null;
        public static DisplayInputHandler displayInputHandler;
        private static int userID = -1;

        static void engine_UserAdded_Event(object sender, EmoEngineEventArgs e)
        {
            Console.WriteLine("User Added Event has occured");
            userID = (int)e.userId;

            EmoEngine.Instance.IEE_FFTSetWindowingType((uint)userID, EdkDll.IEE_WindowingTypes.IEE_HAMMING);
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            engine = EmoEngine.Instance;
            engine.UserAdded += new EmoEngine.UserAddedEventHandler(engine_UserAdded_Event);
            engine.Connect();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            GameWindow window = new GameWindow();
            displayInputHandler = new DisplayInputHandler(window);

            Application.Idle += new EventHandler(Application_Idle);

            Application.Run(window);
        }

        static void Application_Idle(object sender, EventArgs e)
        {
            Random r = new Random();
            displayInputHandler.ApplyNewScaledValue(r.NextDouble());

            if (engine.EngineGetNumUser() > 0)
            {
                engine.IEE_FFTSetWindowingType(0, EdkDll.IEE_WindowingTypes.IEE_HAMMING);
            }

            engine.ProcessEvents(10);

            double[] alpha = new double[1];
            double[] low_beta = new double[1];
            double[] high_beta = new double[1];
            double[] gamma = new double[1];
            double[] theta = new double[1];

            EdkDll.IEE_DataChannel_t[] channelList = new EdkDll.IEE_DataChannel_t[5] { EdkDll.IEE_DataChannel_t.IED_AF3, EdkDll.IEE_DataChannel_t.IED_AF4, EdkDll.IEE_DataChannel_t.IED_T7,
                                                                                       EdkDll.IEE_DataChannel_t.IED_T8, EdkDll.IEE_DataChannel_t.IED_O1 };

            double avgGamma = 0;
            for (int i = 0; i < 5; i++)
            {
                engine.IEE_GetAverageBandPowers(0, channelList[i], theta, alpha, low_beta, high_beta, gamma);
                Console.Write(theta[0] + ",");
                Console.Write(alpha[0] + ",");
                Console.Write(low_beta[0] + ",");
                Console.Write(high_beta[0] + ",");
                Console.WriteLine(gamma[0] + ",");

                Console.WriteLine("");

                avgGamma += gamma[0];
            }

            avgGamma = avgGamma / 5;
            //CurrentMentalState.Text = "Gamma: " + avgGamma;
        }
    }
}
