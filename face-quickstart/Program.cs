using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;

namespace face_quickstart
{
    class Program
    {
        // Used for all examples.
        // URL for the images.
        const string IMAGE_BASE_URL = "https://csdx.blob.core.windows.net/resources/Face/Images/";

        // Used in the Detect Faces and Verify examples.
        // Recognition model 2 is used for feature extraction, use 1 to simply recognize/detect a face. 
        // However, the API calls to Detection that are used with Verify, Find Similar, or Identify must share the same recognition model.
        const string RECOGNITION_MODEL2 = RecognitionModel.Recognition02;
        const string RECOGNITION_MODEL1 = RecognitionModel.Recognition01;

        /// <summary>
        /// プログラムメイン
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {

            //エンドポイント: https://facecognitiveservice.cognitiveservices.azure.com/
            //キー 1: d118775a210d4c9b86e14e85d72febdb
            //キー 2: 
            string SUBSCRIPTION_KEY = "5c9308b337cc4f6e910a60a200ceda26";
            string ENDPOINT = "https://facecognitiveservice.cognitiveservices.azure.com/";

            string apikey = SUBSCRIPTION_KEY;
            // 引数があれば、apikeyを差し替える
            if ((args != null) && (args.Count() > 1))
            {
                apikey = args[0];
            }


            Console.WriteLine("/// Program start. ///");


            //Get source Image file names
            string inputFolder = GetInputFolder();
            string[] imageFileNames = GetSourceImages(inputFolder);

            //Outut file's name to txt file. 
            string outputFolder = GetOutputFolder();
            OutputFileList(outputFolder, imageFileNames);

            // Authenticate.
            IFaceClient client = Authenticate(ENDPOINT, apikey);

            // Detect & 切り出し
            // Detect - get features from faces.
            //DetectFaceExtract(client, IMAGE_BASE_URL, RECOGNITION_MODEL2).Wait();
            AsyncProcDetect(client, imageFileNames, outputFolder, RECOGNITION_MODEL2).Wait();


            Console.WriteLine();
            Console.WriteLine("/// Program end. ///");
            Console.WriteLine("hit any key.");
            Console.ReadKey();

        }

