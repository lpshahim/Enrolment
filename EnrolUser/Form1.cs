using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using Leap;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;

namespace EnrolUser
{
    public partial class Form1 : Form
    {
        #region MyVariablesRegion
        //GLOBAL VARIABLES
        /// <summary>

        private byte[] imagedata = new byte[1];
        private Controller controller = new Controller();
        Bitmap bitmap = new Bitmap(640, 480, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
        //Bitmap bitmap = new Bitmap(196, 246, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);

        //COMBONATIONS OF PRESENTED HAND GEOMETRY VECTOR
        private HashSet<string> combos = new HashSet<string>(); 
        private static HashSet<int> pixelValues = new HashSet<int>();

        bool checkAuthenticateEnrol = true;
        String vector = "";
        int timeLeft = 0;
        int fingerReadingCount = 0;
        int boneReadingCount = 0;
        int indexBonesCount, middleBonesCount, ringBonesCount, pinkyBonesCount, thumbBonesCount = 0;
        int thumbMetaCount, thumbProxCount, thumbIntCount = 0;
        int indexMetaCount, indexProxCount, indexIntCount, indexDistCount = 0;
        int middleMetaCount, middleProxCount, middleIntCount, middleDistCount = 0;
        int ringMetaCount, ringProxCount, ringIntCount, ringDistCount = 0;
        int pinkyMetaCount, pinkyProxCount, pinkyIntCount, pinkyDistCount = 0;


        private decimal[] thumbMeasurements = new decimal[3];
        private decimal[] pinkyMeasurements = new decimal[4];
        private decimal[] indexMeasurements = new decimal[4];
        private decimal[] middleMeasurements = new decimal[4];
        private decimal[] ringMeasurements = new decimal[4];

        //temp arrays of strings
        //string[] tempBoneMeasurements;
        //string[] tempFingerMeasurements;
        //List<String> tempBoneMeasurements = new List<String>();

        ArrayList thumbBoneList = new ArrayList();
        ArrayList indexBoneList = new ArrayList();
        ArrayList middleBoneList = new ArrayList();
        ArrayList ringBoneList = new ArrayList();
        ArrayList pinkyBoneList = new ArrayList();

        //THUMB
        ArrayList thumbMetaList = new ArrayList();
        ArrayList thumbProxList = new ArrayList();
        ArrayList thumbIntList = new ArrayList();

        //INDEX
        ArrayList indexMetaList = new ArrayList();
        ArrayList indexProxList = new ArrayList();
        ArrayList indexIntList = new ArrayList();
        ArrayList indexDistList = new ArrayList();

        //MIDDLE
        ArrayList middleMetaList = new ArrayList();
        ArrayList middleProxList = new ArrayList();
        ArrayList middleIntList = new ArrayList();
        ArrayList middleDistList = new ArrayList();

        //RING
        ArrayList ringMetaList = new ArrayList();
        ArrayList ringProxList = new ArrayList();
        ArrayList ringIntList = new ArrayList();
        ArrayList ringDistList = new ArrayList();

        //PINKY
        ArrayList pinkyMetaList = new ArrayList();
        ArrayList pinkyProxList = new ArrayList();
        ArrayList pinkyIntList = new ArrayList();
        ArrayList pinkyDistList = new ArrayList();


        //HASH TABLES INSTEAD OF ARRAYLIST

        //THUMB
        Hashtable thumbMetaTable = new Hashtable();
        Hashtable thumbProxTable = new Hashtable();
        Hashtable thumbIntTable = new Hashtable();

        //INDEX
        Hashtable indexMetaTable = new Hashtable();
        Hashtable indexProxTable = new Hashtable();
        Hashtable indexIntTable = new Hashtable();
        Hashtable indexDistTable = new Hashtable();

        //MIDDLE
        Hashtable middleMetaTable = new Hashtable();
        Hashtable middleProxTable = new Hashtable();
        Hashtable middleIntTable = new Hashtable();
        Hashtable middleDistTable = new Hashtable();

        //RING
        Hashtable ringMetaTable = new Hashtable();
        Hashtable ringProxTable = new Hashtable();
        Hashtable ringIntTable = new Hashtable();
        Hashtable ringDistTable = new Hashtable();

        //PINKY
        Hashtable pinkyMetaTable = new Hashtable();
        Hashtable pinkyProxTable = new Hashtable();
        Hashtable pinkyIntTable = new Hashtable();
        Hashtable pinkyDistTable = new Hashtable();

        //file writing variables
        string userFileName;
        string directory;
        string bonesFilePath;
        string fingersFilePath;
        string bonesCSVPath;
        string fingersCSVPath;
        string matrixCSVPath;
        //******************** 
        #endregion

        #region enrolUser
        static HashSet<int> userHandGeo = new HashSet<int>();

        //check current authenticating hand geometry
        static HashSet<int> currentUserHandGeo = new HashSet<int>();

        static HashSet<int> userPin = new HashSet<int>();
        static Dictionary<string, string> searchDictionary = new Dictionary<string, string>();
        static string startUserKey = "";

        static void readDictionary(string pixelId)
        {
            using (FileStream fileStream = new FileStream("randomImage.png", FileMode.Open, FileAccess.ReadWrite))
            {
                Bitmap bmp = new Bitmap(fileStream);

                string[] allLines = System.IO.File.ReadAllLines("newMatrixMap.txt");
                Dictionary<string, string> map = new Dictionary<string, string>();

                foreach (var i in allLines)
                {
                    string[] val = Regex.Split(i, "; ");
                    map.Add(val[0], val[1]);
                }
                
                var keysWithMatchingValues = map.Where(p => p.Value == pixelId).Select(p => p.Key);
                //PIXEL ID 
                List<byte> pixelList = new List<byte>();
                string[] keys = new string[8];
                int count = 0;
                //**************
                foreach (var key in keysWithMatchingValues)
                {
                    Console.WriteLine("\n\n" + key);
                    keys[count] = key;
                    count++;
                    string[] values = Regex.Split(key, ",");
                    Color pixel = bmp.GetPixel(Convert.ToInt16(values[0]), Convert.ToInt16(values[1]));
                    pixelList.Add(pixel.A);
                    pixelList.Add(pixel.R);
                    pixelList.Add(pixel.G);
                    pixelList.Add(pixel.B);
                }
                //Crucial to get the first pixelID
                startUserKey = keys[0];
                /*foreach (var i in pixelList)
                {
                    Console.WriteLine(i + " ");
                }*/
            }
        }

        static string authenticatePin(string pin)
        {
            searchDictionary = System.IO.File.ReadLines("newAuthenticationPairs.txt").Select(line => line.Split(',')).ToDictionary(line => line[0], ValueType => ValueType[1]);
            string pixelID = "";
            if (searchDictionary.ContainsValue(pin))
            {
                Console.WriteLine("\nID: " + getPixelId(pin));
                pixelID = getPixelId(pin);
                readDictionary(pixelID);
            }
            else
            {
                //Console.WriteLine("\nFalse");
            }
            return pixelID;
        }

        static void addEnrolledUser(string pin)
        {
            StreamWriter sw = File.AppendText("enrolledUsers.txt");
            sw.WriteLine(pin);
            sw.Flush();
            sw.Close();
        }

        static void userLog(string type, string pin, string transformedGeo)
        {
            StreamWriter sw = File.AppendText("Log.txt");
            string output = String.Format("User has {0} with pin {1} and transformed hand geo of {2}\n",type, pin, transformedGeo);
            sw.WriteLine(output);
            sw.Flush();
            sw.Close();
            addEnrolledUser(pin);
        }

        private void timer1_Tick_1(object sender, EventArgs e)
        {
            if (timeLeft > 0)
            {
                timeLeft = timeLeft - 1;
                lblTimer.Text = Convert.ToString(timeLeft);
            }
            else
            {
                timer1.Stop();
                controller.StopConnection();
                MessageBox.Show("Scan Stopped");
                lblFPS.Text = "";
                lblTimer.Text = "Done";
                MessageBox.Show("Number of finger readings: " + Convert.ToString(fingerReadingCount) + "\nNumber of bone readings: " + Convert.ToString(boneReadingCount));

                List<ArrayList> _listOfLists = new List<ArrayList>();

                _listOfLists.Add(thumbMetaList);
                _listOfLists.Add(thumbProxList);
                _listOfLists.Add(thumbIntList);

                _listOfLists.Add(indexMetaList);
                _listOfLists.Add(indexProxList);
                _listOfLists.Add(indexIntList);
                _listOfLists.Add(indexDistList);

                _listOfLists.Add(middleMetaList);
                _listOfLists.Add(middleProxList);
                _listOfLists.Add(middleIntList);
                _listOfLists.Add(middleDistList);

                _listOfLists.Add(ringMetaList);
                _listOfLists.Add(ringProxList);
                _listOfLists.Add(ringIntList);
                _listOfLists.Add(ringDistList);

                _listOfLists.Add(pinkyMetaList);
                _listOfLists.Add(pinkyProxList);
                _listOfLists.Add(pinkyIntList);
                _listOfLists.Add(pinkyDistList);

                List<string> csv = getCSV(_listOfLists);

                StreamWriter wr = new StreamWriter(matrixCSVPath, true);
                wr.WriteLine("");
                foreach (string l in csv)
                    wr.WriteLine(l);

                wr.Close();


            }
        }

        static string getPixelId(string search)
        {
            return searchDictionary[search];
        }

        static string generateAuthenticateUserHash(string pixel, string input)
        {
            byte[] hash;
            string finalHash = "";
            using (var sha1 = new SHA256CryptoServiceProvider())
            {
                hash = sha1.ComputeHash(Encoding.Unicode.GetBytes(input));
                foreach (byte b in hash)
                {
                    finalHash += b + " ";
                    userHandGeo.Add(b);
                    currentUserHandGeo.Add(b);
                }
            }
            checkUserPixels(startUserKey);
            //setPixelsStegoImage2(startUserKey,hash);
            return finalHash;
        }

        static string generateEnrolUserHash(string pixel, string input)
        {
            byte[] hash;
            string finalHash = "";
            using (var sha1 = new SHA256CryptoServiceProvider())
            {
                hash = sha1.ComputeHash(Encoding.Unicode.GetBytes(input));
                foreach (byte b in hash)
                {
                    finalHash += b + " ";
                    userHandGeo.Add(b);
                }
            }
            //checkUserPixels(startUserKey);
            setPixelsStegoImage2(startUserKey,hash);
            return finalHash;
        }

        static string generatePINHash(string pixel, string pin)
        {
            byte[] hash;
            string finalHash = "";
            using (var sha1 = new SHA256CryptoServiceProvider())
            {
                hash = sha1.ComputeHash(Encoding.Unicode.GetBytes(pin));
                foreach (byte b in hash)
                {
                    finalHash += b + " ";
                    userPin.Add(b);
                }
            }


            checkPinPixels(startUserKey);
            //setPixelsStegoImage2(startUserKey,hash);
            return finalHash;
        }

        static void checkPinPixels(string pixel)
        {
            using (FileStream fileStream = new FileStream("randomImage.png", FileMode.Open, FileAccess.ReadWrite))
            {
                Bitmap bmp = new Bitmap(fileStream);
                HashSet<int> pixelValues = new HashSet<int>();
                string[] xy = Regex.Split(pixel, ",");
                int x = Convert.ToInt16(xy[0]);
                int y = Convert.ToInt16(xy[1]);

                for (int i = 0; i < 32; i += 4)
                {
                    Color pVals = bmp.GetPixel(x, y);
                    pixelValues.Add(pVals.A);
                    pixelValues.Add(pVals.R);
                    pixelValues.Add(pVals.G);
                    pixelValues.Add(pVals.B);
                    x++;
                }

                string hashSet1 = "";
                string hashSet2 = "";

                foreach (int i in pixelValues)
                {
                    hashSet1 += Convert.ToString(i) + " ";
                }

                foreach (int i in userPin)
                {
                    hashSet2 += Convert.ToString(i) + " ";
                }

                //Console.WriteLine("\nHashset1 in random PIN image is: {0}\nHashset 2  from user PIN is {1}\n", hashSet1, hashSet2);

                bool yesNo = pixelValues.SetEquals(userHandGeo);
                Console.WriteLine(yesNo);
                
                //convert hashset to array to sort and then test
                int[] hSet1 = pixelValues.ToArray();
                int[] hSet2 = userPin.ToArray();
                Array.Sort(hSet1);
                Array.Sort(hSet2);

                /*foreach (int i in hSet1)
                    Console.Write(i + " ");
                Console.WriteLine("\n");
                foreach (int i in hSet2)
                    Console.Write(i + " ");*/

                if (hSet1.SequenceEqual(hSet2))
                {
                    MessageBox.Show("There is a MATCH for the PIN");
                }else
                {

                    MessageBox.Show("NO MATCH");
                }
            }
        }

        static void checkUserPixels(string pixel)
        {
            using (FileStream fileStream = new FileStream("stegoImage2.png", FileMode.Open, FileAccess.ReadWrite))
            {
                Bitmap bmp = new Bitmap(fileStream);
                //HashSet<int> pixelValues = new HashSet<int>();
                string[] xy = Regex.Split(pixel, ",");
                int x = Convert.ToInt16(xy[0]);
                int y = Convert.ToInt16(xy[1]);

                for (int i = 0; i < 32; i += 4)
                {
                    Color pVals = bmp.GetPixel(x, y);
                    pixelValues.Add(pVals.A);
                    pixelValues.Add(pVals.R);
                    pixelValues.Add(pVals.G);
                    pixelValues.Add(pVals.B);
                    x++;
                }

                string hashSet1 = "";
                string hashSet2 = "";

                foreach (int i in pixelValues)
                {
                    hashSet1 += Convert.ToString(i) + " ";
                }

                foreach (int i in userHandGeo)
                {
                    hashSet2 += Convert.ToString(i) + " ";
                }

                Console.WriteLine("\nHashset1 of stored user geo is: {0}\nHashset 2 of presented and transformed geo is {1}\n", hashSet1, hashSet2);

                bool yesNo = pixelValues.SetEquals(userHandGeo);
                MessageBox.Show("HASHSET MATCH : " + Convert.ToString(yesNo));
                
                
                //convert hashset to array to sort and then test
                /*int[] hSet1 = pixelValues.ToArray();
                int[] hSet2 = userPin.ToArray();
                Array.Sort(hSet1);
                Array.Sort(hSet2);

                foreach (int i in hSet1)
                    Console.Write(i + " ");
                Console.WriteLine("\n");
                foreach (int i in hSet2)
                    Console.Write(i + " ");

                if (hSet1.SequenceEqual(hSet2))
                {
                    MessageBox.Show("There is a MATCH for the geometry");
                }
                else
                {

                    MessageBox.Show("NO MATCH");
                }*/
            }
        }

        static void setPixelsStegoImage2(string pixel, byte[] h)
        {

            int x = Convert.ToUInt16(pixel.Split(',')[0]);
            int y = Convert.ToUInt16(pixel.Split(',')[1]);

            Console.WriteLine("\n\n X value is: " + x + "\nY value is: " + y + "\n");


            using (FileStream fileStream = new FileStream("stegoImage2.png", FileMode.Open, FileAccess.ReadWrite))
            {
                System.Drawing.Image img = System.Drawing.Image.FromStream(fileStream);
                Bitmap bmp = new Bitmap(img);
                for (int i = 0; i < h.Length; i += 4)
                {
                    //Console.WriteLine(h[i] + " " + h[i+1] + " " + h[i+2] + " " + h[i+3] + "\n");
                    bmp.SetPixel(x, y, Color.FromArgb(h[i], h[i + 1], h[i + 2], h[i + 3]));
                    x++;
                }
                fileStream.Close();
                bmp.Save("stegoImage2.png", System.Drawing.Imaging.ImageFormat.Png);
                bmp.Dispose();
            }


        }
        #endregion

        //LMC FRAME HANDLER
        private void newFrameHandler(object sender, FrameEventArgs eventArgs)
        {
            Frame frame = eventArgs.frame;
            //The following are Label controls added in design view for the form
            //this.displayID.Text = frame.Id.ToString();
            //this.displayTimestamp.Text = frame.Timestamp.ToString();
            this.lblFPS.Text = frame.CurrentFramesPerSecond.ToString();
            // this.displayHandCount.Text = frame.Hands.Count.ToString(); 

            controller.RequestImages(frame.Id, Leap.Image.ImageType.DEFAULT, imagedata);

            //hand data
            string tempFingerMeasurements = "";
            //string tempBoneMeasurements = "";

            //boneWriter = new StreamWriter(bonesCSVPath);
            //fingerWriter = new StreamWriter(fingersCSVPath);

            List<Hand> allHands = frame.Hands;

            foreach (Hand hand in allHands)
            {
                if (hand.IsRight == true)
                {
                    List<Finger> fingers = hand.Fingers;
                    foreach (Finger finger in fingers)
                    {

                        tempFingerMeasurements = finger.Type.ToString() + "," + finger.Length.ToString() + "," + finger.Width.ToString();
                        /*using (StreamWriter fingerWriter = new StreamWriter(fingersCSVPath, true))
                        {
                            fingerWriter.WriteLine(tempFingerMeasurements);

                            //countReadings after write
                            fingerReadingCount++;

                            fingerWriter.Close();
                        }*/
                        fingerReadingCount++;
                        //Console.Write(temp);
                        //Console.Write(finger.Type + "\n" + finger.Length + "\n" + finger.Width);

                        String ftype = finger.Type.ToString();
                        int fingerID = 0;

                        switch (ftype)
                        {
                            case "TYPE_INDEX":
                                fingerID = 1;
                                break;
                            case "TYPE_MIDDLE":
                                fingerID = 2;
                                break;
                            case "TYPE_RING":
                                fingerID = 3;
                                break;
                            case "TYPE_PINKY":
                                fingerID = 4;
                                break;
                            case "TYPE_THUMB":
                                fingerID = 5;
                                break;
                        }

                        int countBones = 0;

                        foreach (Bone.BoneType boneType in (Bone.BoneType[])Enum.GetValues(typeof(Bone.BoneType)))
                        {

                            if (boneType >= 0)
                            {
                                Bone boneLength = finger.Bone(boneType);

                                string typeOfBone = boneLength.Type.ToString();

                                switch (fingerID)
                                {
                                    case 1:
                                        indexMeasurements[countBones] = (decimal)boneLength.Length;// + (decimal)boneLength.Width;
                                        indexBoneList.Add(boneLength.Type + ", " + indexMeasurements[countBones]);
                                        //Console.Write(boneList[countBones]);
                                        indexBonesCount++;
                                        //CASE CHECK MIDDLE BONE AND WRITE TO HASHTABLE
                                        switch (typeOfBone)
                                        {
                                            case "TYPE_METACARPAL":
                                                indexMetaCount++;
                                                indexMetaTable.Add(indexBonesCount, indexMeasurements[countBones]);
                                                indexMetaList.Add(indexMeasurements[countBones]);

                                                break;

                                            case "TYPE_PROXIMAL":
                                                indexProxCount++;
                                                indexProxTable.Add(indexBonesCount, indexMeasurements[countBones]);
                                                indexProxList.Add(indexMeasurements[countBones]);
                                                break;

                                            case "TYPE_INTERMEDIATE":
                                                indexIntCount++;
                                                indexIntTable.Add(indexBonesCount, indexMeasurements[countBones]);
                                                indexIntList.Add(indexMeasurements[countBones]);
                                                break;

                                            case "TYPE_DISTAL":
                                                indexDistCount++;
                                                indexDistTable.Add(indexBonesCount, indexMeasurements[countBones]);
                                                indexDistList.Add(indexMeasurements[countBones]);
                                                break;
                                        }

                                        /*using (StreamWriter boneWriter = new StreamWriter(bonesCSVPath, true))
                                        {
                                            boneWriter.WriteLine(boneLength.Type + ", Index, " + indexMeasurements[countBones]);
                                            //indexBonesCount++;
                                            boneReadingCount++;
                                            boneWriter.Close();
                                        }*/
                                        boneReadingCount++;
                                        break;
                                    case 2:
                                        middleMeasurements[countBones] = (decimal)boneLength.Length;// + (decimal)boneLength.Width;
                                        middleBoneList.Add(boneLength.Type + ", " + middleMeasurements[countBones]);
                                        middleBonesCount++;
                                        //CASE CHECK MIDDLE BONE AND WRITE TO HASHTABLE
                                        switch (typeOfBone)
                                        {
                                            case "TYPE_METACARPAL":
                                                middleMetaCount++;
                                                middleMetaTable.Add(middleBonesCount, middleMeasurements[countBones]);
                                                middleMetaList.Add(middleMeasurements[countBones]);
                                                break;

                                            case "TYPE_PROXIMAL":
                                                middleProxCount++;
                                                middleProxTable.Add(middleBonesCount, middleMeasurements[countBones]);
                                                middleProxList.Add(middleMeasurements[countBones]);
                                                break;

                                            case "TYPE_INTERMEDIATE":
                                                middleIntCount++;
                                                middleIntTable.Add(middleBonesCount, middleMeasurements[countBones]);
                                                middleIntList.Add(middleMeasurements[countBones]);
                                                break;

                                            case "TYPE_DISTAL":
                                                middleDistCount++;
                                                middleDistTable.Add(middleBonesCount, middleMeasurements[countBones]);
                                                middleDistList.Add(middleMeasurements[countBones]);
                                                break;
                                        }

                                        /*using (StreamWriter boneWriter = new StreamWriter(bonesCSVPath, true))
                                        {
                                            boneWriter.WriteLine(boneLength.Type + ", Middle, " + middleMeasurements[countBones]);
                                            //middleBonesCount++;
                                            boneReadingCount++;
                                            boneWriter.Close();
                                        }*/
                                        boneReadingCount++;
                                        break;
                                    case 3:
                                        ringMeasurements[countBones] = (decimal)boneLength.Length;// + (decimal)boneLength.Width;
                                        ringBoneList.Add(boneLength.Type + ", " + ringMeasurements[countBones]);
                                        ringBonesCount++;
                                        //CASE CHECK RING BONE AND WRITE TO HASHTABLE
                                        switch (typeOfBone)
                                        {
                                            case "TYPE_METACARPAL":
                                                ringMetaCount++;
                                                ringMetaTable.Add(ringBonesCount, ringMeasurements[countBones]);
                                                ringMetaList.Add(ringMeasurements[countBones]);
                                                break;

                                            case "TYPE_PROXIMAL":
                                                ringProxCount++;
                                                ringProxTable.Add(ringBonesCount, ringMeasurements[countBones]);
                                                ringProxList.Add(ringMeasurements[countBones]);
                                                break;

                                            case "TYPE_INTERMEDIATE":
                                                ringIntCount++;
                                                ringIntTable.Add(ringBonesCount, ringMeasurements[countBones]);
                                                ringIntList.Add(ringMeasurements[countBones]);
                                                break;

                                            case "TYPE_DISTAL":
                                                ringDistCount++;
                                                ringDistTable.Add(ringBonesCount, ringMeasurements[countBones]);
                                                ringDistList.Add(ringMeasurements[countBones]);
                                                break;
                                        }

                                        /*using (StreamWriter boneWriter = new StreamWriter(bonesCSVPath, true))
                                        {
                                            boneWriter.WriteLine(boneLength.Type + ", Ring, " + ringMeasurements[countBones]);
                                            //ringBonesCount++;
                                            boneReadingCount++;
                                            boneWriter.Close();
                                        }*/
                                        boneReadingCount++;
                                        break;
                                    case 4:
                                        pinkyMeasurements[countBones] = (decimal)boneLength.Length;// + (decimal)boneLength.Width;
                                        pinkyBoneList.Add(boneLength.Type + ", " + pinkyMeasurements[countBones]);
                                        pinkyBonesCount++;
                                        //CASE CHECK pinky BONE AND WRITE TO HASHTABLE
                                        switch (typeOfBone)
                                        {
                                            case "TYPE_METACARPAL":
                                                pinkyMetaCount++;
                                                pinkyMetaTable.Add(pinkyBonesCount, pinkyMeasurements[countBones]);
                                                pinkyMetaList.Add(pinkyMeasurements[countBones]);
                                                break;

                                            case "TYPE_PROXIMAL":
                                                pinkyProxCount++;
                                                pinkyProxTable.Add(pinkyBonesCount, pinkyMeasurements[countBones]);
                                                pinkyProxList.Add(pinkyMeasurements[countBones]);
                                                break;

                                            case "TYPE_INTERMEDIATE":
                                                pinkyIntCount++;
                                                pinkyIntTable.Add(pinkyBonesCount, pinkyMeasurements[countBones]);
                                                pinkyIntList.Add(pinkyMeasurements[countBones]);
                                                break;

                                            case "TYPE_DISTAL":
                                                pinkyDistCount++;
                                                pinkyDistTable.Add(pinkyBonesCount, pinkyMeasurements[countBones]);
                                                pinkyDistList.Add(pinkyMeasurements[countBones]);
                                                break;
                                        }

                                        /*using (StreamWriter boneWriter = new StreamWriter(bonesCSVPath, true))
                                        {
                                            boneWriter.WriteLine(boneLength.Type + ", Pinky, " + pinkyMeasurements[countBones]);
                                            //pinkyBonesCount++;
                                            boneReadingCount++;
                                            boneWriter.Close();
                                        }*/
                                        boneReadingCount++;
                                        break;
                                    case 5:
                                        {
                                            if (countBones >= 0 && countBones < 3)
                                            {
                                                thumbMeasurements[countBones] = (decimal)boneLength.Length;// + (decimal)boneLength.Width;
                                                thumbBoneList.Add(boneLength.Type + ", " + thumbMeasurements[countBones]);
                                                //thumbBoneList.Add(thumbMeasurements[countBones]);
                                                thumbBonesCount++;
                                                //thumbBoneTable.Add(thumbBonesCount, thumbMeasurements[countBones]);


                                                //CASE CHECK THUMB BONE AND WRITE TO HASHTABLE
                                                switch (typeOfBone)
                                                {
                                                    case "TYPE_METACARPAL":
                                                        thumbMetaCount++;
                                                        thumbMetaTable.Add(thumbBonesCount, thumbMeasurements[countBones]);
                                                        thumbMetaList.Add(thumbMeasurements[countBones]);
                                                        break;

                                                    case "TYPE_PROXIMAL":
                                                        thumbProxCount++;
                                                        thumbProxTable.Add(thumbBonesCount, thumbMeasurements[countBones]);
                                                        thumbProxList.Add(thumbMeasurements[countBones]);
                                                        break;

                                                    case "TYPE_INTERMEDIATE":
                                                        thumbIntCount++;
                                                        thumbIntTable.Add(thumbBonesCount, thumbMeasurements[countBones]);
                                                        thumbIntList.Add(thumbMeasurements[countBones]);
                                                        break;
                                                }



                                                /*using (StreamWriter boneWriter = new StreamWriter(bonesCSVPath, true))
                                                {
                                                    boneWriter.WriteLine(boneLength.Type + ", Thumb, " + thumbMeasurements[countBones]);
                                                    //thumbBonesCount++;
                                                    boneReadingCount++;
                                                    boneWriter.Close();
                                                }*/
                                                boneReadingCount++;
                                            }
                                            else { }
                                            break;
                                        }
                                }
                                countBones++;
                            }
                        }
                    }
                }
                else
                {
                    controller.StopConnection();
                    Console.WriteLine("Wrong hand used.");
                }


            }

        }

        private void onImageRequestFailed(object sender, ImageRequestFailedEventArgs e)
        {
            if (e.reason == Leap.Image.RequestFailureReason.Insufficient_Buffer)
            {
                imagedata = new byte[e.requiredBufferSize];
            }

        }

        private void onImageReady(object sender, ImageEventArgs e)
        {
            Rectangle lockArea = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            BitmapData bitmapData = bitmap.LockBits(lockArea, ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
            byte[] rawImageData = imagedata;
            System.Runtime.InteropServices.Marshal.Copy(rawImageData, 0, bitmapData.Scan0, e.image.Width * e.image.Height * 2 * e.image.BytesPerPixel);
            bitmap.UnlockBits(bitmapData);
            pictureBox1.Image = bitmap;
        }

        public Form1()
        {
            InitializeComponent();
            controller.EventContext = WindowsFormsSynchronizationContext.Current;
            //controller.FrameReady += newFrameHandler;
            controller.ImageReady += onImageReady;
            controller.ImageRequestFailed += onImageRequestFailed;
            
            //********************
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            panel1.Enabled = false;
        }

        List<string> getCSV(List<ArrayList> _list)
        {
            int colCount = 0;
            int rowcCount = 0;

            List<string> lines = new List<string>();
            for (rowcCount = 0; rowcCount < _list[1].Count; rowcCount++)
            {
                string tmpLine = "";
                for (colCount = 0; colCount < _list.Count; colCount++)
                {
                    tmpLine += _list[colCount][rowcCount].ToString() + ",";
                }
                tmpLine = tmpLine.Substring(0, tmpLine.Length - 1);
                lines.Add(tmpLine);
            }

            lines.Sort();
            return lines;
        }

        private void startScan(int time)
        {
            timeLeft = time;
            timer1.Start();
            controller.StartConnection();
            controller.FrameReady += newFrameHandler;
        }

        private string assignNewPIN()
        {
            string[] enrolledUsers = System.IO.File.ReadAllLines("enrolledUsers.txt");
            string[] allUsers = System.IO.File.ReadAllLines("authenticationPairs.txt");
            Random rand = new Random();
            int position = rand.Next(allUsers.Length);
            string newUserPin = allUsers[position].Split(',')[0] ;

            foreach (string used in enrolledUsers)
            {
                if (used == newUserPin)
                {
                    return "Cancel and receive new PIN";
                }
            }

            return newUserPin;
        }

        private string enrolUser()
        {
            Console.WriteLine("\nEnrolling new user...\n");
            vector = "";
            decimal[] vec = new decimal[19];

            //LIST ITERATION
            thumbBoneList.Sort();
            indexBoneList.Sort();
            middleBoneList.Sort();
            ringBoneList.Sort();
            pinkyBoneList.Sort();

            //THUMB
            decimal thumbMetaAverage = 0;
            foreach (decimal values in thumbMetaTable.Values)
            {
                thumbMetaAverage += values;
            }
            thumbMetaAverage = Math.Round((thumbMetaAverage / thumbMetaCount), 0, MidpointRounding.AwayFromZero);

            vector += thumbMetaAverage + ", ";
            vec[0] = thumbMetaAverage;

            decimal thumbProxAverage = 0;
            foreach (decimal values in thumbProxTable.Values)
            {
                thumbProxAverage += values;
            }
            thumbProxAverage = Math.Round((thumbProxAverage / thumbProxCount), 0, MidpointRounding.AwayFromZero);

            vector += thumbProxAverage + ", ";
            vec[1] = thumbProxAverage;

            decimal thumbIntAverage = 0;
            foreach (decimal values in thumbIntTable.Values)
            {
                thumbIntAverage += values;
            }
            thumbIntAverage = Math.Round((thumbIntAverage / thumbIntCount), 0, MidpointRounding.AwayFromZero);

            vector += thumbIntAverage + ", ";
            vec[2] = thumbIntAverage;

            //INDEX
            decimal indexMetaAverage = 0;
            foreach (decimal values in indexMetaTable.Values)
            {
                indexMetaAverage += values;
            }
            indexMetaAverage = Math.Round((indexMetaAverage / indexMetaCount), 0, MidpointRounding.AwayFromZero);

            vector += indexMetaAverage + ", ";
            vec[3] = indexMetaAverage;

            decimal indexProxAverage = 0;
            foreach (decimal values in indexProxTable.Values)
            {
                indexProxAverage += values;
            }
            indexProxAverage = Math.Round((indexProxAverage / indexProxCount), 0, MidpointRounding.AwayFromZero);

            vector += indexProxAverage + ", ";
            vec[4] = indexProxAverage;


            decimal indexIntAverage = 0;
            foreach (decimal values in indexIntTable.Values)
            {
                indexIntAverage += values;
            }
            indexIntAverage = Math.Round((indexIntAverage / indexIntCount), 0, MidpointRounding.AwayFromZero);

            vector += indexIntAverage + ", ";
            vec[5] = indexIntAverage;

            decimal indexDistAverage = 0;
            foreach (decimal values in indexDistTable.Values)
            {
                indexDistAverage += values;
            }
            indexDistAverage = Math.Round((indexDistAverage / indexDistCount), 0, MidpointRounding.AwayFromZero);

            vector += indexDistAverage + ", ";
            vec[6] = indexDistAverage;


            //MIDDLE
            decimal middleMetaAverage = 0;
            foreach (decimal values in middleMetaTable.Values)
            {
                middleMetaAverage += values;
            }
            middleMetaAverage = Math.Round((middleMetaAverage / middleMetaCount), 0, MidpointRounding.AwayFromZero);

            vector += middleMetaAverage + ", ";
            vec[7] = middleMetaAverage;

            decimal middleProxAverage = 0;
            foreach (decimal values in middleProxTable.Values)
            {
                middleProxAverage += values;
            }
            middleProxAverage = Math.Round((middleProxAverage / middleProxCount), 0, MidpointRounding.AwayFromZero);

            vector += middleProxAverage + ", ";
            vec[8] = middleProxAverage;

            decimal middleIntAverage = 0;
            foreach (decimal values in middleIntTable.Values)
            {
                middleIntAverage += values;
            }
            middleIntAverage = Math.Round((middleIntAverage / middleIntCount), 0, MidpointRounding.AwayFromZero);

            vector += middleIntAverage + ", ";
            vec[9] = middleIntAverage;

            decimal middleDistAverage = 0;
            foreach (decimal values in middleDistTable.Values)
            {
                middleDistAverage += values;
            }
            middleDistAverage = Math.Round((middleDistAverage / middleDistCount), 0, MidpointRounding.AwayFromZero);

            vector += middleDistAverage + ", ";
            vec[10] = middleDistAverage;



            //RING
            decimal ringMetaAverage = 0;
            foreach (decimal values in ringMetaTable.Values)
            {
                ringMetaAverage += values;
            }
            ringMetaAverage = Math.Round((ringMetaAverage / ringMetaCount), 0, MidpointRounding.AwayFromZero);

            vector += ringMetaAverage + ", ";
            vec[11] = ringMetaAverage;

            decimal ringProxAverage = 0;
            foreach (decimal values in ringProxTable.Values)
            {
                ringProxAverage += values;
            }
            ringProxAverage = Math.Round((ringProxAverage / ringProxCount), 0, MidpointRounding.AwayFromZero);

            vector += ringProxAverage + ", ";
            vec[12] = ringProxAverage;

            decimal ringIntAverage = 0;
            foreach (decimal values in ringIntTable.Values)
            {
                ringIntAverage += values;
            }
            ringIntAverage = Math.Round((ringIntAverage / ringIntCount), 0, MidpointRounding.AwayFromZero);

            vector += ringIntAverage + ", ";
            vec[13] = ringIntAverage;

            decimal ringDistAverage = 0;
            foreach (decimal values in ringDistTable.Values)
            {
                ringDistAverage += values;
            }
            ringDistAverage = Math.Round((ringDistAverage / ringDistCount), 0, MidpointRounding.AwayFromZero);

            vector += ringDistAverage + ", ";
            vec[14] = ringDistAverage;



            //pinky

            decimal pinkyMetaAverage = 0;
            foreach (decimal values in pinkyMetaTable.Values)
            {
                pinkyMetaAverage += values;
            }
            pinkyMetaAverage = Math.Round((pinkyMetaAverage / pinkyMetaCount), 0, MidpointRounding.AwayFromZero);

            vector += pinkyMetaAverage + ", ";
            vec[15] = pinkyMetaAverage;

            decimal pinkyProxAverage = 0;
            foreach (decimal values in pinkyProxTable.Values)
            {
                pinkyProxAverage += values;
            }
            pinkyProxAverage = Math.Round((pinkyProxAverage / pinkyProxCount), 0, MidpointRounding.AwayFromZero);

            vector += pinkyProxAverage + ", ";
            vec[16] = pinkyProxAverage;

            decimal pinkyIntAverage = 0;
            foreach (decimal values in pinkyIntTable.Values)
            {
                pinkyIntAverage += values;
            }
            pinkyIntAverage = Math.Round((pinkyIntAverage / pinkyIntCount), 0, MidpointRounding.AwayFromZero);

            vector += pinkyIntAverage + ", ";
            vec[17] = pinkyIntAverage;

            decimal pinkyDistAverage = 0;
            foreach (decimal values in pinkyDistTable.Values)
            {
                pinkyDistAverage += values;
            }
            pinkyDistAverage = Math.Round((pinkyDistAverage / pinkyDistCount), 0, MidpointRounding.AwayFromZero);

            vector += pinkyDistAverage;
            vec[18] = pinkyDistAverage;



            string newVector = transformVector(vec);
            
            

            return newVector;

        }

        private string authenticateUser()
        {
            Console.WriteLine("\nAuthenticating user...\n");
            vector = "";
            decimal[] vec = new decimal[19];

            //add original transformed vector for calculations and hash possibilities
            int[] originalVector = new int[5];

            //LIST ITERATION
            thumbBoneList.Sort();
            indexBoneList.Sort();
            middleBoneList.Sort();
            ringBoneList.Sort();
            pinkyBoneList.Sort();

            //THUMB
            decimal thumbMetaAverage = 0;
            foreach (decimal values in thumbMetaTable.Values)
            {
                thumbMetaAverage += values;
            }
            thumbMetaAverage = Math.Round((thumbMetaAverage / thumbMetaCount), 0, MidpointRounding.AwayFromZero);
            vector += thumbMetaAverage + ", ";
            vec[0] = thumbMetaAverage;

            decimal thumbProxAverage = 0;
            foreach (decimal values in thumbProxTable.Values)
            {
                thumbProxAverage += values;
            }
            thumbProxAverage = Math.Round((thumbProxAverage / thumbProxCount), 0, MidpointRounding.AwayFromZero);
            vector += thumbProxAverage + ", ";
            vec[1] = thumbProxAverage;

            decimal thumbIntAverage = 0;
            foreach (decimal values in thumbIntTable.Values)
            {
                thumbIntAverage += values;
            }
            thumbIntAverage = Math.Round((thumbIntAverage / thumbIntCount), 0, MidpointRounding.AwayFromZero);
            vector += thumbIntAverage + ", ";
            vec[2] = thumbIntAverage;

            //INDEX
            decimal indexMetaAverage = 0;
            foreach (decimal values in indexMetaTable.Values)
            {
                indexMetaAverage += values;
            }
            indexMetaAverage = Math.Round((indexMetaAverage / indexMetaCount), 0, MidpointRounding.AwayFromZero);
            vector += indexMetaAverage + ", ";
            vec[3] = indexMetaAverage;

            decimal indexProxAverage = 0;
            foreach (decimal values in indexProxTable.Values)
            {
                indexProxAverage += values;
            }
            indexProxAverage = Math.Round((indexProxAverage / indexProxCount), 0, MidpointRounding.AwayFromZero);
            vector += indexProxAverage + ", ";
            vec[4] = indexProxAverage;


            decimal indexIntAverage = 0;
            foreach (decimal values in indexIntTable.Values)
            {
                indexIntAverage += values;
            }
            indexIntAverage = Math.Round((indexIntAverage / indexIntCount), 0, MidpointRounding.AwayFromZero);
            vector += indexIntAverage + ", ";
            vec[5] = indexIntAverage;

            decimal indexDistAverage = 0;
            foreach (decimal values in indexDistTable.Values)
            {
                indexDistAverage += values;
            }
            indexDistAverage = Math.Round((indexDistAverage / indexDistCount), 0, MidpointRounding.AwayFromZero);
            vector += indexDistAverage + ", ";
            vec[6] = indexDistAverage;


            //MIDDLE
            decimal middleMetaAverage = 0;
            foreach (decimal values in middleMetaTable.Values)
            {
                middleMetaAverage += values;
            }
            middleMetaAverage = Math.Round((middleMetaAverage / middleMetaCount), 0, MidpointRounding.AwayFromZero);
            vector += middleMetaAverage + ", ";
            vec[7] = middleMetaAverage;

            decimal middleProxAverage = 0;
            foreach (decimal values in middleProxTable.Values)
            {
                middleProxAverage += values;
            }
            middleProxAverage = Math.Round((middleProxAverage / middleProxCount), 0, MidpointRounding.AwayFromZero);
            vector += middleProxAverage + ", ";
            vec[8] = middleProxAverage;

            decimal middleIntAverage = 0;
            foreach (decimal values in middleIntTable.Values)
            {
                middleIntAverage += values;
            }
            middleIntAverage = Math.Round((middleIntAverage / middleIntCount), 0, MidpointRounding.AwayFromZero);
            vector += middleIntAverage + ", ";
            vec[9] = middleIntAverage;

            decimal middleDistAverage = 0;
            foreach (decimal values in middleDistTable.Values)
            {
                middleDistAverage += values;
            }
            middleDistAverage = Math.Round((middleDistAverage / middleDistCount), 0, MidpointRounding.AwayFromZero);
            vector += middleDistAverage + ", ";
            vec[10] = middleDistAverage;



            //RING
            decimal ringMetaAverage = 0;
            foreach (decimal values in ringMetaTable.Values)
            {
                ringMetaAverage += values;
            }
            ringMetaAverage = Math.Round((ringMetaAverage / ringMetaCount), 0, MidpointRounding.AwayFromZero);
            vector += ringMetaAverage + ", ";
            vec[11] = ringMetaAverage;

            decimal ringProxAverage = 0;
            foreach (decimal values in ringProxTable.Values)
            {
                ringProxAverage += values;
            }
            ringProxAverage = Math.Round((ringProxAverage / ringProxCount), 0, MidpointRounding.AwayFromZero);
            vector += ringProxAverage + ", ";
            vec[12] = ringProxAverage;

            decimal ringIntAverage = 0;
            foreach (decimal values in ringIntTable.Values)
            {
                ringIntAverage += values;
            }
            ringIntAverage = Math.Round((ringIntAverage / ringIntCount), 0, MidpointRounding.AwayFromZero);
            vector += ringIntAverage + ", ";
            vec[13] = ringIntAverage;

            decimal ringDistAverage = 0;
            foreach (decimal values in ringDistTable.Values)
            {
                ringDistAverage += values;
            }
            ringDistAverage = Math.Round((ringDistAverage / ringDistCount), 0, MidpointRounding.AwayFromZero);
            vector += ringDistAverage + ", ";
            vec[14] = ringDistAverage;



            //pinky
            decimal pinkyMetaAverage = 0;
            foreach (decimal values in pinkyMetaTable.Values)
            {
                pinkyMetaAverage += values;
            }
            pinkyMetaAverage = Math.Round((pinkyMetaAverage / pinkyMetaCount), 0, MidpointRounding.AwayFromZero);
            vector += pinkyMetaAverage + ", ";
            vec[15] = pinkyMetaAverage;

            decimal pinkyProxAverage = 0;
            foreach (decimal values in pinkyProxTable.Values)
            {
                pinkyProxAverage += values;
            }
            pinkyProxAverage = Math.Round((pinkyProxAverage / pinkyProxCount), 0, MidpointRounding.AwayFromZero);
            vector += pinkyProxAverage + ", ";
            vec[16] = pinkyProxAverage;

            decimal pinkyIntAverage = 0;
            foreach (decimal values in pinkyIntTable.Values)
            {
                pinkyIntAverage += values;
            }
            pinkyIntAverage = Math.Round((pinkyIntAverage / pinkyIntCount), 0, MidpointRounding.AwayFromZero);
            vector += pinkyIntAverage + ", ";
            vec[17] = pinkyIntAverage;

            decimal pinkyDistAverage = 0;
            foreach (decimal values in pinkyDistTable.Values)
            {
                pinkyDistAverage += values;
            }
            pinkyDistAverage = Math.Round((pinkyDistAverage / pinkyDistCount), 0, MidpointRounding.AwayFromZero);
            vector += pinkyDistAverage;
            vec[18] = pinkyDistAverage;
            


            string newVector = transformVector(vec);
            Console.WriteLine("\nTransformed Geo: " + newVector + "\n\n");

            //vectorCheck(originalTransformedVector(vec));

            //check all vector combinations
            possibleVectorCombinations(originalTransformedVector(vec),0);

            return newVector;

        }

        private string transformVector(decimal[] arr)
        {
            decimal[] transformedVector = new decimal[5];

            transformedVector[0] = arr[0] + arr[1] + arr[2];
            transformedVector[1] = arr[3] + arr[4] + arr[5] + arr[6];
            transformedVector[2] = arr[7] + arr[8] + arr[9] + arr[10];
            transformedVector[3] = arr[11] + arr[12] + arr[13] + arr[14];
            transformedVector[4] = arr[15] + arr[16] + arr[17] + arr[18];

            return Convert.ToString(transformedVector[0]) + "," + Convert.ToString(transformedVector[1] + "," + Convert.ToString(transformedVector[2]) + "," + Convert.ToString(transformedVector[3]) + "," + Convert.ToString(transformedVector[4]));
        }

        private void btnScan_Click(object sender, EventArgs e)
        {
            txtPin.Text = assignNewPIN();
            checkAuthenticateEnrol = true;
            matrixCSVPath = "";
            //user files
            userFileName = txtPin.Text;
            directory = @"C:\Users\23509384\Desktop\Enrol\" + userFileName;
            bonesFilePath = @"C:\Users\23509384\Desktop\Enrol\" + userFileName + "\\Bones";
            fingersFilePath = @"C:\Users\23509384\Desktop\Enrol\" + userFileName + "\\Fingers";
            bonesCSVPath = bonesFilePath + "\\" + userFileName + ".csv";
            fingersCSVPath = fingersFilePath + "\\" + userFileName + ".csv";
            matrixCSVPath = directory + "\\Matrix.csv";
            if (userFileName != "" && userFileName.Length == 4)
            {

                if (!File.Exists(bonesFilePath) && (!File.Exists(fingersFilePath)))
                {

                    Directory.CreateDirectory(bonesFilePath);
                    Directory.CreateDirectory(fingersFilePath);

                    FileStream boneStream = File.Create(bonesCSVPath);
                    boneStream.Close();

                    FileStream fingerStream = File.Create(fingersCSVPath);
                    fingerStream.Close();
                    startScan(5);

                }
                else
                {
                    MessageBox.Show("File Already Exists!");
                }

            }
            else
            {
                MessageBox.Show("Please enter a valid PIN");
            }
        }

        private void btnShow_Click(object sender, EventArgs e)
        {
            authenticatePin(txtPin.Text);
            generatePINHash(startUserKey, txtPin.Text);
            generateEnrolUserHash(startUserKey, enrolUser());
            userLog("enrolled", txtPin.Text, enrolUser());
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                panel1.Enabled = true;
                panel2.Enabled = false;
            }
            else
            {
                panel1.Enabled = false;
                panel2.Enabled = true;
            }
        }

        private void btnValidate_Click(object sender, EventArgs e)
        {
            authenticatePin(txtPin.Text);
            generatePINHash(startUserKey, txtPin.Text);
            string authUser = authenticateUser();
            generateAuthenticateUserHash(startUserKey, authUser);
            userLog("authenticated",txtPin.Text, authUser);

            //writeCombosToTextFile(combos);

            //close text file to read and check
            sw.Flush();
            sw.Close();

            readCombosListForMatch();

        }

        private void btnAuthenticate_Click(object sender, EventArgs e)
        {
            checkAuthenticateEnrol = true;
            matrixCSVPath = "";
            //user files
            userFileName = txtPin.Text;
            directory = @"C:\Users\23509384\Desktop\Authenticate\" + userFileName;
            bonesFilePath = @"C:\Users\23509384\Desktop\Authenticate\" + userFileName + "\\Bones";
            fingersFilePath = @"C:\Users\23509384\Desktop\Authenticate\" + userFileName + "\\Fingers";
            bonesCSVPath = bonesFilePath + "\\" + userFileName + ".csv";
            fingersCSVPath = fingersFilePath + "\\" + userFileName + ".csv";
            matrixCSVPath = directory + "\\Matrix.csv";
            if (userFileName != "" && userFileName.Length == 4)
            {

                if (!File.Exists(bonesFilePath) && (!File.Exists(fingersFilePath)))
                {

                    Directory.CreateDirectory(bonesFilePath);
                    Directory.CreateDirectory(fingersFilePath);

                    FileStream boneStream = File.Create(bonesCSVPath);
                    boneStream.Close();

                    FileStream fingerStream = File.Create(fingersCSVPath);
                    fingerStream.Close();
                    startScan(2);

                }
                else
                {
                    MessageBox.Show("File Already Exists!");
                }

            }
            else
            {
                MessageBox.Show("Please enter a valid PIN");
            }
        }

        private void vectorCheck(int[] originalVector)
        {
            int[] lowVector = new int[5];
            int[] highVector = new int[5];
            int count = 0;

            foreach(int vec in originalVector)
            {
                Array.Copy(originalVector, lowVector, 5);
                Array.Copy(originalVector, highVector, 5);
                highVector[count] = originalVector[count] + 1;
                lowVector[count] = originalVector[count] - 1;
                printLowVector(lowVector);
                printHighVector(highVector);
                count++;
            }
        }

        private string printLowVector(int[] lowVector)
        {
            HashSet<int> lowHashSet = new HashSet<int>();
            string low = "";
            foreach(int i in lowVector)
            {
                low += Convert.ToString(i) + ",";
                lowHashSet.Add(i);
            }
            generateVectorHash(low.TrimEnd(','));
            return low;
        }

        private string printHighVector(int[] highVector)
        {
            HashSet<int> highHashSet = new HashSet<int>();
            string high = "";
            foreach (int i in highVector)
            {
                high += Convert.ToString(i) + ",";
                highHashSet.Add(i);
            }
            generateVectorHash(high.TrimEnd(','));
            return high;
        }

        private HashSet<int> generateVectorHash(string input)
        {
            byte[] hash;
            HashSet<int> returnedHash = new HashSet<int>();
            checkComboDuplicates(input);
            using (var sha2 = new SHA256CryptoServiceProvider())
            {
                hash = sha2.ComputeHash(Encoding.Unicode.GetBytes(input));
                foreach(byte b in hash)
                {
                    returnedHash.Add(b);
                }
            }
            //checkHashes(returnedHash);
            return returnedHash;
        }

        private bool checkHashes(HashSet<int> hand)
        {
            if (hand.SetEquals(userHandGeo))
            {
                //Console.WriteLine("TRUE CHECK");
                return true;
            }else
            {
                //Console.WriteLine("FALSE CHECK");
                return false;
            }
        }

        private int[] originalTransformedVector(decimal[] arr)
        {
            int[] transformedVector = new int[5];

            transformedVector[0] = (int)(arr[0] + arr[1] + arr[2]);
            transformedVector[1] = (int)(arr[3] + arr[4] + arr[5] + arr[6]);
            transformedVector[2] = (int)(arr[7] + arr[8] + arr[9] + arr[10]);
            transformedVector[3] = (int)(arr[11] + arr[12] + arr[13] + arr[14]);
            transformedVector[4] = (int)(arr[15] + arr[16] + arr[17] + arr[18]);

            return transformedVector;
        }
        
        //recursive function checking all possible vector combinations
        private void possibleVectorCombinations(int[] originalVector, int count)
        {
            int increment = 0;
            int[] low = new int[originalVector.Length];
            int[] high = new int[originalVector.Length];

            if (count == originalVector.Length)
                return;

            possibleVectorCombinations(originalVector, count + 1);

            foreach (int val in originalVector)
            {
                increment++;
                Array.Copy(originalVector, low, originalVector.Length);
                low[count] = originalVector[count] - increment;
                Array.Copy(originalVector, high, originalVector.Length);
                high[count] = originalVector[count] + increment;
                printOutCombinations(low, high);
                //RECURSE
                possibleVectorCombinations(low, count + 1);
                possibleVectorCombinations(high, count + 1);
            }
            
        }

        //function to print out combinations
        private void printOutCombinations(int[] low, int[] high)
        {
            string lowArr = "";
            string highArr = "";

            foreach (int l in low)
                lowArr += Convert.ToString(l) + ",";

            foreach (int h in high)
                highArr += Convert.ToString(h) + ",";


            HashSet<int> lowSet = new HashSet<int>();
            HashSet<int> highSet = new HashSet<int>();
            lowSet = generateComboHashes(lowArr.TrimEnd(','));
            highSet = generateComboHashes(highArr.TrimEnd(','));
        }

        //function to generate combination hashes
        private HashSet<int> generateComboHashes(string input)
        {
            HashSet<int> finalHash = new HashSet<int>();
            //string finalHash = "";
            byte[] hash;

            checkComboDuplicates(input);

            using (var sha2 = new SHA256CryptoServiceProvider())
            {
                hash = sha2.ComputeHash(Encoding.Unicode.GetBytes(input));

                foreach (byte b in hash)
                    finalHash.Add(b);
                    //finalHash += b + " ";

            }

            return finalHash;
     
        }

        private void checkComboDuplicates(string input)
        {
            
            if (!combos.Contains(input))
            {
                //Console.WriteLine(input);
                combos.Add(input);
                //sw.WriteLine(input);
            }
        }
        static StreamWriter sw = new StreamWriter("combos.txt");
        private void writeCombosToTextFile(HashSet<string> combinations)
        {
            

            foreach (string i in combinations)
                sw.WriteLine(i);

            sw.Flush();
            sw.Close();
        }

        private HashSet<int> checkComboHashes(string input)
        {
            HashSet<int> finalHash = new HashSet<int>();
            byte[] hash;
            using (var sha2 = new SHA256CryptoServiceProvider())
            {
                hash = sha2.ComputeHash(Encoding.Unicode.GetBytes(input));

                foreach (byte b in hash)
                    finalHash.Add(b);
            }
            return finalHash;
        }

        private void readCombosListForMatch()
        {
            //string[] read = File.ReadAllLines("combos.txt");

            //dictionary of values
            /*Dictionary<string, HashSet<int>> dic = new Dictionary<string, HashSet<int>>();
                        
            foreach(string i in combos)
            {
                dic.Add(i, generateComboHashes(i));   
            }*/

            Console.WriteLine("Scanning...");
            foreach (string i in combos)
            {
                if (pixelValues.SetEquals(checkComboHashes(i)))
                {
                    MessageBox.Show("MATCH FOUND!");
                    return;
                }
                    
            }
                
            /*foreach(var val in dic.Values)
            {
                if (val.SetEquals(currentUserHandGeo))
                    MessageBox.Show("ONE OF THE COMBOS MATCH!!!");
                else
                    Console.WriteLine("NO MATCH!");
            }*/
        }
    }
}