        /// <summary>
        /// detect ＆ 切り出し処理
        /// </summary>
        /// <param name="client"></param>
        /// <param name="imageFileNames"></param>
        /// <param name="outputFolder"></param>
        /// <param name="recognitionModel"></param>
        private static async Task AsyncProcDetect(IFaceClient client, string[] imageFileNames, string outputFolder, string recognitionModel)
        {
            Console.WriteLine("========DETECT FACES ImageFile========");
            Console.WriteLine();

            if (!outputFolder.EndsWith(@"\"))
            {
                outputFolder += @"\";
            }

            IList<DetectedFace> detectedFaces = null;

            foreach (var imageFileName in imageFileNames)
            {

                using (FileStream fs = new FileStream(imageFileName, FileMode.Open, FileAccess.Read))
                {

                    // Detect faces with all attributes from image url.
                    detectedFaces = await client.Face.DetectWithStreamAsync(fs,
                            returnFaceAttributes: new List<FaceAttributeType> { FaceAttributeType.Accessories, FaceAttributeType.Age,
                                    FaceAttributeType.Blur, FaceAttributeType.Emotion, FaceAttributeType.Exposure, FaceAttributeType.FacialHair,
                                    FaceAttributeType.Gender, FaceAttributeType.Glasses, FaceAttributeType.Hair, FaceAttributeType.HeadPose,
                                    FaceAttributeType.Makeup, FaceAttributeType.Noise, FaceAttributeType.Occlusion, FaceAttributeType.Smile },
                            recognitionModel: recognitionModel);

                    Console.WriteLine($"{detectedFaces.Count} face(s) detected from image `{imageFileName}`.");
                }

                using (FileStream fs2 = new FileStream(imageFileName, FileMode.Open, FileAccess.Read))
                {
                    // 切り出し切り出し
                    int faceIndex = 0;
                    using (Bitmap bmp = new Bitmap(fs2))
                    {
                        foreach (var info in detectedFaces)
                        {
                            Rectangle rect = new Rectangle(info.FaceRectangle.Left, info.FaceRectangle.Top,
                                                            info.FaceRectangle.Width, info.FaceRectangle.Height);
                            var cutImage = bmp.Clone(rect, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                            //保存
                            FileInfo info1 = new FileInfo(imageFileName);
                            string outFilePrefix = info1.Name.Substring(0, info1.Name.LastIndexOf("."));
                            string outFileName = outputFolder + outFilePrefix + "_d_" + $"{faceIndex}" + ".jpg";
                            cutImage.Save(outFileName, ImageFormat.Jpeg);
                            faceIndex++;
                        }
                    }
                }

            }

        }

        /// <summary>
        /// Consoleから出力フォルダ取得
        /// </summary>
        /// <returns></returns>
        private static string GetOutputFolder()
        {
            string outputFolder = null;
            while (string.IsNullOrEmpty(outputFolder))
            {
                Console.WriteLine();
                Console.WriteLine("Please input result folder");
                outputFolder = Console.ReadLine();

            }

            return outputFolder;

        }

        /// <summary>
        /// Consoleから入力フォルダ取得
        /// </summary>
        /// <returns></returns>
        private static string GetInputFolder()
        {
            string inputFolder = null;
            while (string.IsNullOrEmpty(inputFolder))
            {
                Console.WriteLine();
                Console.WriteLine("Please input source folder.");
                inputFolder = Console.ReadLine();
            }

            return inputFolder;

        }

        /// <summary>
        /// 指定フォルダの画像ファイル顔認識処理
        /// </summary>
        /// <param name="client"></param>
        /// <param name="imageFileNames"></param>
        /// <param name="outputFolder"></param>
        /// <param name="recognitionModel"></param>
        /// <returns></returns>
        private static async Task<IList<DetectedFace>> DetectFaceExtractImageFileEx(IFaceClient client, string[] imageFileNames, string outputFolder, string recognitionModel)
        {
            Console.WriteLine("========DETECT FACES ImageFile========");
            Console.WriteLine();

            if (!outputFolder.EndsWith(@"\"))
            {
                outputFolder = outputFolder + @"\";
            }

            IList<DetectedFace> detectedFaces = null;

            foreach (var imageFileName in imageFileNames)
            {

                using (FileStream fs = new FileStream(imageFileName, FileMode.Open, FileAccess.Read))
                {
                    // Detect faces with all attributes from image url.
                    detectedFaces = await client.Face.DetectWithStreamAsync(fs,
                            returnFaceAttributes: new List<FaceAttributeType> { FaceAttributeType.Accessories, FaceAttributeType.Age,
                                FaceAttributeType.Blur, FaceAttributeType.Emotion, FaceAttributeType.Exposure, FaceAttributeType.FacialHair,
                                FaceAttributeType.Gender, FaceAttributeType.Glasses, FaceAttributeType.Hair, FaceAttributeType.HeadPose,
                                FaceAttributeType.Makeup, FaceAttributeType.Noise, FaceAttributeType.Occlusion, FaceAttributeType.Smile },
                            recognitionModel: recognitionModel);

                    Console.WriteLine($"{detectedFaces.Count} face(s) detected from image `{imageFileName}`.");
                }
            }

            return detectedFaces;
        }

        /// <summary>
        /// 指定フォルダの画像ファイルリスト取得
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns>List of file names</returns>
        private static string[] GetSourceImages(string folderPath)
        {
            return Directory.GetFiles(folderPath, "*.jpg", SearchOption.TopDirectoryOnly);

        }

        /// <summary>
        /// 指定フォルダにファイルリストのテキストファイルを書き出してみる
        /// </summary>
        /// <param name="outputFolder"></param>
        /// <param name="imageFileNames"></param>
        private static void OutputFileList(string outputFolder, string[] imageFileNames)
        {
            string aaa = $@"{ outputFolder }\_fileList.txt";
            using (StreamWriter sw = new StreamWriter($@"{ outputFolder }\_fileList.txt"))
            {
                foreach (var fileName in imageFileNames)
                {
                    sw.WriteLine(fileName);
                }
            }
        }
        /*
         *	AUTHENTICATE
         *	Uses subscription key and region to create a client.
        */
        public static IFaceClient Authenticate(string endpoint, string key)
        {
            return new FaceClient(new ApiKeyServiceClientCredentials(key)) { Endpoint = endpoint };
        }

        /* 
         * DETECT FACES of Image File
         * Detects features from faces and IDs them.
         */
        public static async Task DetectFaceExtractImageFile(IFaceClient client, string path, string recognitionModel)
        {
            Console.WriteLine("========DETECT FACES ImageFile========");
            Console.WriteLine();

            // Create a list of images
            List<string> imageFileNames = new List<string>
                    {
                        "detection1.jpg",    // single female with glasses
                        // "detection2.jpg", // (optional: single man)
                        // "detection3.jpg", // (optional: single male construction worker)
                        // "detection4.jpg", // (optional: 3 people at cafe, 1 is blurred)
                        "detection5.jpg",    // family, woman child man
                        "detection6.jpg"     // elderly couple, male female
                    };

            if (!path.EndsWith(@"\"))
            {
                path = path + @"\";
            }

            foreach (var imageFileName in imageFileNames)
            {
                IList<DetectedFace> detectedFaces;

                using (FileStream fs = new FileStream($"{path}{imageFileName}", FileMode.Open, FileAccess.Read))
                {
                    // Detect faces with all attributes from image url.
                    detectedFaces = await client.Face.DetectWithStreamAsync(fs,
                            returnFaceAttributes: new List<FaceAttributeType> { FaceAttributeType.Accessories, FaceAttributeType.Age,
                                FaceAttributeType.Blur, FaceAttributeType.Emotion, FaceAttributeType.Exposure, FaceAttributeType.FacialHair,
                                FaceAttributeType.Gender, FaceAttributeType.Glasses, FaceAttributeType.Hair, FaceAttributeType.HeadPose,
                                FaceAttributeType.Makeup, FaceAttributeType.Noise, FaceAttributeType.Occlusion, FaceAttributeType.Smile },
                            recognitionModel: recognitionModel);

                    Console.WriteLine($"{detectedFaces.Count} face(s) detected from image `{imageFileName}`.");
                }
            }

        }

        /* 
         * DETECT FACES
         * Detects features from faces and IDs them.
         */
        public static async Task DetectFaceExtract(IFaceClient client, string url, string recognitionModel)
        {
            Console.WriteLine("========DETECT FACES========");
            Console.WriteLine();

            // Create a list of images
            List<string> imageFileNames = new List<string>
                    {
                        "detection1.jpg",    // single female with glasses
                        // "detection2.jpg", // (optional: single man)
                        // "detection3.jpg", // (optional: single male construction worker)
                        // "detection4.jpg", // (optional: 3 people at cafe, 1 is blurred)
                        "detection5.jpg",    // family, woman child man
                        "detection6.jpg"     // elderly couple, male female
                    };

            foreach (var imageFileName in imageFileNames)
            {
                IList<DetectedFace> detectedFaces;

                // Detect faces with all attributes from image url.
                detectedFaces = await client.Face.DetectWithUrlAsync($"{url}{imageFileName}",
                        returnFaceAttributes: new List<FaceAttributeType> { FaceAttributeType.Accessories, FaceAttributeType.Age,
                FaceAttributeType.Blur, FaceAttributeType.Emotion, FaceAttributeType.Exposure, FaceAttributeType.FacialHair,
                FaceAttributeType.Gender, FaceAttributeType.Glasses, FaceAttributeType.Hair, FaceAttributeType.HeadPose,
                FaceAttributeType.Makeup, FaceAttributeType.Noise, FaceAttributeType.Occlusion, FaceAttributeType.Smile },
                        recognitionModel: recognitionModel);

                Console.WriteLine($"{detectedFaces.Count} face(s) detected from image `{imageFileName}`.");
            }
        }
    }
}
